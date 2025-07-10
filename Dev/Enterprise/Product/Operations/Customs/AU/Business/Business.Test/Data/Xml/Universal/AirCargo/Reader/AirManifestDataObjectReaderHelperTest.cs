using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AirManifestDataObjectReaderHelperTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestSendWithdrawalMessageBeforeDeletingBillIfNecessary()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_HAWB = "HB1";
			hawb1.CS_MsgStatus = Common.AU.CMR.CMRBaseStatuses.Codes.WithdrawalAccepted;
			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "HB2";
			var hawb3 = mawb.ChildBills.AddNew();
			hawb3.CS_HAWB = "HB3";
			hawb3.CS_MsgStatus = Common.AU.CMR.CMRBaseStatuses.Codes.NotSent;
			var hawb4 = mawb.ChildBills.AddNew();
			hawb4.CS_HAWB = "HB4";
			hawb4.CS_MsgStatus = Common.AU.CMR.CMRBaseStatuses.Codes.OriginalAccepted;

			var helper = new AirManifestDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Australia);
			helper.MarkUnprocessedExistingBillsFor(mawb);

			var logger = new TestErrorLogger();
			helper.DeleteUnprocessedBillsFor(mawb, logger);

			foreach (var hawb in new[] { hawb1, hawb2, hawb3 })
			{
				AssertEquals("IsDeleted", true, hawb.IsDeleted);
				Assert(!hawb.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.AuthorisationWithdrawn.Code).Any());
			}
			AssertEquals("IsDeleted", false, hawb4.IsDeleted);
			Assert(hawb4.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.AuthorisationWithdrawn.Code).Any());
		}
	}
}
