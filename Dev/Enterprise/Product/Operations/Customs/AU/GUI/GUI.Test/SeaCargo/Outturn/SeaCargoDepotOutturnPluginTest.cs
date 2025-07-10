using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	sealed class SeaCargoDepotOutturnPluginTest : ZPlugInGenericTest
	{
		public void TestBusinessEntityForPlugin()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertNull(plugin.outturn);
			AssertEquals(false, container.IsLinkedToCustomsForTest);
			var outturn = GetCorrectOutturn(GetCorrectHeader());
			AssertEquals(true, plugin.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal());
			Assert(plugin.MutexForOutturn.IsLocked);
			Assert(plugin.PlugInNotDisplayedMessage.IsEmpty);
			AssertEquals(outturn.Header, plugin.BusinessEntity);
			AssertEquals(true, container.IsLinkedToCustomsForTest);
			AssertNotNull(plugin.outturn);

			AssertNoExceptionThrown(() => plugin.ShowPreSaveDialogsCore());
		}

		public void TestIsActiveWithLinkedOutturn()
		{
			var header = Factory.New<TallyOutturnHeader>();
			var outturn = header.Outturns.AddNew();
			CFSTallyContainerWrapper.Load(container).Outturns.Add(outturn);
			using (var plugin = new SeaCargoDepotOutturnPluginForTest(container))
			{
				AssertEquals(true, plugin.IsActiveInternal);
				AssertEquals(true, container.IsLinkedToCustomsForTest);
				AssertNoExceptionThrown(() => plugin.ShowPreSaveDialogsCore());
			}
		}

		public void TestIsActiveWithNoLinkedOutturn()
		{
			using (var plugin = new SeaCargoDepotOutturnPluginForTest(container))
			{
				AssertEquals(false, plugin.IsActiveInternal);
				AssertEquals(false, container.IsLinkedToCustomsForTest);
				AssertNoExceptionThrown(() => plugin.ShowPreSaveDialogsCore());
			}
		}

		public void TestRegisterPlugInBusinessEntityAsEditable()
		{
			AssertEquals(true, plugin.RegisterPlugInBusinessEntityAsEditableInternal);
		}

		public void TestBusinessEntityForPlugin_BusinessEntityDeleted()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertNull(plugin.outturn);
			AssertEquals(false, container.IsLinkedToCustomsForTest);
			var outturn = GetCorrectOutturn(GetCorrectHeader());

			AssertEquals(true, plugin.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal());

			plugin.wrapper.GetOutturnForTallyPlugin(plugin).Delete();
			Factory.Save();

			AssertNoExceptionThrown(() => plugin.QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal());
		}

		public void TestShouldCreateAndLinkWhenNoMatchesFound()
		{
			const string message = "No existing outturns have been found.\r\nDo you wish to create a new outturn and link it to this container?";
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertEquals(true, findOrCreateOutturnUI.ShouldCreateAndLinkWhenNoMatchesFound());
			Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertEquals(false, findOrCreateOutturnUI.ShouldCreateAndLinkWhenNoMatchesFound());
			Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShouldCreateAndLinkWhenOnlyHeaderFound()
		{
			const string message = "No existing outturn lines have been found, but a matching outturn header has been found.\r\nDo you wish to create a new outturn line under this header and link it to this container?";
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertEquals(true, findOrCreateOutturnUI.ShouldCreateAndLinkWhenOnlyHeaderFound());
			Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertEquals(false, findOrCreateOutturnUI.ShouldCreateAndLinkWhenOnlyHeaderFound());
			Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShouldLinkWhenOutturnFound()
		{
			const string message = "No outturn is linked to this container, but a matching outturn line has been found.\r\nDo you wish to link this outturn to this container?";
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertEquals(true, findOrCreateOutturnUI.ShouldLinkWhenOutturnFound());
			Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertEquals(false, findOrCreateOutturnUI.ShouldLinkWhenOutturnFound());
			Assert(UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShowMultipleMatchesError()
		{
			const string message = "No outturn is linked to this container, but multiple possible matches were found so automatic matching cannot work.\r\nPlease manually attach an outturn line to this container.";
			findOrCreateOutturnUI.ShowMultipleMatchesError();
			Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShowNoConsolError()
		{
			const string message = "No outturn is linked to this container, and automatic matching cannot be attempted because there is no consol attached.\r\nPlease attach a consol to this container if you want to attempt automatic matching.";
			findOrCreateOutturnUI.ShowNoConsolError();
			Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShowOutturnAlreadyLinked()
		{
			const string message = "No outturn is linked to this container, an automatic match was attempted but the outturn that was found is already linked to another container.\r\nPlease reopen form or manually attach or reattach the outturn lines.";
			findOrCreateOutturnUI.ShowOutturnAlreadyLinked();
			Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShowShipmentOutturnsAlreadyLinked()
		{
			const string message = "7 of the shipment outturn lines were found to be already linked against other shipment, and weren't attached.\r\nPlease manually reattach these outturn lines.";
			findOrCreateOutturnUI.ShowShipmentOutturnsAlreadyLinked(7);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasWarning);
			AssertEquals(message, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestName()
		{
			AssertEquals("Sea Cargo Outturn", plugin.Name);
		}

		[ExpectNoExceptions()]
		public void TestDeletingOutturn()
		{
			var header = Factory.New<TallyOutturnHeader>();
			var outturn = header.Outturns.AddNew();
			CFSTallyContainerWrapper.Load(container).Outturns.Add(outturn);
			outturn.Delete();
			plugin.ShowPreSaveDialogs();
		}

		CFSLoadListConsol consol;
		TallyContainer container;
		SeaCargoDepotOutturnPluginForTest plugin;
		protected override void SetUp()
		{
			base.SetUp();
			container = Factory.New<TallyContainer>();
			GlbCompany.CurrentCompany.OrgProxy.MainAddress.LocalControlledPremisesID = "9495C";
			consol = Factory.New<CFSLoadListConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.MainTransport.JW_Vessel = "ADMIRALENGRACHT";
			consol.MainTransport.JW_RL_NKDiscPort = "AUSYD";
			consol.MainTransport.JW_RL_NKLoadPort = "NZAKL";
			consol.MainTransport.JW_VoyageFlight = "54321S";
			consol.Containers.Add(container);
			container.JC_ContainerNum = "BBBB2222227";
			plugin = new SeaCargoDepotOutturnPluginForTest(container);
		}

		protected override void TearDown()
		{
			base.TearDown();
			plugin?.Dispose();
		}

		protected override ZPlugIn GetPlugInToTest()
		{
			var header = Factory.New<TallyOutturnHeader>();
			var outturn = header.Outturns.AddNew();
			CFSTallyContainerWrapper.Load(container).Outturns.Add(outturn);
			return new SeaCargoDepotOutturnPluginForTest(container);
		}

		IFindOrCreateOutturnUI findOrCreateOutturnUI => plugin;

		TallyOutturnHeader GetCorrectHeader()
		{
			var header = Factory.New<TallyOutturnHeader>();
			header.C6_VesselName = "ADMIRALENGRACHT";
			header.C6_OutturningPremiseID = "9495C";
			header.C6_VoyageNum = "54321S";
			return header;
		}

		TallyOutturn GetCorrectOutturn(TallyOutturnHeader header)
		{
			var outturn = header.Outturns.AddNew();
			outturn.C5_CargoType = CMRImportCargoTypes.Codes.FullContainerLoad;
			outturn.C5_ContainerNumber = "BBBB2222227";
			return outturn;
		}

		sealed class SeaCargoDepotOutturnPluginForTest : SeaCargoDepotOutturnPlugin
		{
			public SeaCargoDepotOutturnPluginForTest(TallyContainer hostBusinessEntity)
				: base(hostBusinessEntity)
			{
			}

			internal ZBool IsActiveInternal => IsActive;

			internal bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreatedInternal() => QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();

			internal bool RegisterPlugInBusinessEntityAsEditableInternal => RegisterPlugInBusinessEntityAsEditable;
		}
	}
}
