using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DocCusEntryLineCollection))]
	sealed class DocCusEntryLineCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocCusEntryLineCollection>
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
