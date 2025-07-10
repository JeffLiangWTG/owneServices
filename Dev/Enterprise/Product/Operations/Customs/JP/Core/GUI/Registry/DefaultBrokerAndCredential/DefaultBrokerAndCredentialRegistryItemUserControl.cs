using System;
using Enterprise.Registry.GUI;

namespace Enterprise.Customs.JP.GUI
{
	public partial class DefaultBrokerAndCredentialRegistryItemUserControl : RegistryZUserControl
	{
		public DefaultBrokerAndCredentialRegistryItemUserControl()
		{
			InitializeComponent();

			DefaultBrokerCodeFindBox.TextChanged -= DefaultBrokerCodeFindBox_TextChanged;
			DefaultBrokerCodeFindBox.TextChanged += DefaultBrokerCodeFindBox_TextChanged;
		}

		void DefaultBrokerCodeFindBox_TextChanged(object sender, EventArgs e)
		{
			var isCredentialsReadOnly = string.IsNullOrEmpty(DefaultBrokerCodeFindBox.CurrentCode) || ReadOnly;
			SetCredentialsReadOnly(isCredentialsReadOnly);
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			DefaultBrokerCodeFindBox.ReadOnly = readOnly;

			var isCredentialsReadOnly = string.IsNullOrEmpty(DefaultBrokerCodeFindBox.CurrentCode) || readOnly;
			SetCredentialsReadOnly(isCredentialsReadOnly);
		}

		void SetCredentialsReadOnly(bool readOnly)
		{
			DefaultCredentialSEADropEdit.ReadOnly = readOnly;
			DefaultCredentialAIRDropEdit.ReadOnly = readOnly;
			ForwarderManifestSEADropEdit.ReadOnly = readOnly;
			ForwarderManifestAIRDropEdit.ReadOnly = readOnly;
		}
	}
}
