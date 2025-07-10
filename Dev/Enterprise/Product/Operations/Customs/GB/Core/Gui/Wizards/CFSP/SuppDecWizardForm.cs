using System;
using System.Drawing;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Wizards.CFSP;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Wizards
{
	public partial class SuppDecWizardForm : ZChildForm
	{
		public SuppDecWizardForm()
		{
		}

		public SuppDecWizardForm(SuppDecWizardManager manager)
			: base(manager.SuppDecWizard)
		{
			this.manager = manager;
			this.manager.SuppDecWizard.OnBalanceChanged -= SuppDecWizard_BalanceChanged;
			this.manager.SuppDecWizard.OnBalanceChanged += SuppDecWizard_BalanceChanged;

			initialColour = NumberOfPackagesRemainingBalanceTextBox.ForeColor;
		}

		void SuppDecWizard_BalanceChanged(object sender, EventArgs e)
		{
			var balanceArg = e as SuppDecWizard.EventArgsWithCounter;
			if (balanceArg != null)
			{
				var colourToSet = initialColour;
				if (balanceArg.Balance == 0)
				{
					colourToSet = Color.Green;
				}
				else if (balanceArg.Balance < 0)
				{
					colourToSet = Color.Red;
				}
				NumberOfPackagesRemainingBalanceTextBox.ForeColor = colourToSet;
			}
		}

		void OkButton_Click(object sender, EventArgs e)
		{
			var errors = manager.ValidateEverything();
			if (errors.IsEmpty)
			{
				manager.CreateAndShowSupplemenaryDeclaration(new RelatedDeclarationHelper(new RelatedDeclarationPresenter()));
				Close();
			}
			else
			{
				Globals.Message.ShowError(errors);
			}
		}

		readonly Color initialColour;
		readonly SuppDecWizardManager manager;
	}
}
