using Enterprise.Customs.GB.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.GUI.CDSCashPayments
{
	public partial class CDSCashPaymentsForm : ZForm
	{
		public CDSCashPaymentsForm(CusEntryPayInfo businessObject) : base(businessObject)
		{
			InitializeComponent();
			ControllerID = ControllerIDs.Customs.GB.CDSCashPaymentsController;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);
		}
	}
}
