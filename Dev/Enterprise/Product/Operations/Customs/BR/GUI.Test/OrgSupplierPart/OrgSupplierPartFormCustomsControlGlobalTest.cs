using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class OrgSupplierPartFormCustomsControlTest : OrgSupplierPartFormCustomsControlGlobalTest
	{
		public void TestControlsVisibility()
		{
			var part = Factory.New<Business.OrgSupplierPart>();
			var partPivot = part.PivotsForBinding.AddNew();

			using (var form = new MasterFiles.GUI.OrgSupplierPartForm(part))
			{
				using (var control = new OrgSupplierPartFormCustomsControlGlobal())
				{
					form.Controls.Add(control);
					form.Show();

					CombineAssertions(() =>
					{
						//Exports
						partPivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
						Assert(control.ComplementaryDescriptionTextBox.Visible);
						Assert(!control.NveTabPage.TabVisible);
						Assert(control.AttributesNcmTabPage.TabVisible);
						Assert(!control.AdditionalTariffsTabPage.TabVisible);

						//Imports
						partPivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
						Assert(control.ComplementaryDescriptionTextBox.Visible);
						Assert(control.NveTabPage.TabVisible);
						Assert(!control.AttributesNcmTabPage.TabVisible);
						Assert(control.AdditionalTariffsTabPage.TabVisible);

						//Both
						partPivot.CI_ChildType = ClassificationTypeList.Codes.HTB;
						Assert(control.ComplementaryDescriptionTextBox.Visible);
						Assert(control.NveTabPage.TabVisible);
						Assert(control.AttributesNcmTabPage.TabVisible);
						Assert(control.AdditionalTariffsTabPage.TabVisible);
					});
				}
			}
		}

		public void TestControlTypes()
		{
			using (var control = new OrgSupplierPartFormCustomsControlGlobal())
			{
				CombineAssertions(() =>
				{
					AssertType<ZTabPage>(control.NveTabPage);
					AssertType<OrgSupplierTariffDetailsUserControl>(control.TariffDetailsLayout);
					AssertType<ZTabPage>(control.AttributesNcmTabPage);
					AssertType<AttributesUserControl>(control.AttributesGridLayout);
					AssertType<LongTextControl>(control.ComplementaryDescriptionTextBox);
					AssertType<ZTabPage>(control.AdditionalTariffsTabPage);
					AssertType<AdditionalTariffsUserControl>(control.AdditionalTariffsGridLayout);
				});
			}
		}

		public void TestGoodsCatalogColumnVisibility()
		{
			using (Registry.BRCustomsDataRegistry.Instance.EnableCatalogModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var control = new OrgSupplierPartFormCustomsControlGlobal())
			{
				var pivotGrid = (ZGrid)control.Controls.Find("PivotGrid", true).SingleOrDefault();
				var columnInfo = pivotGrid.GetColumnStyle("CI_CGC_Catalog");

				AssertNull(columnInfo);
			}

			using (Registry.BRCustomsDataRegistry.Instance.EnableCatalogModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var control = new OrgSupplierPartFormCustomsControlGlobal())
			{
				var pivotGrid = (ZGrid)control.Controls.Find("PivotGrid", true).SingleOrDefault();
				var columnInfo = pivotGrid.GetColumnStyle("CI_CGC_Catalog");

				AssertNotNull(columnInfo);
				AssertType<ZGuidFindBoxColumnStyleInfo>(columnInfo);
			}
		}

		protected override ZUserControl GetUserControl()
		{
			return new OrgSupplierPartFormCustomsControlGlobal();
		}

		protected override string ExpectedTariffColumnName => CusClassPartPivotSchema.Constants.CI_TariffNum;
		protected override string ExpectedCustomsCountryCode => Core.Constants.CountryCodes.Brazil;
		protected override string ExpectedDataGrouping => Core.Constants.CountryCodes.Brazil;
	}
}
