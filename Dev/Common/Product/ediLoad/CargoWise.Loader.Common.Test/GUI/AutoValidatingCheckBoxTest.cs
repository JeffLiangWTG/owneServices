using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	class AutoValidatingCheckBoxTest : TestCase
	{
		public void TestCheckedChangedValidates()
		{
			using (AutoValidatingCheckBox checkBox = new AutoValidatingCheckBox())
			{
				bool validated = false;
				checkBox.Validating += delegate
				{
					validated = true;
				};
				checkBox.Checked = true;
				AssertEquals("Validated", true, validated);
			}
		}
	}
}