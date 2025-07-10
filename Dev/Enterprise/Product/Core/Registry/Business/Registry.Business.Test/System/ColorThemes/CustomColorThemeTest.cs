using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CustomColorTheme))]
	sealed class CustomColorThemeTest : NonPersistentBusinessObjectTestCase
	{
		public void TestInitialisation()
		{
			CustomColorTheme custom1 = new CustomColorTheme(1);
			AssertEquals("Custom 1", custom1.Name);
			AssertEquals(true, custom1.CanBeModified);
		}

		public void TestUpdateColor()
		{
			CustomColorTheme custom = new CustomColorTheme(1);

			AssertNotEquals(Color.Red.ToArgb(), custom.MainFormBackgroundColor.ToArgb());
			custom.UpdateColor("MainFormBackgroundColor", Color.Red);
			AssertEquals(Color.Red.ToArgb(), custom.MainFormBackgroundColor.ToArgb());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CustomColorTheme(10);
		}
	}
}
