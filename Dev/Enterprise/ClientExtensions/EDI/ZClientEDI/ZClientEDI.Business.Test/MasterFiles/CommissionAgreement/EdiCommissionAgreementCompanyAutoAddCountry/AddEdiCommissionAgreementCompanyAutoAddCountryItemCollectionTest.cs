using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(AddEdiCommissionAgreementCompanyAutoAddCountryItemCollection))]
	class AddEdiCommissionAgreementCompanyAutoAddCountryItemCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AddEdiCommissionAgreementCompanyAutoAddCountryItemCollection>
	{
		protected override AddEdiCommissionAgreementCompanyAutoAddCountryItemCollection GetCollectionToTest()
		{
			return new AddEdiCommissionAgreementCompanyAutoAddCountryItemCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AddEdiCommissionAgreementCompanyAutoAddCountryItem(Factory);
		}
	}
}
