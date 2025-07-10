using System.IO;
using CargoWise.Bi.Development.Automation.Manager;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Module.GUI.Testing
{
	public class SettingsPopupTest : TestCase
	{
		[RequiresSTA]
		public void TestFormLoad()
		{
			var tempPath = Path.Combine(TempForTest.TempPath, "CargoWise.DbUpgrader");
			Directory.CreateDirectory(tempPath);
			var form = new SettingsPopup("localhost", "localhost", tempPath);
			AssertNotNull(form);
			form.Show();
			form.Close();
			form.Dispose();
			Directory.Delete(tempPath, true);
		}
	}
}
