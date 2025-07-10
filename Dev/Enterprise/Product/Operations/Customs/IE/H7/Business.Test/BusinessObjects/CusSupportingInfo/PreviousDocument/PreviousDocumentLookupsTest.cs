using CargoWise.Types;
using Enterprise.Customs.EU.H7.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	[TestedType(typeof(PreviousDocumentLookups))]
	sealed class PreviousDocumentLookupsTest : PreviousDocumentLookupsAbstractTest
	{
		protected override EU.H7.Business.PreviousDocumentLookups GetLookupsForTesting()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = "LV2";
			var previousDocument = header.PreviousDocuments.AddNew();

			return previousDocument.Lookups;
		}

		protected override ZString ExpectedDataGrouping => Core.Constants.CountryCodes.Ireland;
	}
}
