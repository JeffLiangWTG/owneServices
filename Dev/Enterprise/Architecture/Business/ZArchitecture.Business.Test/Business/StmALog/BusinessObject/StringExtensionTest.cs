using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class StringExtensionTest : TestCase
	{
		public void TestInsertSpacesIntoPascalCasing()
		{
			var testString = new ZString("MAWBNumber");
			AssertEquals("MAWB Number", testString.InsertSpacesIntoPascalCasing());

			testString = "MAWBOriginIATAAirportCode";
			AssertEquals("MAWB Origin IATA Airport Code", testString.InsertSpacesIntoPascalCasing());

			testString = "MAWBNumberOfPieces";
			AssertEquals("MAWB Number Of Pieces", testString.InsertSpacesIntoPascalCasing());

			testString = "NumberOfPieces";
			AssertEquals("Number Of Pieces", testString.InsertSpacesIntoPascalCasing());

			testString = "anApple";
			AssertEquals("an Apple", testString.InsertSpacesIntoPascalCasing());

			testString = "aBike";
			AssertEquals("a Bike", testString.InsertSpacesIntoPascalCasing());

			testString = "Lorem ipsum dolor sit amet";
			AssertEquals("Lorem ipsum dolor sit amet", testString.InsertSpacesIntoPascalCasing());
		}
	}
}
