using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business.MasterFiles;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using OrgSupplierPart = Enterprise.Customs.GB.Business.OrgSupplierPart;

namespace Enterprise.Customs.GB.GUI.Testing
{
	public class GBOrgSupplierPartFormCustomsControlTest : TestCaseWithFactory
	{
		public void TestTariffFindBoxAndColumnIncUniversal()
		{
			using (var control = new GBOrgSupplierPartFormCustomsControl())
			{
				var pivotGrid = control.FindSingle<ZGrid>("PivotGrid");
				AssertType<Universal.GUI.TariffColumnStyleInfo>("TariffColumnStyleInfo", pivotGrid.GetColumnStyle("CI_FormattedTariffNum"));
				AssertType<Universal.GUI.TariffFindBox>(control.Controls.Find("TariffFindBox", searchAllChildren: true)[0]);
			}
		}

		public void TestTaxTabIsHidden()
		{
			var orgSupplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			var partPivot = orgSupplierPart.PivotsForBinding.AddNew();
			using (var form = new ZForm(orgSupplierPart) { AutoSize = true })
			using (var control = new GBOrgSupplierPartFormCustomsControl())
			{
				form.Controls.Add(control);
				form.Show();

				partPivot.CI_ChildType = ClassificationType.IMP;
				var tabPage = control.FindSingleOrDefault<ZTabPage>("taxTabPage");
				AssertEquals("TaxTab should not be visible for IMP", false, tabPage?.TabVisible ?? false);
				partPivot.CI_ChildType = ClassificationType.EXP;
				tabPage = control.FindSingleOrDefault<ZTabPage>("taxTabPage");
				AssertEquals("TaxTab should not be visible for EXP", expected: false, tabPage?.TabVisible ?? false);
			}
		}

		public void TestQuantityColumns()
		{
			var orgSupplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			var partPivot = orgSupplierPart.PivotsForBinding.AddNew();

			partPivot.CI_ChildType = ClassificationType.IMP;
			using (var form = new ZForm(orgSupplierPart) { AutoSize = true })
			using (var control = new GBOrgSupplierPartFormCustomsControl())
			{
				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingleOrDefault<ZGrid>("PivotGrid");
				AssertNotNull("Find PivotGrid", grid);

				AssertNotNull(grid.GetColumnStyle(nameof(CusClassPartPivot.CI_SecondQty)));
				AssertNotNull(grid.GetColumnStyle(nameof(CusClassPartPivot.CI_FourthQty)));
				AssertNotNull(grid.GetColumnStyle(nameof(CusClassPartPivot.CI_FifthQty)));
			}
		}

		public void TestQuantityCalcEdits()
		{
			var orgSupplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			var partPivot = orgSupplierPart.PivotsForBinding.AddNew();

			partPivot.CI_ChildType = ClassificationType.IMP;
			using (var form = new ZForm(orgSupplierPart) { AutoSize = true })
			using (var control = new GBOrgSupplierPartFormCustomsControl())
			{
				form.Controls.Add(control);
				form.Show();

				var detailsTabPage = control.FindSingle<ZTabPage>("DetailsTabPage");

				var secondCalcEdit = detailsTabPage.FindSingle<ZCalcEdit>("secondQtyCalcEdit");
				AssertEquals("BindingMember", nameof(OrgSupplierPart.PivotsForBinding) + "." + nameof(CusClassPartPivot.CI_SecondQty), secondCalcEdit.GetBindingMember());
				var fourthCalcEdit = detailsTabPage.FindSingle<ZCalcEdit>("fourthQtyCalcEdit");
				AssertEquals("BindingMember", nameof(OrgSupplierPart.PivotsForBinding) + "." + nameof(CusClassPartPivot.CI_FourthQty), fourthCalcEdit.GetBindingMember());
				var fifthCalcEdit = detailsTabPage.FindSingle<ZCalcEdit>("fifthQtyCalcEdit");
				AssertEquals("BindingMember", nameof(OrgSupplierPart.PivotsForBinding) + "." + nameof(CusClassPartPivot.CI_FifthQty), fifthCalcEdit.GetBindingMember());
			}
		}

		public void TestGridColumnOrder()
		{
			var orgSupplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			var partPivot = orgSupplierPart.PivotsForBinding.AddNew();

			partPivot.CI_ChildType = ClassificationType.IMP;
			using (var form = new ZForm(orgSupplierPart) { AutoSize = true })
			using (var control = new GBOrgSupplierPartFormCustomsControl())
			{
				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("PivotGrid");
				var styles = grid.ColumnStyles;
				foreach (var (expectedName, expectedIndex) in expectedColumns)
				{
					var style = styles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == expectedName);
					AssertEquals($"Column {expectedName} Index", expectedIndex, styles.IndexOf(style));
				}
			}
		}

		public void TestControlsTabIndex()
		{
			var orgSupplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			var partPivot = orgSupplierPart.PivotsForBinding.AddNew();

			partPivot.CI_ChildType = ClassificationType.IMP;
			using (var form = new ZForm(orgSupplierPart) { AutoSize = true })
			using (var control = new GBOrgSupplierPartFormCustomsControl())
			{
				form.Controls.Add(control);
				form.Show();

				var detailsTabPage = control.FindSingle<ZTabPage>("DetailsTabPage");

				CombineAssertions(() =>
				{
					foreach (var (controlName, tabIndex) in ControlToTabIndex)
					{
						var userControl = detailsTabPage.FindSingle<Control>(controlName);
						AssertEquals($"{controlName} TabIndex", tabIndex, userControl.TabIndex);
					}
				});
			}
		}

		public void TestCaptions()
		{
			var orgSupplierPart = Factory.NewWithValidTestData<OrgSupplierPart>();
			var partPivot = orgSupplierPart.PivotsForBinding.AddNew();

			partPivot.CI_ChildType = ClassificationType.IMP;
			using (var form = new ZForm(orgSupplierPart) { AutoSize = true })
			using (var control = new GBOrgSupplierPartFormCustomsControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions("Captions for Import", () =>
				{
					AssertCaption("[UCC 6/14 & 6/15] Commodity and TARIC", control, "TariffFindBox");
					AssertCaption("[UCC 6/16 & 6/17] Additional Codes", control, "Supplement1TextBox");
					AssertCaption("[UCC 1/10 & 1/11] Procedure", control, "CpcTextBox");
					AssertCaption("[UCC 8/1] Quota", control, "QuotaTextBox");
					AssertCaption("Third Qty", control, "ThirdQtyCalcEdit");
					AssertCaption("[UCC 5/15,16] Country/Region of (Preferential) Origin", control, "CountryOfOriginTextBox");
					AssertCaption("[UCC 4/17] Pref. Code", control, "PreferenceCodeDropEdit");
					AssertCaption("[UCC 2/3 && 8/7] Supporting Documents", control, "supportingDocsTabPage");
					AssertCaption("[UCC 2/2] Additional Info", control, "additionalInfosTabPage");
					AssertCaption("[UCC 2/1] Previous Documents", control, "previousDocsTabPage");
					AssertCaption("SPIMM Category of Goods", control, "GoodsCategoryDropEdit");

					var grid = control.FindSingleOrDefault<ZGrid>("PivotGrid");
					AssertNotNull("Find PivotGrid", grid);
					if (grid != null)
					{
						AssertColumnCaption("[UCC 6/14 & 6/15] Commodity and TARIC", grid, "CI_FormattedTariffNum");
						AssertColumnCaption("[UCC 1/10 & 1/11] Procedure", grid, "CI_CPC");
						AssertColumnCaption("[UCC 6/16 & 6/17] Additional Code 1", grid, "CI_Supplement1");
						AssertColumnCaption("[UCC 6/16 & 6/17] Additional Code 2", grid, "CI_Supplement2");
						AssertColumnCaption("[UCC 5/15,16] Country/Region of (Preferential) Origin", grid, "CI_RN_NKCountryOfOrigin");
						AssertColumnCaption("[UCC 8/1] Quota", grid, "CI_ConcessionOrder");
						AssertColumnCaption("Category", grid, "CI_GoodsCategory");
					}
				});
			}

			partPivot.CI_ChildType = ClassificationType.EXP;

			using (GBCustomsDataRegistry.Instance.CDSEnabledForExports.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true))
			using (var form = new ZForm(orgSupplierPart) { AutoSize = true })
			using (var control = new GBOrgSupplierPartFormCustomsControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions("Captions for Export, with CDS", () =>
				{
					AssertCaption("[UCC 6/14] Commodity", control, "TariffFindBox");
					AssertCaption("[UCC 6/16 & 6/17] Additional Codes", control, "Supplement1TextBox");
					AssertCaption("[UCC 1/10 & 1/11] Procedure", control, "CpcTextBox");
					AssertCaption("[UCC 8/1] Quota", control, "QuotaTextBox");
					AssertCaption("Third Qty", control, "ThirdQtyCalcEdit");
					AssertCaption("[UCC 5/16] Country/Region of Origin", control, "CountryOfOriginTextBox");
					AssertCaption("[UCC 4/17] Pref. Code", control, "PreferenceCodeDropEdit");
					AssertCaption("[UCC 2/3 && 8/7] Supporting Documents", control, "supportingDocsTabPage");
					AssertCaption("[UCC 2/2] Additional Info", control, "additionalInfosTabPage");
					AssertCaption("[UCC 2/1] Previous Documents", control, "previousDocsTabPage");
					AssertCaption("SPIMM Category of Goods", control, "GoodsCategoryDropEdit");

					var grid = control.FindSingleOrDefault<ZGrid>("PivotGrid");
					AssertNotNull("Find PivotGrid", grid);
					if (grid != null)
					{
						AssertColumnCaption("[UCC 6/14] Commodity", grid, "CI_FormattedTariffNum");
						AssertColumnCaption("[UCC 1/10 & 1/11] Procedure", grid, "CI_CPC");
						AssertColumnCaption("[UCC 6/16 & 6/17] Additional Code 1", grid, "CI_Supplement1");
						AssertColumnCaption("[UCC 6/16 & 6/17] Additional Code 2", grid, "CI_Supplement2");
						AssertColumnCaption("[UCC 5/16] Country/Region of Origin", grid, "CI_RN_NKCountryOfOrigin");
						AssertColumnCaption("[UCC 8/1] Quota", grid, "CI_ConcessionOrder");
						AssertColumnCaption("Category", grid, "CI_GoodsCategory");
					}
				});
			}
		}

		public void TestColumnWidths()
		{
			using (var form = new ZForm())
			using (var control = new GBOrgSupplierPartFormCustomsControl())
			{
				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingleOrDefault<ZGrid>("PivotGrid");
				AssertNotNull("Pre-requisite: find PivotGrid", grid);

				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(100), grid.GetColumnStyle(CusClassPartPivot.Schema.CI_FormattedTariffNum).Width);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(100), grid.GetColumnStyle(nameof(CusClassPartPivot.CI_CPC)).Width);
			}
		}

		void AssertCaption(string caption, UserControl userControl, string controlName)
		{
			var control = userControl.FindSingle<Control>(controlName);
			AssertEquals(controlName, caption, control.GetExtension<ILabelCaptionRenderer>().Caption);
		}

		void AssertColumnCaption(string caption, ZGrid grid, string columnName)
		{
			var columnStyle = grid.GetColumnStyle(columnName);
			AssertEquals($"{columnName} Caption", caption, columnStyle.Caption ?? columnStyle.CaptionResourceString.Caption);
		}

		(string, int)[] expectedColumns => new[]
		{
			("CI_FormattedTariffNum", 0),
			("CI_ChildType", 1),
			("CI_ChildListOrder", 2),
			("CI_OH", 3),
			("CI_CPC", 4),
			("CI_Supplement1", 5),
			("CI_Supplement2", 6),
			("CI_SecondQty", 7),
			("CI_ThirdQty", 8),
			("CI_FourthQty", 9),
			("CI_FifthQty", 10),
			("CI_RN_NKCountryOfOrigin", 11),
			("CI_CC", 12),
			("PreferenceCode", 13),
			("CI_ConcessionOrder", 14),
			("CI_UsageComment", 15),
			("CI_Description", 16),
			("CI_GoodsCategory", 17),
			("CI_LastAuditedDate", 18),
			("LastAuditedUserFullName", 19),
			("Classification+CC_TariffNum", 20),
			("Classification+CC_Description", 21),
			("Classification+CC_ClassificationType", 22),
		};

		(string, int)[] ControlToTabIndex => new[]
		{
			("TariffFindBox", 0),
			("Supplement1TextBox", 1), ("Supplement2TextBox", 2), ("AdditionalSupplementaryCodesEditButton", 3),
			("CpcTextBox", 4), ("AdditionalCPCAsStringTextBox", 5), ("AdditionalCPCMoreButton", 6),
			("QuotaTextBox", 7), ("vatDropEdit", 8),
			("secondQtyCalcEdit", 9), ("ThirdQtyCalcEdit", 10), ("fourthQtyCalcEdit", 11), ("fifthQtyCalcEdit", 12),
			("CountryOfOriginTextBox", 13),
			("PreferenceCodeDropEdit", 14),
			("ClassificationDescriptionTextBox", 15),
			("GoodsCategoryDropEdit", 16),
		};
	}
}
