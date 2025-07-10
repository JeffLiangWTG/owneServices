using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(ChargeWithAppendedDescription))]
	public class ChargeWithAppendedDescriptionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestReadOnlyProperties()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var job = objectCreator.CreateJob("J0001", objectCreator.LocalClient, 1.0M, objectCreator.Agent, 1.0M);
			var charge1 = objectCreator.CreateCharge(job, objectCreator.CC1, 20M, 25M);
			charge1.JR_Desc = objectCreator.CC1.AC_Desc;

			var wrappedBizO = new ChargeWithAppendedDescription(charge1, Factory);
			AssertEquals("ChargePK", charge1.PK, wrappedBizO.ChargePK);
			AssertEquals("OriginalDescription", charge1.JR_Desc, wrappedBizO.OriginalDescription);
			AssertEquals("JR_JH", charge1.JR_JH, wrappedBizO.JR_JH);
			AssertEquals("JR_GB", charge1.JR_GB, wrappedBizO.JR_GB);
			AssertEquals("JR_GE", charge1.JR_GE, wrappedBizO.JR_GE);
			AssertEquals("JR_RX_NKSellCurrency", charge1.JR_RX_NKSellCurrency, wrappedBizO.JR_RX_NKSellCurrency);
			AssertEquals("JR_OSSellAmt", charge1.JR_OSSellAmt, wrappedBizO.JR_OSSellAmt);
			AssertEquals("JR_LocalSellAmt", charge1.JR_LocalSellAmt, wrappedBizO.JR_LocalSellAmt);
			AssertEquals("JR_AT_SellGSTRate", charge1.JR_AT_SellGSTRate, wrappedBizO.JR_AT_SellGSTRate);
			AssertEquals("JR_OSSellGSTAmt", charge1.JR_OSSellGSTAmt_Calc, wrappedBizO.JR_OSSellGSTAmt_Calc);
			AssertEquals("JR_InvoiceType", charge1.JR_InvoiceType, wrappedBizO.JR_InvoiceType);
			AssertEquals("ChargeType", charge1.ChargeType, wrappedBizO.ChargeType);
			AssertEquals("JR_RX_NKCostCurrency", charge1.JR_RX_NKCostCurrency, wrappedBizO.JR_RX_NKCostCurrency);
			AssertEquals("JR_OSCostAmt", charge1.JR_OSCostAmt, wrappedBizO.JR_OSCostAmt);
			AssertEquals("JR_LocalCostAmt", charge1.JR_LocalCostAmt, wrappedBizO.JR_LocalCostAmt);
			AssertEquals("JR_AT_CostGSTRate", charge1.JR_AT_CostGSTRate, wrappedBizO.JR_AT_CostGSTRate);
			AssertEquals("JR_OSCostGSTAmt", charge1.JR_OSCostGSTAmt_Calc, wrappedBizO.JR_OSCostGSTAmt);
		}

		public void TestBothChargeAndWrappedChargeObjectsHaveSameLookupList()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var job = objectCreator.CreateJob("J0001", objectCreator.LocalClient, 1.0M, objectCreator.Agent, 1.0M);
			var charge = objectCreator.CreateCharge(job, objectCreator.CC1, 20M, 25M);
			var wrappedCharge = new ChargeWithAppendedDescription(charge, Factory);

			AssertEquals("Must have the same Lookup", charge.Lookups, wrappedCharge.Lookups);
			AssertEquals("Must have the same GSTCollection", charge.GSTCollection, wrappedCharge.GSTCollection);
		}

		public void TestLastTextToAppendAfterSaved()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var job = objectCreator.CreateJob("J0001", objectCreator.LocalClient, 1.0M, objectCreator.Agent, 1.0M);
			var charge = objectCreator.CreateCharge(job, objectCreator.CC1, 20M, 25M);
			var wrappedCharge = new ChargeWithAppendedDescription(charge, Factory);
			AssertEquals(string.Empty, wrappedCharge.LastTextToAppend_ForTestOnly);
			wrappedCharge.TextToAppend = "CHARGE";
			wrappedCharge.AppendText();
			AssertEquals("CHARGE", wrappedCharge.LastTextToAppend_ForTestOnly);

			Factory.Save();

			AssertEquals(string.Empty, wrappedCharge.LastTextToAppend_ForTestOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var charge = Factory.New<Charge>();
			var bizo = new ChargeWithAppendedDescription(charge, Factory);
			return bizo;
		}
	}
}
