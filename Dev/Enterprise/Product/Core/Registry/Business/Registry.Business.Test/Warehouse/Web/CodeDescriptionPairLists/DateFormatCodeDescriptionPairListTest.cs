using CargoWise.Integration;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class DateFormatCodeDescriptionPairListTest : TestCase
	{
		public void TestList()
		{
			DateFormatCodeDescriptionPairList list = new DateFormatCodeDescriptionPairList();
			AssertEquals(2, list.Count);

			AssertNotNull("The list must contain the code for the Standard theme.", list["STD"]);
			AssertNotNull("The list must contain the code for the Alternate theme.", list["LOC"]);

			AssertEquals("DD-MMM-YY in English", list["STD"].Description);
			AssertEquals("DD-MMM-YY in locale of web user", list["LOC"].Description);

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
