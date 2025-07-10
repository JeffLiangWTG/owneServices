using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class ItemPackagesTabUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestMarksAndNumbersTextBoxAllowsNormalCase()
		{
			using (var control = new ItemPackagesTabUserControl())
			{
				var editControl = control.FindSingle<ZTextBox>("MarksAndNumbersTextBox");
				AssertEquals("normal casing", CharacterCasing.Normal, editControl.CharacterCasing);
			}
		}

		public void TestB5_MarksAndNumbersGridColumnAllowsNormalCase()
		{
			using (var control = new ItemPackagesTabUserControl())
			{
				var grid = control.FindSingle<ZGrid>("PackagesGrid");
				AssertEquals("normal casing", CharacterCasing.Normal, grid.GetColumnStyle(NctsPackage.Schema.B5_MarksAndNumbers).CharacterCasing);
			}
		}

		public void TestPanel2MinSize()
		{
			using (var control = new ItemPackagesTabUserControl())
			{
				var splitContainer = control.FindSingle<KSplitContainer>("PackagesSplitContainer");
				AssertEquals(105, splitContainer.Panel2MinSize);
			}
		}
	}
}
