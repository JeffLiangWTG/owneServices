using System;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestsSubclassesOf(typeof(ZModuleButtonGrid))]
	public abstract class ZModuleButtonGridTestBase : TransactionedTestCase
	{
		public void TestZModuleButtonGridImages()
		{
			using (var tester = GetModuleButtonGrid())
			{
				var toolStrip = (ZToolStrip)tester.Controls.Find("toolStrip", true)[0];
				foreach (var button in toolStrip.Items.Cast<ToolStripItem>())
				{
					AssertNotNull("Image should exist in the button", button.Image);
				}
			}
		}

		protected virtual ZModuleButtonGrid GetModuleButtonGrid() =>
			(ZModuleButtonGrid)Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType()));
	}
}
