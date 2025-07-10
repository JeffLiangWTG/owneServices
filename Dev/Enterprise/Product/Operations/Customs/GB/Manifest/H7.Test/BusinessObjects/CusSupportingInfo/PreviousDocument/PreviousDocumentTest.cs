using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	[TestedType(typeof(PreviousDocument))]
	sealed class PreviousDocumentTest : EU.H7.Business.Testing.PreviousDocumentTest<PreviousDocument>
	{
		public void TestLookups()
		{
			var bill = Factory.New<AsycudaBill>();
			var documents = bill.PreviousDocuments;
			var previousDocument = documents.AddNew();
			AssertType<PreviousDocumentLookups>(previousDocument.Lookups);
		}

		public void TestPreviousDocumentDescription()
		{
			var bill = Factory.New<AsycudaBill>();
			var documents = bill.PreviousDocuments;
			var previousDocument = documents.AddNew();
			AssertEquals(string.Empty, previousDocument.DocumentDescription);

			previousDocument.CSI_Code = "AAD";
			AssertEquals("Administrative Accompanying Document", previousDocument.DocumentDescription);

			previousDocument.CSI_Code = "740";
			AssertEquals("Air Waybill", previousDocument.DocumentDescription);

			previousDocument.CSI_Code = "NON";
			AssertEquals(string.Empty, previousDocument.DocumentDescription);
		}

		public void TestSubType()
		{
			var bill = Factory.New<AsycudaBill>();
			var documents = bill.PreviousDocuments;
			var previousDocument = documents.AddNew();
			AssertEquals("Z", previousDocument.CSI_SubType);
		}

		protected override IEnumerable<PreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			yield return bill.PreviousDocuments.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			return bill.PreviousDocuments.AddNew();
		}
	}
}
