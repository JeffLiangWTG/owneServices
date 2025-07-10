using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing
{
	public class CrossTradeDebtorConfigurationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void  TestChargePaymentTypeList()
		{
			AssertEquals(3, ChargePaymentTypeList.Count);
			Assert(ChargePaymentTypeList.ContainsCode("ALL"));
			Assert(ChargePaymentTypeList.ContainsCode("PPD"));
			Assert(ChargePaymentTypeList.ContainsCode("CCX"));
			AssertEquals("Prepaid", ChargePaymentTypeList["PPD"].Description);
			AssertEquals("Collect", ChargePaymentTypeList["CCX"].Description);
			AssertEquals("Both Prepaid and Collect", ChargePaymentTypeList["ALL"].Description);
		}

		public void TestDebtorOptionList()
		{
			AssertEquals(4, DebtorOptionList.Count);
			Assert(DebtorOptionList.ContainsCode("PBP"));
			Assert(DebtorOptionList.ContainsCode("CBP"));
			Assert(DebtorOptionList.ContainsCode("CCP"));
			Assert(DebtorOptionList.ContainsCode("CCC"));
			AssertEquals("Prepaid Bill-To Party", DebtorOptionList["PBP"].Description);
			AssertEquals("Collect Bill-To Party", DebtorOptionList["CBP"].Description);
			AssertEquals("Job's Controlling Customer falling back to Prepaid Bill-To Party", DebtorOptionList["CCP"].Description);
			AssertEquals("Job's Controlling Customer falling back to Collect Bill-To Party", DebtorOptionList["CCC"].Description);
		}

		public void TestCrossTradeJobTypeList()
		{
			AssertEquals(3, JobTypeList.Count);
			Assert(JobTypeList.ContainsCode("ALL"));
			Assert(JobTypeList.ContainsCode("SHP"));
			Assert(JobTypeList.ContainsCode("QSH"));
			AssertEquals("Shipment, Quick Booking", JobTypeList["ALL"].Description);
			AssertEquals("Shipment", JobTypeList["SHP"].Description);
			AssertEquals("Quick Booking", JobTypeList["QSH"].Description);
		}

		#region Implementation
		CrossTradeDebtorConfiguration BizObj
		{
			get { return new CrossTradeDebtorConfiguration(); }
		}

		CodeDescriptionPairList ChargePaymentTypeList
		{
			get { return fChargePaymentTypeList ?? (fChargePaymentTypeList = BizObj.CrossTradeConfigurationLookUp.ChargePaymentTypeList); }
		}

		CodeDescriptionPairList fChargePaymentTypeList;

		CodeDescriptionPairList DebtorOptionList
		{
			get { return fDebtorOptionList ?? (fDebtorOptionList = BizObj.CrossTradeConfigurationLookUp.DebtorOptionList); }
		}

		CodeDescriptionPairList fDebtorOptionList;

		CodeDescriptionPairList JobTypeList
		{
			get { return fJobTypeList ?? (fJobTypeList = BizObj.CrossTradeConfigurationLookUp.JobTypeList); }
		}

		CodeDescriptionPairList fJobTypeList;

		#endregion
	}
}
