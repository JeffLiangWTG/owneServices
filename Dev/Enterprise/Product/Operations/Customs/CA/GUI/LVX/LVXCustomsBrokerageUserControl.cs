using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class LVXCustomsBrokerageUserControl : BaseCustomsBrokerageUserControl
	{
		public LVXCustomsBrokerageUserControl()
		{
			InitializeComponent();
			DisposeRedundantTabs();
			SetCaptions();
		}

		void SetCaptions()
		{
			this.DeclarationTabPage.Text = Res.GetString("7b3f2a61-0ee5-4680-8ff8-02f7662d6d3f", "Courier LVS Declaration");
		}

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl()
		{
			return new LVXUserControl();
		}

		void DisposeRedundantTabs()
		{
			InvoicesTabPage.Dispose();
			PackingTabPage.Dispose();
			InvoiceLinesTabPage.Dispose();
			InvoiceGroupingTabPage.Dispose();
			MiscOptionsTabPage.Dispose();
			ContainerTabPage.Dispose();
			MessagesTabPage.Dispose();
		}

		new JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)base.JobDeclaration; }
		}

		protected override void OnJobDeclarationSet()
		{
			base.OnJobDeclarationSet();
			JobDeclaration.RunMergeForLVSIfRequired();
		}
	}
}
