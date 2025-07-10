using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ForeignOperatorMultiMessageManagerTest : TestCaseWithFactory
	{
		public void TestGetAllMessageManagers()
		{
			var foreignOperator = Factory.New<CusBRForeignOperator>();
			foreignOperator.BFR_MessageStatus = BRMessageStatusList.Codes.NotSent;

			var messageSendingObject = new ForeignOperatorMessageSendingObject(foreignOperator);
			var manager = new ForeignOperatorMultiMessageManagerForTesting(messageSendingObject);
			AssertSame("The TopLevelBizObjToManage should be the foreignOperator", foreignOperator, manager.TopLevelBizObjToManage);

			var sendingObject = manager.GetAllMessageManagers_Exposed().Single();
			AssertType<ForeignOperatorMessageManager>("A ForeignOperatorMessageManager should been created for sending object", sendingObject);
		}

		#region Implementation

		class ForeignOperatorMultiMessageManagerForTesting : ForeignOperatorMultiMessageManager
		{
			public ForeignOperatorMultiMessageManagerForTesting(ForeignOperatorMessageSendingObject messageSendingObjectParent) : base(messageSendingObjectParent)
			{
			}

			public SingleMessageManager[] GetAllMessageManagers_Exposed() => GetAllMessageManagers();
		}

		#endregion
	}
}
