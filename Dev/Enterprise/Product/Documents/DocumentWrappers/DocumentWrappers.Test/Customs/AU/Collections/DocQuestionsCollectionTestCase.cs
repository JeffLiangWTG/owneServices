using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocCMRCusEntryCPDecCollection))]
	sealed class DocQuestionsCollectionTestCase : NonPersistentBusinessObjectCollectionTestCase<DocCMRCusEntryCPDecCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var cMRCusEntryCPDec = Factory.New<CMRCusEntryCPDec>();
			return DocCMRCusEntryCPDec.New(cMRCusEntryCPDec, Factory);
		}

		protected override DocCMRCusEntryCPDecCollection GetCollectionToTest()
		{
			return new DocCMRCusEntryCPDecCollection(Factory);
		}
	}
}
