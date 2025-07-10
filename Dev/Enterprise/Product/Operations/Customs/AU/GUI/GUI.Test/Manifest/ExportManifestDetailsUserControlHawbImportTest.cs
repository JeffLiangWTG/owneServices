using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.AirCargo.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.ExportManifest.GUI.Testing
{
	sealed class ExportManifestDetailsUserControlHawbImportTest : TestCaseWithFactory
	{
		public void TestShowSeaLinesOnOpenIfSea()
		{
			header = Factory.New<ExportCustomsManifestHeader>();
			header.ED_TransportMode = Core.Constants.TransportModes.Sea;
			header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			header.ED_CAN = "a";
			ExportCustomsManifestLines line1 = Header.Lines.AddNew();
			Factory.Save();
			Form.Show();
			Application.DoEvents();
			AssertEquals(false, GetControl<ZGrid>("airCTOLinesGrid").Visible);
			AssertEquals(true, GetControl<ZGrid>("seaLinesGrid").Visible);
			AssertEquals(false, GetControl<ZGrid>("airMainManifestLinesGrid").Visible);
			AssertEquals(false, GetControl<ZGrid>("normalLinesGrid").Visible);
			AssertEquals(false, GetControl<ZGrid>("eSMSeaGrid").Visible);
			AssertEquals(false, GetControl<ZGrid>("eSMAirGrid").Visible);
		}

		public void TestShowAirCTOHawbImportForm_SuccessWithMainManifest()
		{
			Header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			AssertSuccess();
		}

		public void TestShowAirCTOHawbImportForm_SuccessWithCTOManifest()
		{
			Header.ED_ManifestType = AirManifestTypeList.Codes.CtoReceivalRemovalStandAlone;
			AssertSuccess();
		}

		public void TestShowAirCTOHawbImportForm_SuccessWithSubManifest()
		{
			Header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
			AssertSuccess();
		}

		public void TestShowAirCTOHawbImportForm_NoHawbSelected()
		{
			Header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			AssertNotNull("LazyLoad");
			Factory.Save();
			Form.Show();
			AssertEquals("precondition: ", -1, LinesGrid.ListManager.Position);
			Control.lastHawbController = null;
			LinesGrid_DoubleClick();
			AssertNull("Dont show a form if no hawb selected", Control.lastHawbController);
		}

		public void TestShowAirCTOHawbImportForm_UnsavedChanges()
		{
			Header.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			Header.ED_AirWayBill = "08155555625";
			ExportCustomsManifestLines line1 = Header.Lines.AddNew();
			ExportCustomsManifestLines line2 = Header.Lines.AddNew();
			AssertEquals("precondition: ", true, Header.HasChanges);
			Form.Show();
			Control.lastHawbController = null;
			LinesGrid.ListManager.Position = LinesGrid.ListManager.List.IndexOf(line2);
			LinesGrid_DoubleClick();
			AssertNull("Form should not be shown", Control.lastHawbController);
			AssertEquals("Should have shown an error message", "Error The current form has changes, you will need to save before opening this HAWB.", UnitTestUserNotification.Instance.LastMessage.ToString());
		}

		protected override void TearDown()
		{
			base.TearDown();
			control?.Dispose();
			form?.Dispose();
		}

		void AssertSuccess()
		{
			ExportCustomsManifestLines line1 = Header.Lines.AddNew();
			ExportCustomsManifestLines line2 = Header.Lines.AddNew();
			Factory.Save();
			Form.Show();
			Control.lastHawbController = null;
			LinesGrid.ListManager.Position = LinesGrid.ListManager.List.IndexOf(line2);
			LinesGrid_DoubleClick();
			AssertNotNull(Control.lastHawbController);
			using (ZForm subForm = (ZForm)Control.lastHawbController.LastShownForm)
			{
				AssertNotNull("Should have shown a form", subForm);
				AssertEquals("Should have shown the correct form", typeof(AirCTOHawbExportForm), subForm.GetType());
				AssertEquals("Should show the correct BizObj", line2.PK, ((BusinessObject)subForm.BusinessEntity).PK);
			}
		}

		T GetControl<T>(string name)
		{
			return (T)typeof(ExportManifestDetailsUserControl).InvokeMember(name, BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic, null, Control, Array.Empty<object>());
		}

		ExportCustomsManifestHeader header;
		ExportCustomsManifestHeader Header => header ?? (header = Factory.New<AirCTOExportCustomsManifestHeader>());

		ExportManifestDetailsUserControl control;
		ExportManifestDetailsUserControl Control => control ?? (control = new ExportManifestDetailsUserControl());

		ZGrid LinesGrid => (ZGrid)typeof(ExportManifestDetailsUserControl).GetProperty("CurrentGrid", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Control, null);

		void LinesGrid_DoubleClick() => typeof(ExportManifestDetailsUserControl).InvokeMember("LinesGrid_DoubleClick", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, Control, new object[] { LinesGrid, EventArgs.Empty });

		ZForm form;
		ZForm Form
		{
			get
			{
				if (form == null)
				{
					Control.Dock = DockStyle.Fill;
					form = new ZForm(Header);
					form.ClientSize = control.Size;
					form.Controls.Add(Control);
					Control.SetDataBinding(Header, "");
					Control.Header = Header;
				}

				return form;
			}
		}
	}
}
