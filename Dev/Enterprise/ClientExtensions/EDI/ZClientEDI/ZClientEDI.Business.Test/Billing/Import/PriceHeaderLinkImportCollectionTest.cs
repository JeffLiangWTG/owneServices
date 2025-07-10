using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(PriceHeaderLinkImportCollection))]
	public class PriceHeaderLinkImportCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PriceHeaderLinkImportCollection>
	{
		protected override PriceHeaderLinkImportCollection GetCollectionToTest()
		{
			return new PriceHeaderLinkImportCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PriceHeaderLinkImport();
		}
	}
}
