using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class DrawbackRefEntryLinesForm : ZChildForm
	{
		public DrawbackRefEntryLinesForm(JobComInvoiceLine businessEntity)
			: base(businessEntity)
		{
		}

		public new JobComInvoiceLine BusinessEntity
		{
			get { return base.BusinessEntity as JobComInvoiceLine; }
		}

		public override string FormCaption
		{
			get { return "Drawback Reference Entry Lines"; }
		}
	}
}
