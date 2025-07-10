using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CNSCUserControlTestCase : TestCaseWithFactory
	{
		public void TestAvailableLPCOGridFields()
		{
			using (var filterControl = new CNSCUserControl(true))
			{
				filterControl.Show();
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_HolderType).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_HolderName).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_OA_Holder).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.LPCOHolderOrgPK).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RefNo).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_Type).IsUnavailable);
				Assert(filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RN_NKSmeltAndPourCountryCode).IsUnavailable);
			}
		}

		public void TestFieldCaptions()
		{
			var cnscHeader = Factory.New<CNSCPGAHeader>();
			cnscHeader.CA_Category = "CNS";
			using (var form = new ZForm())
			using (var filterControl = new CNSCUserControl(true))
			{
				filterControl.SetDataBinding(cnscHeader, "");
				form.Controls.Add(filterControl);
				form.Show();
				var grid = filterControl.Controls.Find("ComponentGrid", true)[0] as ZArchitecture.ZGrid;
				AssertEquals(typeof(ZMultiControlColumnStyle), grid.Columns["CA_Name"].ColumnStyle.GetType());
				AssertEquals(null, grid.Columns["Specification"]);
			}
		}

		public void TestFieldBinding()
		{
			using (var userControl = new CNSCUserControl(true))
			{
				var packMarks = userControl.Controls.Find("PackMarksTextBox", true)[0] as Customs.GUI.LongTextControl;
				AssertEquals("BindingMember", "CA_PackMarks", userControl.BindingSource.GetBindingMember(packMarks));
			}
		}

		public void TestAutoScroll()
		{
			using (var filterControl = new CNSCUserControl(false))
			{
				filterControl.Show();
				Assert(filterControl.AutoScroll);
				AssertEquals(400, filterControl.AutoScrollMinSize.Height);
				AssertEquals(1080, filterControl.AutoScrollMinSize.Width);
			}
		}

		public void TestDefaultDataSourceBindingMemberAttributeAndDefaultBindingPropertyAttribute()
		{
			var defaultDataSourceBindingMember = typeof(CNSCUserControl).GetCustomAttributes(typeof(DefaultDataSourceBindingMemberAttribute), false).First();
			AssertNotNull(defaultDataSourceBindingMember);
			AssertNull(((DefaultDataSourceBindingMemberAttribute)defaultDataSourceBindingMember).DefaultBindingMember);

			var defaultBindingProperty = typeof(CNSCUserControl).GetCustomAttributes(typeof(DefaultBindingPropertyAttribute), false).First();
			AssertNotNull(defaultBindingProperty);
			AssertEquals("CNSCPGAHeader", ((DefaultBindingPropertyAttribute)defaultBindingProperty).Name);
		}
	}
}
