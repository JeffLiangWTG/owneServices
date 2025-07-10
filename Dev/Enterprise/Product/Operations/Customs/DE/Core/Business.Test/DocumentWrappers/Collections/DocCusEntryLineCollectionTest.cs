using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using CusEntryLine = Enterprise.Customs.DE.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.DE.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocCusEntryLineCollection))]
	sealed class DocCusEntryLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocCusEntryLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection() => DocCusEntryLine.New(Factory.New<CusEntryLine>(), Factory);

		protected override DocCusEntryLineCollection GetCollectionToTest() => new DocCusEntryLineCollection(Factory);
	}
}
