using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	[TestedType(typeof(DocBaseCusEntryHeaderChargesCollection))]
	sealed class DocBaseCusEntryHeaderChargesCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocBaseCusEntryHeaderChargesCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var cusEntryHeaderCharge = Factory.New<CusEntryHeaderCharges>();
			return DocBaseCusEntryHeaderCharges.New(cusEntryHeaderCharge, Factory);
		}

		protected override DocBaseCusEntryHeaderChargesCollection GetCollectionToTest() => new DocBaseCusEntryHeaderChargesCollection(Factory);
	}
}
