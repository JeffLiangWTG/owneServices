using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.CommissionManagement.Business.Test
{
	[TestedType(typeof(NonActiveCommissionAgreementOverallItemCollection))]
	class NonActiveCommissionAgreementOverallItemCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new NonActiveCommissionAgreementOverallItemCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ViewCommissionAgreementOverallItem>();
		}
	}
}
