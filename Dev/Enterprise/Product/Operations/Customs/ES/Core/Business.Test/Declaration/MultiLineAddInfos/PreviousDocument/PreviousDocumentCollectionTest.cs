using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(PreviousDocumentCollection))]
	class PreviousDocumentCollectionTest : EU.Business.Declaration.MultiLineAddInfos.Testing.PreviousDocumentCollectionTest
	{
		public void TestTypedSingleParameterAddNew()
		{
			var bizO = GetCollectionToTest().AddNew(typeof(PreviousDocument));
			AssertNotNull(bizO);
			AssertType<PreviousDocument>(bizO);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new PreviousDocumentCollection(PreviousDocument);

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<PreviousDocument>();

		protected override CusSupportingInfoCollection<EU.Business.Declaration.MultiLineAddInfos.PreviousDocument> GetCusSupportingInfoCollection()
			=> new PreviousDocumentCollection(Factory.New<JobDeclaration>());

		PreviousDocument PreviousDocument => previousDocument ?? (previousDocument = Factory.New<PreviousDocument>());
		PreviousDocument previousDocument;
	}
}
