using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	internal abstract class RelatedJobsTest : TestCaseWithFactory
	{
		#region Implementation

		protected override void TearDown()
		{
			if (form != null)
			{
				form.Dispose();
			}
			if (jobsUserControl != null)
			{
				jobsUserControl.Dispose();
			}

			base.TearDown();
		}

		protected void DoubleClickGrid(Point clickLocation)
		{
			int noClicks = 2;
			MethodInfo onMouseDownMethod = typeof(Control).GetMethod("OnMouseDown", BindingFlags.Instance | BindingFlags.NonPublic);
			onMouseDownMethod.Invoke(Grid, new object[] { new MouseEventArgs(MouseButtons.Left, noClicks, clickLocation.X, clickLocation.Y, 0) });
		}

		protected ZForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZForm(Dummy);
					form.Controls.Add(JobsUserControl);
				}
				return form;
			}
		}

		protected RelatedJobsGrid Grid
		{
			get { return JobsUserControl.RelatedJobsGrid; }
		}

		protected RelatedJobsUserControl JobsUserControl
		{
			get { return jobsUserControl ?? (jobsUserControl = new RelatedJobsUserControl()); }
		}

		protected DummyBusinessObjectWithRelatedJobs Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyBusinessObjectWithRelatedJobs>()); }
		}

		ZForm form;
		RelatedJobsUserControl jobsUserControl;
		DummyBusinessObjectWithRelatedJobs dummy;

		#endregion
	}
}
