using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class CalculateFreightNonAirForm : ZChildForm
	{
		public CalculateFreightNonAirForm(CalculateFreightNonAirBizObj bizObj)
			: base(bizObj)
		{
		}

		public new CalculateFreightNonAirBizObj BusinessEntity => (CalculateFreightNonAirBizObj)base.BusinessEntity;

		public override string FormVerb => string.Empty;

		void cancelButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			if (BusinessEntity.Calculate())
			{
				Close();
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("5B14D465-A62A-40B6-859B-75C3B33AE905", "Please fix all the errors before proceeding"), Res.GetString("4D9D23D7-3539-4A01-822D-8864EE5DF659", "Unable to Calculate"));
			}
		}
	}
}
