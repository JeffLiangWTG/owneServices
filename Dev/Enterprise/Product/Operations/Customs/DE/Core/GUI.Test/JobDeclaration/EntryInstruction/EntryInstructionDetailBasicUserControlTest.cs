using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class EntryInstructionDetailBasicUserControlTest : TestCaseWithFactory
	{
		public void TestControlVisibilitiesExport()
		{
			AssertControlVisibilities(Common.EU.EUJobMessageTypeList.Codes.Export);
		}

		public void TestControlVisibilitiesImport()
		{
			AssertControlVisibilities(Common.EU.EUJobMessageTypeList.Codes.Import);
		}

		public void TestResizeOtherPartiesGroupBoxForVisibleControls_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;

			using (var frm = new ZForm(declaration))
			using (var control = new EntryInstructionDetailBasicUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();
				control.ChangeControlsVisibility();

				var otherPartiesGroupBox = control.FindSingle<ZGroupBox>("OtherPartiesGroupBox");
				var fromWarehouseGroupBox = control.FindSingle<ZGroupBox>("FromWarehouseGroupBox");

				AssertEquals(fromWarehouseGroupBox.Bottom + ControlDpiScalingHelper.OnePixel * 2, otherPartiesGroupBox.Bottom);
			}
		}

		public void TestResizeOtherPartiesGroupBoxForVisibleControls_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

			using (var frm = new ZForm(declaration))
			using (var control = new EntryInstructionDetailBasicUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();
				control.ChangeControlsVisibility();

				var otherPartiesGroupBox = control.FindSingle<ZGroupBox>("OtherPartiesGroupBox");
				var bondHolderRemoverPanel = control.FindSingle<ZPanel>("BondHolderRemoverPanel");

				AssertEquals(bondHolderRemoverPanel.Bottom + ControlDpiScalingHelper.OnePixel * 2, otherPartiesGroupBox.Bottom);
			}
		}

		void AssertControlVisibilities(ZString messageType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			bool isExport = declaration.IsExport;
			bool isImport = declaration.IsImport;

			using (var frm = new ZForm(declaration))
			using (var control = new EntryInstructionDetailBasicUserControl())
			{
				frm.Controls.Add(control);
				frm.Show();
				control.ChangeControlsVisibility();
				CombineAssertions(() =>
				{
					var otherPartiesDetailsPanel = control.FindSingle<ZPanel>("OtherPartiesDetailsPanel");
					AssertEquals("OtherPartiesDetailsPanel visible for both Export + Import", true, otherPartiesDetailsPanel.Visible);
					var otherPartiesGroupBox = otherPartiesDetailsPanel.FindSingle<ZGroupBox>("OtherPartiesGroupBox");
					AssertEquals("OtherPartiesGroupBox visible for both Export + Import", true, otherPartiesGroupBox.Visible);

					var bondHolderRemoverPanel = otherPartiesGroupBox.FindSingle<ZPanel>("BondHolderRemoverPanel");
					AssertEquals("BondHolderRemoverPanel only Visible for import", isImport, bondHolderRemoverPanel.Visible);
					AssertEquals("NewOwnerOrganisationControl only Visible for import", isImport, bondHolderRemoverPanel.FindSingle<ZOrganisationControl>("NewOwnerOrganisationControl").Visible);
					AssertEquals("RemoverOrganisationControl only Visible for import", isImport, bondHolderRemoverPanel.FindSingle<ZOrganisationControl>("RemoverOrganisationControl").Visible);
					AssertEquals("BondHolderOrganisationControl only Visible for import", isImport, bondHolderRemoverPanel.FindSingle<ZOrganisationControl>("BondHolderOrganisationControl").Visible);

					var fromWarehouseGroupBox = otherPartiesGroupBox.FindSingle<ZGroupBox>("FromWarehouseGroupBox");
					AssertEquals("FromWarehouseGroupBox visible for both Export + Import", true, fromWarehouseGroupBox.Visible);
					AssertEquals("FromWarehouseCodeTextBox visible for both Export + Import", true, fromWarehouseGroupBox.FindSingle<ZTextBox>("FromWarehouseCodeTextBox").Visible);
					AssertEquals("FromWarehouseAddressControl visible for both Export + Import", true, fromWarehouseGroupBox.FindSingle<ZAddressControl>("FromWarehouseAddressControl").Visible);

					var toWarehouseGroupBox = otherPartiesGroupBox.FindSingle<ZGroupBox>("ToWarehouseGroupBox");
					AssertEquals("ToWarehouseGroupBox visible for both Export + Import", true, toWarehouseGroupBox.Visible);
					AssertEquals("ToWarehouseCodeTextBox visible for both Export + Import", true, toWarehouseGroupBox.FindSingle<ZTextBox>("ToWarehouseCodeTextBox").Visible);
					AssertEquals("ToWarehouseAddressControl visible for both Export + Import", true, toWarehouseGroupBox.FindSingle<ZAddressControl>("ToWarehouseAddressControl").Visible);
				});
			}
		}
	}
}
