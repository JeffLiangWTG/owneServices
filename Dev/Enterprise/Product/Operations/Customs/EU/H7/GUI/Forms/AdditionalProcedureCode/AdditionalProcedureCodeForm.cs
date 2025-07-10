using System.ComponentModel;
using Enterprise.Customs.EU.H7.Business;

namespace Enterprise.Customs.EU.H7.GUI
{
	public partial class AdditionalProcedureCodeForm : EU.GUI.AdditionalProcedureCodeForm
	{
		public AdditionalProcedureCodeForm(EU.Business.IAdditionalProcedureParent procedureParent) : base(procedureParent)
		{
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			var bill = DataSource as AsycudaBill;
			bill.ValidateAdditionalProcedureCodeAsString();
			bill.AdditionalProcedureCodesAsStringInfo.RefreshBinding();
		}
	}
}
