using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(CusEntryHeaderCharges))]
	class CusEntryHeaderChargesTest : EU.Business.Declaration.Testing.CusEntryHeaderChargesTest
	{
		public void TestUserEnteredStashSource()
		{
			var charge = Factory.New<CusEntryHeaderCharges>();
			AssertType<CusEntryHeaderChargeUserEnteredStashSource>(charge.UserEnteredStashSource);
		}

		public void TestC1_ChargeAmount_ReadOnly()
		{
			var charge = Factory.New<CusEntryHeaderCharges>();
			charge.C1_RateOverrideReasonCode = ZString.Empty;
			Assert(charge.C1_ChargeAmountInfo.ReadOnly);
			charge.C1_RateOverrideReasonCode = "1";
			Assert(!charge.C1_ChargeAmountInfo.ReadOnly);
		}

		public void TestNationalFeeTypeCode()
		{
			var charge = Factory.New<CusEntryHeaderCharges>();
			AssertEquals("National Type", DataBoundResourceStrings.GetDataForProperty(charge.NationalFeeTypeCodeInfo).Caption);
		}

		public void TestTaxStatus()
		{
			var charge = Factory.New<CusEntryHeaderCharges>();
			AssertEquals("Tax Status", DataBoundResourceStrings.GetDataForProperty(charge.TaxStatusInfo).Caption);
		}

		public void TestC1_RateOverrideReasonCode_Caption()
		{
			var charge = Factory.New<CusEntryHeaderCharges>();
			AssertEquals("Action", DataBoundResourceStrings.GetDataForProperty(charge.C1_RateOverrideReasonCodeInfo).Caption);
		}

		public void TestC1_ChargeType_Caption()
		{
			var charge = Factory.New<CusEntryHeaderCharges>();
			AssertEquals("Fee Code", DataBoundResourceStrings.GetDataForProperty(charge.C1_ChargeTypeInfo).Caption);
		}

		public void TestC1_ChargeAmount_Caption()
		{
			var charge = Factory.New<CusEntryHeaderCharges>();
			AssertEquals("Amount", DataBoundResourceStrings.GetDataForProperty(charge.C1_ChargeAmountInfo).Caption);
		}

		public void TestC1_MethodOfPayment_Caption()
		{
			var charge = Factory.New<CusEntryHeaderCharges>();
			AssertEquals("Method Of Payment", DataBoundResourceStrings.GetDataForProperty(charge.C1_MethodOfPaymentInfo).Caption);
		}

		public override void TestIDocSADHLineTaxBoxSupporter()
		{
			var charge = (CusEntryHeaderCharges)GetNewBusinessObject();
			charge.C1_ChargeType = UniversalReferenceConstants.RefCusRateCodes.A00;
			charge.C1_MethodOfPayment = "1";
			charge.C1_ChargeAmount = 10m;
			charge.EntryHeader.Declaration.JE_PaymentMethod = "5";

			var docSADHLineTaxBoxSupporter = (IDocSADHLineTaxBoxSupporter)charge;
			CombineAssertions(() =>
			{
				AssertEquals("Type", UniversalReferenceConstants.RefCusRateCodes.A00, docSADHLineTaxBoxSupporter.Type);
				AssertEquals("AmountInDeclarationCurrency", "10", docSADHLineTaxBoxSupporter.AmountInDeclarationCurrency);
				AssertEquals("MethodOfPayment", "1", docSADHLineTaxBoxSupporter.MethodOfPayment);
				AssertEquals("DeclarationMethodOfPayment", "5", docSADHLineTaxBoxSupporter.DeclarationMethodOfPayment);
			});
		}

		public override void TestRoundC1_ChargeAmount()
		{
			var charge = (CusEntryHeaderCharges)GetNewBusinessObject();

			CombineAssertions(() =>
			{
				charge.C1_ChargeAmount = 10.5m;
				AssertEquals("10.5m", 10m, charge.C1_ChargeAmount);
				charge.C1_ChargeAmount = 10.51m;
				AssertEquals("10.51m", 11m, charge.C1_ChargeAmount);
				charge.C1_ChargeAmount = 0.4m;
				AssertEquals("o.4m", 1m, charge.C1_ChargeAmount);
			});
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var dec = Factory.New<JobDeclaration>();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			return entryHeader.Charges.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var dec = Factory.New<JobDeclaration>();
			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			return entryHeader.Charges.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var charge = (CusEntryHeaderCharges)GetNewBusinessObject();
			charge.C1_ChargeAmount = 10m;
			return charge;
		}
	}
}
