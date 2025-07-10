using System;
using Enterprise.Customs.GB.CDS;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Wizards
{
	public partial class CdsFsdWizardForm : ZChildForm
	{
		public CdsFsdWizardForm()
		{
			InitializeComponent();
		}

		public CdsFsdWizardForm(ICDSFinalSupplementaryDeclarationHelperManager manager)
			: base(manager.CDSFsdWizard)
		{
			InitializeComponent();
			this.manager = manager;
		}
		readonly ICDSFinalSupplementaryDeclarationHelperManager manager;

		void ButtonCreateEidr_Click(object sender, EventArgs e)
		{
			manager.CreateCDSFsd();
		}

		void countTimelyEntriesButton_Click(object sender, EventArgs e)
		{
			if (manager.CDSFsdWizard.DueDate.IsValid && manager.CDSFsdWizard.StartOfPeriod.IsValid)
			{
				manager.CountTimelyEntries();
			}
			else
			{
				Globals.Message.ShowError("Please select a valid Due Date and Start of Period!");
			}
		}
	}
}
