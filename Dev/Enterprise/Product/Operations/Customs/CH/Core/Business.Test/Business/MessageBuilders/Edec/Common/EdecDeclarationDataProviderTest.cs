using System;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

abstract class EdecDeclarationDataProviderTest<T> : TestCaseWithFactory
														where T : DeclarationMessageSendingObject
{
	protected abstract string MessageType { get; }

	protected abstract DeclarationMessageSendingObject GetMessageSendingObject(CusEntryHeader header);

	protected abstract EdecDeclarationDataProvider<T> GetEdecDeclarationDataProvider(DeclarationMessageSendingObject sendingObject);

	protected abstract string ExpectedServiceType { get; }

	public void TestConstructorNullArgument()
	{
		AssertExceptionThrown<ArgumentNullException>("Argument == null", () => GetEdecDeclarationDataProvider(null));
	}

	public virtual void TestProvider()
	{
		declaration.JE_OwnerRef = "HB345678901234567890123456789012345";
		declaration.JE_DeclarationLanguage = "FR";
		declaration.JE_CustomsOffice = "CO345678";
		declaration.JE_ContainerMode = Core.Constants.ContainerModes.FCL;

		entryHeader.CH_BGMReference = "TDN4567890123456789012";

		entryInstruction.CEI_Style = "01";
		entryInstruction.CEI_SubStyle = "02";
		entryInstruction.CEI_DeclarationReason = "03";

		messageSendingObject.MessageType = PassarMessageTypeList.Codes.NI015;

		CombineAssertions(() =>
		{
			AssertEquals(nameof(dataProvider.ServiceType), ExpectedServiceType, dataProvider.ServiceType);
			AssertEquals(nameof(dataProvider.TraderDeclarationNumber), "TDN4567890123456789012", dataProvider.TraderDeclarationNumber);
			AssertEquals(nameof(dataProvider.TraderReference), "HB345678901234567890123456789012345", dataProvider.TraderReference);
			AssertEquals(nameof(dataProvider.DeclarationType), "01", dataProvider.DeclarationType);
			AssertEquals(nameof(dataProvider.DeclarationTime), "02", dataProvider.DeclarationTime);
			AssertEquals(nameof(dataProvider.CorrectionCode), "1", dataProvider.CorrectionCode);
			AssertEquals(nameof(dataProvider.CorrectionReason), string.Empty, dataProvider.CorrectionReason);
			AssertEquals(nameof(dataProvider.Language), "fr", dataProvider.Language);
			AssertEquals(nameof(dataProvider.Reason), "03", dataProvider.Reason);
			AssertEquals(nameof(dataProvider.TransportInContainer), true, dataProvider.TransportInContainer);
		});
	}

	public void TestTransportInContainer()
	{
		CombineAssertions(() =>
		{
			var containerCodes = new[] { Core.Constants.ContainerModes.FCL, Core.Constants.ContainerModes.LCL, Core.Constants.ContainerModes.Containerised };
			foreach (var value in declaration.Lookups.CargoIdTypeList.GetAllCodes())
			{
				declaration.JE_ContainerMode = value;
				AssertEquals(declaration.JE_ContainerMode, containerCodes.Contains(value), dataProvider.TransportInContainer);
			}
		});
	}

	public void TestSpecialMention()
	{
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine.PK;

		(var invoice3, var invoiceLine3, var instruction, var header, var line) = CreateDeclarationInvoice(declaration);

		invoice.SpecialMentions = "Line 1a\r\nLine 1b\r\nLine 1c";
		invoice2.SpecialMentions = "Line 2a\r\nLine 2b";
		invoice3.SpecialMentions = "Line 3a\r\nLine 3b";

		CombineAssertions(() =>
		{
			var specialMentionDataProviders = dataProvider.SpecialMentions;
			AssertEquals("Count", 5, specialMentionDataProviders.Count());
			AssertEquals("Text[0]", "Line 1a", specialMentionDataProviders.ElementAt(0).Text);
			AssertEquals("Text[1]", "Line 1b", specialMentionDataProviders.ElementAt(1).Text);
			AssertEquals("Text[2]", "Line 1c", specialMentionDataProviders.ElementAt(2).Text);
			AssertEquals("Text[3]", "Line 2a", specialMentionDataProviders.ElementAt(3).Text);
			AssertEquals("Text[4]", "Line 2b", specialMentionDataProviders.ElementAt(4).Text);
			AssertEquals("Seq#[0]", 1, specialMentionDataProviders.ElementAt(0).SequenceNumber);
			AssertEquals("Seq#[1]", 2, specialMentionDataProviders.ElementAt(1).SequenceNumber);
			AssertEquals("Seq#[2]", 3, specialMentionDataProviders.ElementAt(2).SequenceNumber);
			AssertEquals("Seq#[3]", 4, specialMentionDataProviders.ElementAt(3).SequenceNumber);
			AssertEquals("Seq#[4]", 5, specialMentionDataProviders.ElementAt(4).SequenceNumber);

			AssertSame("Cached", dataProvider.SpecialMentions, dataProvider.SpecialMentions);
		});
	}

	public void TestSpecialMentionMax99Lines()
	{
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine.PK;

		var eightyLines = new ZStringBuilder();
		for (var n = 1; n <= 80; n++)
		{
			eightyLines.AppendFormat("Line {0}\r\n", n.ToString());
		}

		invoice.SpecialMentions = eightyLines.ToString();
		invoice2.SpecialMentions = eightyLines.ToString();

		CombineAssertions(() =>
		{
			var specialMentionDataProviders = dataProvider.SpecialMentions;
			AssertEquals("Count", 99, specialMentionDataProviders.Count());
		});
	}

	public void TestContainers()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageType;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

		JobComInvoiceLine addNewInvoiceLineToNewInstruction()
		{
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CEI = declaration.CustomsEntryInstructions.AddNew().PK;
			return invoiceLine;
		}

		void linkNewPackageToInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			var package = declaration.Packages.AddNew();
			invoiceLine.PackagesForInvoiceLinesForBindingOnly.Cast<BaseCusLinkPackage>().First(x => x.PackagePk == package.PK).IsLinked = true;
		}

		void addPackageToNewContainer(JobComInvoiceLine invoiceLine, string containerNumber)
		{
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = containerNumber;
			var pivot1 = invoiceLine.ContainersPivot.AddNew();
			pivot1.C2_CO = container.PK;
		}

		var invoiceLine1 = addNewInvoiceLineToNewInstruction();
		var invoiceLine2 = addNewInvoiceLineToNewInstruction();

		linkNewPackageToInvoiceLine(invoiceLine1);
		linkNewPackageToInvoiceLine(invoiceLine2);

		addPackageToNewContainer(invoiceLine1, "CNT1");
		addPackageToNewContainer(invoiceLine1, "CNT2");
		addPackageToNewContainer(invoiceLine1, "CNT2");
		addPackageToNewContainer(invoiceLine2, "CNT3");

		declaration.DoMerge();

		var entryHeader = invoiceLine1.CusEntryLine.Header;
		var messageSendingObject = GetMessageSendingObject((CusEntryHeader)entryHeader);
		var messageBuilder = GetEdecDeclarationDataProvider(messageSendingObject);

		var containers = messageBuilder.Containers;

		CombineAssertions(() =>
		{
			AssertNotNull("Containers should be defined", containers);
			AssertEquals(true, containers.FirstOrDefault() is IEdecContainer);
			AssertEquals("MessageBuilder should contain 2 containers", 2, containers.ToList().Count);
			AssertContainsExactElementsInAnyOrder("MessageBuilder should contain CNT1 and CNT2", new[] { "CNT1", "CNT2" }, containers.Select(x => x.ContainerNumber).ToList());
		});
	}

	public void TestPreviousDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageType;
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

		JobComInvoiceHeader addPreviousDocumentToNewInstruction()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			invoiceHeader.PreviousDocuments.AddNew();
			return invoiceHeader;
		}

		var invoiceHeader1 = addPreviousDocumentToNewInstruction();
		addPreviousDocumentToNewInstruction();

		declaration.DoMerge();

		var entryHeader = declaration.CustomsEntryHeaders[0];
		var messageSendingObject = GetMessageSendingObject(entryHeader);
		var messageBuilder = GetEdecDeclarationDataProvider(messageSendingObject);

		var messageBuilderPreviousDocuments = messageBuilder.PreviousDocuments;
		CombineAssertions(() =>
		{
			AssertNotNull("Previous Documents should be defined", messageBuilderPreviousDocuments);
			AssertEquals(true, messageBuilderPreviousDocuments.FirstOrDefault() is IEdecPreviousDocument);
			AssertEquals("Number of total previous document should be: ", 1, invoiceHeader1.PreviousDocuments.Count);
		});
	}

	public void TestDefaultLanguage() => AssertEquals(nameof(dataProvider.Language), "xx", dataProvider.Language);

	public void TestTransportMeans()
	{
		declaration = Factory.New<JobDeclaration>();

		CombineAssertions(() =>
		{
			AssertNotNull("Transport Means should be defined", dataProvider.TransportMeans);
			AssertEquals(true, dataProvider.TransportMeans is IEdecTransportMeans);
		});
	}

	public void TestBusiness() => AssertNotNull(nameof(dataProvider.Business), dataProvider.Business);

	public void TestEdecDeclarant()
	{
		var orgHeader = Factory.New<OrgHeader>();
		var orgAddress = Factory.New<OrgAddress>();

		orgHeader.OH_FullName = "Test Company Ltd.";
		orgAddress.OA_Address1 = "5th Ave";
		orgAddress.OA_PostCode = "10118-4810";
		orgAddress.OA_City = "New York";
		orgAddress.OA_RN_NKCountryCode = "US";
		orgAddress.OA_State = "NY";

		orgHeader.Addresses.Add(orgAddress);
		declaration.JE_OA_DeclarantAddress = orgAddress.PK;

		var messageSendingObject = GetMessageSendingObject(entryHeader);
		var messageBuilder = GetEdecDeclarationDataProvider(messageSendingObject);

		var declarantAdress = messageBuilder.Declarant;

		CombineAssertions(() =>
		{
			AssertNotNull(declarantAdress);

			AssertEquals("Declarant Name", orgHeader.OH_FullName, messageBuilder.Declarant.Name);
			AssertEquals("Declarant Street", orgAddress.OA_Address1, messageBuilder.Declarant.Street);
			AssertEquals("Declarant Postcode", orgAddress.Postcode, messageBuilder.Declarant.PostalCode);
			AssertEquals("Declarant City", orgAddress.OA_City, messageBuilder.Declarant.City);
			AssertEquals("Declarant Country", orgAddress.OA_RN_NKCountryCode, messageBuilder.Declarant.Country);
		});
	}

	public void TestCorrectionCode()
	{
		CombineAssertions(() =>
		{
			messageSendingObject.MessageType = PassarMessageTypeList.Codes.NI015;
			AssertEquals($"{nameof(messageSendingObject.MessageType)}={messageSendingObject.MessageType}", MessagingConstants.CustomsCorrectionCode.Original, dataProvider.CorrectionCode);

			messageSendingObject.MessageType = PassarMessageTypeList.Codes.NI013;
			AssertEquals($"{nameof(messageSendingObject.MessageType)}={messageSendingObject.MessageType}", MessagingConstants.CustomsCorrectionCode.Correction, dataProvider.CorrectionCode);

			messageSendingObject.MessageType = PassarMessageTypeList.Codes.NI014;
			AssertEquals($"{nameof(messageSendingObject.MessageType)}={messageSendingObject.MessageType}", MessagingConstants.CustomsCorrectionCode.Cancellation, dataProvider.CorrectionCode);

			messageSendingObject.MessageType = PassarMessageTypeList.Codes.NI016;
			AssertEquals($"{nameof(messageSendingObject.MessageType)}={messageSendingObject.MessageType}", MessagingConstants.CustomsCorrectionCode.RequestLastResponse, dataProvider.CorrectionCode);
		});
	}

	public void TestCorrectionReason()
	{
		var reason = "reason";

		CombineAssertions(() =>
		{
			messageSendingObject.VOCReason = reason;
			messageSendingObject.MessageType = PassarMessageTypeList.Codes.NI015;
			AssertEquals($"{nameof(messageSendingObject.MessageType)}={messageSendingObject.MessageType}", string.Empty, dataProvider.CorrectionReason);

			messageSendingObject.VOCReason = reason;
			messageSendingObject.MessageType = PassarMessageTypeList.Codes.NI013;
			AssertEquals($"{nameof(messageSendingObject.MessageType)}={messageSendingObject.MessageType}", reason, dataProvider.CorrectionReason);

			messageSendingObject.VOCReason = reason;
			messageSendingObject.MessageType = PassarMessageTypeList.Codes.NI014;
			AssertEquals($"{nameof(messageSendingObject.MessageType)}={messageSendingObject.MessageType}", reason, dataProvider.CorrectionReason);

			messageSendingObject.VOCReason = reason;
			messageSendingObject.MessageType = PassarMessageTypeList.Codes.NI016;
			AssertEquals($"{nameof(messageSendingObject.MessageType)}={messageSendingObject.MessageType}", string.Empty, dataProvider.CorrectionReason);
		});
	}

	public virtual void TestAddresses()
	{
		declaration.JE_OH_Supplier = newOrgHeader("SUPPLIER").PK;
		declaration.JE_OH_Consignee = newOrgHeader("CONSIGNEE").PK;
		declaration.JE_OA_Representative = newOrgHeader("REPRESENTATIVE").MainAddress.PK;

		CombineAssertions(() =>
		{
			AssertEquals(nameof(dataProvider.ConsignorAddress), "SUPPLIER", dataProvider.ConsignorAddress?.Name);
			AssertSame($"{nameof(dataProvider.ConsignorAddress)} cached", dataProvider.ConsignorAddress, dataProvider.ConsignorAddress);
		});
	}

	public virtual void TestAddressesMissing()
	{
		AssertNull(nameof(dataProvider.ConsignorAddress), dataProvider.ConsignorAddress);
	}

	public void TestConsigneeMissing()
	{
		CombineAssertions(() =>
		{
			var consigneeDataProvider = dataProvider.ConsigneeAddress;
			AssertEquals(nameof(consigneeDataProvider.Name), "-", consigneeDataProvider.Name);
			AssertEquals(nameof(consigneeDataProvider.PostalCode), ".", consigneeDataProvider.PostalCode);
			AssertEquals(nameof(consigneeDataProvider.City), "-", consigneeDataProvider.City);
			AssertEquals(nameof(consigneeDataProvider.Country), "xx", consigneeDataProvider.Country);
		});
	}

	public void TestTraderReference() => CombineAssertions(() =>
	{
		declaration.JE_DeclarationReference = "B00183715";
		declaration.JE_OwnerRef = "CH000001";

		AssertEquals("TraderReference = JE_OwnerRef not empty", declaration.JE_OwnerRef, dataProvider.TraderReference);

		declaration.JE_OwnerRef = ZString.Empty;
		AssertEquals("TraderReference = JE_DeclarationReference if JE_OwnerRef empty", declaration.JE_DeclarationReference, dataProvider.TraderReference);
	});

	protected override void SetUp()
	{
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageType;

		(invoice, invoiceLine, entryInstruction, entryHeader, entryLine) = CreateDeclarationInvoice(declaration);

		messageSendingObject = GetMessageSendingObject(entryHeader);
		CreateNewDataProvider();
	}

	protected internal JobDeclaration declaration;
	protected internal JobComInvoiceHeader invoice;
	protected internal JobComInvoiceLine invoiceLine;
	protected internal CusEntryHeader entryHeader;
	protected internal CusEntryLine entryLine;
	protected internal CusEntryInstruction entryInstruction;
	protected internal DeclarationMessageSendingObject messageSendingObject;
	protected internal EdecDeclarationDataProvider<T> dataProvider;

	protected void CreateNewDataProvider()
	{
		dataProvider = GetEdecDeclarationDataProvider(messageSendingObject);
	}

	protected internal (JobComInvoiceHeader, JobComInvoiceLine, CusEntryInstruction, CusEntryHeader, CusEntryLine) CreateDeclarationInvoice(JobDeclaration declaration)
	{
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var header = declaration.CustomsEntryHeaders.AddNew();
		header.CH_CEI_Instruction = instruction.PK;
		var line = header.AllEntryLines.AddNew();
		invoiceLine.JI_CL = line.PK;

		return (invoice, invoiceLine, instruction, header, line);
	}

	protected internal OrgHeader newOrgHeader(ZString name)
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = name;
		return orgHeader;
	}

	protected internal JobDocAddress newDocAddress(ZString addressType, ZString name)
	{
		var docAddress = Factory.New<JobDocAddress>();
		docAddress.E2_AddressType = addressType;
		docAddress.OrganisationPK = newOrgHeader(name).PK;
		return docAddress;
	}
}
