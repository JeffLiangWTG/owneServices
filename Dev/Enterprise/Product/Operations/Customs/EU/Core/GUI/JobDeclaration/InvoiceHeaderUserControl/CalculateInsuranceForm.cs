using System;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class CalculateInsuranceForm : ZChildForm
	{
		public CalculateInsuranceForm(CalculateInsuranceBizObj bizObj) : base(bizObj)
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			DutiablePercentCalcEdit.Visible = BusinessEntity.IsDutiablePercentEnabled;
		}

		public new CalculateInsuranceBizObj BusinessEntity => (CalculateInsuranceBizObj)base.BusinessEntity;

		public override string FormVerb => string.Empty;

		void OkButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.Calculate())
			{
				Close();
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("DD6B920E-C9BD-4548-9A96-6E9BAC6B9C41", "Please fix all the errors before proceeding"), Res.GetString("FE307C10-153E-44B3-A4E2-EBAC82F47EDF", "Unable to Calculate"));
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
