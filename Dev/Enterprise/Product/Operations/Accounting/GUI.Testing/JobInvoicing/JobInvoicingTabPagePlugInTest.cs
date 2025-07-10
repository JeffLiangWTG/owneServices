using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class JobInvoicingTabPagePlugInTest : TestCaseWithFactory
	{
		public void TestAutoSizing()
		{
			AssertEquals(true, TabPage.IsAutoSized_ForTestOnly);
		}

		#region Test Classes

		protected class TestPlugIn : ZPlugIn
		{
			public TestPlugIn()
				: base(null)
			{
			}

			protected override ZBool HasUserControl
			{
				get { return false; }
			}

			protected override LicenceCheckpoint LicenceCheckPoint
			{
				get { return null; }
			}

			public override string Name
			{
				get { return null; }
			}
		}

		#endregion

		#region Implementation

		protected TestPlugIn PlugIn
		{
			get
			{
				if (plugIn == null)
				{
					plugIn = new TestPlugIn();
				}
				return plugIn;
			}
		}
		TestPlugIn plugIn;

		protected JobInvoicingTabPagePlugIn TabPage
		{
			get
			{
				if (tabPage == null)
				{
					tabPage = new JobInvoicingTabPagePlugIn(PlugIn);
				}
				return tabPage;
			}
		}
		JobInvoicingTabPagePlugIn tabPage;

		protected override void TearDown()
		{
			base.TearDown();
			if (tabPage != null)
			{
				tabPage.Dispose();
			}
			if (plugIn != null)
			{
				plugIn.Dispose();
			}
		}

		#endregion
	}
}
