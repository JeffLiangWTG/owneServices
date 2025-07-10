using Enterprise.DocumentEngine.Scheduler.Business;

namespace Enterprise.DocumentEngine.GUI
{
	public partial class StmPrintJobForm : Enterprise.ZArchitecture.GUI.ZForm
	{
		public StmPrintJobForm()
			: base()
		{
			InitializeComponent();
		}

		public StmPrintJobForm(StmPrintJob stmPrintJob)
			: base(stmPrintJob)
		{
			InitializeComponent();
		}
	}
}
