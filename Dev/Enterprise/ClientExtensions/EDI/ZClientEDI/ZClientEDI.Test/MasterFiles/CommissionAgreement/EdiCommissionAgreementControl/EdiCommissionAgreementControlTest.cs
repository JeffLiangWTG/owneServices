using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Testing
{
	class EdiCommissionAgreementControlTest : TestCaseWithFactory
	{
		#region Customer Filters Button
		public void TestCustomerFiltersButton()
		{
			var dissolvedOrg = Factory.NewWithValidTestData<OrgHeader>();
			var orgOpportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			orgOpportunity.P8_OH = dissolvedOrg.PK;
			var agreement = Factory.New<EdiCommissionAgreement>();
			agreement.CA0_P8 = orgOpportunity.PK;
			agreement.CA0_LastApprovedDateUtc = ZDateTime.Empty;

			using (var form = new ZForm(agreement))
			using (var control = new EdiCommissionAgreementControl())
			{
				form.Controls.Add(control);
				form.Show();
				var customerFiltersButton = (ZButton)control.Controls.Find("customerFiltersButton", true).Single();
				AssertEquals("Add Filters", customerFiltersButton.Text);
				customerFiltersButton.PerformClick();
				using (var lastFormShown = ZFormModaliser.LastFormShownForTest)
				{
					AssertType(typeof(EdiCommissionAgreementCustomizationForm), lastFormShown);
					lastFormShown.Close();
				}

				AssertEquals("View Filters", customerFiltersButton.Text);
			}
		}
		#endregion
	}
}
