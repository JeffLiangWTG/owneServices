using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SpecificCircumstanceIndicatorValidDataCombinationTest : TestCaseWithFactory
	{
		public void TestMatch()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var validDataCombination = new SpecificCircumstanceIndicatorValidDataCombination(ApplicationCodeTypeList.Codes.ShippingLine, new string[] { TransportModes.Sea });

			AssertMatch("return false when Manifest Type not matched",
				ApplicationCodeTypeList.Codes.Consolidator,
				TransportModes.Sea,
				false);

			AssertMatch("return false when Transport Mode not matched",
				ApplicationCodeTypeList.Codes.ShippingLine,
				TransportModes.Air,
				false);

			AssertMatch("return true when Manifest Type and Transport Mode are all matched",
				ApplicationCodeTypeList.Codes.ShippingLine,
				TransportModes.Sea,
				true);

			void AssertMatch(string message, string applicationCode, string transportMode, bool isMatched)
			{
				header.AMA_ApplicationCode = applicationCode;
				header.AMA_TransportMode = transportMode;
				AssertEquals(message, isMatched, validDataCombination.Match(header));
			}
		}
	}
}
