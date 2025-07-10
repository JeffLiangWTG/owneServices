using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.ZArchitecture.PlugIn.Testing
{
	public abstract class ZPlugInGenericTest : TestCaseWithFactory
	{
		public void TestBashUserControlOfPlugIn()
		{
			using (var testPlugIn = GetPlugInToTest())
			{
				if (testPlugIn.HasUserControlInternal)
				{
					AssertNotNull("Business Entity is null", testPlugIn.BusinessEntity);
					var plugInForm = new TestPlugInForm(testPlugIn);

					var formBasher = new ZFormBasher(plugInForm);
					formBasher.BashFormWithoutMemoryChecking();
				}
				else
				{
					Assert(true);
				}
			}
		}

		protected abstract ZPlugIn GetPlugInToTest();

		class TestPlugInForm : ZForm
		{
			public TestPlugInForm(ZPlugIn testPlugIn) : base(testPlugIn.BusinessEntity)
			{
				var control = testPlugIn.UserControl;
				Controls.Add(control);
				control.Dock = System.Windows.Forms.DockStyle.Fill;
				const int MinScreenWidthSupported = 1024;
				const int MinScreenHeightSupported = 768;
				Size = new System.Drawing.Size(MinScreenWidthSupported, MinScreenHeightSupported);
				ControllerID = testPlugIn.ControllerID;
			}
		}
	}
}
