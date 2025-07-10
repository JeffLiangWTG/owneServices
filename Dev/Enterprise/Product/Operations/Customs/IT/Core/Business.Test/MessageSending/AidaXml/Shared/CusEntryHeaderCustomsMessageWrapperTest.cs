using System;
using System.Linq;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.IT.Business.Declaration.CusEntryHeader;
using CusEquipment = Enterprise.Customs.EU.Business.Declaration.CusEquipment;
using JobDeclaration = Enterprise.Customs.IT.Business.Declaration.JobDeclaration;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Shared.Testing;

sealed class CusEntryHeaderCustomsMessageWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new CusEntryHeaderCustomsMessageWrapper(entryHeader: null));
		AssertNoExceptionThrown(() => new CusEntryHeaderCustomsMessageWrapper(entryHeader));
	}

	public void TestInvoiceCurrencyCode()
	{
		var wrapper = GetNewWrapper();
		AssertNullOrEmpty(nameof(ICusEntryHeaderCustomsMessageWrapper.InvoiceCurrencyCode), wrapper.InvoiceCurrencyCode);

		entryHeader.InvoiceAmountCurrency = "AUD";
		wrapper = GetNewWrapper();
		AssertEquals(nameof(ICusEntryHeaderCustomsMessageWrapper.InvoiceCurrencyCode), "AUD", wrapper.InvoiceCurrencyCode);
	}

	public void TestExchangeRate()
	{
		var wrapper = GetNewWrapper();
		AssertEquals(nameof(ICusEntryHeaderCustomsMessageWrapper.ExchangeRate), 0m, wrapper.ExchangeRate);

		var entryLine = entryHeader.MergedLines.AddNew();

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		invoice.JZ_InvoiceCurrExRate = 1.1239;
		wrapper = GetNewWrapper();
		AssertEquals(nameof(ICusEntryHeaderCustomsMessageWrapper.ExchangeRate), 1.1239m, wrapper.ExchangeRate);
	}

	public void TestInvoiceTotalAmount()
	{
		var wrapper = GetNewWrapper();
		AssertNull(nameof(ICusEntryHeaderCustomsMessageWrapper.InvoiceTotalAmount), wrapper.InvoiceTotalAmount);

		entryHeader.InvoiceAmount = 334m;
		wrapper = GetNewWrapper();
		AssertEquals(nameof(ICusEntryHeaderCustomsMessageWrapper.InvoiceTotalAmount), 334m, wrapper.InvoiceTotalAmount);
	}

	public void TestNatureOfTransaction()
	{
		var wrapper = GetNewWrapper();
		AssertEquals(nameof(ICusEntryHeaderCustomsMessageWrapper.NatureOfTransaction), 0, wrapper.NatureOfTransaction);

		var entryLine = entryHeader.MergedLines.AddNew();

		var invoice = declaration.Invoices.AddNew();
		invoice.InvoiceLines.AddNew().JI_CL = entryLine.PK;
		invoice.JZ_ValuationCode = "11";

		wrapper = GetNewWrapper();
		AssertEquals(nameof(ICusEntryHeaderCustomsMessageWrapper.NatureOfTransaction), 11, wrapper.NatureOfTransaction);
	}

	public void TestExportNatureOfTransaction()
	{
		var wrapper = GetNewWrapper();
		AssertNull("When no invoices are found, ExportNatureOfTransaction", wrapper.ExportNatureOfTransaction);

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();

		var entryLine1 = entryHeader.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;
		entryLine1.InvoiceLines.Add(invoiceLine1);

		var entryLine2 = entryHeader.MergedLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;
		entryLine2.InvoiceLines.Add(invoiceLine2);

		CombineAssertions(() =>
		{
			invoice1.JZ_ValuationCode = "11";
			invoice2.JZ_ValuationCode = "11";
			wrapper = GetNewWrapper();
			AssertEquals("When all invoices have same ValuationCode, ExportNatureOfTransaction", 11, wrapper.ExportNatureOfTransaction);

			invoice2.JZ_ValuationCode = "22";
			wrapper = GetNewWrapper();
			AssertNull("When invoices have different ValuationCode, ExportNatureOfTransaction", wrapper.ExportNatureOfTransaction);

			invoice2.JZ_ValuationCode = "";
			wrapper = GetNewWrapper();
			AssertNull("When invoices have different ValuationCode (one does not have value), ExportNatureOfTransaction", wrapper.ExportNatureOfTransaction);

			invoice1.JZ_ValuationCode = "";
			wrapper = GetNewWrapper();
			AssertNull("When invoices have same ValuationCode (all do not have value), ExportNatureOfTransaction", wrapper.ExportNatureOfTransaction);
		});
	}

	public void TestCountryOfDestinationWithSingleLine()
	{
		AssertExportWrapperFieldWithSingleLine("CountryOfDestination"
			, (line, value) => { line.ZG_CountryOfDestination = value; }
			, (declaration, value) => { declaration.JE_GoodsDestination = value; }
			, (wrapper) => wrapper.CountryOfDestination);
	}

	public void TestCountryOfDestination()
	{
		AssertExportWrapperField("CountryOfDestination"
		, (line, value) => { line.ZG_CountryOfDestination = value; }
		, (declaration, value) => { declaration.JE_GoodsDestination = value; }
		, (wrapper) => wrapper.CountryOfDestination);
	}

	public void TestCountryOfExportWithSingleLine()
	{
		declaration.MessageVersion = MessageVersionList.Codes.XML;
		AssertExportWrapperFieldWithSingleLine("CountryOfExport"
			, (line, value) => { line.JI_RN_NKCountryOfExport = value; }
			, (declaration, value) => { declaration.JE_GoodsOrigin = value; }
			, (wrapper) => wrapper.CountryOfExport);
	}

	public void TestCountryOfExport()
	{
		declaration.MessageVersion = MessageVersionList.Codes.XML;
		AssertExportWrapperField("CountryOfExport"
		, (line, value) => { line.JI_RN_NKCountryOfExport = value; }
		, (declaration, value) => { declaration.JE_GoodsOrigin = value; }
		, (wrapper) => wrapper.CountryOfExport);
	}

	public void TestConsignorWithSingleLine()
	{
		AssertExportWrapperTraderWithSingleLine("Consignor"
			, (line, value) => { line.JI_OA_ExporterAddress = value; }
			, (declaration, value) => { declaration.SupplierDocumentaryAddress.E2_OA_Address = value; }
			, (wrapper) => wrapper.Consignor);
	}

	public void TestConsignor()
	{
		AssertExportWrapperTrader("Consignor"
			, (line, value) => { line.JI_OA_ExporterAddress = value; }
			, (declaration, value) => { declaration.SupplierDocumentaryAddress.E2_OA_Address = value; }
			, (wrapper) => wrapper.Consignor);
	}

	public void TestConsigneeWithSingleLine()
	{
		AssertExportWrapperTraderWithSingleLine("Consignee"
			, (line, value) => { line.JI_OA_ConsigneeAddress = value; }
			, (declaration, value) => { declaration.ImporterDocumentaryAddress.E2_OA_Address = value; }
			, (wrapper) => wrapper.Consignee);
	}

	public void TestConsignmentPartiesWhenOrganisationHasTwoAddressesWithSingleEori()
	{
		var wrapper = GetNewWrapper();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;
		invoiceLine2.JI_CL = entryLine2.PK;

		CombineAssertions("When invoice lines have same Consignee/Consignor with different address selected for each line", () =>
		{
			var trader = Factory.NewWithValidTestData<OrgHeader>();
			var eoriCode = trader.CustomsCodes.AddNew("EOR", "135040449", "IT");

			var orgAddress1 = trader.Addresses.AddNew();
			orgAddress1.OA_Address1 = "ADDRESS1";
			orgAddress1.OA_City = "VENICE";
			orgAddress1.OA_RN_NKCountryCode = "IT";
			orgAddress1.OA_Code = "10000";

			var orgAddress2 = trader.Addresses.AddNew();
			orgAddress2.OA_Address1 = "ADDRESS2";
			orgAddress2.OA_City = "MESTRE";
			orgAddress2.OA_RN_NKCountryCode = "IT";
			orgAddress2.OA_Code = "10001";

			declaration.ImporterDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			declaration.SupplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			invoiceLine1.JI_OA_ConsigneeAddress = orgAddress1.PK;
			invoiceLine2.JI_OA_ConsigneeAddress = orgAddress2.PK;
			invoiceLine1.JI_OA_ExporterAddress = orgAddress1.PK;
			invoiceLine2.JI_OA_ExporterAddress = orgAddress2.PK;
			wrapper = GetNewWrapper();
			AssertEquals("Consignee Eori number", "IT135040449", wrapper.Consignee?.EoriNumber);
			AssertEquals("Consignor Eori number", "IT135040449", wrapper.Consignor?.EoriNumber);

			trader.CustomsCodes.Remove(eoriCode);
			wrapper = GetNewWrapper();
			AssertNull("Eori number is empty and different address, Consignee", wrapper.Consignee);
			AssertNull("Eori number is empty and different address, Consignor", wrapper.Consignor);
		});
	}

	public void TestConsignmentPartiesWhenTwoOrganisationsHaveSameEori()
	{
		var wrapper = GetNewWrapper();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;
		invoiceLine2.JI_CL = entryLine2.PK;

		CombineAssertions("When invoice lines have different Consignee/Consignor with same EORI number", () =>
		{
			var trader1 = Factory.NewWithValidTestData<OrgHeader>();
			var trader2 = Factory.NewWithValidTestData<OrgHeader>();
			trader1.CustomsCodes.AddNew("EOR", "135040449", "IT");
			trader2.CustomsCodes.AddNew("EOR", "135040449", "IT");

			var trader1Address = trader1.Addresses.AddNew();
			trader1Address.OA_Address1 = "ADDRESS1";
			trader1Address.OA_City = "VENICE";
			trader1Address.OA_RN_NKCountryCode = "IT";
			trader1Address.OA_Code = "10000";

			var trader2Address = trader2.Addresses.AddNew();
			trader2Address.OA_Address1 = "ADDRESS2";
			trader2Address.OA_City = "MESTRE";
			trader2Address.OA_RN_NKCountryCode = "IT";
			trader2Address.OA_Code = "10001";

			declaration.ImporterDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			declaration.SupplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			invoiceLine1.JI_OA_ConsigneeAddress = trader1Address.PK;
			invoiceLine2.JI_OA_ConsigneeAddress = trader2Address.PK;
			invoiceLine1.JI_OA_ExporterAddress = trader1Address.PK;
			invoiceLine2.JI_OA_ExporterAddress = trader2Address.PK;
			wrapper = GetNewWrapper();
			AssertEquals("Consignee Eori number", "IT135040449", wrapper.Consignee?.EoriNumber);
			AssertEquals("Consignor Eori number", "IT135040449", wrapper.Consignor?.EoriNumber);
		});
	}

	public void TestConsignmentPartiesWhenTwoOrganisationsHaveSameAddressAndNoEoriOrTcuCode()
	{
		var wrapper = GetNewWrapper();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
		var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		var entryLine2 = entryHeader.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;
		invoiceLine2.JI_CL = entryLine2.PK;

		CombineAssertions("When invoice lines have different Consignee/Consignor with same address and no EORI/Tcu number", () =>
		{
			var trader1 = Factory.NewWithValidTestData<OrgHeader>();
			var trader2 = Factory.NewWithValidTestData<OrgHeader>();

			var trader1Address = trader1.Addresses.AddNew();
			trader1Address.OA_Address1 = "ADDRESS";
			trader1Address.OA_City = "VENICE";
			trader1Address.OA_RN_NKCountryCode = "IT";
			trader1Address.OA_Code = "10000";

			var trader2Address = trader2.Addresses.AddNew();
			trader2Address.OA_Address1 = "ADDRESS";
			trader2Address.OA_City = "VENICE";
			trader2Address.OA_RN_NKCountryCode = "IT";
			trader2Address.OA_Code = "10000";

			declaration.ImporterDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			declaration.SupplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			invoiceLine1.JI_OA_ConsigneeAddress = trader1Address.PK;
			invoiceLine2.JI_OA_ConsigneeAddress = trader2Address.PK;
			invoiceLine1.JI_OA_ExporterAddress = trader1Address.PK;
			invoiceLine2.JI_OA_ExporterAddress = trader2Address.PK;
			wrapper = GetNewWrapper();
			AssertNotNull("Consignee", wrapper.Consignee);
			AssertEquals("Consignee city", "VENICE", wrapper.Consignee?.Address.City);
			AssertNotNull("Consignor", wrapper.Consignor);
			AssertEquals("Consignor city", "VENICE", wrapper.Consignee?.Address.City);
		});
	}

	public void TestConsignee()
	{
		AssertExportWrapperTrader("Consignee"
			, (line, value) => { line.JI_OA_ConsigneeAddress = value; }
			, (declaration, value) => { declaration.ImporterDocumentaryAddress.E2_OA_Address = value; }
			, (wrapper) => wrapper.Consignee);
	}

	public void TestTermsOfDelivery()
	{
		var wrapper = GetNewWrapper();
		var termOfDeliveryWrapper = wrapper.TermOfDelivery;
		AssertSame(nameof(ICusEntryHeaderCustomsMessageWrapper.TermOfDelivery), termOfDeliveryWrapper, wrapper.TermOfDelivery);
	}

	public void TestNumberOfPackages()
	{
		var wrapper = GetNewWrapper();
		AssertEquals(nameof(ICusEntryHeaderCustomsMessageWrapper.NumberOfPackages), 0, wrapper.NumberOfPackages);

		var pack1 = declaration.Packages.AddNew();
		var pack2 = declaration.Packages.AddNew();
		declaration.Packages.AddNew();

		var entryLine = entryHeader.MergedLines.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();

		invoiceLine.JI_CL = entryLine.PK;
		var packPivot1 = invoiceLine.PackagesPivot.AddNew();
		var packPivot2 = invoiceLine.PackagesPivot.AddNew();

		packPivot1.CHC_CW = pack1.PK;
		packPivot1.CHC_NumberOfPacks = 10;
		packPivot2.CHC_CW = pack2.PK;
		packPivot2.CHC_NumberOfPacks = 2;

		wrapper = GetNewWrapper();
		entryLine.InvoiceLines.Reload(reLoadExistingRows: true);
		entryHeader.ReloadSafe();
		AssertEquals(nameof(ICusEntryHeaderCustomsMessageWrapper.NumberOfPackages), 12, wrapper.NumberOfPackages);
	}

	public void TestAdditionalInformation()
	{
		var wrapper = GetNewWrapper();
		AssertNotNull(nameof(ICusEntryHeaderCustomsMessageWrapper.AdditionalInformation), wrapper.AdditionalInformation);
		AssertEquals($"{nameof(ICusEntryHeaderCustomsMessageWrapper.AdditionalInformation)} count", 1, wrapper.AdditionalInformation.Count);

		var additionalInformationCollection = wrapper.AdditionalInformation;
		AssertSame(nameof(ICusEntryHeaderCustomsMessageWrapper.AdditionalInformation), additionalInformationCollection, wrapper.AdditionalInformation);
		AssertType<NoneOfAboveAdditionalInformationWrapper>("Default Additional Information type", additionalInformationCollection.Single());
	}

	public void TestGrossMass()
	{
		var wrapper = GetNewWrapper();
		AssertEquals(nameof(ICusEntryHeaderCustomsMessageWrapper.GrossMass), 0m, wrapper.GrossMass);

		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		var invoiceLine2 = invoice.InvoiceLines.AddNew();

		var entryLine = entryHeader.MergedLines.AddNew();

		invoiceLine1.JI_CL = entryLine.PK;
		invoiceLine2.JI_CL = entryLine.PK;

		invoiceLine1.JI_Weight = 23;
		invoiceLine1.JI_WeightUQ = "KG";
		invoiceLine2.JI_Weight = 56.3;
		invoiceLine2.JI_WeightUQ = "G";

		entryLine.InvoiceLines.Reload(true);
		entryHeader.ReloadSafe();

		wrapper = GetNewWrapper();
		AssertEquals(nameof(ICusEntryHeaderCustomsMessageWrapper.GrossMass), 23.0563m, wrapper.GrossMass);
	}

	public void TestTransportChargesMethodOfPayment()
	{
		var wrapper = GetNewWrapper();
		AssertNullOrEmpty(nameof(ICusEntryHeaderCustomsMessageWrapper.TransportChargesMethodOfPayment), wrapper.TransportChargesMethodOfPayment);

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();

		var entryLine1 = entryHeader.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;
		entryLine1.InvoiceLines.Add(invoiceLine1);

		var entryLine2 = entryHeader.MergedLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;
		entryLine2.InvoiceLines.Add(invoiceLine2);

		CombineAssertions(() =>
		{
			invoice1.ZG_TransportChargesMethodOfPayment = "A";
			invoice2.ZG_TransportChargesMethodOfPayment = "A";
			wrapper = GetNewWrapper();
			AssertEquals("Same MOP for all headers, TransportChargesMethodOfPayment", "A", wrapper.TransportChargesMethodOfPayment);

			invoice2.ZG_TransportChargesMethodOfPayment = "B";
			wrapper = GetNewWrapper();
			AssertEquals("Different MOP for headers, TransportChargesMethodOfPayment", "", wrapper.TransportChargesMethodOfPayment);

			invoice2.ZG_TransportChargesMethodOfPayment = "";
			wrapper = GetNewWrapper();
			AssertEquals("Different MOP at header and one is empty, TransportChargesMethodOfPayment", "", wrapper.TransportChargesMethodOfPayment);

			invoice1.ZG_TransportChargesMethodOfPayment = "";
			wrapper = GetNewWrapper();
			AssertEquals("Empty MOP at headers, TransportChargesMethodOfPayment", "", wrapper.TransportChargesMethodOfPayment);
		});
	}

	public void TestTransportEquipment()
	{
		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		{
			var wrapper = GetNewWrapper();
			AssertEquals("When no containers or equipment are populated, TransportEquipment Count", 0, wrapper.TransportEquipment.Count);

			var container1 = CreateCusContainer(declaration, "C1", "C1_S1", "C1_S2", "C1_ADDS1", "C1_S1");
			var container2 = CreateCusContainer(declaration, "C2", "C2_S1", "C2_S1", "C2_ADDS2", "C2_ADDS1");
			var container3 = CreateCusContainer(declaration, "C3", "C3_S1", "");

			var equipment1 = CreateCusEquipment(declaration, "E1", "E1_S1", "E1_S2");
			var equipment2 = CreateCusEquipment(declaration, "E2", "E2_S1", "", " ");
			var equipment3 = CreateCusEquipment(declaration, "E3", "E3_S1", "E3_S2");

			var package1 = CreateNewPackageWithContainerOrEquipmentLink(declaration, container1.CO_ContainerNumber);
			var package2 = CreateNewPackageWithContainerOrEquipmentLink(declaration, container2.CO_ContainerNumber);
			var package3 = CreateNewPackageWithContainerOrEquipmentLink(declaration, container3.CO_ContainerNumber);
			var package4 = CreateNewPackageWithContainerOrEquipmentLink(declaration, equipment1.CEQ_IdentificationNumber);
			var package5 = CreateNewPackageWithContainerOrEquipmentLink(declaration, equipment2.CEQ_IdentificationNumber);
			var package6 = CreateNewPackageWithContainerOrEquipmentLink(declaration, equipment3.CEQ_IdentificationNumber);

			var invoice = declaration.Invoices.AddNew();

			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var entryLine1 = entryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			AddInvoiceLinePackagePivots(invoiceLine1, package1, package2, package3, package4, package6);
			invoiceLine1.JI_CL = entryLine1.PK;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			AddInvoiceLinePackagePivots(invoiceLine2, package1, package2);
			invoiceLine2.JI_CL = entryLine1.PK;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			var entryLine2 = entryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			AddInvoiceLinePackagePivots(invoiceLine3, package1, package2, package3, package4, package5, package6);
			invoiceLine3.JI_CL = entryLine2.PK;

			wrapper = GetNewWrapper();
			var transportEquipment = wrapper.TransportEquipment.ToArray();
			AssertEquals("When containers and equipment are populated, TransportEquipment Count", 6, transportEquipment.Length);
			CombineAssertions(() =>
			{
				AssertTransportEquipment(transportEquipment[0], 0, "C1", new int[] { 1, 2 }, 3, new string[] { "C1_S1", "C1_S2", "C1_ADDS1" });
				AssertTransportEquipment(transportEquipment[1], 1, "C2", new int[] { 1, 2 }, 3, new string[] { "C2_S1", "C2_ADDS2", "C2_ADDS1" });
				AssertTransportEquipment(transportEquipment[2], 2, "C3", new int[] { 1, 2 }, 1, new string[] { "C3_S1" });
				AssertTransportEquipment(transportEquipment[3], 3, "", new int[] { 1, 2 }, 2, new string[] { "E1_S1", "E1_S2" });
				AssertTransportEquipment(transportEquipment[4], 4, "", new int[] { 2 }, 1, new string[] { "E2_S1" });
				AssertTransportEquipment(transportEquipment[5], 5, "", new int[] { 1, 2 }, 2, new string[] { "E3_S1", "E3_S2" });
			});
		}
	}

	public void TestDeferredPayment()
	{
		var wrapper = GetNewWrapper();
		AssertNullOrEmpty(nameof(wrapper.DeferredPayment), wrapper.DeferredPayment);

		declaration.JE_DefermentAccountNumber = "123456A";
		wrapper = GetNewWrapper();
		AssertEquals(nameof(wrapper.DeferredPayment), "123456A", wrapper.DeferredPayment);

		declaration.JE_PaymentMethod = "A";
		declaration.JE_DefermentAccountNumber = "123456A";
		wrapper = GetNewWrapper();
		AssertEquals(nameof(wrapper.DeferredPayment), "123456", wrapper.DeferredPayment);
	}

	void AssertTransportEquipment(ITransportEquipment transportEquipment
		, int index
		, string expectedContainerID
		, int[] expectedLinkedGoodsItemNumbers
		, int expectedNumberOfSeals
		, string[] expectedSeals)
	{
		AssertEquals($"Element At '{index}', ContainerID", expectedContainerID, transportEquipment.ContainerID);
		AssertContainsExactElementsInAnyOrder($"Element At '{index}', LinkedGoodsItemNumbers", expectedLinkedGoodsItemNumbers, transportEquipment.LinkedGoodsItemNumbers.ToArray());
		AssertEquals($"Element At '{index}', NumberOfSeals", expectedNumberOfSeals, transportEquipment.NumberOfSeals);
		AssertArrayEqualsByElements($"Element At '{index}', Seals", expectedSeals, transportEquipment.Seals.ToArray());
	}

	CusContainer CreateCusContainer(JobDeclaration declaration, string containerNumber, string seal, string secondSeal, params string[] additionalSeals)
	{
		var container = declaration.CusContainers.AddNew();
		container.CO_ContainerNumber = containerNumber;

		foreach (var additionalSeal in additionalSeals)
		{
			container.AdditionalSeals.AddNew().BK_SealNumber = additionalSeal;
		}

		container.CO_Seal = seal;
		container.CO_SecondSeal = secondSeal;

		return container;
	}

	CusEquipment CreateCusEquipment(JobDeclaration declaration, string identificationNumber, params string[] sealNumberList)
	{
		var equipment = declaration.Equipments.AddNew();
		equipment.CEQ_IdentificationNumber = identificationNumber;
		foreach (var sealNumber in sealNumberList)
		{
			equipment.Seals.AddNew().BK_SealNumber = sealNumber;
		}
		return equipment;
	}

	BasePackage CreateNewPackageWithContainerOrEquipmentLink(JobDeclaration declaration, string containerOrEquipmentNo)
	{
		var package = declaration.Packages.AddNew();
		package.CW_ContainerNoOrEquipmentNo = containerOrEquipmentNo;
		return package;
	}

	void AddInvoiceLinePackagePivots(JobComInvoiceLine invoiceLine, params BasePackage[] packageList)
	{
		foreach (var package in packageList)
		{
			invoiceLine.PackagesPivot.AddPivotFor(package);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;

	ICusEntryHeaderCustomsMessageWrapper GetNewWrapper() => new CusEntryHeaderCustomsMessageWrapper(entryHeader);

	void AssertExportWrapperFieldWithSingleLine(string fieldName, Action<JobComInvoiceLine, string> setLineFieldTo, Action<JobDeclaration, string> setDeclarationTo, Func<ICusEntryHeaderCustomsMessageWrapper, string> getWrapperFieldValue)
	{
		var wrapper = GetNewWrapper();
		AssertNullOrEmpty($"When no invoices lines are found, {fieldName}", getWrapperFieldValue(wrapper));

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		var entryLine1 = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine1.PK;

		CombineAssertions(() =>
		{
			setDeclarationTo(declaration, "FI");
			setLineFieldTo(invoiceLine, string.Empty);
			wrapper = GetNewWrapper();
			AssertEquals($"When {fieldName} is only provided at declaration level and not at invoice line, {fieldName}", "FI", getWrapperFieldValue(wrapper));

			setLineFieldTo(invoiceLine, "FI");
			wrapper = GetNewWrapper();
			AssertEquals($"When invoice line and Declaration have same {fieldName}, {fieldName}", "FI", getWrapperFieldValue(wrapper));

			setLineFieldTo(invoiceLine, "US");
			wrapper = GetNewWrapper();
			AssertEquals($"When invoice line has {fieldName} different from declaration, {fieldName}", "US", getWrapperFieldValue(wrapper));

			setDeclarationTo(declaration, string.Empty);
			wrapper = GetNewWrapper();
			AssertEquals($"When invoice line has {fieldName} and declaration is empty, {fieldName}", "US", getWrapperFieldValue(wrapper));

			setLineFieldTo(invoiceLine, string.Empty);
			wrapper = GetNewWrapper();
			AssertEquals($"When both line and declaration don't have {fieldName}, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));
		});
	}

	void AssertExportWrapperField(string fieldName, Action<JobComInvoiceLine, string> setLineFieldTo, Action<JobDeclaration, string> setDeclarationTo, Func<ICusEntryHeaderCustomsMessageWrapper, string> getWrapperFieldValue)
	{
		var wrapper = GetNewWrapper();
		AssertNullOrEmpty($"When no invoices lines are found, {fieldName}", wrapper.CountryOfDestination);

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();

		var entryLine1 = entryHeader.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;

		var entryLine2 = entryHeader.MergedLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;

		CombineAssertions($"{fieldName} only in invoice lines", () =>
		{
			setDeclarationTo(declaration, string.Empty);

			setLineFieldTo(invoiceLine1, "IT");
			setLineFieldTo(invoiceLine2, string.Empty);
			wrapper = GetNewWrapper();
			AssertEquals($"When {fieldName} is only available in one line, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setLineFieldTo(invoiceLine1, "US");
			setLineFieldTo(invoiceLine2, "US");
			wrapper = GetNewWrapper();
			AssertEquals($"When all invoice lines have same {fieldName}, {fieldName}", "US", getWrapperFieldValue(wrapper));

			setLineFieldTo(invoiceLine1, "BS");
			wrapper = GetNewWrapper();
			AssertEquals($"When invoices lines have different {fieldName}, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setLineFieldTo(invoiceLine2, string.Empty);
			wrapper = GetNewWrapper();
			AssertEquals($"When invoices lines have different {fieldName} (one does not have value), {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setLineFieldTo(invoiceLine1, string.Empty);
			wrapper = GetNewWrapper();
			AssertEquals($"When invoices lines have same {fieldName} (all do not have value), {fieldName}", string.Empty, getWrapperFieldValue(wrapper));
		});

		CombineAssertions($"{fieldName} both in invoice lines and declaration", () =>
		{
			setDeclarationTo(declaration, "US");
			setLineFieldTo(invoiceLine1, "US");
			setLineFieldTo(invoiceLine2, "US");
			wrapper = GetNewWrapper();
			AssertEquals($"When {fieldName} is the same in lines and declaration, {fieldName}", "US", getWrapperFieldValue(wrapper));

			setDeclarationTo(declaration, "US");
			setLineFieldTo(invoiceLine1, "IT");
			setLineFieldTo(invoiceLine2, "BS");
			wrapper = GetNewWrapper();
			AssertEquals($"When {fieldName} is present in lines and declaration and different in all cases, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setDeclarationTo(declaration, "US");
			setLineFieldTo(invoiceLine1, "IT");
			setLineFieldTo(invoiceLine2, string.Empty);
			wrapper = GetNewWrapper();
			AssertEquals($"When {fieldName} is missing only in one or more lines but present everywhere, {fieldName}", string.Empty, getWrapperFieldValue(wrapper));

			setDeclarationTo(declaration, "US");
			setLineFieldTo(invoiceLine1, string.Empty);
			setLineFieldTo(invoiceLine2, string.Empty);
			wrapper = GetNewWrapper();
			AssertEquals($"When {fieldName} is only available at declaration level, {fieldName}", "US", getWrapperFieldValue(wrapper));
		});
	}

	void AssertExportWrapperTraderWithSingleLine(string fieldName, Action<JobComInvoiceLine, ZGuid> setLineFieldTo, Action<JobDeclaration, ZGuid> setDeclarationTo, Func<ICusEntryHeaderCustomsMessageWrapper, IEoriTrader> getWrapperFieldValue)
	{
		var wrapper = GetNewWrapper();

		AssertEquals($"When no invoices lines are found, {fieldName}", null, getWrapperFieldValue(wrapper));

		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();

		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine1 = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine1.PK;

		CombineAssertions(() =>
		{
			var declarationOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			declarationOrgHeader.CustomsCodes.AddNew("EOR", "135040449", "IT");

			var orgAddress1 = declarationOrgHeader.Addresses.AddNew();
			declaration.SupplierDocumentaryAddress.OrganisationPK = declarationOrgHeader.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = orgAddress1.PK;
			declaration.ImporterDocumentaryAddress.OrganisationPK = declarationOrgHeader.PK;
			declaration.ImporterDocumentaryAddress.E2_OA_Address = orgAddress1.PK;

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.CustomsCodes.AddNew("EOR", "999040449", "IT");

			wrapper = GetNewWrapper();
			AssertEquals($"When {fieldName} is only provided at declaration level and not at invoice line, {fieldName}", "IT135040449", getWrapperFieldValue(wrapper).EoriNumber);

			setLineFieldTo(invoiceLine, orgAddress1.PK);
			wrapper = GetNewWrapper();
			AssertEquals($"When invoice line and Declaration have same {fieldName}, {fieldName}", "IT135040449", getWrapperFieldValue(wrapper).EoriNumber);

			var lineAddress = Factory.New<OrgAddress>();
			lineAddress.OA_OH = orgHeader2.PK;

			setLineFieldTo(invoiceLine, lineAddress.PK);
			wrapper = GetNewWrapper();
			AssertEquals($"When invoice line has {fieldName} different from declaration, {fieldName}", "IT999040449", getWrapperFieldValue(wrapper).EoriNumber);

			setDeclarationTo(declaration, ZGuid.Empty);
			wrapper = GetNewWrapper();
			AssertEquals($"When invoice line has {fieldName} and declaration is empty, {fieldName}", "IT999040449", getWrapperFieldValue(wrapper).EoriNumber);

			setLineFieldTo(invoiceLine, ZGuid.Empty);
			wrapper = GetNewWrapper();
			AssertEquals($"When both line and declaration don't have {fieldName}, {fieldName}", null, getWrapperFieldValue(wrapper));
		});
	}

	void AssertExportWrapperTrader(string fieldName, Action<JobComInvoiceLine, ZGuid> setLineFieldTo, Action<JobDeclaration, ZGuid> setDeclarationTo, Func<ICusEntryHeaderCustomsMessageWrapper, IEoriTrader> getWrapperFieldValue)
	{
		var wrapper = GetNewWrapper();
		AssertNull($"When no invoices lines are found, {fieldName}", getWrapperFieldValue(wrapper));

		var invoice1 = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice1.InvoiceLines.AddNew();
		var invoice2 = declaration.Invoices.AddNew();
		var invoiceLine2 = invoice2.InvoiceLines.AddNew();

		var entryLine1 = entryHeader.MergedLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;

		var entryLine2 = entryHeader.MergedLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;

		CombineAssertions($"{fieldName} only in invoice lines", () =>
		{
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.CustomsCodes.AddNew("EOR", "222222222", "IT");

			var orgHeader3 = Factory.New<OrgHeader>();
			orgHeader3.CustomsCodes.AddNew("EOR", "333333333", "IT");

			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_OH = orgHeader2.PK;

			var orgAddress3 = Factory.New<OrgAddress>();
			orgAddress3.OA_OH = orgHeader3.PK;

			setLineFieldTo(invoiceLine1, orgAddress2.PK);
			setLineFieldTo(invoiceLine2, ZGuid.Empty);
			wrapper = GetNewWrapper();
			AssertEquals($"When {fieldName} is only available in one line, {fieldName}", null, wrapper.Consignor);

			setLineFieldTo(invoiceLine1, orgAddress2.PK);
			setLineFieldTo(invoiceLine2, orgAddress2.PK);
			wrapper = GetNewWrapper();
			AssertEquals($"When all invoice lines have same {fieldName}, {fieldName}", "IT222222222", getWrapperFieldValue(wrapper).EoriNumber);

			setLineFieldTo(invoiceLine1, orgAddress3.PK);
			wrapper = GetNewWrapper();
			AssertEquals($"When invoices lines have different {fieldName}, {fieldName}", null, getWrapperFieldValue(wrapper));

			setLineFieldTo(invoiceLine2, ZGuid.Empty);
			wrapper = GetNewWrapper();
			AssertEquals($"When invoices lines have different {fieldName} (one does not have value), {fieldName}", null, getWrapperFieldValue(wrapper));

			setLineFieldTo(invoiceLine1, ZGuid.Empty);
			wrapper = GetNewWrapper();
			AssertEquals($"When invoices lines have same {fieldName} (all do not have value), {fieldName}", null, getWrapperFieldValue(wrapper));
		});

		CombineAssertions($"{fieldName} both in invoice lines and declaration", () =>
		{
			var headerOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
			headerOrgHeader.CustomsCodes.AddNew("EOR", "111111111", "IT");

			var orgAddress1 = headerOrgHeader.Addresses.AddNew();
			declaration.SupplierDocumentaryAddress.OrganisationPK = headerOrgHeader.PK;
			declaration.SupplierDocumentaryAddress.E2_OA_Address = orgAddress1.PK;

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.CustomsCodes.AddNew("EOR", "222222222", "IT");

			var orgHeader3 = Factory.New<OrgHeader>();
			orgHeader3.CustomsCodes.AddNew("EOR", "333333333", "IT");

			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_OH = orgHeader2.PK;

			var orgAddress3 = Factory.New<OrgAddress>();
			orgAddress3.OA_OH = orgHeader3.PK;

			setDeclarationTo(declaration, orgAddress1.PK);
			setLineFieldTo(invoiceLine1, orgAddress1.PK);
			setLineFieldTo(invoiceLine2, orgAddress1.PK);
			wrapper = GetNewWrapper();
			AssertEquals($"When {fieldName} is the same in lines and declaration, {fieldName}", "IT111111111", getWrapperFieldValue(wrapper).EoriNumber);

			setDeclarationTo(declaration, orgAddress1.PK);
			setLineFieldTo(invoiceLine1, orgAddress2.PK);
			setLineFieldTo(invoiceLine2, orgAddress3.PK);
			wrapper = GetNewWrapper();
			AssertEquals($"When {fieldName} is present in lines and declaration and different in all cases, {fieldName}", null, getWrapperFieldValue(wrapper));

			setDeclarationTo(declaration, orgAddress1.PK);
			setLineFieldTo(invoiceLine1, orgAddress2.PK);
			setLineFieldTo(invoiceLine2, ZGuid.Empty);
			wrapper = GetNewWrapper();
			AssertEquals($"When {fieldName} is missing only in one or more lines but present everywhere, {fieldName}", null, getWrapperFieldValue(wrapper));

			setDeclarationTo(declaration, orgAddress1.PK);
			setLineFieldTo(invoiceLine1, ZGuid.Empty);
			setLineFieldTo(invoiceLine2, ZGuid.Empty);
			wrapper = GetNewWrapper();
			AssertEquals($"When {fieldName} is only available at declaration level, {fieldName}", "IT111111111", getWrapperFieldValue(wrapper).EoriNumber);
		});
	}
}
