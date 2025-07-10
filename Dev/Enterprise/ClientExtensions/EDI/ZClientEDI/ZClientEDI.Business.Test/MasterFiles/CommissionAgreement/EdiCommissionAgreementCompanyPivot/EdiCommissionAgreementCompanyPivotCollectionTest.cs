using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EdiCommissionAgreementCompanyPivotCollection))]
	class EdiCommissionAgreementCompanyPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiCommissionAgreementCompanyPivotCollection>
	{
		#region Overrides

		protected override EdiCommissionAgreementCompanyPivotCollection GetCollectionToTest()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			return new EdiCommissionAgreementCompanyPivotCollection(customization);
		}

		#endregion
	}
}
