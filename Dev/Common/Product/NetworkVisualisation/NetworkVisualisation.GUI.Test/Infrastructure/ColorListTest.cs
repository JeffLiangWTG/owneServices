using System.Linq;
using System.Reflection;
using System.Windows.Media;
using CargoWise.NetworkVisualisation.Business;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test.Infrastructure
{
	public class ColorListTest : TestCase
	{
		public void TestColorListIsConsistentBetweenBusinessAndGUI()
		{
			// The ColorList data is populated from colors in System.Drawing but when used in WPF is using the colors from System.Media
			// This test will ensure that all the color names are consistent between the Business and GUI projects
			var wpfColors = typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static).Where(p => p.PropertyType == typeof(Color)).OrderBy(p => p.Name).Select(c => c.Name);
			var colorList = new ColorList().Select(c => c.Color.Name);

			var missingFromSystemMedia = colorList.Except(wpfColors);
			AssertEquals($"The following colors were present in System.Drawing but not System.Media (WPF): \r\n {string.Join("\r\n", missingFromSystemMedia)}", 0, missingFromSystemMedia.Count());

			var missingFromSystemDrawing = wpfColors.Except(colorList);
			AssertEquals($"The following colors were present in System.Media (WPF) but not System.Drawing: \r\n {string.Join("\r\n", missingFromSystemDrawing)}", 0, missingFromSystemDrawing.Count());
		}
	}
}
