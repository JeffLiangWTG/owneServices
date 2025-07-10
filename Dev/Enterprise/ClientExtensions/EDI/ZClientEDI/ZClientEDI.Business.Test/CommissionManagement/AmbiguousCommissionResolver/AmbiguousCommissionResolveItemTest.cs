using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.CommissionManagement.Business.Test
{
	[TestedType(typeof(AmbiguousCommissionResolveItem))]
	internal class AmbiguousCommissionResolveItemTest : NonPersistentBusinessObjectTestCase
	{
		#region Default Values

		public void TestDefaultValues()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			var ambiguousCommission = Factory.New<AccAmbiguousCommission>();
			ambiguousCommission.AC0_CA0_SelectedAgreement = agreement.PK;
			var item = new AmbiguousCommissionResolveItem(ambiguousCommission);

			AssertEquals(agreement.PK, item.SelectedAgreementPk);
		}

		#endregion

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var ambiguousCommission = Factory.NewWithValidTestData<AccAmbiguousCommission>();
			return new AmbiguousCommissionResolveItem(ambiguousCommission);
		}

		#endregion
	}
}
