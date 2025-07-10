using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class OceanBillDetailsUserControl : ZUserControl
	{
		public OceanBillDetailsUserControl()
		{
			InitializeComponent();
			CB_RV_NKVessel.PopupSelected += CB_RV_NKVessel_PopupSelected;
		}

		CusSCAOceanBill OceanBill => (CusSCAOceanBill)CurrentDataItem;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateForOceanBillUnpack();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			UnhookFreightToCustomsSyncronisation();
			base.SetDataBinding(dataSource, dataMember);
			HookFreightToCustomsSyncronisation();
		}

		/// <summary>
		/// Designer property to set all of the contained Controls to ReadOnly.
		/// Is one-way.  Cannot be un-set to preserve any existing readonly state.
		/// Changes at the Business Object level may override this setting.
		/// </summary>
		public bool ReadOnly
		{
			set
			{
				if (value)
				{
					GroupBoxOceanBill.Controls.Cast<Control>().Where(ctl => ctl is IEditableInViewMode).ForEach(ctl => ctl.SetReadOnly(true));
				}
			}
		}

		public void SetMessagingModeVisiblity(bool value)
		{
			CB_ApplicationCodeBoundDropEdit.Visible = value;
			MessagingModeLabel.Visible = value;
		}

		public void UpdateForOceanBillUnpack()
		{
			var isUnpack = OceanBill?.CB_MultiOBLUnpack ?? false;
			OceanBillLabel.Visible = !isUnpack;
			CB_OceanBillBoundTextBox.Visible = !isUnpack;
			CusSCAOceanBillParentBillLabel.Visible = !isUnpack;
			CusSCAOceanBillParentBillTextBox.Visible = !isUnpack;
		}

		void HookFreightToCustomsSyncronisation()
		{
			var oceanBill = OceanBill;
			if (oceanBill != null)
			{
				oceanBill.OverrideFreightDefaultsChanging += OnOverrideFreightDefaultsChanging;
			}
			this.overrideFreightDefaultsCheckBox.Visible = OceanBill?.OverrideFreightDefaultsVisible ?? false;
		}

		void UnhookFreightToCustomsSyncronisation()
		{
			var oceanBill = OceanBill;
			if (oceanBill != null)
			{
				oceanBill.OverrideFreightDefaultsChanging -= OnOverrideFreightDefaultsChanging;
			}
		}

		void OnOverrideFreightDefaultsChanging(object sender, CancelEventArgs e)
		{
			if (ParentForm is ZForm parentForm && !parentForm.IsSavingInProgress)
			{
				if (OceanBill?.OverrideFreightDefaults ?? false)
				{
					var result = Globals.Message.Show(Declaration.GUI.Res.GetString("F893453C-3A30-4E2F-B600-1D3E7C8AE9FA", "Removing the override will reset your Sea Cargo data.\r\nYou will lose changes that you have made to the Sea Cargo.\r\n\r\nProceed?"), Declaration.GUI.Res.GetString("7A2196C2-C0AD-4F09-8D3C-6E6013B6DCB1", "Confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes);
					e.Cancel = result == DialogResult.No;
				}
			}
		}

		void CB_RV_NKVessel_PopupSelected(object sender, ZArchitecture.GUI.Internal.EmbeddedModulePopup.SelectedEventArgs e)
		{
			var bizos = e.SelectedBusinessObjects;
			if (bizos.Length == 1 && bizos[0] is RefVessel vessel)
			{
				OceanBill.SetVessel(vessel);
			}
		}
	}
}
