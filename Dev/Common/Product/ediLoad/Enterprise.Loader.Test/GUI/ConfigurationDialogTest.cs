using System.Windows.Forms;
using CargoWise.Loader.Common;
using NUnit.Framework;

namespace Enterprise.Loader.Testing.GUI
{
	class ConfigurationDialogTest : TestCase
	{
		public void TestAutoScaleModeIsFront()
		{
			// Arrange
			var form = new ConfigurationDialog(new Installation(new EnterpriseConfiguration()));

			// Act
			// Assert
			AssertEquals(AutoScaleMode.Font, form.AutoScaleMode);
		}

		public void TestLabelAutoSizeIsAuto()
		{
			// Arrange
			// Act
			using (var form = new ConfigurationDialog(new Installation(new EnterpriseConfiguration())))
			{
				// Assert
				AssertEquals(true, form.serverNameLabel.AutoSize);
				AssertEquals(true, form.databaseNameLabel.AutoSize);
			}
		}
	}
}
