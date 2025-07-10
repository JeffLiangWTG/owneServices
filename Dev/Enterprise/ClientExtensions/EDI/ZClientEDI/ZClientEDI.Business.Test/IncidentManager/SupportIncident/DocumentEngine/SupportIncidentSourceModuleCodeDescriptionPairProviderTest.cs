using System.Globalization;
using System.Linq;
using CargoWise.Integration;
using Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class SupportIncidentSourceModuleCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new SupportIncidentSourceModuleCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			AssertContainsExactElementsInAnyOrder(pair => string.Format(CultureInfo.CurrentCulture, "Code: {0} Description: {1}", pair.Code, pair.Description), SupportIncidentLookups.GetNewSearchableSourceModuleList().Cast<ICodeDescription>(), CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList().Cast<ICodeDescription>());
		}
	}
}