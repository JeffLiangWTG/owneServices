using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.JobManagement
{
	public partial class JobInvoicingForm : ZEditForm
	{
		readonly IBusiness ParentEntity;
		public JobInvoicingForm(Job job) : base(job)
		{
			ParentEntity = job.Parent as IBusiness;
			PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.JobInvoicingForm, null, requestedTabPageIndexDeterminer: () => -1, getBusinessEntityOverride: () => ParentEntity);

			var plugIn = PlugIns.GetPlugIn(ControllerIDs.JobInvoicingForm) as InvoicingFormPlugin;
			plugIn.OnUserControlShown();

			CaptionRenderingEnabled = true;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();

			((IFileMenuItemsProvider)this).ActionsMenuItem.Visible = false;
		}

		protected override void SaveToRecentItems()
		{
		}

		protected override void RemoveFromRecentItems()
		{
		}

		#region GUI Setup

		public override string FormVerb => string.Empty;

		protected override bool AllowNew => false;

		public override string FormCaption => ParentEntity.HumanReadableName + (NoResString)" Job Invoicing";

		#endregion

	}
}

