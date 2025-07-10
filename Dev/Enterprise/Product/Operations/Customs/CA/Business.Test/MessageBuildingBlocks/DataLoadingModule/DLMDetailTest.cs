using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageBuildingBlocks
{
	sealed class DLMDetailTest : TestCase
	{
		public void TestSerialise()
		{
			DLMDetail detail = new DLMDetail();
			detail.CountryOfOrigin = "Canada";
			detail.ProvinceOfOrigin = "British Columbia";
			detail.HarmonizedSystemCode = "87019010";
			detail.ProductDescription = "Tracteur";
			detail.ConveyanceIdentificationNumber = "ID#123";
			detail.Quantity = 10.231m;
			detail.UnitOfMeasure = "Number";
			detail.ValueFOBPointOfExit = 5000.33m;

			string expectedMessage = "D" +
				new string(' ', 2) +
				"Canada".PadRight(20) +
				new string(' ', 2) +
				"British Columbia".PadRight(30) +
				"87019010  " +
				"Tracteur".PadRight(255) +
				"ID#123".PadRight(30) +
				new string(' ', 14) +
				new string(' ', 3) +
				new string(' ', 50) +
				"10.231".PadRight(14) +
				new string(' ', 3) +
				"Number".PadRight(50) +
				"5000.33".PadRight(16);

			AssertMultilineASCIIEquals("Detail", expectedMessage, detail.Serialise());
		}
	}
}
