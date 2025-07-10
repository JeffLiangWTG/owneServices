using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class AccPayableOrderFilterControl : ZFilterStripControl
	{
		public AccPayableOrderFilterControl(AccPayableOrderHeaderCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, gridCollection, Core.Constants.DocManagerCodes.Order);
		}

		#region Grid Layout Persister

		protected CustomLabelsGridLayoutPersister fGridLayoutPersister;

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (fGridLayoutPersister != null)
				{
					fGridLayoutPersister.Dispose();
				}
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}

