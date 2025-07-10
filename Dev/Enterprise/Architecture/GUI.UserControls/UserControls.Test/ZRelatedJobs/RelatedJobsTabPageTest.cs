using System;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class RelatedJobsTabPageTest : ZTabPageControlTest
	{
		#region TestExcludeFromBindingOnSave

		public void TestExcludeFromBindingOnSave()
		{
			AssertEquals("For performance don't bind on save", true, TestTabPage.ExcludeFromBindingOnSave);
		}

		#endregion

		#region TestRelatedJobsUserControl

		public void TestRelatedJobsUserControl()
		{
			Form.Controls.Add(TestTabControl);
			TestTabPage.TabVisible = true;

			Form.Show();
			AssertNotNull(TestTabPage.RelatedJobsUserControl);
		}

		#endregion

		#region Implementation

		protected override void TearDown()
		{
			if (form != null)
			{
				form.Dispose();
			}
			base.TearDown();
		}

		protected override Type TypeOfDummy
		{
			get { return typeof(DummyBusinessObjectWithRelatedJobs); }
		}

		protected override ZTabPage NewTabPage()
		{
			return new RelatedJobsTabPage();
		}

		new RelatedJobsTabPage TestTabPage
		{
			get { return (RelatedJobsTabPage)base.TestTabPage; }
		}

		ZForm Form
		{
			get { return form ?? (form = new ZForm(Dummy)); }
		}

		ZForm form;

		#endregion
	}
}
