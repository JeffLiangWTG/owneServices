using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.DevTools;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Core.DevTools.Testing
{
	public class DeveloperDiagnosticsFormTest : TestCase
	{
		public void TestButtonHeightIsScaled()
		{
			int closeButtonOriginalSize;
			int showButtonOriginalSize;
			using (ControlDpiScalingHelper.OverrideDPI_ForTesting(100, 100))
			{
				using (var someForm = new ZForm())
				using (var testForm = new DeveloperDiagnosticsForm(someForm, new List<IDevTool>()))
				{
					var closeButton = (Button)testForm.CancelButton;
					AssertNotNull("Precondition", closeButton);
					closeButtonOriginalSize = closeButton.Height;
					var showButton = (Button)testForm.AcceptButton;
					AssertNotNull("Precondition", showButton);
					showButtonOriginalSize = showButton.Height;
				}
			}

			CheckButtonScaling(125, closeButtonOriginalSize, showButtonOriginalSize);
			CheckButtonScaling(350, closeButtonOriginalSize, showButtonOriginalSize);
		}

		void CheckButtonScaling(int scale, int closeButtonOriginalSize, int showButtonOriginalSize)
		{
			using (ControlDpiScalingHelper.OverrideDPI_ForTesting(scale, scale))
			{
				using (var someForm = new ZForm())
				using (var testForm = new DeveloperDiagnosticsForm(someForm, new List<IDevTool>()))
				{
					var closeButton = (Button)testForm.CancelButton;
					AssertCorrectSize(closeButtonOriginalSize * scale / 100, closeButton.Height);
					var showButton = (Button)testForm.AcceptButton;
					AssertCorrectSize(showButtonOriginalSize * scale / 100, showButton.Height);
				}
			}
		}

		void AssertCorrectSize(int expected, int actual, int margin = 1)
		{
			Assert($"Expected: {expected}+-{margin}, but was: {actual}", actual >= expected - margin && actual <= expected + margin);
		}
	}
}
