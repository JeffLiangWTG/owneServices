using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(DocEntryFeeCollection))]
	sealed class DocEntryFeeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocEntryFeeCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection() => DocEntryHeaderFee.New(new EntryFee(), Factory);

		protected override DocEntryFeeCollection GetCollectionToTest() => new DocEntryFeeCollection(Factory);
	}
}

