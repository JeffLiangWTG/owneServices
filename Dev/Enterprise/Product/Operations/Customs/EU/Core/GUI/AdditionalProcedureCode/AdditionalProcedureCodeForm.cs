using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class AdditionalProcedureCodeForm : ZChildForm
	{
		public AdditionalProcedureCodeForm(IAdditionalProcedureParent procedureParent) : base(procedureParent.BusinessObject)
		{
			this.procedureParent = procedureParent;
		}

		public override string FormVerb => "";

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			fOldItems = procedureParent.AdditionalProcedureCodes.AsString;
		}

		string fOldItems;

		protected override void OnClosing(CancelEventArgs e)
		{
			zButtonClose.Focus();

			if (DialogResult == DialogResult.Cancel)
			{
				procedureParent.AdditionalProcedureCodes.AsString = fOldItems;
			}
			else
			{
				procedureParent.AdditionalProcedureCodes.RunPreSaveValidation();
				if (procedureParent.AdditionalProcedureCodes.Cast<AdditionalProcedureCode>().Any(code => code.NotificationsIncludingChildren.GetErrors().Any()))
				{
					Globals.Message.ShowError(Res.GetString("F678B31A-605E-4CBD-AB80-231575734B7B", "The form has errors. Please fix them before continuing."));
					e.Cancel = true;
				}
			}

			base.OnClosing(e);
		}

		void ZButtonOK_Click(object sender, EventArgs e)
		{
			Close();
		}

		void ZButtonClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		readonly IAdditionalProcedureParent procedureParent;
	}
}
