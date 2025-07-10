using System;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUOrgSupplierPartFormCustomsControlTest : Customs.GUI.Testing.OrgSupplierPartFormCustomsControlGlobalTest
	{
		public void TestColumnAvailability()
		{
			var orgSupplierPart = Factory.New<AUOrgSupplierPart>();
			using (var form = new ZForm(orgSupplierPart))
			using (var control = (AUOrgSupplierPartFormCustomsControl)GetUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var grid = control.FindSingle<ZGrid>("PivotGrid");
				AssertEquals("TariffNum", false, grid.GetColumnStyle(control.TariffColumnName).IsUnavailable);
				AssertEquals("DateStart", true, grid.GetColumnStyle(control.DateStartColumnName).IsUnavailable);
				AssertEquals("DateEnd", true, grid.GetColumnStyle(control.DateEndColumnName).IsUnavailable);
				AssertEquals("Tariff Column is customised", AutoCusClassPartPivot.Schema.CI_TariffNum, control.TariffColumnName);
			}
		}

		public void TestClassificationAndTariff()
		{
			var orgSupplierPart = Factory.New<AUOrgSupplierPart>();

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(orgSupplierPart))
			using (var control = (AUOrgSupplierPartFormCustomsControl)GetUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var pivot = orgSupplierPart.PivotsForBinding[0];
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				AssertEquals("IsImport", true, control.IsImport);

				var grid = control.FindSingle<ZGrid>("PivotGrid");
				var tariffColumn = grid.GetColumnStyle(control.TariffColumnName);
				AssertType<Universal.GUI.TariffColumnStyleInfo>("tariffColumn is TariffColumnStyleInfo", tariffColumn);
				AssertEquals("tariffColumn TariffType", "IMP", (tariffColumn as Universal.GUI.TariffColumnStyleInfo).GetTariffType());

				var classificationColumn = grid.GetColumnStyle(AutoCusClassPartPivot.Schema.CI_CC);
				var importTariffFindBox = control.FindSingle<UniversalTariffImportFindBox>("ImportTariffFindBox");
				var exportTariffFindBox = control.FindSingle<UniversalTariffExportFindBox>("ExportTariffFindBox");
				var classificationFindBox = control.FindSingle<ZGuidFindBox>("ClassificationFindBox");
				AssertEquals("tariffColumn visible", true, tariffColumn.IsVisible);
				AssertEquals("classificationColumn visible", true, classificationColumn.IsVisible);
				AssertEquals("importTariffFindBox visible", true, importTariffFindBox.Visible);
				AssertEquals("exportTariffFindBox visible", false, exportTariffFindBox.Visible);
				AssertEquals("classificationFindBox visible", true, classificationFindBox.Visible);
				AssertEquals("importTariffFindBox ReadOnly", false, importTariffFindBox.ReadOnly);
				AssertEquals("exportTariffFindBox ReadOnly", false, exportTariffFindBox.ReadOnly);
				AssertEquals("classificationFindBox ReadOnly", false, classificationFindBox.ReadOnly);
				pivot.CI_CC = ZGuid.NewZGuid();
				AssertEquals("importTariffFindBox ReadOnly", true, importTariffFindBox.ReadOnly);
				AssertEquals("exportTariffFindBox ReadOnly", true, exportTariffFindBox.ReadOnly);
				AssertEquals("classificationFindBox ReadOnly", false, classificationFindBox.ReadOnly);
				pivot.CI_CC = ZGuid.Empty;
				pivot.CI_FormattedTariffNum = "12345";
				AssertEquals("importTariffFindBox ReadOnly", false, importTariffFindBox.ReadOnly);
				AssertEquals("exportTariffFindBox ReadOnly", false, exportTariffFindBox.ReadOnly);
				AssertEquals("classificationFindBox ReadOnly", true, classificationFindBox.ReadOnly);

				pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
				AssertEquals("IsImport", false, control.IsImport);
				AssertEquals("importTariffFindBox visible", false, importTariffFindBox.Visible);
				AssertEquals("exportTariffFindBox visible", true, exportTariffFindBox.Visible);
				AssertEquals("tariffColumn TariffType", "EXP", (tariffColumn as Universal.GUI.TariffColumnStyleInfo).GetTariffType());
			}
		}

		public void TestClassificationAndTariff_AHECC_AUCClass()
		{
			var orgSupplierPart = Factory.New<AUOrgSupplierPart>();

			using (AUCustomsDataRegistry.Instance.UseCustomsReferenceData.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm(orgSupplierPart))
			using (var control = (AUOrgSupplierPartFormCustomsControl)GetUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var pivot = orgSupplierPart.PivotsForBinding[0];
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				AssertEquals("IsImport", true, control.IsImport);

				var grid = control.FindSingle<ZGrid>("PivotGrid");
				var tariffColumn = grid.GetColumnStyle(control.TariffColumnName);
				AssertType("tariffColumn is TariffColumnStyleInfo", typeof(AHECCTariffColumnStyleInfo), tariffColumn);

				var classificationColumn = grid.GetColumnStyle(AutoCusClassPartPivot.Schema.CI_CC);
				var importTariffFindBox = control.FindSingle<AUCClassFindBox>("ImportTariffFindBoxAUCClass");
				var exportTariffFindBox = control.FindSingle<AHECCFindBox>("ExportTariffFindBoxAHECC");
				var classificationFindBox = control.FindSingle<ZGuidFindBox>("ClassificationFindBox");
				AssertEquals("tariffColumn visible", true, tariffColumn.IsVisible);
				AssertEquals("classificationColumn visible", true, classificationColumn.IsVisible);
				AssertEquals("importTariffFindBox visible", true, importTariffFindBox.Visible);
				AssertEquals("exportTariffFindBox visible", false, exportTariffFindBox.Visible);
				AssertEquals("classificationFindBox visible", true, classificationFindBox.Visible);
				AssertEquals("importTariffFindBox ReadOnly", false, importTariffFindBox.ReadOnly);
				AssertEquals("exportTariffFindBox ReadOnly", false, exportTariffFindBox.ReadOnly);
				AssertEquals("classificationFindBox ReadOnly", false, classificationFindBox.ReadOnly);
				pivot.CI_CC = ZGuid.NewZGuid();
				AssertEquals("importTariffFindBox ReadOnly", true, importTariffFindBox.ReadOnly);
				AssertEquals("exportTariffFindBox ReadOnly", true, exportTariffFindBox.ReadOnly);
				AssertEquals("classificationFindBox ReadOnly", false, classificationFindBox.ReadOnly);
				pivot.CI_CC = ZGuid.Empty;
				pivot.CI_FormattedTariffNum = "12345";
				AssertEquals("importTariffFindBox ReadOnly", false, importTariffFindBox.ReadOnly);
				AssertEquals("exportTariffFindBox ReadOnly", false, exportTariffFindBox.ReadOnly);
				AssertEquals("classificationFindBox ReadOnly", true, classificationFindBox.ReadOnly);

				pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
				AssertEquals("IsImport", false, control.IsImport);
				AssertEquals("importTariffFindBox visible", false, importTariffFindBox.Visible);
				AssertEquals("exportTariffFindBox visible", true, exportTariffFindBox.Visible);
			}
		}

		public void TestChangeVisiblityofQuarantineFormForImportAndExport()
		{
			var orgSupplierPart = Factory.New<AUOrgSupplierPart>();
			using (var form = new ZForm(orgSupplierPart))
			using (var control = new AUOrgSupplierPartFormCustomsControl())
			{
				form.Controls.Add(control);
				form.Show();
				var pivot = orgSupplierPart.PivotsForBinding[0];
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				AssertEquals("IsImport", true, control.IsImport);
				Assert("HTIQuarantineTabPage is visible", control.HTIQuarantineTabPage.TabVisible);
				Assert("HTEQuarantineTabPage is not visible", !control.HTEQuarantineTabPage.TabVisible);
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
				AssertEquals("Is export", true, control.IsExport);
				Assert("HTIQuarantineTabPage is not visible", !control.HTIQuarantineTabPage.TabVisible);
				Assert("HTEQuarantineTabPage is visible", control.HTEQuarantineTabPage.TabVisible);
				pivot.CI_ChildType = "";
				AssertEquals("Is not Import", false, control.IsImport);
				AssertEquals("Is not export", false, control.IsExport);
				Assert("HTIQuarantineTabPage is not visible", !control.HTIQuarantineTabPage.TabVisible);
				Assert("HTEQuarantineTabPage is not visible", !control.HTEQuarantineTabPage.TabVisible);
			}
		}

		public void TestChangeVisiblityOfICSPermitsForImport()
		{
			var orgSupplierPart = Factory.New<AUOrgSupplierPart>();
			using (var form = new ZForm(orgSupplierPart))
			using (var control = new AUOrgSupplierPartFormCustomsControl())
			{
				form.Controls.Add(control);
				form.Show();
				var pivot = orgSupplierPart.PivotsForBinding[0];
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
				var permitsTabPage = control.FindSingle<ZTabPage>("HTIPermitsTabPage");
				AssertEquals("IsImport", true, control.IsImport);
				AssertEquals("HTIPermitsTabPage is visible", true, permitsTabPage.TabVisible);
				pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
				AssertEquals("Is export", true, control.IsExport);
				AssertEquals("HTIPermitsTabPage is visible", false, permitsTabPage.TabVisible);
				pivot.CI_ChildType = "";
				AssertEquals("Is not Import", false, control.IsImport);
				AssertEquals("Is not export", false, control.IsExport);
				AssertEquals("HTIPermitsTabPage is visible", false, permitsTabPage.TabVisible);
			}
		}

		protected override ZUserControl GetUserControl() => new AUOrgSupplierPartFormCustomsControl();

		protected override void AssertTariffColumns(Core.Forms.ZGridColumnInfo columnStyleInfo) => AssertType<AHECCTariffColumnStyleInfo>("TariffColumnStyleInfo", columnStyleInfo);

		protected override string UserControlName => "AUOrgSupplierPartFormCustomsControl";

		protected override string ExpectedTariffColumnName => AutoCusClassPartPivot.Schema.CI_TariffNum;
	}
}
