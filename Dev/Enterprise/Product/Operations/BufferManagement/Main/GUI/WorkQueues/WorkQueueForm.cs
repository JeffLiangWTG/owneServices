using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.GUI.Tags;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class WorkQueueForm : ZTemplateForm, INavigableTagForm
	{
		public WorkQueueForm(WorkQueue queue)
			: base(queue)
		{
			InitializeComponent();
		}

		public new WorkQueue DataSource
		{
			get { return (WorkQueue)base.DataSource; }
		}

		public void NavigateToTagMagnitude(BusinessObject objectToNavigateTo)
		{
			WorkQueueControl.NavigateToTagMagnitude(objectToNavigateTo);
		}

		#region ZForm Overrides

		public override string FormCaption
		{
			get
			{
				var text = Res.GetString("707946a5-db69-4fac-a637-454a43a5af34", "Work Queue");
				var queue = DataSource;
				if (queue != null && !queue.TGM_Description.IsEmpty)
				{
					text += " " + queue.TGM_Description;
				}

				return text;
			}
		}

		protected override bool SupportsEDocs => false;

		protected override bool ShowAuditTab => true;

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					if (components != null)
					{
						components.Dispose();
					}
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		#endregion
	}
}
