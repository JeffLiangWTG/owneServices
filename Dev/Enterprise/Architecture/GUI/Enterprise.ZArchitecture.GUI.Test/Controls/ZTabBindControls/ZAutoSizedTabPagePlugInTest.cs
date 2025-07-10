using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.ZArchitecture.GUI
{
	sealed class ZAutoSizedTabPagePlugInTest : TestCaseWithFactory
	{
		public void TestIsAutoSized()
		{
			AssertEquals(true, TabPage.IsAutoSized);
		}

		#region Test Classes

		class TestPlugIn : ZPlugIn
		{
			public TestPlugIn() : base(null)
			{
			}

			protected internal override ZBool HasUserControl
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

		TestPlugIn PlugIn
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

		ZAutoSizedTabPagePlugIn TabPage
		{
			get
			{
				if (tabPage == null)
				{
					tabPage = new ZAutoSizedTabPagePlugIn(PlugIn);
				}
				return tabPage;
			}
		}
		ZAutoSizedTabPagePlugIn tabPage;

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
