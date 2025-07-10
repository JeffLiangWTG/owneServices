using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class ThemeCodeDescriptionPairListTest : TestCase
	{
		public void TestList()
		{
			ThemeCodeDescriptionPairList list = new ThemeCodeDescriptionPairList();
			AssertEquals(4, list.Count);

			AssertNotNull("The list must contain the code for the Standard theme.", list["STD"]);
			AssertNotNull("The list must contain the code for the Alternate theme.", list["ALT"]);
			AssertNotNull("The list must contain the code for the Classic theme.", list["CLS"]);
			AssertNotNull("The list must contain the code for the Custom theme.", list["CUS"]);

			AssertEquals("The description for the Standard theme must be 'Standard', as it represents a folder name in the App_Themes directory in the Tracking.Web project.",
				"Standard", list["STD"].Description);
			AssertEquals("The description for the Alternate theme must be 'Alternate', as it represents a folder name in the App_Themes directory in the Tracking.Web project.",
				"Alternate", list["ALT"].Description);
			AssertEquals("The description for the Classic theme must be 'Classic', as it represents a folder name in the App_Themes directory in the Tracking.Web project.",
				"Classic", list["CLS"].Description);
			AssertEquals("The description for the Custom theme must be 'Custom'.",
				"Custom", list["CUS"].Description);

			foreach (ICodeDescription codePair1 in list)
			{
				foreach (ICodeDescription codePair2 in list)
				{
					if (codePair1.Description == codePair2.Description && codePair1.Code != codePair2.Code)
					{
						Fail("All of the descriptions in the list must be unique.");
					}
				}
			}
		}
	}
}
