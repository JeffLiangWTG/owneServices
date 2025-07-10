using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	[TestedType(typeof(DocBaseCusEntryLineFeeCollection))]
	sealed class DocBaseCusEntryLineCollectionFeeTestCase : NonPersistentBusinessObjectCollectionTestCase<DocBaseCusEntryLineFeeCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var cusEntryLineFee = Factory.New<CusEntryLineFee>();
			return DocBaseCusEntryLineFee.New(cusEntryLineFee, Factory);
		}

		protected override DocBaseCusEntryLineFeeCollection GetCollectionToTest()
		{
			return new DocBaseCusEntryLineFeeCollection(Factory);
		}
	}
}
