using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(CreditReportsRegistryControl))]
	sealed class CreditReportsRegistryControlTest : RegistryZUserControlTestCase
	{
		[RequiresSTA]
		public void TestControlsSetToReadOnly()
		{
			using (var form = new ZForm())
			using (var creditReportsRegistryControl = new CreditReportsRegistryControlForTest())
			{
				form.Controls.Add(creditReportsRegistryControl);
				form.Show();
				AssertEquals("Precondition: ", false, creditReportsRegistryControl.CreditReportRegistryGrid.ReadOnly);

				creditReportsRegistryControl.SetControlOrBusinessEntityReadOnly(true);
				AssertEquals(true, creditReportsRegistryControl.CreditReportRegistryGrid.ReadOnly);

				creditReportsRegistryControl.SetControlOrBusinessEntityReadOnly(false);
				AssertEquals(false, creditReportsRegistryControl.CreditReportRegistryGrid.ReadOnly);
			}
		}

		public void TestControlColumns()
		{
			using (var form = new ZForm())
			using (var creditReportsRegistryControl = new CreditReportsRegistryControlForTest())
			{
				var columns = creditReportsRegistryControl.CreditReportRegistryGrid.ColumnStyles;
				CombineAssertions(() =>
				{
					AssertEquals(typeof(ZTextBoxColumnStyleInfo), columns[0].GetType());
					AssertEquals("CountryCode", ((ZTextBoxColumnStyleInfo)columns[0]).ColumnName);
					AssertContains("Code", ((ZTextBoxColumnStyleInfo)columns[0]).CaptionResourceString.ToString());
					AssertEquals(typeof(ZTextBoxColumnStyleInfo), columns[1].GetType());
					AssertEquals("Country", ((ZTextBoxColumnStyleInfo)columns[1]).ColumnName);
					AssertContains("Country", ((ZTextBoxColumnStyleInfo)columns[1]).CaptionResourceString.ToString());
					AssertEquals(typeof(ZCheckBoxColumnStyleInfo), columns[2].GetType());
					AssertEquals("CountryEnabledForCompany", ((ZCheckBoxColumnStyleInfo)columns[2]).ColumnName);
					AssertContains("Enable Company Reports", ((ZCheckBoxColumnStyleInfo)columns[2]).CaptionResourceString.ToString());
					AssertEquals(typeof(ZCheckBoxColumnStyleInfo), columns[3].GetType());
					AssertEquals("CountryEnabledForOrganisation", ((ZCheckBoxColumnStyleInfo)columns[3]).ColumnName);
					AssertContains("Enable Organization Reports", ((ZCheckBoxColumnStyleInfo)columns[3]).CaptionResourceString.ToString());
					AssertEquals(typeof(ZCheckBoxColumnStyleInfo), columns[4].GetType());
					AssertEquals("ComprehensiveReportEnabled", ((ZCheckBoxColumnStyleInfo)columns[4]).ColumnName);
					AssertContains("Comprehensive Report", ((ZCheckBoxColumnStyleInfo)columns[4]).CaptionResourceString.ToString());
					AssertEquals(typeof(ZCheckBoxColumnStyleInfo), columns[5].GetType());
					AssertEquals("FailureRiskEnabled", ((ZCheckBoxColumnStyleInfo)columns[5]).ColumnName);
					AssertContains("Failure Risk", ((ZCheckBoxColumnStyleInfo)columns[5]).CaptionResourceString.ToString());
					AssertEquals(typeof(ZCheckBoxColumnStyleInfo), columns[6].GetType());
					AssertEquals("LatePaymentRiskEnabled", ((ZCheckBoxColumnStyleInfo)columns[6]).ColumnName);
					AssertContains("Late Payment Risk", ((ZCheckBoxColumnStyleInfo)columns[6]).CaptionResourceString.ToString());
					AssertEquals(typeof(ZCheckBoxColumnStyleInfo), columns[7].GetType());
					AssertEquals("CommercialBureauEnquiryEnabled", ((ZCheckBoxColumnStyleInfo)columns[7]).ColumnName);
					AssertContains("Commercial Bureau Enquiry", ((ZCheckBoxColumnStyleInfo)columns[7]).CaptionResourceString.ToString());
				});
			}
		}

		protected override IBusiness GetNewBusinessEntity() => new CreditReportItemCollection();

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CreditReportsRegistryControl)control).CreditReportRegistryGrid.ReadOnly;
		}
	}
}
