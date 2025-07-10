using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EdiCommissionAgreementDatabasePivotCollection))]
	class EdiCommissionAgreementDatabasePivotCollectionTest : ActiveBusinessObjectCollectionTestCase<EdiCommissionAgreementDatabasePivotCollection>
	{
		#region Overrides

		protected override EdiCommissionAgreementDatabasePivotCollection GetCollectionToTest()
		{
			var customization = Factory.New<EdiCommissionAgreementCustomization>();
			return new EdiCommissionAgreementDatabasePivotCollection(customization);
		}

		#endregion
	}
}
