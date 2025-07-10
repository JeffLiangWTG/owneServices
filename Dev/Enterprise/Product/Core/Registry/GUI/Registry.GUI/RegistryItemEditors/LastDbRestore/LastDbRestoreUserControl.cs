using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.GUI
{
	[SuppressFormsLocalizedTest]
	public partial class LastDbRestoreUserControl : RegistryZUserControl
	{
		public LastDbRestoreUserControl()
		{
			InitializeComponent();
			completionDateTextBox.TextChanged += new EventHandler(completionDateTextBox_TextChanged);
		}

		void completionDateTextBox_TextChanged(object sender, EventArgs e)
		{
			var date = new ZDateTime((sender as ZTextBox).Text);
			var location = new BusinessObjectFactory().Load<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, Environment.Env.CurrentBranch.NKUNLOCO))[0];
			if (location != null && date.IsValid)
			{
				date = location.TimeZoneSet.GetCalculationTimeZone().ToLocalTime(date.ToDateTime());
			}
			completionDateTextBox.TextChanged -= completionDateTextBox_TextChanged;
			completionDateTextBox.Text = date.ToString(Core.Constants.LastDatabaseRestoreRegistryConstants.CompletionDateFormat);
		}
	}
}
