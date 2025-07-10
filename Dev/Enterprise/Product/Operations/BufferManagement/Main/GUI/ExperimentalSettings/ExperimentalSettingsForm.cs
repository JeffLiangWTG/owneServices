using System;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class ExperimentalSettingsForm : ZChildForm
	{
		readonly ExperimentalSettingsProvider provider;

		public ExperimentalSettingsForm(ExperimentalSettingsProvider provider)
			: base(provider)
		{
			this.provider = provider;
		}

		void SaveButton_Click(object sender, EventArgs e)
		{
			provider.SaveSettings();
			Close();
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
