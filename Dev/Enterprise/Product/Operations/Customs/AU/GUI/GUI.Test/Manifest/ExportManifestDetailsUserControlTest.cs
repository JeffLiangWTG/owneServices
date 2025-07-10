using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.ExportManifest.GUI.Testing
{
	sealed class ExportManifestDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestESMAirLinesGrid()
		{
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			header.ED_ManifestType = AirManifestTypeList.Codes.ConsolidationExportSubManifest;
			AssertEquals(false, Control.normalLinesGrid.Visible);
			AssertEquals(false, Control.airCTOLinesGrid.Visible);
			AssertEquals(false, Control.airMainManifestLinesGrid.Visible);
			AssertEquals(false, Control.seaLinesGrid.Visible);
			AssertEquals(true, Control.eSMAirGrid.Visible);
			AssertEquals(false, Control.eSMSeaGrid.Visible);
		}

		public void TestESMSeaLinesGrid()
		{
			header.ED_TransportMode = Core.Constants.TransportModes.Sea;
			header.ED_ManifestType = AirManifestTypeList.Codes.ConsolidationExportSubManifest;
			AssertEquals(false, Control.normalLinesGrid.Visible);
			AssertEquals(false, Control.airCTOLinesGrid.Visible);
			AssertEquals(false, Control.airMainManifestLinesGrid.Visible);
			AssertEquals(false, Control.seaLinesGrid.Visible);
			AssertEquals(false, Control.eSMAirGrid.Visible);
			AssertEquals(true, Control.eSMSeaGrid.Visible);
		}

		public void TestAirCTOLinesGrid()
		{
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			header.ED_ManifestType = AirManifestTypeList.Codes.CtoReceivalRemovalStandAlone;
			AssertEquals(false, Control.normalLinesGrid.Visible);
			AssertEquals(true, Control.airCTOLinesGrid.Visible);
			AssertEquals(false, Control.airMainManifestLinesGrid.Visible);
			AssertEquals(false, Control.seaLinesGrid.Visible);
			AssertEquals(false, Control.eSMAirGrid.Visible);
			AssertEquals(false, Control.eSMSeaGrid.Visible);
		}

		public void TestSeaLinesGrid()
		{
			header.ED_TransportMode = Core.Constants.TransportModes.Sea;
			header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			header.ED_CAN = "a";
			AssertEquals(false, Control.normalLinesGrid.Visible);
			AssertEquals(false, Control.airCTOLinesGrid.Visible);
			AssertEquals(false, Control.airMainManifestLinesGrid.Visible);
			AssertEquals(true, Control.seaLinesGrid.Visible);
			AssertEquals(false, Control.eSMAirGrid.Visible);
			AssertEquals(false, Control.eSMSeaGrid.Visible);
		}

		public void TestSeaDepartureNoLinesGrid()
		{
			header.ED_TransportMode = Core.Constants.TransportModes.Sea;
			header.ED_ManifestType = ManifestTypeList.Codes.DepartureReport;
			AssertEquals(false, Control.normalLinesGrid.Visible);
			AssertEquals(false, Control.airCTOLinesGrid.Visible);
			AssertEquals(false, Control.airMainManifestLinesGrid.Visible);
			AssertEquals(false, Control.seaLinesGrid.Visible);
			AssertEquals(false, Control.eSMAirGrid.Visible);
			AssertEquals(false, Control.eSMSeaGrid.Visible);
		}

		public void TestAirDepartureNoLinesGrid()
		{
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			header.ED_ManifestType = ManifestTypeList.Codes.DepartureReport;
			AssertEquals(false, Control.normalLinesGrid.Visible);
			AssertEquals(false, Control.airCTOLinesGrid.Visible);
			AssertEquals(false, Control.airMainManifestLinesGrid.Visible);
			AssertEquals(false, Control.seaLinesGrid.Visible);
			AssertEquals(false, Control.eSMAirGrid.Visible);
			AssertEquals(false, Control.eSMSeaGrid.Visible);
		}

		public void TestAirMainManifestLinesGrid()
		{
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			AssertEquals(false, Control.normalLinesGrid.Visible);
			AssertEquals(false, Control.airCTOLinesGrid.Visible);
			AssertEquals(true, Control.airMainManifestLinesGrid.Visible);
			AssertEquals(false, Control.seaLinesGrid.Visible);
			AssertEquals(false, Control.eSMAirGrid.Visible);
			AssertEquals(false, Control.eSMSeaGrid.Visible);
		}

		public void TestNormalLinesGrid()
		{
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			header.ED_ManifestType = ManifestTypeList.Codes.SlotExportSubManifest;
			AssertEquals(true, Control.normalLinesGrid.Visible);
			AssertEquals(false, Control.airCTOLinesGrid.Visible);
			AssertEquals(false, Control.airMainManifestLinesGrid.Visible);
			AssertEquals(false, Control.seaLinesGrid.Visible);
			AssertEquals(false, Control.eSMAirGrid.Visible);
			AssertEquals(false, Control.eSMSeaGrid.Visible);
		}

		public void TestVisibilityForAirESM()
		{
			header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(typeof(ESMHeaderDetails), Control.headerDetailsControl.GetType());
		}

		public void TestVisibilityForSeaESM()
		{
			header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
			header.ED_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(typeof(ESMHeaderDetails), Control.headerDetailsControl.GetType());
		}

		public void TestVisibilityForAirEMM()
		{
			header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(typeof(EMMHeaderDetails), Control.headerDetailsControl.GetType());
		}

		public void TestForAirCTOHeaderEMM()
		{
			AirCTOExportCustomsManifestHeader airCTOHeader = Factory.New<AirCTOExportCustomsManifestHeader>();
			using (ZForm form = new ZForm(airCTOHeader))
			using (ExportManifestDetailsUserControl control = new ExportManifestDetailsUserControl())
			{
				form.Controls.Add(control);
				control.Header = airCTOHeader;
				airCTOHeader.ED_ManifestType = ManifestTypeList.Codes.SlotExportSubManifest;
				form.Show();
				AssertEquals("precondition: control visible", true, control.Visible);
				airCTOHeader.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
				AssertEquals(typeof(CTOEMMHeaderDetails), control.headerDetailsControl.GetType());
				AssertEquals(false, control.normalLinesGrid.Visible);
				AssertEquals(true, control.airCTOLinesGrid.Visible);
				AssertEquals(false, control.airMainManifestLinesGrid.Visible);
				AssertEquals(false, control.seaLinesGrid.Visible);
				AssertEquals(true, control.lineMessagesTabPage.TabVisible);
			}
		}

		public void TestForAirCTOHeaderESM()
		{
			AirCTOExportCustomsManifestHeader airCTOHeader = Factory.New<AirCTOExportCustomsManifestHeader>();
			using (ZForm form = new ZForm(airCTOHeader))
			using (ExportManifestDetailsUserControl control = new ExportManifestDetailsUserControl())
			{
				form.Controls.Add(control);
				control.Header = airCTOHeader;
				form.Show();
				AssertEquals("precondition: control visible", true, control.Visible);
				airCTOHeader.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
				AssertEquals(typeof(CTOESMHeaderDetails), control.headerDetailsControl.GetType());
				AssertEquals(false, control.normalLinesGrid.Visible);
				AssertEquals(true, control.airCTOLinesGrid.Visible);
				AssertEquals(false, control.airMainManifestLinesGrid.Visible);
				AssertEquals(false, control.seaLinesGrid.Visible);
				AssertEquals(true, control.lineMessagesTabPage.TabVisible);
			}
		}

		public void TestVisibilityForSeaEMM()
		{
			header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			header.ED_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(typeof(EMMHeaderDetails), Control.headerDetailsControl.GetType());
		}

		public void TestLineItemVisibility()
		{
			header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
			ExportCustomsManifestLines line1 = header.Lines.AddNew();
			ExportCustomsManifestLines line2 = header.Lines.AddNew();
			line1.EL_TypeOfCAN = Common.AU.CMR.CMRExportExemptionCodes.EXLV.Code;
			using (ExportManifestForm form = new ExportManifestForm(header))
			{
				form.Show();
				control = form.FindSingle<ExportManifestDeclarationUserControl>("exportManifestDeclarationUserControl").ManifestUserControl;
				Control.BindingSource.ForceBinding(Control.normalLinesGrid);
				Control.normalLinesGrid.CurrentRowIndex = 0;
				AssertExtraLineItemsVisibility(true);
				line2.EL_TypeOfCAN = Common.AU.CMR.CMRExportExemptionCodes.EXDC.Code;
				Control.normalLinesGrid.CurrentRowIndex = 1;
				typeof(DataGrid).InvokeMember("OnCurrentCellChanged", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod, null, Control.normalLinesGrid, new object[] { EventArgs.Empty }); // this event always gets raised when the grid is visible at runtime, just not in this test for some reason :)
				AssertExtraLineItemsVisibility(false);
				line2.EL_TypeOfCAN = Common.AU.CMR.CMRExportExemptionCodes.EXLV.Code;
				AssertExtraLineItemsVisibility(true);
				header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
				AssertExtraLineItemsVisibility(false);
			}
		}

		public void TestCANFieldIsInvisibleIfExemptionSelected()
		{
			header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
			ExportCustomsManifestLines line1 = header.Lines.AddNew();
			line1.EL_TypeOfCAN = Common.AU.CMR.CMRExportExemptionCodes.EXLV.Code;
			using (ExportManifestForm form = new ExportManifestForm(header))
			{
				form.Show();
				var control = form.FindSingle<ExportManifestDeclarationUserControl>("exportManifestDeclarationUserControl").ManifestUserControl;
				control.BindingSource.ForceBinding(control.normalLinesGrid);
				control.normalLinesGrid.CurrentRowIndex = 0;
				Assert(!control.lineCANLabel.Visible);
				Assert(!control.lineCANTextBox.Visible);
				line1.EL_TypeOfCAN = CANType.ContingencyCustomsAuthorityNumber.Code;
				Assert(control.lineCANLabel.Visible);
				Assert(control.lineCANTextBox.Visible);
			}
		}

		public void TestLineContainersFieldIsInvisibleForAir()
		{
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
			ExportCustomsManifestLines line1 = header.Lines.AddNew();
			line1.EL_TypeOfCAN = Common.AU.CMR.CMRExportExemptionCodes.EXLV.Code;
			using (ExportManifestForm form = new ExportManifestForm(header))
			{
				form.Show();
				var control = form.FindSingle<ExportManifestDeclarationUserControl>("exportManifestDeclarationUserControl").ManifestUserControl;
				control.eSMAirGrid.CurrentRowIndex = 0;
				Assert(!control.lineContainersCalcEdit.Visible);
				Assert(!control.containersLabel.Visible);
				header.ED_TransportMode = Core.Constants.TransportModes.Sea;
				Assert(control.lineContainersCalcEdit.Visible);
				Assert(control.containersLabel.Visible);
			}
		}

		public void TestDepartureReportSeaVisibility()
		{
			header.ED_TransportMode = Core.Constants.TransportModes.Sea;
			header.ED_ManifestType = ManifestTypeList.Codes.DepartureReport;
			using (ExportManifestForm form = new ExportManifestForm(header))
			{
				form.Show();
				var control = form.FindSingle<ExportManifestDeclarationUserControl>("exportManifestDeclarationUserControl").ManifestUserControl;
				AssertEquals(true, control.departureReportStatusDropEdit.Visible);
				AssertEquals(true, control.departureReportStatusLabel.Visible);
				//AssertEquals(false, control.lineDetailsTabPage.Visible);
				AssertEquals(false, control.normalLinesGrid.Visible);
				AssertEquals(false, control.documentStatusConditionsDropEdit.Visible);
				AssertEquals(false, control.documentStatusConditionsLabel.Visible);
				AssertEquals(false, control.documentStatusDropEdit.Visible);
				AssertEquals(false, control.documentStatusLabel.Visible);
			}
		}

		public void TestDepartureReportAirVisibility()
		{
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			header.ED_ManifestType = ManifestTypeList.Codes.DepartureReport;
			using (ExportManifestForm form = new ExportManifestForm(header))
			{
				form.Show();
				var control = form.FindSingle<ExportManifestDeclarationUserControl>("exportManifestDeclarationUserControl").ManifestUserControl;
				AssertEquals(true, control.departureReportStatusDropEdit.Visible);
				AssertEquals(true, control.departureReportStatusLabel.Visible);
				AssertEquals(false, control.linesTabControl.Visible);
				AssertEquals(false, control.normalLinesGrid.Visible);
				AssertEquals(false, control.documentStatusConditionsDropEdit.Visible);
				AssertEquals(false, control.documentStatusConditionsLabel.Visible);
				AssertEquals(false, control.documentStatusDropEdit.Visible);
				AssertEquals(false, control.documentStatusLabel.Visible);
			}
		}

		public void TestCTOAirVisibility()
		{
			header.ED_TransportMode = Core.Constants.TransportModes.Air;
			header.ED_ManifestType = AirManifestTypeList.Codes.CtoReceivalRemovalStandAlone;
			using (ExportManifestForm form = new ExportManifestForm(header))
			{
				form.Show();
				var control = form.FindSingle<ExportManifestDeclarationUserControl>("exportManifestDeclarationUserControl").ManifestUserControl;
				AssertEquals(false, control.departureReportStatusDropEdit.Visible);
				AssertEquals(false, control.departureReportStatusLabel.Visible);
				AssertEquals(true, control.linesTabControl.Visible);
				AssertEquals(true, control.airCTOLinesGrid.Visible);
				AssertEquals(false, control.normalLinesGrid.Visible);
				AssertEquals(false, control.airMainManifestLinesGrid.Visible);
				AssertEquals(false, control.seaLinesGrid.Visible);
				AssertEquals(false, control.documentStatusConditionsDropEdit.Visible);
				AssertEquals(false, control.documentStatusConditionsLabel.Visible);
				AssertEquals(false, control.documentStatusDropEdit.Visible);
				AssertEquals(false, control.documentStatusLabel.Visible);
			}
		}

		public void TestStatusRequestMenuItem()
		{
			AirCTOExportCustomsManifestHeader airCTOHeader = Factory.New<AirCTOExportCustomsManifestHeader>();
			ExportCustomsManifestLines line1 = airCTOHeader.Lines.AddNew();
			line1.EL_UserReferenceNum = "K00000001";
			line1.EL_CAN = "12345678";
			line1.EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
			ExportCustomsManifestLines line2 = airCTOHeader.Lines.AddNew();
			line2.EL_UserReferenceNum = "K00000002";
			line2.EL_CAN = "123456789";
			line2.EL_TypeOfCAN = CANType.CustomsAuthorityNumber.Code;
			ExportCustomsManifestLines line3 = airCTOHeader.Lines.AddNew();
			line3.EL_UserReferenceNum = "K00000003";
			line3.EL_TypeOfCAN = Common.AU.CMR.CMRExportExemptionCodes.EXLV.Code;
			using (ZForm form = new ZForm(airCTOHeader))
			using (ExportManifestDetailsUserControl detailsControl = new ExportManifestDetailsUserControl())
			{
				form.Controls.Add(detailsControl);
				detailsControl.Header = airCTOHeader;
				airCTOHeader.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
				form.Show();
				Assert(detailsControl.Visible);
				Assert(detailsControl.airCTOLinesGrid.Visible);
				Assert(detailsControl.lineMessagesTabPage.TabVisible);
				MenuItem statusRequestMenuItem = null;
				foreach (MenuItem item in detailsControl.airCTOLinesGrid.ContextMenu.MenuItems)
				{
					if (item.Text == "Request Customs CAN Status")
					{
						statusRequestMenuItem = item;
						break;
					}
				}

				AssertNotNull("Status request menu found", statusRequestMenuItem);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				statusRequestMenuItem.PerformClick();
				AssertEquals("Please save the form before using this feature.", UnitTestUserNotification.Instance.LastMessage.Text);
				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				statusRequestMenuItem.PerformClick();
				AssertEquals("Please select one or more lines for which you wish to send status requests.", UnitTestUserNotification.Instance.LastMessage.Text);
				detailsControl.airCTOLinesGrid.SelectAllElements();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				statusRequestMenuItem.PerformClick();
				AssertEquals(@"Invalid CAN (12345678), message will not be sent for line 1
Send Status Request for: CAN: 123456789 Line: 2 Ref: K00000002
Line type must be 'CAN' to request status, message will not be sent for line 3
1 messages will be sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No messages generated for line 1", 0, line1.Messages.Count);
				AssertEquals("No messages generated for line 2", 0, line2.Messages.Count);
				detailsControl.airCTOLinesGrid.SelectAllElements();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				statusRequestMenuItem.PerformClick();
				AssertEquals(@"Invalid CAN (12345678), message will not be sent for line 1
Send Status Request for: CAN: 123456789 Line: 2 Ref: K00000002
Line type must be 'CAN' to request status, message will not be sent for line 3
1 messages will be sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No messages generated for line 1", 0, line1.Messages.Count);
				AssertEquals("1 message generated for line 2", 1, line2.Messages.Count);
			}
		}

		ExportCustomsManifestHeader header;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<ExportCustomsManifestHeader>();
		}

		protected override void TearDown()
		{
			control?.Dispose();
			base.TearDown();
		}

		void AssertExtraLineItemsVisibility(bool visible)
		{
			AssertEquals(visible, Control.descriptionLabel.Visible);
			AssertEquals(visible, Control.descriptionTextBox.Visible);
			AssertEquals(visible, Control.lineCountryOfDestinationLabel.Visible);
			AssertEquals(visible, Control.lineCountryOfDestinationCodeFindBox.Visible);
			AssertEquals(visible, Control.ownerLabel.Visible);
			AssertEquals(visible, Control.ownerOrganisationFindBox.Visible);
			AssertEquals(visible, Control.ownerNameLabel.Visible);
			AssertEquals(visible, Control.goodsOwnerTextBox.Visible);
			AssertEquals(visible, Control.ownerIDLabel.Visible);
			AssertEquals(visible, Control.goodsOwnerIDTextBox.Visible);
		}

		ExportManifestDetailsUserControl control;
		ExportManifestDetailsUserControl Control => control ?? (control = new ExportManifestDetailsUserControl { Header = header });
	}
}
