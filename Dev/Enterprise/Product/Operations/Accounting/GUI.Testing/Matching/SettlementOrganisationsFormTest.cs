using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.GUI.Matching;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(SettlementOrganisationsForm))]
	public class SettlementOrganisationsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			TestMatchingBase = new ARMatchingBase(Factory);
			TestSettlementOrganisation = new SettlementOrganisation(TestMatchingBase);
			return new SettlementOrganisationsForm(TestSettlementOrganisation);
		}

		public void TestCloseButton()
		{
			TestMatchingBase = new ARMatchingBase(Factory, Factory.NewWithValidTestData<ARPayment>());
			TestSettlementOrganisation = new SettlementOrganisation(TestMatchingBase);
			using (var form = new SettlementOrganisationsForm(TestSettlementOrganisation))
			{
				form.Show();
				AssertEquals(true, form.CloseButton.Enabled);
				form.CloseButton.PerformClick();
				Assert(form.IsDisposed);
			}
		}

		#region TestSettlementOrgGridEditable

		public void TestSettlementOrgGridEditable()
		{
			var testPay = Factory.NewWithValidTestData<APPayment>();
			TestMatchingBase = new ARMatchingBase(Factory, testPay);
			TestMatchingBase.PrimaryOrganization = ZGuid.Empty;
			TestSettlementOrganisation = new SettlementOrganisation(TestMatchingBase);

			using (var testForm = new SettlementOrganisationsForm(TestSettlementOrganisation))
			{
				testForm.Show();
				Assert("SettlementOrgGrid should not be editable because Primary org has not been entered", testForm.zGrid1_ForTestOnly.ReadOnly);
			}

			using (var testForm = new SettlementOrganisationsForm(TestSettlementOrganisation))
			{
				TestMatchingBase.PrimaryOrganization = ZGuid.NewZGuid();
				testForm.Show();
				Assert("SettlementOrgGrid should be editable because Primary org has already been entered", !testForm.zGrid1_ForTestOnly.ReadOnly);
			}
		}

		#endregion

		SettlementOrganisation TestSettlementOrganisation;
		MatchingBase TestMatchingBase;
	}
}
