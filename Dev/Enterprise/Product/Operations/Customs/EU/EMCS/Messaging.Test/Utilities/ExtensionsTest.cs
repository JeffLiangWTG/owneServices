using System.Xml.Serialization;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(Enterprise.Core.Constants.CountryCodes))]

namespace Enterprise.Customs.EU.EMCS.Messaging.Testing
{
	class ExtensionsTest : TestCaseWithFactory
	{
		public void TestXmlEnumToString()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Returns Enum Name", "M", Gender.M.XmlEnumToString());
				AssertEquals("Returns Enum Attribute", "Female", Gender.F.XmlEnumToString());
				AssertEquals("Invalid Value", ZString.Empty, ((Gender)3).XmlEnumToString());
			});
		}

		public void TestGetCountryPrefix()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Country code for DETI001", Core.Constants.CountryCodes.Germany, Extensions.GetCountryPrefix("DETI001"));
				AssertEquals("Country code for elTI001", Core.Constants.CountryCodes.Greece, Extensions.GetCountryPrefix("elTI001"));
				AssertEquals("Country code for GRTI001", Core.Constants.CountryCodes.Greece, Extensions.GetCountryPrefix("GRTI001"));
			});
		}

		public void TestRemoveCountryPrefix()
		{
			var tradeId = "DETI001";
			AssertEquals("Removed country code", "TI001", tradeId.RemoveCountryPrefix());
		}

		public void TestGetAddress()
		{
			var streetName = "A Street";
			var streetNo = "9";
			AssertEquals("A Street 9", Extensions.GetAddress(streetName, streetNo));
		}

		enum Gender
		{
			M,
			[XmlEnum("Female")]
			F
		}
	}
}
