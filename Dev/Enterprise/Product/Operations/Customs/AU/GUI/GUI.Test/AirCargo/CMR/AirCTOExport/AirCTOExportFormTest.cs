using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	sealed class AirCTOExportFormTest : TestCaseWithFactory
	{
		public void TestShowPreSaveDialogs()
		{
			CMREMMMessage emmMessage = Factory.New<CMREMMMessage>();
			emmMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			emmMessage.EM_MessageText = EDIMessage.SendersReferencePlaceHolder + EDIMessage.MessageNumberPlaceHolder;
			Header.Messages.Add(emmMessage);
			Header.ED_CAN = "foo";
			Factory.Save();
			Header.ED_FlightNumber = "AA321"; // trigger an amendment
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			try
			{
				using (AirCTOExportForm form = new AirCTOExportForm(Header))
				{
					AssertEquals(ContinueWithSave.No, ((IShowPreSaveDialog)form).ShowPreSaveDialogs());
				}
			}
			finally
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestShowPreSaveDialogsNoAmendment()
		{
			CMREMMMessage emmMessage = Factory.New<CMREMMMessage>();
			emmMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			emmMessage.EM_MessageText = EDIMessage.SendersReferencePlaceHolder + EDIMessage.MessageNumberPlaceHolder;
			Header.Messages.Add(emmMessage);
			Header.ED_CAN = "foo";
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			try
			{
				using (AirCTOExportForm form = new AirCTOExportForm(Header))
				{
					AssertEquals(ContinueWithSave.Yes, ((IShowPreSaveDialog)form).ShowPreSaveDialogs());
				}
			}
			finally
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestCMRMessagingFormIsGettingTheRightManager()
		{
			AssertNotNull(Form.Manager);
			AssertEquals(typeof(AirCTOExportMessageManager), Form.Manager.GetType());
			AirCTOExportMessageManager manager = (AirCTOExportMessageManager)Form.Manager;
			AssertEquals(Header, manager.TopLevelBusinessObject);
		}

		public void TestCMRMessagingFormIsGettingTheRightMenu()
		{
			AssertNotNull(Form.CTOMenu);
			AssertCollectionContains(Form.CTOMenu, ((IFileMenuItemsProvider)Form).MainMenu.MenuItems);
		}

		public void TestCorrectMenuIsShownForManifestType()
		{
			AirManifestTypeList list = new AirManifestTypeList();
			Assert(list.Count > 3);
			AssertContains(AirManifestTypeList.Codes.CtoReceivalRemovalStandAlone, list.CodesAsString);
			AssertContains(AirManifestTypeList.Codes.ExportMainManifest, list.CodesAsString);
			AssertContains(AirManifestTypeList.Codes.ConsolidationExportSubManifest, list.CodesAsString);
			foreach (CodeDescriptionPair type in list)
			{
				Header.ED_ManifestType = type.Code;
				if (type.Code == AirManifestTypeList.Codes.CtoReceivalRemovalStandAlone)
				{
					AssertEquals(true, Form.CTOMenu.Visible);
					AssertEquals(false, Form.exportManifestMenu.Visible);
				}
				else if (type.Code == AirManifestTypeList.Codes.ExportMainManifest || type.Code == AirManifestTypeList.Codes.ConsolidationExportSubManifest)
				{
					AssertEquals(true, Form.CTOMenu.Visible);
					AssertEquals(true, Form.exportManifestMenu.Visible);
				}
				else
				{
					AssertEquals(false, Form.CTOMenu.Visible);
					AssertEquals(true, Form.exportManifestMenu.Visible);
				}
			}
		}

		public void TestThatTheRightMenuItemsAreShownOnTheExportMenu()
		{
			foreach (CodeDescriptionPair type in new AirManifestTypeList())
			{
				Header.ED_ManifestType = type.Code;
				if (type.Code == AirManifestTypeList.Codes.DepartureReport)
				{
					AssertEquals(true, Form.exportManifestMenu.DeclareDepartureReportMenuItem.Visible);
				}
				else
				{
					AssertEquals(false, Form.exportManifestMenu.DeclareDepartureReportMenuItem.Visible);
				}
			}
		}

		public void TestDetailsUserControlGetsAReferenceToTheHeader()
		{
			Form.Show();
			AssertEquals(Header, Form.exportManifestDetailsUserControl1.Header);
		}

		#region Implementation
		protected override void TearDown()
		{
			if (form != null && !form.IsDisposed)
			{
				form.Dispose();
			}

			base.TearDown();
		}

		public AirCTOExportForm Form
		{
			get
			{
				if (form == null || form.IsDisposed)
				{
					form = new AirCTOExportForm(Header);
				}

				return form;
			}
		}

		AirCTOExportForm form;
		public AirCTOExportCustomsManifestHeader Header
		{
			get
			{
				if (header == null)
				{
					header = Factory.New<AirCTOExportCustomsManifestHeader>();
				}

				return header;
			}
		}

		AirCTOExportCustomsManifestHeader header;
		#endregion
	}
}
