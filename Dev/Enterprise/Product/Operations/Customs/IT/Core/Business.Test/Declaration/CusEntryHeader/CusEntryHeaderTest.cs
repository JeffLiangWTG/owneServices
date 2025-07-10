using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Documents.DocDataObjects;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(CusEntryHeader))]
sealed class CusEntryHeaderTest : EU.Business.Declaration.Testing.CusEntryHeaderTest<CusEntryHeader>
{
	public void TestTopLevelBusinessObject()
	{
		ITopLevelBusinessObjectProvider entryHeader = Factory.New<CusEntryHeader>();
		AssertNull("When entryHeader.Declaration is null", entryHeader.TopLevelBusinessObject);

		var declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		AssertSame("When entryHeader.Declaration has value but declaration.Shipment is null", declaration, entryHeader.TopLevelBusinessObject);

		var shipment = Factory.New<ForwardingShipment>();
		declaration.JE_JS = shipment.PK;
		AssertSame("When entryHeader.Declaration.Shipment has value", shipment, entryHeader.TopLevelBusinessObject);
	}

	public void TestPackagesCountThrowsNoException()
	{
		var declaration = Factory.New<JobDeclarationForNoExceptionWhenIsMergingTest>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		declaration.IsMergeInProgress_Exposed = true;
		AssertNoExceptionThrown("When declaration is merging, PackageCount can be called with no exception", () => _ = entryHeader.PackagesCount);
		AssertEquals("When declaration is merging, packages count is zero", ZInt.Zero, entryHeader.PackagesCount);
	}

	public void TestTotalCustomsQuantityThrowsNoException()
	{
		var declaration = Factory.New<JobDeclarationForNoExceptionWhenIsMergingTest>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		declaration.IsMergeInProgress_Exposed = true;
		AssertNoExceptionThrown("When declaration is merging, TotalCustomsQuantity can be called with no exception", () => _ = entryHeader.TotalCustomsQuantity);
		AssertEquals("When declaration is merging, total Customs quantity is zero", ZDecimal.Zero, entryHeader.TotalCustomsQuantity);
	}

	public void TestISingleWindowRequestDataProviderMembers()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		var accountCollection = new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "845A")
			.AppendAccountDetail("845A-DEC1", "DEC1")
			.Build();

		CombineAssertions("Both Declaration and CusEntryNum filled", () =>
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = (ISingleWindowRequestDataProvider)declaration.CustomsEntryHeaders.AddNew();
			var ediMessage = Factory.New<ITEDIMessage>();

			declaration.JE_CustomsProfile = "845A-DEC1";
			declaration.JE_GS_NKCusAgent = "CRR";
			declaration.JE_CustomsOffice = "IT279100";
			Factory.NewCusEntryNumber((CusEntryHeader)entryHeader, entryType: "REG", entryNum: "4 T-123456G", issueDate: ZDate.Today);
			AssertEquals("Register including series", "4 T", entryHeader.RegisterIncludingSeries);
			AssertEquals("Application reference", "845A:CRR:IT279100", entryHeader.ApplicationReference);
			AssertEquals("Issue date", ZDate.Today, entryHeader.IssueDate);
			AssertEquals("Register including series", "123456", entryHeader.RegistrationNumberWithoutCin);
			AssertEquals("Customs office", "IT279100", entryHeader.CustomsOffice);
			AssertEquals("TableName", CusEntryHeader.Schema.TableName, entryHeader.TableName);
		});

		CombineAssertions("Null Declaration", () =>
		{
			var entryHeader = (ISingleWindowRequestDataProvider)Factory.New<CusEntryHeader>();
			AssertEquals("Application reference", "", entryHeader.ApplicationReference);
			AssertEquals("Customs office", "", entryHeader.CustomsOffice);
		});

		CombineAssertions("Null CusEntryNum", () =>
		{
			var entryHeader = (ISingleWindowRequestDataProvider)Factory.New<CusEntryHeader>();
			AssertEquals("Register including series", "", entryHeader.RegisterIncludingSeries);
			AssertEquals("Issue date", ZDate.Empty, entryHeader.IssueDate);
			AssertEquals("Register including series", "", entryHeader.RegistrationNumberWithoutCin);
		});
	}

	public void TestEntryLines()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		AssertType<CusEntryLineCollection<CusEntryLine>>(entryHeader.MergedLines);
		AssertType<AllCusEntryLineCollection<CusEntryLine>>(entryHeader.AllEntryLines);
	}

	public void TestMessages()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var message = entryHeader.Messages.AddNew();
		AssertType<ITEDIMessageCollection>(entryHeader.Messages);
		AssertType<ITEDIMessage>(message);
	}

	public void TestEntryInstructionType()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		AssertType<CusEntryInstruction>(entryHeader.EntryInstruction);
	}

	public void TestCH_IncoTerm()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
		declaration.JE_MergeBy = "TRF";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Procedure = "40";
		var invoice1 = declaration.Invoices.AddNew();
		invoice1.JZ_IncoTerm = "FOB";
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;

		declaration.DoMerge();

		AssertEquals("Declaration should have one EntryHeader", 1, declaration.CustomsEntryHeaders.Count);
		var entryHeader = declaration.CustomsEntryHeaders[0];

		AssertEquals("EntryHeader.CH_Incoterm should be", "FOB", entryHeader.CH_IncoTerm);
	}

	public void TestInvoiceAmountCurrency()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
		declaration.JE_MergeBy = "TRF";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Procedure = "40";
		var invoice1 = declaration.Invoices.AddNew();
		invoice1.JZ_RX_NKInvoice_Currency = "EUR";
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		invoiceLine1.JI_CEI = entryInstruction.PK;

		declaration.DoMerge();

		AssertEquals("Declaration should have one EntryHeader", 1, declaration.CustomsEntryHeaders.Count);
		var entryHeader = declaration.CustomsEntryHeaders[0];

		AssertEquals("EntryHeader.InvoiceAmountCurrency should be", "EUR", entryHeader.InvoiceAmountCurrency);
	}

	public void TestInvoiceAmountCurrencyMaxLength()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertEquals("InvoiceAmountCurrency MaxLength", 3, entryHeader.InvoiceAmountCurrencyInfo.MaxLength);
	}

	public void TestInvoiceAmountCurrencyPersistance()
	{
		var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "IT_InvoiceAmountCurrency");

		var entryHeader = Factory.New<CusEntryHeader>();
		entryHeader.InvoiceAmountCurrency = "EUR";
		AssertEquals("InvoiceAmountCurrency", "EUR", entryHeader.InvoiceAmountCurrency);
		AssertNotNull("InvoiceAmountCurrency is persisted in GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));

		entryHeader.InvoiceAmountCurrency = "";
		AssertEquals("InvoiceAmountCurrency", "", entryHeader.CustomsChannel);
		AssertNull("InvoiceAmountCurrency is deleted from dbo.GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));
	}

	public void TestAllGroupedPreviousDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_Tariff = "1111111111";
		invoiceLine1.JI_CL = entryLine.PK;
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;
		invoiceLine2.JI_Tariff = "2222222222";
		AssertNotNull("All Grouped Previous Documents", entryHeader.AllGroupedPreviousDocuments);

		var previousDocument1 = invoiceLine1.PreviousDocuments.AddNew();
		previousDocument1.CSI_Procedure = "A3";
		declaration.DoMerge();
		AssertEquals("All Grouped Previous Documents count", 0, entryHeader.AllGroupedPreviousDocuments.Count);

		var previousDocument2 = invoiceLine1.PreviousDocuments.AddNew();
		previousDocument2.CSI_Procedure = "MRN";
		declaration.DoMerge();
		AssertEquals("All Grouped Previous Documents count", 2, entryHeader.AllGroupedPreviousDocuments.Count);

		var previousDocument3 = invoiceLine2.PreviousDocuments.AddNew();
		previousDocument3.CSI_Procedure = "2";
		var previousDocument4 = invoiceLine2.PreviousDocuments.AddNew();
		previousDocument4.CSI_Procedure = "7";
		declaration.DoMerge();
		AssertEquals("All Grouped Previous Documents count", 4, entryHeader.AllGroupedPreviousDocuments.Count);
	}

	public void TestResetAllGroupedPreviousDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine.PK;
		AssertEquals("All Grouped Previous Documents count", 0, entryHeader.AllGroupedPreviousDocuments.Count);

		var previousDocument1 = invoiceLine1.PreviousDocuments.AddNew();
		previousDocument1.CSI_Procedure = "A3";
		var previousDocument2 = invoiceLine1.PreviousDocuments.AddNew();
		previousDocument2.CSI_Procedure = "MRN";
		AssertEquals("All Grouped Previous Documents count", 0, entryHeader.AllGroupedPreviousDocuments.Count);

		declaration.ResetApportionedPreviousDocuments();
		AssertEquals("All Grouped Previous Documents count", 2, entryHeader.AllGroupedPreviousDocuments.Count);
	}

	public void TestITEntryNumber()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		CombineAssertions("EntryNumber", () =>
		{
			AssertEquals("Declaration is IMP, EntryType is null", "", entryHeader.EntryNumber);

			var cusEntryNumber = Factory.NewCusEntryNumber(entryHeader, entryType: "REG", entryNum: "4 T-2343G", issueDate: null);
			AssertEquals("Declaration is IMP, EntryType is REG", "", entryHeader.EntryNumber);

			cusEntryNumber.CE_EntryType = "CLR";
			AssertEquals("Declaration is IMP, EntryType is CLR", "", entryHeader.EntryNumber);

			declaration.JE_MessageType = "EXP";
			cusEntryNumber.CE_EntryType = "REG";
			AssertEquals("Declaration is EXP, EntryType is REG", "", entryHeader.EntryNumber);

			Factory.NewCusEntryNumber(entryHeader, entryType: "MRN", entryNum: "20ITQXT080007705T2", issueDate: null);
			AssertEquals("Declaration is EXP, EntryType is MRN", "20ITQXT080007705T2", entryHeader.EntryNumber);
		});
	}

	public void TestEntryNumberType()
	{
		var entryHeader = Factory.New<CusEntryHeaderForEntryNumberTypeTest>();
		AssertEquals("When the entry is not linked to any declaration", "", entryHeader.EntryNumberTypeExposed);

		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryHeaders.Add(entryHeader);

		CombineAssertions("EntryNumberTypeExposed", () =>
		{
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "ITF";
			AssertEquals("When the declaration is Import and Interfaced", "IMP", entryHeader.EntryNumberTypeExposed);

			declaration.JE_ApplicationCode = "";
			AssertEquals("When the declaration is Import but not Interfaced", "MRN", entryHeader.EntryNumberTypeExposed);

			declaration.JE_MessageType = "EXP";
			AssertEquals("When the declaration is Export", "MRN", entryHeader.EntryNumberTypeExposed);
		});
	}

	public void TestIsEntryStatusRegisteredOrNbRejected()
	{
		var entry = Factory.New<CusEntryHeader>();

		entry.CH_EntryStatus = "";
		AssertEquals("IsEntryStatusRegisteredOrNbRejected", ZBool.False, entry.IsEntryStatusRegisteredOrNbRejected);

		entry.CH_EntryStatus = "REG";
		AssertEquals("IsEntryStatusRegisteredOrNbRejected", ZBool.True, entry.IsEntryStatusRegisteredOrNbRejected);

		entry.CH_EntryStatus = "ICC";
		AssertEquals("IsEntryStatusRegisteredOrNbRejected", ZBool.False, entry.IsEntryStatusRegisteredOrNbRejected);

		entry.CH_EntryStatus = "NBR";
		AssertEquals("IsEntryStatusRegisteredOrNbRejected", ZBool.True, entry.IsEntryStatusRegisteredOrNbRejected);
	}

	public void TestMergedLinesWithSendableGroupedPreviousDocuments()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;

		AssertEquals("MergedLinesWithSendableGroupedPreviousDocuments count", 0, entryHeader.MergedLinesWithSendableGroupedPreviousDocuments.Count());

		var previousDocument1 = invoiceLine1.PreviousDocuments.AddNew();
		previousDocument1.CSI_Procedure = "A3";
		var previousDocument2 = invoiceLine1.PreviousDocuments.AddNew();
		previousDocument2.CSI_Procedure = "MRN";
		declaration.ResetApportionedPreviousDocuments();

		AssertEquals("MergedLinesWithSendableGroupedPreviousDocuments count", 1, entryHeader.MergedLinesWithSendableGroupedPreviousDocuments.Count());

		entryLine1.ZG_NBStatus = "NBR";
		AssertEquals("MergedLinesWithSendableGroupedPreviousDocuments count", 1, entryHeader.MergedLinesWithSendableGroupedPreviousDocuments.Count());

		entryLine1.ZG_NBStatus = "NBA";
		AssertEquals("MergedLinesWithSendableGroupedPreviousDocuments count", 0, entryHeader.MergedLinesWithSendableGroupedPreviousDocuments.Count());

		entryLine1.ZG_NBStatus = "NBS";
		AssertEquals("MergedLinesWithSendableGroupedPreviousDocuments count", 1, entryHeader.MergedLinesWithSendableGroupedPreviousDocuments.Count());

		invoiceLine1.PreviousDocuments.RemoveAndDeleteAll();
		declaration.ResetApportionedPreviousDocuments();
		AssertEquals("MergedLinesWithSendableGroupedPreviousDocuments count", 0, entryHeader.MergedLinesWithSendableGroupedPreviousDocuments.Count());
	}

	public void TestDefaultStatusDescription()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertEquals(ZString.Empty, entryHeader.DefaultStatusDescription);
	}

	public void TestGetNewValidation()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertEquals("Validation", "Enterprise.Customs.IT.Business.Declaration.CusEntryHeaderValidation", entryHeader.Validation.GetType().FullName);
	}

	public void TestIsEntryLockedForEditing()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		var entry = jobDeclaration.CustomsEntryHeaders.AddNew();

		CombineAssertions(() =>
		{
			entry.CH_Status = "";
			AssertEquals(GetAssertionMessage(), ZBool.False, entry.IsEntryLockedForEditing);

			entry.CH_Status = ITMessageStatusList.Codes.AwaitingOriginal;
			AssertEquals(GetAssertionMessage(), ZBool.True, entry.IsEntryLockedForEditing);

			entry.CH_Status = ITMessageStatusList.Codes.AcknowledgedOriginal;
			AssertEquals(GetAssertionMessage(), ZBool.True, entry.IsEntryLockedForEditing);

			entry.CH_Status = "";
			entry.CH_EntryStatus = "";
			AssertEquals(GetAssertionMessage(), ZBool.False, entry.IsEntryLockedForEditing);

			entry.CH_EntryStatus = ITEntryStatusList.Codes.Registered;
			AssertEquals(GetAssertionMessage(), ZBool.True, entry.IsEntryLockedForEditing);

			entry.CH_EntryStatus = ITEntryStatusList.Codes.ImportCleared;
			AssertEquals(GetAssertionMessage(), ZBool.True, entry.IsEntryLockedForEditing);

			entry.CH_EntryStatus = ITEntryStatusList.Codes.NbRejected;
			AssertEquals(GetAssertionMessage(), ZBool.True, entry.IsEntryLockedForEditing);

			entry.CH_EntryStatus = ITEntryStatusList.Codes.Arrival;
			AssertEquals(GetAssertionMessage(), ZBool.True, entry.IsEntryLockedForEditing);

			entry.CH_EntryStatus = ITEntryStatusList.Codes.Exit;
			AssertEquals(GetAssertionMessage(), ZBool.True, entry.IsEntryLockedForEditing);

			entry.CH_EntryStatus = ITEntryStatusList.Codes.Canceled;
			AssertEquals(GetAssertionMessage(), ZBool.True, entry.IsEntryLockedForEditing);

			entry.CH_EntryStatus = ITEntryStatusList.Codes.Amended;
			AssertEquals(GetAssertionMessage(), ZBool.True, entry.IsEntryLockedForEditing);

			entry.CH_EntryStatus = ITEntryStatusList.Codes.Amending;
			entry.CH_Status = ITMessageStatusList.Codes.AwaitingOriginal;
			AssertEquals(GetAssertionMessage(), ZBool.True, entry.IsEntryLockedForEditing);

			entry.CH_Status = ITMessageStatusList.Codes.AcknowledgedOriginal;
			AssertEquals(GetAssertionMessage(), ZBool.True, entry.IsEntryLockedForEditing);

			entry.CH_Status = ITMessageStatusList.Codes.AcceptedBySystem;
			AssertEquals(GetAssertionMessage(), ZBool.True, entry.IsEntryLockedForEditing);

			entry.CH_Status = ITMessageStatusList.Codes.ClearOriginal;
			AssertEquals(GetAssertionMessage(), ZBool.False, entry.IsEntryLockedForEditing);

			entry.CH_EntryStatus = ITEntryStatusList.Codes.Deposited;
			entry.CH_Status = "";
			AssertEquals(GetAssertionMessage(), ZBool.False, entry.IsEntryLockedForEditing);

			entry.CH_EntryStatus = ITEntryStatusList.Codes.UnderControl;
			AssertEquals(GetAssertionMessage(), ZBool.True, entry.IsEntryLockedForEditing);

			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertEntryLockedForCancelledAndCancellingEntryStatus("With IMP Declaration");

			jobDeclaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(jobDeclaration, true))
			{
				AssertEntryLockedForCancelledAndCancellingEntryStatus("With UCC6 EXP Declaration");
			}
		});

		void AssertEntryLockedForCancelledAndCancellingEntryStatus(string assertionMessagePrefix)
		{
			entry.CH_EntryStatus = ITEntryStatusList.Codes.Canceling;
			AssertEquals($"{assertionMessagePrefix}, {GetAssertionMessage()}", ZBool.True, entry.IsEntryLockedForEditing);

			entry.CH_EntryStatus = ITEntryStatusList.Codes.Canceled;
			AssertEquals($"{assertionMessagePrefix}, {GetAssertionMessage()}", ZBool.True, entry.IsEntryLockedForEditing);
		}

		string GetAssertionMessage() => $"When {nameof(entry.CH_Status)} = '{entry.CH_Status}' and {nameof(entry.CH_EntryStatus)} = '{entry.CH_EntryStatus}', {nameof(entry.IsEntryLockedForEditing)}";
	}

	public void TestIsWaitingForResponse()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		entryHeader.CH_Status = "";
		AssertEquals("When CH_Status is empty", false, entryHeader.IsWaitingForResponse);

		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AcknowledgedOriginal;
		AssertEquals("When CH_Status is ACO", false, entryHeader.IsWaitingForResponse);

		entryHeader.CH_Status = Common.Shared.MessageStatusList.Codes.AwaitingOriginal;
		AssertEquals("When CH_Status is AWO", true, entryHeader.IsWaitingForResponse);
	}

	public void TestSetAsFailedFromTransmissionDoesNotOverrideEntryStatus()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		entryHeader.CH_Status = "AWO";
		entryHeader.CH_EntryStatus = "NBR";
		declaration.JE_EntryStatus = "NBR";

		entryHeader.SetAsFailedFromTransmission();

		CombineAssertions("SetAsFailedFromTransmission(), Check only CH_Status has been updated", () =>
		{
			AssertEquals("CH_Status", "FFT", entryHeader.CH_Status);
			AssertEquals("CH_EntryStatus", "NBR", entryHeader.CH_EntryStatus);
			AssertEquals("JE_EntryStatus", "NBR", declaration.JE_EntryStatus);
		});
	}

	public void TestIsFailedFromTrasmission()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		entryHeader.CH_EntryStatus = "FFT";
		entryHeader.CH_Status = "";
		AssertEquals("When CH_Status is Empty and CH_EntryStatus is FFT, IsFailedFromTrasmission", false, entryHeader.IsFailedFromTransmission);

		entryHeader.CH_EntryStatus = "";
		entryHeader.CH_Status = "FFT";
		AssertEquals("When CH_Status is FFT and CH_EntryStatus is Empty, IsFailedFromTrasmission", true, entryHeader.IsFailedFromTransmission);
	}

	public void TestStatusAllowsSending()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		entryHeader.CH_EntryStatus = "";
		entryHeader.CH_Status = "";
		AssertEquals("[CHStatus: Empty, CH_EntryStatus: Empty] StatusAllowsSending", true, entryHeader.StatusAllowsSending);

		entryHeader.CH_Status = "AWO";
		AssertEquals("[CHStatus: AWO, CH_EntryStatus: Empty] StatusAllowsSending", false, entryHeader.StatusAllowsSending);

		entryHeader.CH_Status = "ERO";
		AssertEquals("[CHStatus: ERO, CH_EntryStatus: Empty] StatusAllowsSending", true, entryHeader.StatusAllowsSending);

		entryHeader.CH_Status = "ACO";
		AssertEquals("[CHStatus: ACO, CH_EntryStatus: Empty] StatusAllowsSending", false, entryHeader.StatusAllowsSending);

		entryHeader.CH_EntryStatus = "NBR";
		entryHeader.CH_Status = "ACO";
		AssertEquals("[CHStatus: ACO, CH_EntryStatus: NBR] StatusAllowsSending", true, entryHeader.StatusAllowsSending);

		entryHeader.CH_EntryStatus = "NBR";
		entryHeader.CH_Status = "AWO";
		AssertEquals("[CHStatus: ACO, CH_EntryStatus: NBR] StatusAllowsSending", false, entryHeader.StatusAllowsSending);

		entryHeader.CH_Status = "FFT";
		AssertEquals("[CHStatus: FFT, CH_EntryStatus: NBR] StatusAllowsSending", true, entryHeader.StatusAllowsSending);

		entryHeader.CH_Status = "ACO";
		entryHeader.CH_EntryStatus = "DEP";
		AssertEquals("[CHStatus: Empty, CH_EntryStatus: DEP] StatusAllowsSending", true, entryHeader.StatusAllowsSending);

		entryHeader.CH_Status = "AWO";
		entryHeader.CH_EntryStatus = "AMG";
		AssertEquals("[CHStatus: AWO, CH_EntryStatus: AMG] StatusAllowsSending", false, entryHeader.StatusAllowsSending);

		entryHeader.CH_Status = "CLO";
		AssertEquals("[CHStatus: CLO, CH_EntryStatus: AMG] StatusAllowsSending", true, entryHeader.StatusAllowsSending);
	}

	public void TestCustomsChannel()
	{
		var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "IT_CustomsChannel");

		var entryHeader = Factory.New<CusEntryHeader>();
		entryHeader.CustomsChannel = "CA";
		AssertEquals("CustomsChannel", "CA", entryHeader.CustomsChannel);
		AssertNotNull("CustomsChannel is persisted in GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));

		entryHeader.CustomsChannel = "";
		AssertEquals("CustomsChannel", "", entryHeader.CustomsChannel);
		AssertNull("CustomsChannel is deleted from dbo.GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));
	}

	public void TestCustomsChannelMaxLength()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertEquals("CustomsChannel MaxLength", 2, entryHeader.CustomsChannelInfo.MaxLength);
	}

	public void TestCustomsChannelReadOnly()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertEquals("CustomsChannel ReadOnly", true, entryHeader.CustomsChannelInfo.ReadOnly);
	}

	public void TestCH_MessageTypeReadOnly()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertEquals("CH_MessageType ReadOnly", true, entryHeader.CH_MessageTypeInfo.ReadOnly);
	}

	public void TestCustomsChannelDescription()
	{
		var entry = Factory.New<CusEntryHeader>();

		entry.CustomsChannel = "CD";
		AssertEquals("Document Control", entry.CustomsChannelDescription);
		entry.CustomsChannel = "XX";
		AssertEquals("", entry.CustomsChannelDescription);
	}

	public void TestInvoiceAmount()
	{
		var filterQuery = new ZQuery(GenAddOnColumnSchema.XA_Name, "IT_InvoiceAmount");

		var entryHeader = Factory.New<CusEntryHeader>();
		entryHeader.InvoiceAmount = 123.456m;
		AssertEquals("InvoiceAmount", 123.456m, entryHeader.InvoiceAmount);
		AssertNotNull("InvoiceAmount is persisted in GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));

		entryHeader.InvoiceAmount = 0m;
		AssertEquals("InvoiceAmount", "", entryHeader.CustomsChannel);
		AssertNull("InvoiceAmount is deleted from dbo.GenAddOnColumn", Factory.LoadTop1<GenAddOnColumn>(filterQuery));
	}

	public void TestEFMessages()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		var ediMessage = Factory.New<ITEDIMessage>();

		ediMessage.EM_LinkTable = "CusEntryHeader";
		ediMessage.EM_LinkedObject = entryHeader;
		ediMessage.EM_MessageType = MessageProcessorConstants.InterchangeTypes.SingleWindowRequest;
		var messages = entryHeader.Messages;
		AssertEquals("Message count 1", 1, messages.Count);
		ediMessage.EM_MessageType = "SWS";
		AssertEquals("Message count 1", 1, messages.Count);
		ediMessage.EM_MessageType = "PDF";
		AssertEquals("Message count 1", 1, messages.Count);
		ediMessage.EM_MessageType = "WSA";
		AssertEquals("Message count 1", 1, messages.Count);
		ediMessage.EM_MessageType = "WSE";
		AssertEquals("Message count 1", 1, messages.Count);
	}

	public void TestSupplierEoriOfMainOfficeCore()
	{
		var entryHeaderWithoutDeclaration = Factory.New<CusEntryHeader>();
		AssertEquals("Null parent declaration", ZString.Empty, entryHeaderWithoutDeclaration.SupplierEoriOfMainOffice);

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		AssertEquals("Empty supplier", ZString.Empty, entryHeader.SupplierEoriOfMainOffice);

		var organization = Factory.New<OrgHeader>();
		organization.OH_Category = "NAT";
		declaration.JE_OH_Supplier = organization.PK;
		var customsCode = organization.CustomsCodes.AddNew();
		customsCode.OK_CodeType = "EOR";
		customsCode.OK_RN_NKCodeCountry = "DE";
		customsCode.OK_CustomsRegNo = "385040449";
		AssertEquals("DE385040449", entryHeader.SupplierEoriOfMainOffice);
	}

	public void TestImporterEoriOfMainOfficeCore()
	{
		var entryHeaderWithoutDeclaration = Factory.New<CusEntryHeader>();
		AssertEquals("Null parent declaration", ZString.Empty, entryHeaderWithoutDeclaration.ImporterEoriOfMainOffice);

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		AssertEquals("Empty importer", ZString.Empty, entryHeader.ImporterEoriOfMainOffice);

		var organization = Factory.New<OrgHeader>();
		organization.OH_Category = "NAT";
		declaration.JE_OH_Importer = organization.PK;
		var customsCode = organization.CustomsCodes.AddNew();
		customsCode.OK_CodeType = "EOR";
		customsCode.OK_RN_NKCodeCountry = "DE";
		customsCode.OK_CustomsRegNo = "385040449";
		AssertEquals("DE385040449", entryHeader.ImporterEoriOfMainOffice);
	}

	public void TestLocationOfGoods()
	{
		var entryHeaderWithoutDeclaration = Factory.New<CusEntryHeader>();
		AssertEquals("When no linked Declaration, LocationOfGoods", ZString.Empty, entryHeaderWithoutDeclaration.LocationOfGoods);

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		AssertEquals("When no linked EntryInstruction, LocationOfGoods", ZString.Empty, entryHeader.LocationOfGoods);

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		declaration.JE_LocationQualifier = "X";
		declaration.JE_LocationOfGoods = "000123A";
		entryInstruction.ElectronicDocuments = true;
		AssertEquals("When both parent Declaration and EntryInstruction are linked, LocationOfGoods", "X-FE-000123A", entryHeader.LocationOfGoods);
	}

	public void TestRegistrationNumber()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		AssertEquals("Expected empty", ZString.Empty, entryHeader.RegistrationNumber);

		Factory.NewCusEntryNumber(entryHeader, entryType: "REG", entryNum: "4 T-2343G", issueDate: null);
		AssertEquals("Expected filled", "4 T-2343G", entryHeader.RegistrationNumber);
	}

	public void TestShouldLogEntryStatus()
	{
		var entryHeader = Factory.New<CusEntryHeader>();

		AssertEquals("ShouldLogEntryStatus", true, entryHeader.ShouldLogEntryStatus);
	}

	public void TestEntryPayInfos()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		AssertType<CusEntryPayInfoCollection<CusEntryPayInfo>>(entryHeader.EntryPayInfos);
	}

	public void TestSetAsFailedFromTransmissionCode()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_BGMReference = "12345";
		entryHeader.SetAsFailedFromTransmission();
		AssertEquals("Entry Header Status should be FFT", ITMessageStatusList.Codes.FailedFromTransmission, entryHeader.CH_Status);
		Assert("JobDeclaration should have Set to FFT log", entryHeader.Logs.HasLogWith(x => x.SL_SE_NKEvent == "CSO" && x.SL_Reference.Contains("Entry set to FFT, original status:")));
	}

	public void TestSadBoxAText()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eun);

		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT014101", "MANFREDONIA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice);
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction1.CEI_DateForDuty = ZDate.Today;

		invoiceLine1.JI_CEI = entryInstruction1.PK;
		invoiceLine1.JI_CL = entryLine.PK;

		declaration.JE_MessageType = "IMP";

		AssertEquals($"When entryHeader has no EntryNumbers, {nameof(CusEntryHeader.SadBoxAText)}", "", entryHeader.SadBoxAText);

		var cusEntryNumber = Factory.NewCusEntryNumber(entryHeader, entryType: "REG", entryNum: "4 T-2343G", issueDate: null);
		AssertEquals(nameof(CusEntryHeader.SadBoxAText), "", entryHeader.SadBoxAText);

		cusEntryNumber.CE_EntryType = "CLR";
		AssertEquals(nameof(CusEntryHeader.SadBoxAText), "", entryHeader.SadBoxAText);

		var mrn = "20ITQXT080007705T2";
		var expectedFormattedMRN = "MRN: 20ITQXT080007705T2";

		var mrnEntryNum = Factory.NewCusEntryNumber(entryHeader, entryType: "MRN", entryNum: mrn, issueDate: null);
		AssertEquals($"When MRN is available, {nameof(CusEntryHeader.SadBoxAText)}", expectedFormattedMRN, entryHeader.SadBoxAText);

		declaration.JE_CustomsOffice = "IT014101";
		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfPresentation, "IT014101");

		var expectedBoxAText = "IT014101 - MANFREDONIA" + System.Environment.NewLine + expectedFormattedMRN;
		AssertEquals($"When Customs Office is available, {nameof(CusEntryHeader.SadBoxAText)}", expectedBoxAText, entryHeader.SadBoxAText);
	}

	public void TestEntryNumbersProvider()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		var entryNumbersProvider = entryHeader.EntryNumbersProvider;
		AssertNotNull(nameof(entryHeader.EntryNumbersProvider), entryNumbersProvider);
		AssertSame("Cached", entryNumbersProvider, entryHeader.EntryNumbersProvider);
	}

	public void TestICustomsEntryApplicationReferenceMembersWithDeclaration()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		var accountCollection = new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();

		var entryHeader = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
		entryHeader.Declaration.JE_CustomsOffice = "IT137100";
		entryHeader.Declaration.JE_CustomsProfile = "1234-DEC1";
		entryHeader.Declaration.JE_GS_NKCusAgent = "XXX";

		CombineAssertions(() =>
		{
			var customsEntryApplicationReference = (ICustomsEntryApplicationReference)entryHeader;
			AssertEquals("CustomsOffice", "IT137100", customsEntryApplicationReference.CustomsOffice);
			AssertEquals("Node", "1234", customsEntryApplicationReference.Node);
			AssertEquals("Subscriber", "XXX", customsEntryApplicationReference.Subscriber);
			AssertEquals("CustomsProfile", "1234-DEC1", customsEntryApplicationReference.CustomsProfile);
		});
	}

	public void TestICustomsEntryApplicationReferenceMembersWithoutDeclaration()
	{
		CombineAssertions(() =>
		{
			var customsEntryApplicationReference = (ICustomsEntryApplicationReference)Factory.New<CusEntryHeader>();
			AssertEquals("CustomsOffice", ZString.Empty, customsEntryApplicationReference.CustomsOffice);
			AssertEquals("Node", ZString.Empty, customsEntryApplicationReference.Node);
			AssertEquals("Subscriber", ZString.Empty, customsEntryApplicationReference.Subscriber);
		});
	}

	public override void TestAdditionalInfos()
	{
		// AddInfo was excluded from merge keys in WI00314777 - (IMP/EXP) EntryCreationStrategy GetKeyForHeader/ForLine fix
		AssertEquals(0, new EntryCreationStrategy(Factory.New<JobDeclaration>()).GetAdditionalInfoKeys().Length);
	}

	public void TestICustomsLinkedObjectAdapterProviderMembers()
	{
		var entryHeader = Factory.New<JobDeclaration>()
			.CustomsEntryHeaders
			.AddNew();

		var customsLinkedObjectAdapterProvider = entryHeader as ICustomsLinkedObjectAdapterProvider;

		AssertNotNull("NctsHeader must be implement ICustomsLinkedObjectAdapterProvider", customsLinkedObjectAdapterProvider);
		CombineAssertions("Assert ICustomsLinkedObjectAdapterProviderMembers members", () =>
		{
			AssertType<CusEntryHeaderCustomsLinkedObjectAdapter>("GetSadCustomsLinkedObjectAdapter", customsLinkedObjectAdapterProvider.GetSadCustomsLinkedObjectAdapter());
			AssertType<CusEntryHeaderCustomsLinkedObjectAdapter>("GetNewSingleWindowCustomsLinkedObjectAdapter", customsLinkedObjectAdapterProvider.GetNewSingleWindowCustomsLinkedObjectAdapter());
		});
	}

	public void TestShouldCalculatePackagesCountBasedOnLinesPackagesPivot()
	{
		var entryHeader = Factory.New<CusEntryHeaderForPackagesCountTest>();
		AssertEquals("ShouldCalculatePackagesCountBasedOnLinesPackagesPivot", true, entryHeader.ShouldCalculatePackagesCountBasedOnLinesPackagesPivotExposed);
	}

	public void TestIsNbRejected()
	{
		var entryHeader = Factory.New<CusEntryHeader>();

		entryHeader.CH_EntryStatus = "";
		AssertEquals("When CH_EntryStatus is Empty, IsNbRejected", false, entryHeader.IsNbRejected);

		entryHeader.CH_EntryStatus = ITEntryStatusList.Codes.ImportCleared;
		AssertEquals("When CH_EntryStatus is ICC, IsNbRejected", false, entryHeader.IsNbRejected);

		entryHeader.CH_EntryStatus = ITEntryStatusList.Codes.NbRejected;
		AssertEquals("When CH_EntryStatus is NBR, IsNbRejected", true, entryHeader.IsNbRejected);
	}

	public void TestIsInDepositStatus()
	{
		var entryHeader = Factory.New<CusEntryHeader>();

		CombineAssertions(() =>
		{
			entryHeader.CH_EntryStatus = "";
			AssertEquals("When CH_EntryStatus is not DEP, IsInDepositStatus", false, entryHeader.IsInDepositStatus);

			entryHeader.CH_EntryStatus = "REG";
			AssertEquals("When CH_EntryStatus is not DEP, IsInDepositStatus", false, entryHeader.IsInDepositStatus);

			entryHeader.CH_EntryStatus = "DEP";
			AssertEquals("When CH_EntryStatus is DEP, IsInDepositStatus", true, entryHeader.IsInDepositStatus);
		});
	}

	public void TestHasCertificateOfOriginMessage()
	{
		var entryHeader = Factory.New<CusEntryHeader>();

		AssertEquals(nameof(entryHeader.HasCertificateOfOriginMessage), false, entryHeader.HasCertificateOfOriginMessage);

		AddXcoMessage(entryHeader);
		AssertEquals(nameof(entryHeader.HasCertificateOfOriginMessage), true, entryHeader.HasCertificateOfOriginMessage);
	}

	public void TestGetCertificateOfOriginMessage()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertNull(nameof(entryHeader.GetCertificateOfOriginMessage), entryHeader.GetCertificateOfOriginMessage());

		var xcoMessage = AddXcoMessage(entryHeader);
		AssertSame(nameof(entryHeader.GetCertificateOfOriginMessage), xcoMessage, entryHeader.GetCertificateOfOriginMessage());
	}

	public void TestVisualizableDocumentsSupportableAttribute()
	{
		var visualizableDocumentsSupportableAttributes = typeof(CusEntryHeader).GetCustomAttributes(typeof(VisualizableDocumentsSupportableAttribute), false);
		AssertEquals("VisualizableDocumentsSupportableAttribute", 1, visualizableDocumentsSupportableAttributes.Length);

		var visualizableDocumentsSupportableAttribute = (VisualizableDocumentsSupportableAttribute)visualizableDocumentsSupportableAttributes.Single();
		AssertEquals("SupporterType", typeof(CusEntryHeaderITVisualizableDocumentSupporter), visualizableDocumentsSupportableAttribute.SupporterType);
	}

	public void TestDocumentSupporter()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		AssertType<CusEntryHeaderDocumentSupporter>(entryHeader.DocumentSupporter);
	}

	public void TestReadOnlyFields()
	{
		var entryHeader = Factory.New<CusEntryHeader>();

		AssertForReadOnlyAttribute(entryHeader.CH_StatusInfo);
		AssertForReadOnlyAttribute(entryHeader.CH_EntryStatusInfo);
		AssertForReadOnlyAttribute(entryHeader.CH_EntrySubmittedDateInfo);
		AssertForReadOnlyAttribute(entryHeader.CH_EntryReleaseDateInfo);
		AssertForReadOnlyAttribute(entryHeader.CH_WarehouseTransactionStatusInfo);

		void AssertForReadOnlyAttribute(ZPropertyInfo propertyInfo)
		{
			var isReadOnly = propertyInfo.ReadOnly;
			AssertEquals($"{propertyInfo.Name} ReadOnly", true, isReadOnly);
		}
	}

	public void TestGetSupportingDocumentsToProcess()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		var supportingDocAtDecLevel = declaration.SupportingDocuments.AddNew();
		supportingDocAtDecLevel.CSI_Code = "1001";

		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		Factory.InvalidateCachedProperties();
		AssertEntryHeaderSupportingDocumentsSameAsThatOfDeclarationLevel();

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		Factory.InvalidateCachedProperties();
		AssertEntryHeaderSupportingDocumentsSameAsThatOfDeclarationLevel();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			Factory.InvalidateCachedProperties();
			var supportingDocumentAtEntryInstructionLevel = entryInstruction.SupportingDocuments.AddNew();
			supportingDocumentAtEntryInstructionLevel.CSI_Code = "2001";

			CombineAssertions($"UCC6, Declaration Type: {declaration.JE_MessageType}", () =>
			{
				AssertEquals("Supporting Documents Count", 1, entryHeader.SupportingDocuments.Count());
				AssertEquals("Supporting Document ID", supportingDocumentAtEntryInstructionLevel.PK, entryHeader.SupportingDocuments.FirstOrDefault()?.PK);
			});
		}

		void AssertEntryHeaderSupportingDocumentsSameAsThatOfDeclarationLevel()
		{
			CombineAssertions($"Declaration Type: {declaration.JE_MessageType}", () =>
			{
				AssertEquals("Supporting Documents Count", 1, entryHeader.SupportingDocuments.Count());
				AssertEquals("Supporting Document ID", supportingDocAtDecLevel.PK, entryHeader.SupportingDocuments.FirstOrDefault()?.PK);
			});
		}
	}

	ITEDIMessage AddXcoMessage(CusEntryHeader entryHeader)
	{
		var xcoMessage = entryHeader.Messages.AddNew();
		xcoMessage.EM_MessageType = "XCO";
		xcoMessage.IsTransmitMessage = false;
		xcoMessage.EM_Status = EDIMessageStatusList.Codes.Manual;
		xcoMessage.EM_MessageText = "XCO Message";
		return xcoMessage;
	}

	public void TestNatureOfTransaction()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		AssertEquals("NatureOfTransaction", "", entryHeader.NatureOfTransaction);

		var entryLine = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		invoice.JZ_ValuationCode = "11";
		entryLine.RefreshInvoiceLines();
		AssertEquals("NatureOfTransaction", "11", entryHeader.NatureOfTransaction);
	}

	public void TestTransportChargesMethodOfPayment()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		AssertEquals("TransportChargesMethodOfPayment", "", entryHeader.TransportChargesMethodOfPayment);

		var entryLine = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		invoice.ZG_TransportChargesMethodOfPayment = "A";
		entryLine.RefreshInvoiceLines();
		AssertEquals("TransportChargesMethodOfPayment", "A", entryHeader.TransportChargesMethodOfPayment);
	}

	public void TestCountryOfDestination()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		AssertEquals("When no CountryOfDestination has been set", "", entryHeader.CountryOfDestination);

		entryHeader.Declaration.JE_GoodsDestination = "IS";
		AssertEquals("When CountryOfDestination has been set", "IS", entryHeader.CountryOfDestination);
	}

	public void TestCountryOfExport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		AssertEquals("When no CountryOfExport has been set", "", entryHeader.CountryOfExport);

		entryHeader.Declaration.JE_GoodsOrigin = "IS";
		AssertEquals("When CountryOfExport has been set", "IS", entryHeader.CountryOfExport);
	}

	public void TestHasMrn()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("When no Mrn has been set", false, entryHeader.HasMrn);

			entryHeader.MovementReferenceNumberSetter("123");
			AssertEquals("When Mrn has been set", true, entryHeader.HasMrn);
		});
	}

	public void TestResetCancelledEntry()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		entryHeader.MovementReferenceNumberSetter("123");
		entryHeader.CH_Status = "ACS";
		entryHeader.CH_EntryStatus = "CNC";
		entryHeader.CusEntryNumber.CE_EntryLineReference = "789";
		entryHeader.CH_EntrySubmittedDate = ZDateTime.UtcToday;
		entryHeader.CH_EntryReleaseDate = ZDateTime.BrettsBirthday;

		var payInfo1 = entryHeader.EntryPayInfos.AddNew();
		payInfo1.C9_PaymentAmount = 20.0m;
		payInfo1.C9_PaymentDate = new ZDateTime(2016, 03, 17);
		payInfo1.C9_TransactionType = "XYZ";
		payInfo1.C9_PaymentParty = "D";
		var payInfo2 = entryHeader.EntryPayInfos.AddNew();
		payInfo2.C9_PaymentAmount = 1000m;
		payInfo2.C9_PaymentDate = new ZDateTime(2016, 03, 17);
		payInfo2.C9_TransactionType = "ABC";
		payInfo2.C9_PaymentParty = "F";

		var entryNumber1 = NewCusEntryHeader(entryHeader, CusEntryNumberConstants.EntryTypes.RegistrationNumber, "456", ZDate.Today, ZString.Empty);
		var entryNumber2 = NewCusEntryHeader(entryHeader, CusEntryNumberConstants.EntryTypes.ClereanceCode, "135", ZDate.Today, ZString.Empty);
		var entryNumber3 = NewCusEntryHeader(entryHeader, CusEntryNumberConstants.EntryTypes.Ivisto, "456", ZDate.Today, ZString.Empty);
		entryNumber3.CE_EntryLineReference = "IT275100";
		entryNumber3.CE_EntryStatus = "EXC";

		CombineAssertions(() =>
		{
			entryHeader.ResetCancelledEntry();
			AssertNullOrEmpty("Message Status should be empty", entryHeader.CH_Status);
			AssertNullOrEmpty("Entry Status should be empty", entryHeader.CH_EntryStatus);
			AssertNull("CusEntryNumber should be empty", entryHeader.CusEntryNumber);
			AssertEquals("CH_EntrySubmittedDate should be empty", ZDateTime.Empty, entryHeader.CH_EntrySubmittedDate);
			AssertNullOrEmpty("MRN should be empty", entryHeader.MovementReferenceNumber);
			AssertEquals("ReleaseDate should be empty", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
			AssertEquals("EntryPayInfos collection should be empty", 0, entryHeader.EntryPayInfos.Count);
			AssertNull("RegistrationInfo should be empty", entryHeader.EntryNumbersProvider.RegistrationInfo);
			AssertNull("ReleaseInfo should be empty", entryHeader.EntryNumbersProvider.ReleaseInfo);
			AssertNull("Ivisto should be empty", entryHeader.EntryNumbersProvider.Ivisto);
		});
	}

	CusEntryNumber NewCusEntryHeader(CusEntryHeader entryHeader, ZString entryType, ZString entryNum, ZDateTime? issueDate, string entryLineReference = null)
	{
		var cusEntryNumber = Factory.New<CusEntryNumber>();
		cusEntryNumber.CE_EntryType = entryType;
		cusEntryNumber.CE_ParentID = entryHeader.PK;
		cusEntryNumber.CE_ParentTable = entryHeader.TableName;
		cusEntryNumber.CE_Category = "CUS";
		cusEntryNumber.CE_EntryNum = entryNum;
		cusEntryNumber.CE_EntryLineReference = entryLineReference;
		if (issueDate.HasValue)
		{
			cusEntryNumber.CE_IssueDate = issueDate.Value;
		}
		return cusEntryNumber;
	}

	public void TestIsCancellationAcceptedBySystem()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		entryHeader.CH_Status = "ACS";
		entryHeader.CH_EntryStatus = "CNC";
		AssertEquals("MessageStatus = ACS and EntryStatus = CNC, expected true", true, entryHeader.IsCancellationAcceptedBySystem);

		entryHeader.CH_Status = "AMG";
		AssertEquals("MessageStatus != ACS and EntryStatus != CNC, expected false", false, entryHeader.IsCancellationAcceptedBySystem);
	}

	public void TestBondedWarehouseProcessingRequired()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		(string WarehouseStatus, string EntryStatus, string Status, bool ExpectedProcessingRequired)[] testCases =
		[
			(WarehouseTransactionStatusList.Codes.InwardCreatedPending, ITEntryStatusList.Codes.Arrival, MessageStatusList.Codes.OK, true),
			(WarehouseTransactionStatusList.Codes.InwardUpdatedPending, ITEntryStatusList.Codes.Arrival, MessageStatusList.Codes.OK, true),
			(WarehouseTransactionStatusList.Codes.InwardCanceledPendingWithdrawal, ITEntryStatusList.Codes.Arrival, MessageStatusList.Codes.OK, true),
			(WarehouseTransactionStatusList.Codes.InwardCreationHeld, ITEntryStatusList.Codes.Arrival, MessageStatusList.Codes.OK, true),
			(WarehouseTransactionStatusList.Codes.OutwardCreatedPending, ITEntryStatusList.Codes.Arrival, MessageStatusList.Codes.OK, true),
			(WarehouseTransactionStatusList.Codes.OutwardUpdatedPending, ITEntryStatusList.Codes.Arrival, MessageStatusList.Codes.OK, true),
			(WarehouseTransactionStatusList.Codes.OutwardCanceledPendingWithdrawal, ITEntryStatusList.Codes.Arrival, MessageStatusList.Codes.OK, true),
			(WarehouseTransactionStatusList.Codes.OutwardHolding, ITEntryStatusList.Codes.Arrival, MessageStatusList.Codes.OK, true),
			(string.Empty, ITEntryStatusList.Codes.Arrival, MessageStatusList.Codes.OK, false),
			(WarehouseTransactionStatusList.Codes.InwardCreatedPending, string.Empty, string.Empty, false),
			(WarehouseTransactionStatusList.Codes.InwardCreatedPending, string.Empty, ITMessageStatusList.Codes.AcknowledgedOriginal, false),
			(WarehouseTransactionStatusList.Codes.InwardCreatedPending, ITEntryStatusList.Codes.Registered, ITMessageStatusList.Codes.AcknowledgedOriginal, false),
			(WarehouseTransactionStatusList.Codes.InwardCreatedPending, ITEntryStatusList.Codes.UnderControl, ITMessageStatusList.Codes.Unknown, false),
			(WarehouseTransactionStatusList.Codes.InwardCreatedPending, ITEntryStatusList.Codes.Canceling, ITMessageStatusList.Codes.AcknowledgedOriginal, false),
			(WarehouseTransactionStatusList.Codes.InwardCreatedPending, ITEntryStatusList.Codes.Canceling, ITMessageStatusList.Codes.AcceptedBySystem, false),
			(WarehouseTransactionStatusList.Codes.InwardCreatedPending, ITEntryStatusList.Codes.Amending, ZString.Empty, false),
			(WarehouseTransactionStatusList.Codes.InwardCreatedPending, ITEntryStatusList.Codes.Amending, ITMessageStatusList.Codes.AcknowledgedOriginal, false),
			(WarehouseTransactionStatusList.Codes.InwardCreatedPending, ITEntryStatusList.Codes.Amending, ITMessageStatusList.Codes.AcceptedBySystem, false),
		];

		foreach (var (warehouseStatus, entryStatus, status, expectedProcessingRequired) in testCases)
		{
			entryHeader.CH_WarehouseTransactionStatus = warehouseStatus;
			entryHeader.CH_EntryStatus = entryStatus;
			entryHeader.CH_Status = status;
			AssertEquals($"CH_WarehouseTransactionStatus: {warehouseStatus}, CH_EntryStatus: {entryStatus}, CH_Status: {status}", expectedProcessingRequired, entryHeader.BondedWarehouseProcessingRequired);
		}
	}

	public override void TestRepresentativeOrDeclarantEoriOfMainOffice()
	{
		var factory = Factory;
		var representativeOrgHeader = factory.New<OrgHeader>();
		representativeOrgHeader.OH_FullName = "Representative FullName";
		var representativeAddress = representativeOrgHeader.Addresses.AddNew();
		representativeAddress.OA_Address1 = "Representative Address Line";
		representativeOrgHeader.CustomsCodes.AddNew("EOR", "111111", "IT");

		var declarantOrgHeader = factory.New<OrgHeader>();
		declarantOrgHeader.OH_FullName = "declarant FullName";
		var declarantAddress = declarantOrgHeader.Addresses.AddNew();
		declarantAddress.OA_Address1 = "declarant Address";
		declarantOrgHeader.CustomsCodes.AddNew("EOR", "222222", "IT");

		var declaration = factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
		{
			AssertNullOrEmpty("When not UCC6, Representative and Declarant are empty", entryHeader.RepresentativeOrDeclarantEoriOfMainOffice);
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			declaration.JE_OA_Representative = representativeAddress.PK;

			AssertEquals("When not UCC6, Representative and Declarant are valid", "IT222222", entryHeader.RepresentativeOrDeclarantEoriOfMainOffice);
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;
			declaration.JE_OA_Representative = representativeAddress.PK;
			AssertEquals("When UCC6, Representative and Declarant are valid", "IT111111", entryHeader.RepresentativeOrDeclarantEoriOfMainOffice);

			declaration.JE_OA_Representative = ZGuid.Empty;
			AssertEquals("When UCC6 and Only Declarant is valid", "IT222222", entryHeader.RepresentativeOrDeclarantEoriOfMainOffice);

			representativeOrgHeader.CustomsCodes.RemoveAll();
			AssertEquals("When UCC6 and Only Declarant has EORI", "IT222222", entryHeader.RepresentativeOrDeclarantEoriOfMainOffice);

			declarantOrgHeader.CustomsCodes.RemoveAll();
			AssertNullOrEmpty("When UCC6 and both Declarant and Representative has not EORI", entryHeader.RepresentativeOrDeclarantEoriOfMainOffice);
		}
	}

	class CusEntryHeaderForPackagesCountTest : CusEntryHeader
	{
		public CusEntryHeaderForPackagesCountTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool ShouldCalculatePackagesCountBasedOnLinesPackagesPivotExposed => ShouldCalculatePackagesCountBasedOnLinesPackagesPivot;
	}

	class CusEntryHeaderForEntryNumberTypeTest : CusEntryHeader
	{
		public CusEntryHeaderForEntryNumberTypeTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZString EntryNumberTypeExposed => EntryNumberType;
	}

	protected override void DoMerge(BaseJobDeclaration declaration)
	{
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		base.DoMerge(declaration);
	}

	protected override IChargesCurrencyTestSetup GetChargesCurrencyTestSetup() => new ChargesCurrencyTestSetup();

	sealed class ChargesCurrencyTestSetup : IChargesCurrencyTestSetup
	{
		void IChargesCurrencyTestSetup.SetupJobDecWithOFTAndCIFCharges(BaseJobDeclaration declaration, ZString currencyCode)
		{
			declaration.AutoCreateChargesBasedOnIncoTerm = false;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 10000m;
			invoiceHeader.JZ_RX_NKInvoice_Currency = currencyCode;
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;

			var nonDutiableCharge = invoiceHeader.Charges.AddNew();
			nonDutiableCharge.J7_ChargeType = Enterprise.Customs.Business.CustomsChargeTypeList.Codes.ForeignInlandFreight;
			nonDutiableCharge.J7_Amount = 200m;
			nonDutiableCharge.J7_IsDutiable = false;
			nonDutiableCharge.J7_IsIncludedInITOT = true;

			var oft = invoiceHeader.Charges.AddNew();
			oft.J7_ChargeType = Enterprise.Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight;
			oft.J7_Amount = 500m;
			oft.J7_RX_NKCurrency = invoiceHeader.Invoice_Currency.RX_Code;
		}

		ZDecimal IChargesCurrencyTestSetup.ExpectedFOB => 10500m;
		ZDecimal IChargesCurrencyTestSetup.ExpectedCIF => 11000m;
	}

	class JobDeclarationForNoExceptionWhenIsMergingTest : JobDeclaration
	{
		public JobDeclarationForNoExceptionWhenIsMergingTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool IsMergeInProgress_Exposed
		{
			get => base.IsMergeInProgress;
			set => base.IsMergeInProgress = value;
		}
	}
}
