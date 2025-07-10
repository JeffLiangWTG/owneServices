using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CAOrgSupplierPartFormCustomsControlTest : Customs.GUI.Testing.OrgSupplierPartFormCustomsControlAbstractTest
	{
		public void TestTariffUserControlForCAGlobalTariff_HTI()
		{
			var gridName = "PivotGrid";
			var columnName = CusClassPartPivot.Schema.CI_FormattedTariffNum;
			var tariffFindBoxName_TrfCA = "classificationNumberFindBox";
			var tariffFindBoxName_SRDb = "classificationNumberFromRefDbFindBox";
			var tariffColumnInfoType = typeof(Universal.GUI.TariffColumnStyleInfo);

			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Factory.Save();

			using (var form = new ZForm(part))
			using (var partControl = GetUserControl())
			{
				partControl.FindSingle<ZGrid>("PivotGrid").SetDataBinding(pivot, "");
				form.Controls.Add(partControl);
				form.Show();
				var tariffColumnInfoCaption = ClassificationTariffUserControlTestHelper.AssertTariffColumnInfo_GetTariffFromTrfCA(partControl, gridName, columnName, tariffColumnInfoType);
				ClassificationTariffUserControlTestHelper.AssertTariffColumnInfo_GetTariffFromSRDb(partControl, gridName, columnName, tariffColumnInfoCaption);
				ClassificationTariffUserControlTestHelper.AssertTariffFindBox_GetTariffFromSRDb(partControl, tariffFindBoxName_TrfCA, tariffFindBoxName_SRDb);
			}
		}

		public void TestTariffUserControlForCAGlobalTariff_HTE()
		{
			var gridName = "PivotGrid";
			var columnName = CusClassPartPivot.Schema.CI_FormattedTariffNum;
			var tariffFindBoxName_TrfCA = "exportTariffCodeFindBox";
			var tariffFindBoxName_SRDb = "exportTariffFromSRDbCodeFindBox";
			var tariffColumnInfoType = typeof(Universal.GUI.TariffColumnStyleInfo);

			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			Factory.Save();

			using (var form = new ZForm(part))
			using (var partControl = GetUserControl())
			{
				partControl.FindSingle<ZGrid>("PivotGrid").SetDataBinding(pivot, "");
				form.Controls.Add(partControl);
				form.Show();
				var tariffColumnInfoCaption = ClassificationTariffUserControlTestHelper.AssertTariffColumnInfo_GetTariffFromTrfCA(partControl, gridName, columnName, tariffColumnInfoType);
				ClassificationTariffUserControlTestHelper.AssertTariffColumnInfo_GetTariffFromSRDb(partControl, gridName, columnName, tariffColumnInfoCaption);
				ClassificationTariffUserControlTestHelper.AssertTariffFindBox_GetTariffFromSRDb(partControl, tariffFindBoxName_TrfCA, tariffFindBoxName_SRDb);
			}
		}

		public void TestTariffUserControlForCAGlobalTariff_SHB()
		{
			var gridName = "PivotGrid";
			var columnName = CusClassPartPivot.Schema.CI_FormattedTariffNum;
			var tariffFindBoxName_TrfCA = "exportTariffCodeFindBox";
			var tariffFindBoxName_SRDb = "exportTariffFromSRDbCodeFindBox";
			var tariffColumnInfoType = typeof(Universal.GUI.TariffColumnStyleInfo);

			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			Factory.Save();

			using (var form = new ZForm(part))
			using (var partControl = GetUserControl())
			{
				partControl.FindSingle<ZGrid>("PivotGrid").SetDataBinding(pivot, "");
				form.Controls.Add(partControl);
				form.Show();
				var tariffColumnInfoCaption = ClassificationTariffUserControlTestHelper.AssertTariffColumnInfo_GetTariffFromTrfCA(partControl, gridName, columnName, tariffColumnInfoType);
				ClassificationTariffUserControlTestHelper.AssertTariffColumnInfo_GetTariffFromSRDb(partControl, gridName, columnName, tariffColumnInfoCaption);
				ClassificationTariffUserControlTestHelper.AssertTariffFindBox_GetTariffFromSRDb(partControl, tariffFindBoxName_TrfCA, tariffFindBoxName_SRDb);
			}
		}

		public void TestPGATabsVisibility()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Factory.Save();

			using (var form = new ZForm(part))
			using (var partControl = GetUserControl())
			{
				partControl.FindSingle<ZGrid>("PivotGrid").SetDataBinding(pivot, "");
				form.Controls.Add(partControl);
				form.Show();

				var pgaTabPage = partControl.FindSingleOrDefault<ZTabPage>(p => p.Text == "PGA Requirements");
				AssertNotNull(pgaTabPage);

				pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
				pgaTabPage = partControl.FindSingleOrDefault<ZTabPage>(p => p.Text == "PGA Requirements");
				AssertNull(pgaTabPage);
			}
		}

		public void TestCI_CCValueChanged()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Factory.Save();

			using (var form = new ZForm(part))
			using (var partControl = GetUserControl())
			{
				partControl.FindSingle<ZGrid>("PivotGrid").SetDataBinding(pivot, "");
				form.Controls.Add(partControl);
				form.Show();

				var pgaTabPage = partControl.FindSingleOrDefault<ZTabPage>(p => p.Text == "ECCC");
				AssertNull(pgaTabPage);
				pgaTabPage = partControl.FindSingleOrDefault<ZTabPage>(p => p.Text == "CNSC");
				AssertNull(pgaTabPage);
				pgaTabPage = partControl.FindSingleOrDefault<ZTabPage>(p => p.Text == "HC");
				AssertNull(pgaTabPage);
				var simaTabPage = partControl.Controls.Find("SIMATabPage", true).FirstOrDefault();
				AssertNotNull(simaTabPage);
				var simaForCCTabPage = partControl.Controls.Find("SIMAForCCTabPage", true).FirstOrDefault();
				AssertNull(simaForCCTabPage);

				var classification = Factory.New<CusClassification>();
				classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
				classification.CCA_HCIndicator = YesNoList.Codes.Yes;
				classification.HCPGAHeader.CA_APIProgramInd = YesNoList.Codes.Yes;
				pivot.CI_CC = classification.PK;
				pgaTabPage = partControl.FindSingleOrDefault<ZTabPage>(p => p.Text == "HC");
				AssertNotNull(pgaTabPage);
				simaTabPage = partControl.Controls.Find("SIMATabPage", true).FirstOrDefault();
				AssertNull(simaTabPage);
				simaForCCTabPage = partControl.Controls.Find("SIMAForCCTabPage", true).FirstOrDefault();
				AssertNotNull(simaForCCTabPage);

				pgaTabPage = partControl.FindSingleOrDefault<ZTabPage>(p => p.Text == "ECCC");
				AssertNull(pgaTabPage);
				pgaTabPage = partControl.FindSingleOrDefault<ZTabPage>(p => p.Text == "CNSC");
				AssertNull(pgaTabPage);
				classification = Factory.New<CusClassification>();
				classification.CC_ClassificationType = CusClassification.ClassificationType.IMP;
				classification.CCA_ECCCIndicator = YesNoList.Codes.Yes;
				classification.ECCCPGAHeader.CA_ODSProgramInd = YesNoList.Codes.Yes;
				classification.CCA_CNSCIndicator = YesNoList.Codes.Yes;
				classification.CNSCPGAHeader.CA_AllProgramInd = YesNoList.Codes.Yes;
				pivot.CI_CC = classification.PK;
				pgaTabPage = partControl.FindSingleOrDefault<ZTabPage>(p => p.Text == "ECCC");
				AssertNotNull(pgaTabPage);
				pgaTabPage = partControl.FindSingleOrDefault<ZTabPage>(p => p.Text == "CNSC");
				AssertNotNull(pgaTabPage);
				pgaTabPage = partControl.FindSingleOrDefault<ZTabPage>(p => p.Text == "HC");
				AssertNull(pgaTabPage);
				simaTabPage = partControl.Controls.Find("SIMATabPage", true).FirstOrDefault();
				AssertNull(simaTabPage);
				simaForCCTabPage = partControl.Controls.Find("SIMAForCCTabPage", true).FirstOrDefault();
				AssertNotNull(simaForCCTabPage);

				pivot.CI_CC = ZGuid.Empty;
				pgaTabPage = partControl.FindSingleOrDefault<ZTabPage>(p => p.Text == "ECCC");
				AssertNull(pgaTabPage);
				pgaTabPage = partControl.FindSingleOrDefault<ZTabPage>(p => p.Text == "CNSC");
				AssertNull(pgaTabPage);
				pgaTabPage = partControl.FindSingleOrDefault<ZTabPage>(p => p.Text == "HC");
				AssertNull(pgaTabPage);
				simaTabPage = partControl.Controls.Find("SIMATabPage", true).FirstOrDefault();
				AssertNotNull(simaTabPage);
				simaForCCTabPage = partControl.Controls.Find("SIMAForCCTabPage", true).FirstOrDefault();
				AssertNull(simaForCCTabPage);
			}
		}

		public void TestPGARequirementsDisposed()
		{
			using (var form = new ZForm(Factory.New<OrgSupplierPart>()))
			{
				var partControl = new CAOrgSupplierPartFormCustomsControlForTest();
				form.Controls.Add(partControl);
				AssertNotNull("PGA tab Collection has requirements.", partControl.pgaTabCollectionExposed);
				partControl.Dispose();
				AssertNull("PGA tab collection should dispose when CAOrgSupplierPartFormCustomsControl is disposed.", partControl.pgaTabCollectionExposed);
			}
		}

		public void TestMissingLabels()
		{
			using (var form = new ZForm(Factory.New<OrgSupplierPart>()))
			using (var partControl = new CAOrgSupplierPartFormCustomsControl())
			{
				form.Controls.Add(partControl);
				form.Show();
				var treatmentCodeDropEdit = (IResCaptionedControl)form.Controls.Find("TreatmentCodeDropEdit", true)[0];
				var gstStatusCodeDropEdit = (IResCaptionedControl)form.Controls.Find("GSTStatusCodeDropEdit", true)[0];
				var etExemptionDropEdit = (IResCaptionedControl)form.Controls.Find("ETExemptionDropEdit", true)[0];
				AssertNotNull(treatmentCodeDropEdit.CaptionResourceString);
				Assert(!treatmentCodeDropEdit.CaptionResourceString.IsEmpty());
				AssertNotNull(gstStatusCodeDropEdit.CaptionResourceString);
				Assert(!gstStatusCodeDropEdit.CaptionResourceString.IsEmpty());
				AssertNotNull(etExemptionDropEdit.CaptionResourceString);
				Assert(!etExemptionDropEdit.CaptionResourceString.IsEmpty());
			}
		}

		public void TestCCA_ProvinceOfOriginDropEdit2_ReadOnly()
		{
			var part = Factory.NewWithValidTestData<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			Factory.Save();

			using (var form = new ZForm(part))
			using (var partControl = GetUserControl())
			{
				partControl.FindSingle<ZGrid>("PivotGrid").SetDataBinding(pivot, "");
				form.Controls.Add(partControl);
				form.Show();

				var provinceOfOriginDropEdit2 = (ZDropEdit)partControl.Controls.Find("CCA_ProvinceOfOriginDropEdit2", true).First();
				Assert(provinceOfOriginDropEdit2.ReadOnly);

				pivot.CCA_RN_NKOrigin = "US";
				Assert(!provinceOfOriginDropEdit2.ReadOnly);
			}
		}

		public void TestExportPanelIsNotVisible()
		{
			using (var form = new ZForm(Factory.New<OrgSupplierPart>()))
			using (var partControl = new CAOrgSupplierPartFormCustomsControl())
			{
				form.Controls.Add(partControl);
				form.Show();
				var exportPanel = (ZPanel)form.Controls.Find("exportPanel", true)[0];
				Assert(!exportPanel.Visible);
			}
		}

		protected override ZUserControl GetUserControl() => new CAOrgSupplierPartFormCustomsControl();

		protected override string UserControlName => "CAOrgSupplierPartFormCustomsControl";

		sealed class CAOrgSupplierPartFormCustomsControlForTest : CAOrgSupplierPartFormCustomsControl
		{
			public CAOrgSupplierPartFormCustomsControlForTest() : base()
			{
			}

			internal PGATabCollection pgaTabCollectionExposed => pgaTabCollection;
		}
	}
}
