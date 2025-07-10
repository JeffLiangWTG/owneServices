using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using CusUnderbond = Enterprise.Customs.AU.Declaration.Business.CusUnderbond;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SCDContainerPlugInTest : SCDPlugInTest
	{
		public void TestLoadZPlugIn()
		{
			using (var testPlugIn = new SCDContainerPlugIn(container))
			{
				ZArchitecture.Environment.UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testPlugIn.OnUserControlShown();
				AssertNotNull(testPlugIn.BusinessEntity);
			}
		}

		[TestDateIncremental(0, 0, 0, 1)]
		public void TestCanDelete()
		{
			using (var testPlugIn = new SCDContainerPlugIn(container))
			{
				testPlugIn.OnUserControlShown();
				var sCDContainer = testPlugIn.SCDContainer;
				AssertEquals("Can Delete is true if Customs messaging is not active or has been cancelled", true, testPlugIn.CanDelete);
				sCDContainer.Container.Logs.AddNew(AutoEvents.SeaCargoDepotEvent, DepotEvents.ImpendingCargo);
				AssertEquals("Can Delete should be false if Customs Messaging is active", false, testPlugIn.CanDelete);
				AssertNotNull(testPlugIn.BusinessEntity);
				sCDContainer.Container.Logs.AddNew(AutoEvents.SeaCargoDepotEvent, DepotEvents.ImpendingCargoCancelled);
				AssertEquals("Can Delete is true if Customs messaging is not active or has been cancelled", true, testPlugIn.CanDelete);
			}
		}

		public void TestForceLegacy()
		{
			var newMessage = Factory.New<EDIMessage>();
			newMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.SeaCargo;
			newMessage.EM_LinkedObject = container;
			using (var testPlugIn = new SCDContainerPlugIn(container))
			{
				AssertEquals("Force CMR should be true", true, testPlugIn.ForceLegacy);
			}
		}

		public void TestForceCMR()
		{
			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_ParentTableCode = JobContainerSchema.Constants.Prefix;
			underbond.C4_ParentID = container.PK;
			using (var testPlugIn = new SCDContainerPlugIn(container))
			{
				AssertEquals("Force CMR should be true", true, testPlugIn.ForceCMR);
			}
		}

		public void TestIsActive()
		{
			var container = Factory.New<CFSContainer>();
			using (var plugIn = new SCDContainerPlugInForTesting(container))
			{
				AssertEquals("Should Show Plug In", false, plugIn.IsActiveForTesting());
			}
		}

		CFSContainer container;
		protected override void SetUp()
		{
			base.SetUp();
			var consol = Factory.New<CFSLoadListConsol>();
			container = consol.Containers.AddNew();
		}

		protected override SCDPlugIn GetPlugIn() => new SCDContainerPlugIn(container);

		sealed class SCDContainerPlugInForTesting : SCDContainerPlugIn
		{
			public SCDContainerPlugInForTesting(CFSContainer container) : base(container)
			{
			}

			public ZBool IsActiveForTesting() => ShouldPlugInGUIAndBusinessEntityBeCreated();
		}
	}
}
