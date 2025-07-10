using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CFIAUserControlTestCase : TestCaseWithFactory
	{
		public void TestVisibleAIRSRegistrationNumberDescription()
		{
			using (var filterControl = new CFIAUserControl(true))
			{
				filterControl.Show();
				var grid = (ZGrid)filterControl.Controls.Find("CFIARegNumbersGrid", true)[0];
				Assert(grid.GetColumnStyle("CY_Description").IsVisible);
			}
		}

		public void TestAvailableLPCOGridFields()
		{
			using (var filterControl = new CFIAUserControl(true))
			{
				filterControl.Show();
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RefNo).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_Type).IsUnavailable);
				Assert(filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RN_NKSmeltAndPourCountryCode).IsUnavailable);
			}
		}

		public void TestVisibility()
		{
			using (var control = new CFIAUserControl(false))
			{
				var numberColumn = control.CFIARegNumbersGrid.GetColumnStyle(CusCodeData.Schema.CY_Data) as ZDropEditColumnStyleInfo;
				AssertEquals("[AIRS Registration Number] should be available", false, numberColumn.IsUnavailable);
				AssertEquals("[AIRS Registration Type] should be available", false, control.CFIARegNumbersGrid.GetColumnStyle(CusCodeData.Schema.CY_Code).IsUnavailable);

				var sourceCountryCodeFindBox = control.Controls.Find("SourceCountryCodeFindBox", true);

				Assert("SourceCountryCodeFindBox", control.Controls.Find("SourceCountryCodeFindBox", true).Any());
				Assert("SourceStateDropEdit", control.Controls.Find("SourceStateDropEdit", true).Any());
				Assert("DeliveryAddressUserControl", !control.Controls.Find("DeliveryAddressUserControl", true).Any());
				Assert("AIRSToolLinkButton", control.Controls.Find("AIRSToolLinkButton", true).Any());
			}
		}

		public void TestDefaultDataSourceBindingMemberAttributeAndDefaultBindingPropertyAttribute()
		{
			var defaultDataSourceBindingMember = typeof(CFIAUserControl).GetCustomAttributes(typeof(DefaultDataSourceBindingMemberAttribute), false).First();
			AssertNotNull(defaultDataSourceBindingMember);
			AssertNull(((DefaultDataSourceBindingMemberAttribute)defaultDataSourceBindingMember).DefaultBindingMember);

			var defaultBindingProperty = typeof(CFIAUserControl).GetCustomAttributes(typeof(DefaultBindingPropertyAttribute), false).First();
			AssertNotNull(defaultBindingProperty);
			AssertEquals("CFIAPGAHeader", ((DefaultBindingPropertyAttribute)defaultBindingProperty).Name);
		}
	}
}
