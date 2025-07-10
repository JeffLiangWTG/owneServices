using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class ECCCWildlifeUserControlTestCase : TestCaseWithFactory
	{
		public void TestAvailableLPCOGridFields()
		{
			using (var filterControl = new ECCCWildlifeUserControl(true))
			{
				filterControl.Show();
				var gridUserControl = filterControl.FindSingleOrDefault<LPCOGridUserControl>(c => c.Name == "LPCOGridUserControl");
				Assert(!gridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_HolderContactEmail).IsUnavailable);
				Assert(!gridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_HolderContactName).IsUnavailable);
				Assert(!gridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_HolderContactPhone).IsUnavailable);
				Assert(!gridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation).IsUnavailable);
				Assert(!gridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_EndDate).IsUnavailable);
				Assert(!gridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_HolderType).IsUnavailable);
				Assert(!gridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.LPCOHolderOrgPK).IsUnavailable);
				Assert(!gridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_HolderName).IsUnavailable);
				Assert(!gridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_OA_Holder).IsUnavailable);
				Assert(!gridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_IssueDate).IsUnavailable);
				Assert(!gridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RefNo).IsUnavailable);
				Assert(!gridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_Type).IsUnavailable);
				Assert(gridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RN_NKSmeltAndPourCountryCode).IsUnavailable);
			}
		}

		public void TestControlDisplay()
		{
			using (ZForm form = new ZForm())
			{
				using (var control = new ECCCWildlifeUserControl(true))
				{
					control.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 134, true);
					form.Controls.Add(control);
					form.Show();

					CheckControlParentRelationship(control.FindSingleOrDefault<ZLabel>(c => c.Name == "TitleLabel"));
					CheckControlParentRelationship(control.FindSingleOrDefault<ZDropEdit>(c => c.Name == "SexDropEdit"));
					CheckControlParentRelationship(control.FindSingleOrDefault<ZDropEdit>(c => c.Name == "LifeStageDropEdit"));
				}

				using (var control = new ECCCWildlifeUserControl(true))
				{
					control.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 134, true);
					form.Controls.Add(control);
					form.Show();
					var topPanel = control.FindSingleOrDefault<ZPanel>(c => c.Name == "TopPanel");
					Assert("The HorizontalScroll of TopPanel should be hide", !topPanel.HorizontalScroll.Visible);
					Assert("The VerticalScroll of TopPanel should be hide", !topPanel.VerticalScroll.Visible);

					CheckControlParentRelationship(control.FindSingleOrDefault<ZLabel>(c => c.Name == "TitleLabel"));
					CheckControlParentRelationship(control.FindSingleOrDefault<ZDropEdit>(c => c.Name == "SexDropEdit"));
					CheckControlParentRelationship(control.FindSingleOrDefault<ZDropEdit>(c => c.Name == "LifeStageDropEdit"));
					var detailGroupBox = control.FindSingleOrDefault<ZGroupBox>(c => c.Name == "DetailsGroupBox");
					Assert("The HorizontalScroll of TopPanel should be hide", detailGroupBox.Left + detailGroupBox.Width <= detailGroupBox.Parent.Width);
				}
			}
		}

		public void TestAutoScroll()
		{
			using (var filterControl = new ECCCWildlifeUserControl(false))
			{
				filterControl.Show();
				Assert(filterControl.AutoScroll);
				AssertEquals(400, filterControl.AutoScrollMinSize.Height);
				AssertEquals(1080, filterControl.AutoScrollMinSize.Width);
			}
		}

		void CheckControlParentRelationship(System.Windows.Forms.Control control) => Assert($"{control.Name} positioned incorrectly (outside the bounds of the parent control)", !(control.Left + control.Width > control.Parent.Width || control.Top + control.Height > control.Parent.Height));
	}
}
