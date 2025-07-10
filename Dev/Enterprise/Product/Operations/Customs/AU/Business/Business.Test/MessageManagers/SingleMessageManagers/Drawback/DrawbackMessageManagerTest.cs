using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DrawbackMessageManagerTest : TestCaseWithFactory
	{
		public void TestBusinessObject()
		{
			AssertEquals(Declaration, Manager.BusinessObject);
		}

		public void TestMessageFriendlyName()
		{
			Declaration.JE_DeclarationReference = "B12345678";
			AssertEquals("MessageFriendlyName", "Drawback: B12345678", Manager.MessageFriendlyName);
		}

		public void TestGetStatus()
		{
			SetStatus("123");
			AssertEquals("GetStatus()", "123", Manager.GetStatus());
		}

		void SetStatus(ZString status)
		{
			Declaration.JE_MessageStatus = status;
		}

		DrawbackMessageManager manger;
		DrawbackMessageManager Manager => manger ?? (manger = new DrawbackMessageManager(Declaration));

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
					declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				}
				return declaration;
			}
		}
	}
}
