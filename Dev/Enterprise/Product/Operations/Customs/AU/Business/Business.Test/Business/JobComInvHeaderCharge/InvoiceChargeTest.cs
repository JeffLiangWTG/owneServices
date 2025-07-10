using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(InvoiceCharge))]
	public class InvoiceChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPrepaidCollectForUAF()
		{
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedAtFactory;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceAmount = 1000;

			BaseJobComInvHeaderCharge pC = invoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100);
			BaseJobComInvHeaderCharge fIFT = invoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100);

			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100, JobDeclaration.LocalCurrencyConstantCode);
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100, JobDeclaration.LocalCurrencyConstantCode);
			testDec.ResumeApportionment();
			AssertEquals("PC is collect", Core.Constants.PaymentType.Collect, pC.J7_PrepaidCollect);
			AssertEquals("FIFT is collect", Core.Constants.PaymentType.Collect, fIFT.J7_PrepaidCollect);
			AssertEquals("OFT is collect", Core.Constants.PaymentType.Collect, invoice.GroupCharges[0].J7_PrepaidCollect);
			AssertEquals("ONS is collect", Core.Constants.PaymentType.Collect, invoice.GroupCharges[1].J7_PrepaidCollect);
		}

		public void TestPrepaidCollectForUFB()
		{
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedFreeOnBoard;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceAmount = 1000;

			BaseJobComInvHeaderCharge pC = invoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100);
			BaseJobComInvHeaderCharge fIFT = invoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100);

			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100, JobDeclaration.LocalCurrencyConstantCode);
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100, JobDeclaration.LocalCurrencyConstantCode);
			testDec.ResumeApportionment();
			AssertEquals("PC is collect", Core.Constants.PaymentType.Collect, pC.J7_PrepaidCollect);
			AssertEquals("FIFT is prepaid", Core.Constants.PaymentType.Prepaid, fIFT.J7_PrepaidCollect);
			AssertEquals("OFT is collect", Core.Constants.PaymentType.Collect, invoice.GroupCharges[0].J7_PrepaidCollect);
			AssertEquals("ONS is collect", Core.Constants.PaymentType.Collect, invoice.GroupCharges[1].J7_PrepaidCollect);
		}

		public void TestPrepaidCollectForUCF()
		{
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostAndFreight;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceAmount = 1000;

			BaseJobComInvHeaderCharge pC = invoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100);
			BaseJobComInvHeaderCharge fIFT = invoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100);

			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100, JobDeclaration.LocalCurrencyConstantCode);
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100, JobDeclaration.LocalCurrencyConstantCode);
			testDec.ResumeApportionment();
			AssertEquals("PC is collect", Core.Constants.PaymentType.Collect, pC.J7_PrepaidCollect);
			AssertEquals("FIFT is prepaid", Core.Constants.PaymentType.Prepaid, fIFT.J7_PrepaidCollect);
			invoice.GroupCharges.Sort("J7_PrepaidCollect");
			AssertEquals("OFT is prepaid", Core.Constants.PaymentType.Prepaid, invoice.GroupCharges[1].J7_PrepaidCollect);
			AssertEquals("ONS is collect", Core.Constants.PaymentType.Collect, invoice.GroupCharges[0].J7_PrepaidCollect);
		}

		public void TestPrepaidCollectForUCI()
		{
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.UnpackedCostInsuranceAndFreight;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceAmount = 1000;

			BaseJobComInvHeaderCharge pC = invoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100);
			BaseJobComInvHeaderCharge fIFT = invoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100);

			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100, JobDeclaration.LocalCurrencyConstantCode);
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100, JobDeclaration.LocalCurrencyConstantCode);
			testDec.ResumeApportionment();
			AssertEquals("PC is collect", Core.Constants.PaymentType.Collect, pC.J7_PrepaidCollect);
			AssertEquals("FIFT is prepaid", Core.Constants.PaymentType.Prepaid, fIFT.J7_PrepaidCollect);
			AssertEquals("OFT is prepaid", Core.Constants.PaymentType.Prepaid, invoice.GroupCharges[0].J7_PrepaidCollect);
			AssertEquals("ONS is prepaid", Core.Constants.PaymentType.Prepaid, invoice.GroupCharges[1].J7_PrepaidCollect);
		}

		public void TestPrepaidCollectForC_I()
		{
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.CostAndInsurance;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			invoice.JZ_InvoiceAmount = 1000;

			BaseJobComInvHeaderCharge pC = invoice.Charges.AddNew(AUChargeCodeList.Codes.PackingCost, 100);
			BaseJobComInvHeaderCharge fIFT = invoice.Charges.AddNew(AUChargeCodeList.Codes.ForeignInlandFreight, 100);

			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasFreight, 100, JobDeclaration.LocalCurrencyConstantCode);
			groupHeader.Charges.AddNew(AUChargeCodeList.Codes.OverseasInsurance, 100, JobDeclaration.LocalCurrencyConstantCode);
			testDec.ResumeApportionment();
			AssertEquals("PC is prepaid", Core.Constants.PaymentType.Prepaid, pC.J7_PrepaidCollect);
			AssertEquals("FIFT is prepaid", Core.Constants.PaymentType.Prepaid, fIFT.J7_PrepaidCollect);
			AssertEquals("OFT is collect", Core.Constants.PaymentType.Collect, invoice.GroupCharges[0].J7_PrepaidCollect);
			AssertEquals("ONS is prepaid", Core.Constants.PaymentType.Prepaid, invoice.GroupCharges[1].J7_PrepaidCollect);
		}

		public void TestDefaultForOverseasFreight()
		{
			AssertDefaultFlagsForInvoiceCharge(AUChargeCodeList.Codes.OverseasFreight, false, true);
		}

		public void TestDefaultForOverseasInsurance()
		{
			AssertDefaultFlagsForInvoiceCharge(AUChargeCodeList.Codes.OverseasInsurance, false, true);
		}

		public void TestDefaultForBuyingCommission()
		{
			AssertDefaultFlagsForInvoiceCharge(AUChargeCodeList.Codes.BuyingCommission, true, true);
		}

		public void TestDefaultForOtherCommission()
		{
			AssertDefaultFlagsForInvoiceCharge(AUChargeCodeList.Codes.OtherCommission, true, true);
		}

		public void TestDefaultForExWorks()
		{
			AssertDefaultFlagsForInvoiceCharge(AUChargeCodeList.Codes.ExWorks, true, true);
		}

		public void TestDefaultForPackingCost()
		{
			AssertDefaultFlagsForInvoiceCharge(AUChargeCodeList.Codes.PackingCost, true, true);
		}

		public void TestDefaultForLandingCharges()
		{
			AssertDefaultFlagsForInvoiceCharge(AUChargeCodeList.Codes.LandingCharges, false, false);
		}

		public void TestDefaultForAddtionCharges()
		{
			AssertDefaultFlagsForInvoiceCharge(AUChargeCodeList.Codes.AdditionCharge, true, true);
		}

		public void TestDefaultForDeductionCharges()
		{
			AssertDefaultFlagsForInvoiceCharge(AUChargeCodeList.Codes.DeductionCharge, false, false);
		}

		public void TestDefaultForDiscount()
		{
			AssertDefaultFlagsForInvoiceCharge(AUChargeCodeList.Codes.Discount, false, false);
		}

		public void TestDefaultForForeignInlandFreight()
		{
			AssertDefaultFlagsForInvoiceCharge(AUChargeCodeList.Codes.ForeignInlandFreight, true, true);
		}

		#region Implementation

		JobDeclaration testDec;
		JobComInvoiceGroupHeader groupHeader;
		JobComInvoiceHeader invoice;
		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			groupHeader = testDec.JobComInvoiceGroupHeaders[0];
			invoice = groupHeader.JobComInvoiceHeaders.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			SetUp();
			return invoice.Charges.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected void AssertDefaultFlagsForInvoiceCharge(string chargeName, bool isDutiable, bool isGSTApplicable)
		{
			BaseJobComInvHeaderCharge charge = invoice.Charges.AddNew(chargeName);
			AssertEquals("Dutiable", isDutiable, charge.J7_IsDutiable);
			AssertEquals("GST", isGSTApplicable, charge.J7_IsGSTApplicable);
		}

		#endregion
	}
}
