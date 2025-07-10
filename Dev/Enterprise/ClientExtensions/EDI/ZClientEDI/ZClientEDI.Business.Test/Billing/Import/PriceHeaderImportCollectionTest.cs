using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(PriceHeaderImportCollection))]
	public class PriceHeaderImportCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PriceHeaderImportCollection>
	{
		protected override PriceHeaderImportCollection GetCollectionToTest()
		{
			return new PriceHeaderImportCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PriceHeaderImport();
		}
	}
}
