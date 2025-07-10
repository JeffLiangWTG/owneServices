using System;
using System.Windows.Forms;

using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class RelatedJobsUserControl : ZUserControl
	{
		public RelatedJobsUserControl()
		{
			InitializeComponent();

			Dock = DockStyle.Fill;
			ControlExtensions.SetBindingMember(this, "RelatedJobs");
			RelatedJobsGrid.RowDoubleClick += new EventHandler(RelatedJobsGrid_RowDoubleClick);
			RelatedJobsGrid.RowChanged += new EventHandler(RelatedJobsGrid_RowChanged);
		}

		#region Edit Button Enabled State

		void RelatedJobsGrid_RowChanged(object sender, EventArgs e)
		{
			UpdateEditButtonEnabledState();
		}

		void UpdateEditButtonEnabledState()
		{
			EditJobButton.Enabled = (RelatedJobsGrid.SelectedJob != null);
		}

		#endregion

		#region Edit Button Click / Row Double-Click

		public bool ShouldShowFormAsDialog { get; set; }

		void RelatedJobsGrid_RowDoubleClick(object sender, EventArgs e)
		{
			EditSelectedJob();
		}

		void EditJobButton_Click(object sender, EventArgs e)
		{
			EditSelectedJob();
		}

		void EditSelectedJob()
		{
			BusinessObject selectedJob = RelatedJobsGrid.SelectedJob as BusinessObject;

			if (selectedJob != null)
			{
				ZController controller = ZControllerFactory.Create(RelatedJobsGrid.SelectedJob.ControllerID);
				controller.ShowChildrenAsDialog = ShouldShowFormAsDialog;

#if DEBUG
				ControllerForTest = controller;
#endif

				if (DataSource is RelatedJobCollection collection)
				{
					var form = (controller as ZControllerInternals).GetForm(selectedJob);
					var moduleResultsBusinessObject = new ModuleResultsBusinessObject(new ZPKCollection(collection.GetPKs()));
					moduleResultsBusinessObject.CurrentPK = selectedJob.PK;
					form.ModuleResultsBusinessObject = moduleResultsBusinessObject;
					form.ControllerID = controller.ID;
					ZFormModaliser.ShowDialogAndDispose(form as Form, ParentForm);
				}
				else
				{
					if (selectedJob.IsInDatabase)
					{
						controller.ShowEditForm(selectedJob);
					}
					else
					{
						controller.ShowFormForNewEntity(selectedJob);
					}
				}
			}
		}

#if DEBUG
		internal ZController ControllerForTest;
#endif

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				RelatedJobsGrid.RowDoubleClick -= new EventHandler(RelatedJobsGrid_RowDoubleClick);
				RelatedJobsGrid.RowChanged -= new EventHandler(RelatedJobsGrid_RowChanged);
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
