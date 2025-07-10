using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.CommissionManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.CommissionManagement.GUI.Testing
{
	class AppendCommissionAgreementItemModuleDecisionProviderTest : TestCaseWithFactory
	{
		public void TestHandleFindBoxOKButton()
		{
			var agreement1 = Factory.New<OrgCommissionAgreement>();
			agreement1.CA0_LastApprovedDateUtc = new ZDateTime(2002, 2, 2);
			var agreement2 = Factory.New<OrgCommissionAgreement>();
			agreement2.CA0_LastApprovedDateUtc = new ZDateTime(2002, 2, 2);
			var agreement3 = Factory.New<OrgCommissionAgreement>();
			agreement3.CA0_LastApprovedDateUtc = new ZDateTime(2002, 2, 2);
			var agreement4 = Factory.New<OrgCommissionAgreement>();
			agreement4.CA0_LastApprovedDateUtc = new ZDateTime(2002, 2, 2);

			var popup = new EmbeddedModulePopup();
			var wizard = new CommissionAgreementApprovalWizard(Factory);
			var provider = new AppendCommissionAgreementItemModuleDecisionProvider(popup, wizard);

			provider.HandleFindBoxOKButton(new[] { agreement1, agreement2 });

			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<OrgCommissionAgreement>.PKOnlyComparer,
				new[]
				{
					agreement1,
					agreement2,
				},
				wizard.CommissionAgreementApprovalItems.Select(x => x.CommissionAgreement));

			provider.HandleFindBoxOKButton(new[] { agreement2, agreement3 });

			AssertContainsExactElementsInAnyOrder(
				BusinessObjectEqualityComparer<OrgCommissionAgreement>.PKOnlyComparer,
				new[]
				{
					agreement1,
					agreement2,
					agreement3,
				},
				wizard.CommissionAgreementApprovalItems.Select(x => x.CommissionAgreement));
		}
	}
}
