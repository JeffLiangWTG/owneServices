using System;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.Common;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Security.ActiveDirectory.GUI.Registry
{
	public partial class ADRegistryControl : RegistryZUserControl
	{
		public ADRegistryControl()
		{
			InitializeComponent();

			UpdateNoteLinkLabel.Text = Res.GetString("3518948a-c4b9-41ac-893c-b78cc4de1dfe", "Active Directory Integration");
			SingleSignOnHintLabel.Text = Res.GetString("0b5902da-c8b3-4da0-a9ba-312ddd4fcdfa", "When activated, staff members will be logged into {0} based on the currently logged in Windows account without having to enter a password.", BrandingFactory.Instance.ProductName);
		}

		public ADConfig Value
		{
			get { return adConfig; }
			set
			{
				adConfig = value;
				SetDataBinding(value, "");
				adConfig.EntitiesToSyncCodeInfo.ValueChanged += ShowOrHideGroupControls;
				ShowOrHideGroupControls(null, null);
			}
		}
		ADConfig adConfig;

		void ShowOrHideGroupControls(object sender, EventArgs e)
		{
			if (Value != null)
			{
				var showGroup = Value.EntitiesToSyncCode == ActiveDirectory.EntitiesToSyncList.Codes.UsersAndGroups;
				SyncDirectionGroupDropEdit.Visible = showGroup;
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			if (readOnly)
			{
				Value = ADConfig.DefaultValue;
			}
			this.OptionGroupBox.Enabled = !readOnly;
			this.SingleSignOnGroupBox.Enabled = !readOnly;
		}

		void UpdateNoteLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			try
			{
				WebUrlLauncher.Launch(UpdateNoteUrl);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowError(ex.Message, Res.GetString("c0110f6b-0e19-464f-95b3-749e8a1eb636", "Error displaying update note"));
			}
		}

		const string UpdateNoteUrl = "http://www.cargowise.com/Documents/UpdateNotes/ediEnterpriseupdatenote20130628.pdf";
	}
}
