using System;
using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSb;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Registry;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb.Testing;

[TestedType(typeof(ExportSbCACHE01DataProvider))]
sealed class ExportSbTableSbDataProviderTest : ExportSbTableSbDataProviderAbstractClassBase
{
	public override void TestAmendmentDate()
	{
		AdditionalDataProviderMock.Setup(x => x.GetAmendmentDate(It.IsAny<CusEntryHeader>())).Returns(new DateTime(2025, 04, 22));
		AssertEquals(new DateTime(2025, 04, 22), CreateDataProvider().AmendmentDate);
	}

	public override void TestAmendmentNo()
	{
		AdditionalDataProviderMock.Setup(x => x.GetAmendmentNo(It.IsAny<CusEntryHeader>())).Returns("00001");
		AssertEquals("00001", CreateDataProvider().AmendmentNo);
	}

	public override void TestAmendmentType()
	{
		AdditionalDataProviderMock.Setup(x => x.GetAmendmentType(It.IsAny<CusEntryHeader>())).Returns("F");
		AssertEquals("F", CreateDataProvider().AmendmentType);
	}

	public override void TestAuthorizedDealerCode()
	{
		var supplier = Factory.New<OrgHeader>();
		var supplierAddress = supplier.Addresses.AddNew();
		supplier.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.ADC, "111", countryCode: Core.Constants.CountryCodes.India)
			.OK_OA_PremisesAddress = supplierAddress.PK;

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Declaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
		AssertEquals(ZString.Empty, CreateDataProvider().AuthorizedDealerCode);
		Declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierAddress.PK;
		AssertEquals("111", CreateDataProvider().AuthorizedDealerCode);
	}

	public override void TestBranchSrNoOfExporter()
	{
		var supplier = Factory.New<OrgHeader>();
		var supplierAddress = supplier.Addresses.AddNew();
		supplier.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.BSN, "111", countryCode: Core.Constants.CountryCodes.India)
			.OK_OA_PremisesAddress = supplierAddress.PK;

		Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		Declaration.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
		AssertEquals(ZString.Empty, CreateDataProvider().BranchSrNoOfExporter);
		Declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierAddress.PK;
		AssertEquals("111", CreateDataProvider().BranchSrNoOfExporter);
	}

	public override void TestCategoryOfNfeiSb()
	{
		Instruction.CEI_SubStyle = "02";
		AssertEquals("02", CreateDataProvider().CategoryOfNfeiSb);
	}

	public override void TestChaLicenseNumber()
	{
		var registryItem = INCustomsDataRegistry.Instance.INCHALicenseNumber;
		AssertEquals("Before registry setup", ZString.Empty, CreateDataProvider().ChaLicenseNumber);

		var expectedValue = "ABCabc123!@#<>?";
		using (registryItem.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, expectedValue))
		{
			AssertEquals("After registry setup", expectedValue, CreateDataProvider().ChaLicenseNumber);
		}
	}

	public override void TestConsigneeName()
	{
		CreateDataForConsigneeInfo();
		AssertEquals("Full Name Full Name Full Name Full ", CreateDataProvider().ConsigneeName);
	}

	public override void TestConsigneeAddress1()
	{
		CreateDataForConsigneeInfo();
		AssertEquals("Name Full Name Full Name Address 1 ", CreateDataProvider().ConsigneeAddress1);
	}

	public override void TestConsigneeAddress2()
	{
		CreateDataForConsigneeInfo();
		AssertEquals("Address 1 Address 1 Address 2 Addre", CreateDataProvider().ConsigneeAddress2);
	}

	public override void TestConsigneeAddress3()
	{
		CreateDataForConsigneeInfo();
		AssertEquals("ss 2 Address 2 Address 2 City State", CreateDataProvider().ConsigneeAddress3);
	}

	public override void TestConsigneeAddress4()
	{
		CreateDataForConsigneeInfo();
		AssertEquals(" 239000", CreateDataProvider().ConsigneeAddress4);
	}

	public override void TestConsigneeCountry()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		var address = org.MainAddress;
		address.OA_RN_NKCountryCode = "IN";
		Declaration.JE_OH_Importer = org.PK;
		AssertEquals("IN", CreateDataProvider().ConsigneeCountry);
	}

	public override void TestCountryOfDischarge()
	{
		Declaration.JE_RL_NKPortOfArrival = "USLAX";
		AssertEquals("US", CreateDataProvider().CountryOfDischarge);
	}

	public override void TestCountryOfFinalDestination()
	{
		Declaration.JE_RL_NKFinalDestination = "AUSYD";
		AssertEquals("AU", CreateDataProvider().CountryOfFinalDestination);
	}

	public override void TestCustomHouseCode()
	{
		Declaration.JE_CustomsOffice = "INBLR";
		AssertEquals("INBLR", CreateDataProvider().CustomHouseCode);
	}

	public override void TestEpzCode()
	{
		Declaration.JE_EPZCode = "E";
		AssertEquals("E", CreateDataProvider().EpzCode);
	}

	public override void TestExporterClass()
	{
		var supplier = Factory.New<OrgHeader>();
		supplier.OH_Category = OrgConstants.Category.Business;
		Declaration.JE_OH_Supplier = supplier.PK;
		AssertEquals("P", CreateDataProvider().ExporterClass);
	}

	public override void TestGrossWeight()
	{
		var invoiceLine1 = GetInvoiceLine();
		var invoiceLine2 = GetInvoiceLine();
		invoiceLine1.JI_Weight = 20.5m;
		invoiceLine2.JI_Weight = 30.6m;
		invoiceLine1.JI_WeightUQ = "KG";
		invoiceLine2.JI_WeightUQ = "KG";
		Instruction.CEI_WeightUQ = "KG";
		AssertEquals(51.1m, CreateDataProvider().GrossWeight);
	}

	public override void TestGstnId()
	{
		AssertEquals("TBA", CreateDataProvider().GstnId);
	}

	public override void TestGstnType()
	{
		AssertEquals("TBA", CreateDataProvider().GstnType);
	}

	public override void TestHawbNumber()
	{
		Declaration.JE_HouseBill = "HB123";
		AssertEquals("HB123", CreateDataProvider().HawbNumber);
	}

	public override void TestImpExpAddress1()
	{
		CreateDataForImpExpInfo();
		AssertEquals("Address 1", CreateDataProvider().ImpExpAddress1);
	}

	public override void TestImpExpAddress2()
	{
		CreateDataForImpExpInfo();
		AssertEquals("Address 2", CreateDataProvider().ImpExpAddress2);
	}

	public override void TestImpExpCity()
	{
		CreateDataForImpExpInfo();
		AssertEquals("City", CreateDataProvider().ImpExpCity);
	}

	public override void TestImpExpName()
	{
		CreateDataForImpExpInfo();
		AssertEquals("Full Name1", CreateDataProvider().ImpExpName);
	}

	public override void TestImpExpPin()
	{
		CreateDataForImpExpInfo();
		AssertEquals("239000", CreateDataProvider().ImpExpPin);
	}

	public override void TestImpExpState()
	{
		CreateDataForImpExpInfo();
		AssertEquals("State", CreateDataProvider().ImpExpState);
	}

	public override void TestImporterExporterCode()
	{
		var importer = Factory.New<OrgHeader>();
		Declaration.JE_OH_Importer = importer.PK;
		var cusCode = importer.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.IEC, "1234567890", Core.Constants.CountryCodes.India);

		AssertEquals("1234567890", CreateDataProvider().ImporterExporterCode);
	}

	public override void TestJobDate()
	{
		header.CH_SystemCreateTimeUtc = new ZDateTime(2024, 6, 13);
		AssertEquals(new ZDateTime(2024, 6, 13).ToLocalBranchTime().ToDateTime(), CreateDataProvider().JobDate);
	}

	public override void TestJobNumber()
	{
		header.CH_BGMReference = "1234";
		AssertEquals("1234", CreateDataProvider().JobNumber);
	}

	public override void TestMarksNumbers()
	{
		AdditionalDataProviderMock.Setup(x => x.GetMarksNumbers(It.IsAny<CusEntryHeader>())).Returns("1234567");
		AssertEquals("1234567", CreateDataProvider().MarksNumbers);
	}

	public override void TestMawbNumber()
	{
		Declaration.JE_MasterBill = "MB123";
		AssertEquals("MB123", CreateDataProvider().MawbNumber);
	}

	public override void TestMessageType()
	{
		AdditionalDataProviderMock.Setup(x => x.GetMessageType(It.IsAny<CusEntryHeader>())).Returns("F");
		AssertEquals("F", CreateDataProvider().MessageType);

		AdditionalDataProviderMock.Setup(x => x.GetMessageType(It.IsAny<CusEntryHeader>())).Returns("D");
		AssertEquals("D", CreateDataProvider().MessageType);
	}

	public override void TestNatureOfCargo()
	{
		AdditionalDataProviderMock.Setup(x => x.GetNatureOfCargo(It.IsAny<CusEntryHeader>())).Returns("P");
		AssertEquals("P", CreateDataProvider().NatureOfCargo);
	}

	public override void TestNetWeight()
	{
		var invoiceLine1 = GetInvoiceLine();
		var invoiceLine2 = GetInvoiceLine();
		invoiceLine1.JI_NetWeight = 20.5m;
		invoiceLine2.JI_NetWeight = 30.6m;
		invoiceLine1.JI_NetWeightUQ = "KG";
		invoiceLine2.JI_NetWeightUQ = "KG";
		Instruction.CEI_WeightUQ = "KG";
		AssertEquals(51.1m, CreateDataProvider().NetWeight);
	}

	public override void TestNumberOfContainers()
	{
		Instruction.CEI_TotalContainer = 5;
		AssertEquals(5, CreateDataProvider().NumberOfContainers);
	}

	public override void TestNumberOfLoosePackets()
	{
		Instruction.CEI_LoosePackages = 1;
		AssertEquals(1, CreateDataProvider().NumberOfLoosePackets);
	}

	public override void TestPortOfDischarge()
	{
		AdditionalDataProviderMock.Setup(x => x.GetPortOfDischarge(It.IsAny<CusEntryHeader>())).Returns("USLAX");
		AssertEquals("USLAX", CreateDataProvider().PortOfDischarge);
	}

	public override void TestPortOfFinalDestination()
	{
		AdditionalDataProviderMock.Setup(x => x.GetPortOfFinalDestination(It.IsAny<CusEntryHeader>())).Returns("USLAX");
		AssertEquals("USLAX", CreateDataProvider().PortOfFinalDestination);
	}

	public override void TestPortOfLoading()
	{
		Declaration.JE_CustomsLoadPort = "CNHXU";
		AssertEquals("CNHXU", CreateDataProvider().PortOfLoading);
	}

	public override void TestRbiWaiverDate()
	{
		AssertNull(CreateDataProvider().RbiWaiverDate);

		Instruction.RBIWaiverDate = new ZDate(2025, 1, 19);
		AssertEquals(new ZDate(2025, 1, 19), CreateDataProvider().RbiWaiverDate);
	}

	public override void TestRbiWaiverNumber()
	{
		Instruction.RBIWaiverNumber = "1234";
		AssertEquals("1234", CreateDataProvider().RbiWaiverNumber);
	}

	public override void TestSbDate()
	{
		Instruction.ShippingBillDate = new ZDate(2025, 04, 22);
		AssertEquals(new ZDate(2025, 04, 22), CreateDataProvider().SbDate);

		Instruction.ShippingBillDate = ZDateTime.Empty;
		AssertEquals(null, CreateDataProvider().SbDate);
	}

	public override void TestSbNo()
	{
		Instruction.ShippingBillNumber = "1234";
		AssertEquals("1234", CreateDataProvider().SbNo);
	}

	public override void TestSealType()
	{
		Declaration.JE_SealBy = SealByCodeList.Codes.S;
		AssertEquals("S", CreateDataProvider().SealType);
	}

	public override void TestStateOfOriginExporter()
	{
		AdditionalDataProviderMock.Setup(x => x.GetStateOfOriginExporter(It.IsAny<CusEntryHeader>())).Returns("CA");
		AssertEquals("CA", CreateDataProvider().StateOfOriginExporter);
	}

	public override void TestTotalNumberOfPackages()
	{
		Instruction.CEI_NumberOfPackages = 1;
		AssertEquals(1, CreateDataProvider().TotalNumberOfPackages);
	}

	public override void TestTypeOfExporter()
	{
		Declaration.JE_ExporterType = "R";
		AssertEquals("R", CreateDataProvider().TypeOfExporter);
	}

	public override void TestUnitOfMeasurement()
	{
		Instruction.CEI_WeightUQ = "KG";
		AssertEquals("KG", CreateDataProvider().UnitOfMeasurement);
	}

	protected override TableSbDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbCACHE01DataProvider.CreateProvider(header, AdditionalDataProviderMock.Object).Sb.TableSb.First();
	}

	Mock<IExportSbCACHE01AdditionalDataProvider> AdditionalDataProviderMock => additionalDataProviderMock ??= new Mock<IExportSbCACHE01AdditionalDataProvider>();
	Mock<IExportSbCACHE01AdditionalDataProvider> additionalDataProviderMock;

	JobDeclaration Declaration => declaration ??= GetJobDeclaration();
	JobDeclaration declaration;

	CusEntryInstruction Instruction => instruction ??= GetInstruction();
	CusEntryInstruction instruction;

	JobComInvoiceHeader InvoiceHeader => invoiceHeader ??= Declaration.Invoices.AddNew();
	JobComInvoiceHeader invoiceHeader;

	CusEntryInstruction GetInstruction()
	{
		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		header.CH_CEI_Instruction = entryInstruction.PK;
		return entryInstruction;
	}

	JobDeclaration GetJobDeclaration()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		header.CH_JE = jobDeclaration.PK;
		return jobDeclaration;
	}

	JobComInvoiceLine GetInvoiceLine()
	{
		var invoiceLine = InvoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CEI = Instruction.PK;
		return invoiceLine;
	}

	void CreateDataForImpExpInfo()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		var address = org.MainAddress;
		org.OH_FullName = "Full Name1";
		address.OA_Address1 = "Address 1";
		address.OA_Address2 = "Address 2";
		address.OA_City = "City";
		address.OA_State = "State";
		address.OA_PostCode = "239000";
		address.OA_RN_NKCountryCode = "IN";
		Declaration.JE_OH_Supplier = org.PK;
	}

	void CreateDataForConsigneeInfo()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		var address = org.MainAddress;
		org.OH_FullName = "Full Name Full Name Full Name Full Name Full Name Full Name";
		address.OA_Address1 = "Address 1 Address 1 Address 1";
		address.OA_Address2 = "Address 2 Address 2 Address 2 Address 2";
		address.OA_City = "City";
		address.OA_State = "State";
		address.OA_PostCode = "239000";
		Declaration.JE_OH_Importer = org.PK;
	}
}

