using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EdiCommissionAgreementCompanyAutoAddDatabaseCollection))]
	class EdiCommissionAgreementCompanyAutoAddDatabaseCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiCommissionAgreementCompanyAutoAddDatabaseCollection>
	{
		protected override EdiCommissionAgreementCompanyAutoAddDatabaseCollection GetCollectionToTest()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			return new EdiCommissionAgreementCompanyAutoAddDatabaseCollection(customization);
		}
	}
}
