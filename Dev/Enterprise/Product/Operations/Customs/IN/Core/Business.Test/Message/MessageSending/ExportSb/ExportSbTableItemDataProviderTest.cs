using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSb;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb.Testing;

[TestedType(typeof(ExportSbCACHE01DataProvider))]
sealed class ExportSbTableItemDataProviderTest : ExportSbTableItemDataProviderAbstractClassBase
{
	public override void TestMessageType()
	{
		Assert("to do in future WI", true);
	}

	public override void TestCustomHouseCode()
	{
		declaration.JE_CustomsOffice = "INBLR";
		AssertEquals("INBLR", CreateDataProvider().CustomHouseCode);
	}

	public override void TestJobNumber()
	{
		header.CH_BGMReference = "1234";
		AssertEquals("1234", CreateDataProvider().JobNumber);
	}

	public override void TestJobDate()
	{
		header.CH_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
		AssertEquals(ZDateTime.BrettsBirthday.ToLocalBranchTime().ToDateTime(), CreateDataProvider().JobDate);
	}

	public override void TestSbNo()
	{
		Instruction.ShippingBillNumber = "1234";
		AssertEquals("1234", CreateDataProvider().SbNo);
	}

	public override void TestSbDate()
	{
		Instruction.ShippingBillDate = ZDateTime.BrettsBirthday;
		AssertEquals(ZDateTime.BrettsBirthday.ToDateTime(), CreateDataProvider().SbDate);
	}

	public override void TestInvoiceSrNumber()
	{
		invoiceHeader.JZ_InvoiceDisplaySequence = 1;
		AssertEquals(1, CreateDataProvider().InvoiceSrNumber);
	}

	public override void TestItemSrNumberInInvoice()
	{
		entryLine.CL_LineNumber = 1;
		AssertEquals(1, CreateDataProvider().ItemSrNumberInInvoice);
	}

	public override void TestSchemeCode()
	{
		invoiceLine.JI_Procedure = "1234";
		AssertEquals("1234", CreateDataProvider().SchemeCode);
	}

	public override void TestRitcCodeItchsCode()
	{
		invoiceLine.JI_Tariff = "1234";
		AssertEquals("1234", CreateDataProvider().RitcCodeItchsCode);
	}

	public override void TestDescriptionOfTheGoods1()
	{
		invoiceLine.JI_Description = fullDescription;
		AssertEquals(new string('a', 40), CreateDataProvider().DescriptionOfTheGoods1);
	}

	public override void TestDescriptionOfTheGoods2()
	{
		invoiceLine.JI_Description = fullDescription;
		AssertEquals(new string('b', 40), CreateDataProvider().DescriptionOfTheGoods2);
	}

	public override void TestDescriptionOfTheGoods3()
	{
		invoiceLine.JI_Description = fullDescription;
		AssertEquals(new string('c', 40), CreateDataProvider().DescriptionOfTheGoods3);
	}

	public override void TestUnitOfMeasurement()
	{
		invoiceLine.JI_InvoiceUQ = "ABC";
		AssertEquals("ABC", CreateDataProvider().UnitOfMeasurement);
	}

	public override void TestQuantity()
	{
		invoiceLine.JI_InvoiceQuantity = 1234;
		AssertEquals(1234m, CreateDataProvider().Quantity);
	}

	public override void TestUnitPrice()
	{
		invoiceLine.JI_UnitPrice = 5678;
		AssertEquals(5678m, CreateDataProvider().UnitPrice);
	}

	public override void TestUnitOfRate()
	{
		invoiceLine.JI_UnitUQ = "BAG";
		AssertEquals("BAG", CreateDataProvider().UnitOfRate);
	}

	public override void TestNoOfUnit()
	{
		invoiceLine.JI_UnitQuantity = 10;
		AssertEquals(10, CreateDataProvider().NoOfUnit);
	}

	public override void TestPresentMarketValue()
	{
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		invoiceLine.InvoiceHeader.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.India;
		invoiceLine.JI_ValuationMarkup = 0.00m;
		invoiceLine.JI_PMV = 12.00m;

		AssertEquals(12.00m, CreateDataProvider().PresentMarketValue);
	}

	public override void TestJobWorkNotificationNo()
	{
		invoiceLine.JI_JobWorkNotificationNo = "Number";
		AssertEquals("Number", CreateDataProvider().JobWorkNotificationNo);
	}

	public override void TestThirdParty()
	{
		Assert("to do in future WI", true);
	}

	public override void TestRewardItem()
	{
		invoiceLine.JI_RewardItem = "Y";
		AssertEquals("Y", CreateDataProvider().RewardItem);
	}

	public override void TestAmendmentType()
	{
		Assert("to do in future WI", true);
	}

	public override void TestAmendmentNo()
	{
		Assert("to do in future WI", true);
	}

	public override void TestAmendmentDate()
	{
		Assert("to do in future WI", true);
	}

	public override void TestItemManufacturerProducerCodeType()
	{
		Assert("to do in future WI", true);
	}

	public override void TestItemManufacturerProducerGrowerCode()
	{
		Assert("to do in future WI", true);
	}

	public override void TestItemManufacturerProducerGrowerAddress1()
	{
		Assert("to do in future WI", true);
	}

	public override void TestItemManufacturerProducerGrowerAddress2()
	{
		Assert("to do in future WI", true);
	}

	public override void TestItemManufacturerProducerGrowerCity()
	{
		Assert("to do in future WI", true);
	}

	public override void TestItemManufacturerProducerGrowerCountrySubdivision()
	{
		Assert("to do in future WI", true);
	}

	public override void TestItemManufacturerProducerGrowerPin()
	{
		Assert("to do in future WI", true);
	}

	public override void TestItemManufacturerCountry()
	{
		invoiceLine.JI_CountryOfOrigin = "AB";
		AssertEquals("AB", CreateDataProvider().ItemManufacturerCountry);
	}

	public override void TestSourceState()
	{
		AdditionalDataProviderMock.Setup(x => x.GetSourceState(It.IsAny<CusEntryLine>())).Returns("KA");
		AssertEquals("KA", CreateDataProvider().SourceState);
	}

	public override void TestTransitCountry()
	{
		Assert("to do in future WI", true);
	}

	public override void TestAccessoryStatus()
	{
		invoiceLine.JI_AccessoryStatus = "0";
		AssertEquals("0", CreateDataProvider().AccessoryStatus);
	}

	public override void TestEndUseOfItem()
	{
		invoiceLine.JI_EndUse = "X";
		AssertEquals("X", CreateDataProvider().EndUseOfItem);
	}

	public override void TestHawbNo()
	{
		declaration.JE_HouseBill = "1234";
		AssertEquals("1234", CreateDataProvider().HawbNo);
	}

	public override void TestTotalPackage()
	{
		Assert("to do in future WI", true);
	}

	public override void TestIgstPaymentStatus()
	{
		AdditionalDataProviderMock.Setup(x => x.GetIgstPaymentStatus(It.IsAny<CusEntryLine>())).Returns("NA");
		AssertEquals("NA", CreateDataProvider().IgstPaymentStatus);
	}

	public override void TestTaxableValue()
	{
		Assert("to do in future WI", true);
	}

	public override void TestIgstAmount()
	{
		Assert("to do in future WI", true);
	}

	protected override TableItemDataProviderAbstractClass CreateDataProvider() => ExportSbCACHE01DataProvider.CreateProvider(header, AdditionalDataProviderMock.Object).Sb.TableItem.First();

	Mock<IExportSbCACHE01AdditionalDataProvider> AdditionalDataProviderMock => additionalDataProviderMock ??= new Mock<IExportSbCACHE01AdditionalDataProvider>();
	Mock<IExportSbCACHE01AdditionalDataProvider> additionalDataProviderMock;

	JobDeclaration declaration;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;

	static readonly string fullDescription = new string('a', 40) + new string('b', 40) + new string('c', 40);

	CusEntryInstruction Instruction => instruction ??= GetEntryInstruction();
	CusEntryInstruction instruction;

	protected override void SetUp()
	{
		base.SetUp();
		entryLine = header.MergedLines.AddNew();
		declaration = GetJobDeclaration();
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = GetInvoiceLine(invoiceHeader);
	}

	JobDeclaration GetJobDeclaration()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		header.CH_JE = jobDeclaration.PK;
		return jobDeclaration;
	}

	JobComInvoiceLine GetInvoiceLine(JobComInvoiceHeader invoiceHeader)
	{
		var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		return invoiceLine;
	}

	CusEntryInstruction GetEntryInstruction()
	{
		var entryInstruction = Factory.New<CusEntryInstruction>();
		invoiceLine.JI_CEI = entryInstruction.PK;
		header.CH_CEI_Instruction = entryInstruction.PK;
		return invoiceLine.EntryInstruction;
	}
}
