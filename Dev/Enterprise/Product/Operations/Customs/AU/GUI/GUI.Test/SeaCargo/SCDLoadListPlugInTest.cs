using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SCDLoadListPlugInTest : SCDPlugInTest
	{
		public void TestLoadZPlugIn()
		{
			CFSLoadListConsol loadList = GetImportLCLConsol();
			using (SCDLoadListPlugIn testPlugIn = new SCDLoadListPlugIn(loadList))
			{
				ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPlugIn.OnUserControlShown();
				AssertNotNull(testPlugIn.BusinessEntity);
			}
		}

		public void TestCanDelete()
		{
			CFSLoadListConsol loadList = GetImportLCLConsol();
			using (SCDLoadListPlugIn testPlugIn = new SCDLoadListPlugIn(loadList))
			{
				AssertEquals("Can Delete Plug In", true, testPlugIn.CanDelete);
				loadList.Delete();
				AssertEquals("Load List Deleted", true, loadList.IsDeleted);
			}
		}

		public void TestForceLegacy()
		{
			CFSLoadListConsol loadList = GetImportLCLConsol();
			CFSContainer container = loadList.Containers.AddNew();
			EDIMessage newMessage = Factory.New<EDIMessage>();
			newMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.SeaCargo;
			newMessage.EM_LinkedObject = container;
			using (SCDContainerPlugIn testPlugIn = new SCDContainerPlugIn(container))
			{
				AssertEquals("Force CMR should be true", true, testPlugIn.ForceLegacy);
			}
		}

		public void TestForceCMR()
		{
			CFSLoadListConsol loadList = GetImportLCLConsol();
			CFSContainer container = loadList.Containers.AddNew();
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_ParentTableCode = JobContainerSchema.Constants.Prefix;
			underbond.C4_ParentID = container.PK;
			using (SCDLoadListPlugIn testPlugIn = new SCDLoadListPlugIn(loadList))
			{
				AssertEquals("Force CMR should be true", true, testPlugIn.ForceCMR);
			}
		}

		public void TestIsActive()
		{
			CFSLoadListConsol consol = Factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			using (SCDLoadListPlugInForTest plugIn = new SCDLoadListPlugInForTest(consol))
			{
				AssertEquals("Should Show Plug In", false, plugIn.IsActiveForTesting());
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("Should Show Plug In", true, plugIn.IsActiveForTesting());
			}
		}

		protected override SCDPlugIn GetPlugIn()
		{
			var loadList = GetImportLCLConsol();
			return new SCDLoadListPlugIn(loadList);
		}

		sealed class SCDLoadListPlugInForTest : SCDLoadListPlugIn
		{
			public SCDLoadListPlugInForTest(IBusiness hostEntity) : base(hostEntity)
			{
			}

			public bool IsActiveForTesting() => ShouldPlugInGUIAndBusinessEntityBeCreated();
		}
	}
}
