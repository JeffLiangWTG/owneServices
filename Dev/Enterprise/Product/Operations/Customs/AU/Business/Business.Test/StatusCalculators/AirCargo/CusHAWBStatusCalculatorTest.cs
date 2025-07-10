using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.AU.CMR;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusHAWBStatusCalculator))]
	sealed class CusHAWBStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestStatusInfo()
		{
			var calculator = GetNewBusinessObject() as CusHAWBStatusCalculator;
			AssertEquals("StatusInfo.Name", "CS_CustomsStatus", calculator.StatusInfo.Name);
		}

		public void TestInterestedMessageTypes()
		{
			var calculator = GetNewBusinessObject() as CusHAWBStatusCalculator;
			AssertEquals("InterestedMessageTypes.Length", 1, calculator.InterestedMessageTypes.Length);
			AssertEquals("InterestedMessageTypes[0]", CMRMessage.CMRMessageTypes.CARST, calculator.InterestedMessageTypes[0]);
		}

		public void TestDefaultValues()
		{
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			var underbond = mawb.Underbonds.AddNew();
			var outturn = underbond.Outturns.AddNew();
			var hawb = mawb.ChildBills.AddNew();
			outturn.C5_ParentID = hawb.PK;
			outturn.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusConsignment;
			hawb.CS_CustomsStatus = "HLD";
			hawb.Calculator.DeriveStatusNow();
			hawb.MessageStatusCalculator.DeriveStatusNow();
			AssertEquals("HLD", hawb.CS_CustomsStatus);
			AssertEquals(CMRBaseStatuses.Codes.NotSent, hawb.CS_MsgStatus);
		}

		protected override BusinessObject GetNewBusinessObject() => new CusHAWBStatusCalculator(Factory.New<CusHAWB>());
	}
}
