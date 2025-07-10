using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DrawbackMultiMessageManagerTest : TestCaseWithFactory
	{
		public void TestAllMessageManagers()
		{
			var allMessageManagers = Manager.GetAllMessageManagers();
			AssertEquals("AllMessageManagers.Length", 1, allMessageManagers.Length);
			AssertEquals(typeof(DrawbackMessageManager), allMessageManagers[0].GetType());
		}

		public void TestAllMessageManagersWithNull()
		{
			var allMessageManagers = new DrawbackMultiMessageManagerForTest(null, CMRMessage.MessageSubTypes.Original).GetAllMessageManagers();
			AssertEquals("AllMessageManagers.Length", 0, allMessageManagers.Length);
		}

		public void TestSendWheneverPossibleOnceMessagingActive()
		{
			AssertEquals("SendWheneverPossibleOnceMessagingActive", true, Manager.SendWheneverPossibleOnceMessagingActive);
		}

		DrawbackMultiMessageManagerForTest Manager
		{
			get
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Drawback;
				declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				return new DrawbackMultiMessageManagerForTest(declaration, CMRMessage.MessageSubTypes.Original);
			}
		}

		sealed class DrawbackMultiMessageManagerForTest : DrawbackMultiMessageManager
		{
			public DrawbackMultiMessageManagerForTest(JobDeclaration declaration, string messageSubType) : base(declaration, messageSubType)
			{
			}

			internal new SingleMessageManager[] GetAllMessageManagers() => base.GetAllMessageManagers();

			internal new bool SendWheneverPossibleOnceMessagingActive => base.SendWheneverPossibleOnceMessagingActive;
		}
	}
}
