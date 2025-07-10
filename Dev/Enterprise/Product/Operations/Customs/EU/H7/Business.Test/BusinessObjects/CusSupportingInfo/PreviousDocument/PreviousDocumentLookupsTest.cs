using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	[TestedType(typeof(PreviousDocumentLookups))]
	sealed class PreviousDocumentLookupsTest : PreviousDocumentLookupsAbstractTest
	{
		protected override PreviousDocumentLookups GetLookupsForTesting()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var previousDocument = header.PreviousDocuments.AddNew();

			return previousDocument.Lookups;
		}

		protected override ZString ExpectedDataGrouping => Core.Constants.CountryCodes.Latvia;
	}
}
