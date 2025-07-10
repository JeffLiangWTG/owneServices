using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocCusEntryLineCollection))]
	sealed class DocCusEntryLineCollectionTestCase : NonPersistentBusinessObjectCollectionTestCase<DocCusEntryLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var cusEntryLine = Factory.New<CusEntryLine>();
			return DocCusEntryLine.New(cusEntryLine, Factory);
		}

		protected override DocCusEntryLineCollection GetCollectionToTest()
		{
			return new DocCusEntryLineCollection(Factory);
		}
	}
}
