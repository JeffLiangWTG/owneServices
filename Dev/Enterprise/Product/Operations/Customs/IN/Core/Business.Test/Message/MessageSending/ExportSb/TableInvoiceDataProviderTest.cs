using System.Linq;
using CargoWise.Customs.IN.MessageContracts.ExportSb;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.MessageSending.ExportSb.Testing;

[TestedType(typeof(ExportSbCACHE01DataProvider))]
sealed class TableInvoiceDataProviderAbstractClassTest : ExportSbTableInvoiceDataProviderAbstractClassBase
{
	public override void TestActualInvoiceNumber()
	{
		AssertEquals("TBA", CreateDataProvider().ActualInvoiceNumber);
	}

	public override void TestAddFreight()
	{
		AssertEquals("TBA", CreateDataProvider().AddFreight);
	}

	public override void TestAmendmentDate()
	{
		AssertEquals("TBA", CreateDataProvider().AmendmentDate);
	}

	public override void TestAmendmentNo()
	{
		AssertEquals("TBA", CreateDataProvider().AmendmentNo);
	}

	public override void TestAmendmentType()
	{
		AssertEquals("TBA", CreateDataProvider().AmendmentType);
	}

	public override void TestAuthorizedEconomicOperatorCode()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		var addressIN = org.Addresses.AddNew();
		var cusCode = org.CustomsCodes.AddNew(IndiaOrgCusCodeInfo.OrgCusCodes.AEO, "1234");
		cusCode.OK_OA_PremisesAddress = addressIN.PK;

		invoice.AuthorizedEconomicOperatorAddress.E2_OA_Address = addressIN.PK;
		AssertEquals("1234", CreateDataProvider().AuthorizedEconomicOperatorCode);
	}

	public override void TestAuthorizedEconomicOperatorCountry()
	{
		var org = Factory.NewWithValidTestData<OrgHeader>();
		org.MainAddress.OA_RN_NKCountryCode = "IN";

		var addressIN = org.Addresses.AddNew();
		invoice.AuthorizedEconomicOperatorAddress.E2_OA_Address = addressIN.PK;
		AssertEquals("IN", CreateDataProvider().AuthorizedEconomicOperatorCountry);
	}

	public override void TestAuthorizedEconomicOperatorRole()
	{
		invoice.JZ_AuthorizedEconomicOperatorRole = "ABC";
		AssertEquals("ABC", CreateDataProvider().AuthorizedEconomicOperatorRole);
	}

	public override void TestBuyerAddress1()
	{
		AssertEquals("TBA", CreateDataProvider().BuyerAddress1);
	}

	public override void TestBuyerAddress2()
	{
		AssertEquals("TBA", CreateDataProvider().BuyerAddress2);
	}

	public override void TestBuyerAddress3()
	{
		AssertEquals("TBA", CreateDataProvider().BuyerAddress3);
	}

	public override void TestBuyerAddress4()
	{
		AssertEquals("TBA", CreateDataProvider().BuyerAddress4);
	}

	public override void TestBuyerName()
	{
		AssertEquals("TBA", CreateDataProvider().BuyerName);
	}

	public override void TestCommissionAmount()
	{
		AssertEquals("TBA", CreateDataProvider().CommissionAmount);
	}

	public override void TestCommissionCurrency()
	{
		AssertEquals("TBA", CreateDataProvider().CommissionCurrency);
	}

	public override void TestCommissionRate()
	{
		AssertEquals("TBA", CreateDataProvider().CommissionRate);
	}

	public override void TestCustomHouseCode()
	{
		declaration.JE_CustomsOffice = "INBLR";
		AssertEquals("INBLR", CreateDataProvider().CustomHouseCode);
	}

	public override void TestDiscountAmount()
	{
		AssertEquals("TBA", CreateDataProvider().DiscountAmount);
	}

	public override void TestDiscountCurrency()
	{
		AssertEquals("TBA", CreateDataProvider().DiscountCurrency);
	}

	public override void TestDiscountOnFobInPercentage()
	{
		AssertEquals("TBA", CreateDataProvider().DiscountOnFobInPercentage);
	}

	public override void TestExporterContractNumber()
	{
		AssertEquals("TBA", CreateDataProvider().ExporterContractNumber);
	}

	public override void TestFreightAmount()
	{
		AssertEquals("TBA", CreateDataProvider().FreightAmount);
	}

	public override void TestFreightCurrency()
	{
		AssertEquals("TBA", CreateDataProvider().FreightCurrency);
	}

	public override void TestInsuranceAmount()
	{
		AssertEquals("TBA", CreateDataProvider().InsuranceAmount);
	}

	public override void TestInsuranceCurrency()
	{
		AssertEquals("TBA", CreateDataProvider().InsuranceCurrency);
	}

	public override void TestInsuranceRate()
	{
		AssertEquals("TBA", CreateDataProvider().InsuranceRate);
	}

	public override void TestInvoiceCurrency()
	{
		AssertEquals("TBA", CreateDataProvider().InvoiceCurrency);
	}

	public override void TestInvoiceDate()
	{
		AssertEquals("TBA", CreateDataProvider().InvoiceDate);
	}

	public override void TestInvoiceSrNo()
	{
		AssertEquals("TBA", CreateDataProvider().InvoiceSrNo);
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

	public override void TestMessageType()
	{
		messageSendingObject.MessageType = DeclarationMessageTypeList.Codes.Fresh;
		AssertEquals(DeclarationMessageTypeList.Codes.Fresh, CreateDataProvider().MessageType);
	}

	public override void TestNatureOfContract()
	{
		invoice.JZ_IncoTerm = INIncoTermList.Codes.CostInsuranceAndFreight;
		AssertEquals("CIF", CreateDataProvider().NatureOfContract);
	}

	public override void TestNatureOfPayment()
	{
		AssertEquals("TBA", CreateDataProvider().NatureOfPayment);
	}

	public override void TestOtherDeductionsAmount()
	{
		AssertEquals("TBA", CreateDataProvider().OtherDeductionsAmount);
	}

	public override void TestOtherDeductionsCurrency()
	{
		AssertEquals("TBA", CreateDataProvider().OtherDeductionsCurrency);
	}

	public override void TestOtherDeductionsInPercentage()
	{
		AssertEquals("TBA", CreateDataProvider().OtherDeductionsInPercentage);
	}

	public override void TestPackingCharges()
	{
		AssertEquals("TBA", CreateDataProvider().PackingCharges);
	}

	public override void TestPeriodOfPaymentInDays()
	{
		AssertEquals("TBA", CreateDataProvider().PeriodOfPaymentInDays);
	}

	public override void TestSbDate()
	{
		header.EntryInstruction.ShippingBillDate = new ZDateTime(2024, 6, 13);
		AssertEquals(new ZDateTime(2024, 6, 13).ToDateTime(), CreateDataProvider().SbDate);
	}

	public override void TestSbNo()
	{
		header.EntryInstruction.ShippingBillNumber = "1234";
		AssertEquals("1234", CreateDataProvider().SbNo);
	}

	public override void TestTermsPlace()
	{
		AssertEquals("TBA", CreateDataProvider().TermsPlace);
	}

	public override void TestThirdPartyAddress1()
	{
		AssertEquals("Kundalahalli", CreateDataProvider().ThirdPartyAddress1);
	}

	public override void TestThirdPartyAddress2()
	{
		AssertEquals("Brookefield", CreateDataProvider().ThirdPartyAddress2);
	}

	public override void TestThirdPartyCity()
	{
		AssertEquals("Bengaluru", CreateDataProvider().ThirdPartyCity);
	}

	public override void TestThirdPartyCountryCode()
	{
		AssertEquals("IN", CreateDataProvider().ThirdPartyCountryCode);
	}

	public override void TestThirdPartyCountrySubdivision()
	{
		AssertEquals("KA", CreateDataProvider().ThirdPartyCountrySubdivision);
	}

	public override void TestThirdPartyName()
	{
		AssertEquals("Brigade", CreateDataProvider().ThirdPartyName);
	}

	public override void TestThirdPartyPin()
	{
		AssertEquals("560036", CreateDataProvider().ThirdPartyPin);
	}

	protected override TableInvoiceDataProviderAbstractClass CreateDataProvider()
	{
		return ExportSbCACHE01DataProvider.CreateProvider(header, AdditionalDataProvider).Sb.TableInvoice.First();
	}

	ExportSbCACHE01AdditionalDataProvider AdditionalDataProvider => new ExportSbCACHE01AdditionalDataProvider(messageSendingObject);

	protected override void SetUp()
	{
		base.SetUp();

		var entryInstruction = Factory.New<CusEntryInstruction>();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.CustomsEntryHeaders.Add(header);
		header.CH_CEI_Instruction = entryInstruction.PK;
		entryLine = header.MergedLines.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.JobComInvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;

		invoice.JZ_OA_ExporterAddress = GetSampleOrgAddress().PK;
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
	CusEntryLine entryLine;

	OrgAddress GetSampleOrgAddress()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "Brigade";
		var orgAddress = orgHeader.MainAddress;
		orgAddress.OA_Address1 = "Kundalahalli";
		orgAddress.OA_Address2 = "Brookefield";
		orgAddress.OA_City = "Bengaluru";
		orgAddress.OA_RN_NKCountryCode = "IN";
		orgAddress.OA_State = "KA";
		orgAddress.OA_PostCode = "560036";

		return orgAddress;
	}
}
