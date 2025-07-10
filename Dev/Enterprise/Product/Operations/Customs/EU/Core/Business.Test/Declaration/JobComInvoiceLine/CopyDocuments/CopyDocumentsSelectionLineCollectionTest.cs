using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(CopyDocumentsSelectionLineCollection))]
	sealed class CopyDocumentsSelectionLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CopyDocumentsSelectionLineCollection>
	{
		protected override CopyDocumentsSelectionLineCollection GetCollectionToTest()
		{
			return new CopyDocumentsSelectionLineCollection(Factory.New<JobComInvoiceLine>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CopyDocumentsSelectionLine(Factory.New<SupportingDocument>());
		}
	}
}
