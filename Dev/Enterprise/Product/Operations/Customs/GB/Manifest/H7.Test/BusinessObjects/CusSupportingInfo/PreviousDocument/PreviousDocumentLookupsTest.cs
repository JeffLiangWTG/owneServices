using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	[TestedType(typeof(PreviousDocumentLookups))]
	sealed class PreviousDocumentLookupsTest : TestCaseWithFactory
	{
		public void TestPreviousDocumentCodeList()
		{
			var bill = Factory.New<AsycudaBill>();
			var previousDocument = bill.PreviousDocuments.AddNew();
			AssertType<PreviousDocumentCodeListCDS>(previousDocument.Lookups.CodeList);
		}
	}
}
