using NUnit.Framework;

namespace CargoWise.Windows.UI.Design.Testing
{
	class KTabControlDesignerTest : TestCase
	{
		public void TestDotNetVersionForRippedDataGridViewDesigner()
		{
			AssertEquals(@"If a new version of the .net base class library is in use, you'll need to re-rip the base class " + @"(System.Windows.Forms.Design.Ripped.TabControlDesigner).", true, typeof(System.Windows.Forms.Design.DocumentDesigner).Assembly.FullName.Contains("4.0.0.0"));
		}

		public void TestTabPageType()
		{
			Designer.Initialize(TabControl);
			AssertEquals(typeof(KTabPage), Designer.ExternalTabPageType);
		}

		KTabControl TabControl
		{
			get
			{
				return tabControl ?? (tabControl = new KTabControl());
			}
		}

		KTabControl tabControl;
		KTabControlDesigner Designer
		{
			get
			{
				return designer ?? (designer = new KTabControlDesigner());
			}
		}

		KTabControlDesigner designer;
	}
}
