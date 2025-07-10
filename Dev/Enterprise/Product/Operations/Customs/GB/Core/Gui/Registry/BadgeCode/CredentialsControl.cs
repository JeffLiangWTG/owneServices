using System;
using Enterprise.Customs.GB.Registry;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Registry
{
	public partial class CredentialsControl : RegistryZUserControl
	{
		public CredentialsControl()
		{
			InitializeComponent();
			AddEndpointColumn();
			CredentialsGrid.ContextMenu.MenuItems.Add(new ZMenuItem(Res.GetData("1E72D49E-6BD4-4871-A2E6-5EE76C310561", "Check credentials"), CheckCredentialsClick));
		}

		void AddEndpointColumn()
		{
			var zDropEditColumnStyleInfo3 = new ZDropEditColumnStyleInfo();
			zDropEditColumnStyleInfo3.CaptionResourceString = Res.GetData("7068e752-3829-433e-a79c-e9c44554e3ae", "Endpoint");
			zDropEditColumnStyleInfo3.ColumnName = "Endpoint";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			CredentialsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
		}

		void CheckCredentialsClick(object s, EventArgs e)
		{
			if (CredentialsGrid.SelectedElements.Length != 1)
			{
				Globals.Message.ShowInformation(Res.GetString("756D60AA-EE08-4D3E-AA4A-CEB3DFDD6B08", "Select a single row first"));
			}
			else
			{
				((CredentialsSetting)CredentialsGrid.SelectedElements[0]).CheckCredentialsAgainstCspWebService(Globals.Message);
			}
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			CredentialsGrid.ReadOnly = readOnly;
		}
	}
}
