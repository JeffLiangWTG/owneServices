using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.CFS.GUI;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SCDTallyPlugInTest : TestCaseWithFactory
	{
		public void TestLoadZPlugIn()
		{
			using (SCDTallyPlugIn testPlugIn = new SCDTallyPlugIn(container))
			{
				ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPlugIn.OnUserControlShown();
				AssertNotNull(testPlugIn.BusinessEntity);
			}
		}

		[TestDateIncremental(0, 0, 0, 1)]
		public void TestCanDelete()
		{
			using (SCDTallyPlugIn testPlugIn = new SCDTallyPlugIn(container))
			{
				testPlugIn.OnUserControlShown();
				SeaCargoDepotTally sCDTally = (SeaCargoDepotTally)testPlugIn.BusinessEntity;
				AssertEquals("Can Delete is true if Customs messaging is not active or has been cancelled", true, testPlugIn.CanDelete);
				sCDTally.Container.Logs.AddNew(AutoEvents.SeaCargoDepotEvent, DepotEvents.ImpendingCargo);
				AssertEquals("Can Delete should be false if Customs Messaging is active", false, testPlugIn.CanDelete);
				AssertNotNull(testPlugIn.BusinessEntity);
				sCDTally.Container.Logs.AddNew(AutoEvents.SeaCargoDepotEvent, DepotEvents.ImpendingCargoCancelled);
				AssertEquals("Can Delete is true if Customs messaging is not active or has been cancelled", true, testPlugIn.CanDelete);
			}
		}

		[ExpectNoExceptions]
		public void TestCoLoadWizardClick()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			TallyContainer testContainer = Factory.New<TallyContainer>();
			testContainer.JC_ContainerNum = "HGFU39839030";
			using (ManifestTallyForm testForm = new ManifestTallyForm(testContainer))
			{
				testForm.Show();
				UserIdleWorker.Flush();
				SCDTallyPlugIn testPlugIn = null;
				foreach (ZPlugIn aPlugIn in testForm.PlugIns.Instances)
				{
					if (aPlugIn is SCDTallyPlugIn)
					{
						testPlugIn = (SCDTallyPlugIn)aPlugIn;
					}
				}

				testPlugIn.OnUserControlShown();
				testPlugIn.CoLoadShipmentWizardMenuItem.PerformClick();
			}
		}

		public void TestForceLegacy()
		{
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
			CusUnderbond underbond = Factory.New<CusUnderbond>();
			underbond.C4_ParentTableCode = JobContainerSchema.Constants.Prefix;
			underbond.C4_ParentID = container.PK;
			using (SCDTallyPlugIn testPlugIn = new SCDTallyPlugIn(container))
			{
				AssertEquals("Force CMR should be true", true, testPlugIn.ForceCMR);
			}
		}

		TallyContainer container;
		protected override void SetUp()
		{
			base.SetUp();
			PackUnpackLoadListConsol consol = Factory.New<PackUnpackLoadListConsol>();
			container = consol.Containers.AddNew();
		}
	}
}
