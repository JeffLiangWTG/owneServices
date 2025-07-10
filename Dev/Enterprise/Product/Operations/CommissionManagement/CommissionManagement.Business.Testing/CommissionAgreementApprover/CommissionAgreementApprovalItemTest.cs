using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.CommissionManagement.Business.Testing
{
	[TestedType(typeof(CommissionAgreementApprovalItem))]
	public class CommissionAgreementApprovalItemTest : NonPersistentBusinessObjectTestCase
	{
		#region HumanReadableName

		public void TestHumanReadableName()
		{
			var item = new CommissionAgreementApprovalItem(new CommissionAgreementApprovalWizard(Factory), Factory.New<OrgCommissionAgreement>());
			AssertEquals("Commission Agreement ", item.HumanReadableName);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CommissionAgreementApprovalItem(new CommissionAgreementApprovalWizard(Factory), Factory.New<OrgCommissionAgreement>());
		}

		#endregion
	}
}
