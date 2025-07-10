using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocCusEntryLineFeeCollection))]
	sealed class DocCusEntryLineCollectionFeeTestCase : NonPersistentBusinessObjectCollectionTestCase<DocCusEntryLineFeeCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var cusEntryLineFee = Factory.New<CusEntryLineFee>();
			return DocCusEntryLineFee.New(cusEntryLineFee, Factory);
		}

		protected override DocCusEntryLineFeeCollection GetCollectionToTest()
		{
			return new DocCusEntryLineFeeCollection(Factory);
		}
	}
}
