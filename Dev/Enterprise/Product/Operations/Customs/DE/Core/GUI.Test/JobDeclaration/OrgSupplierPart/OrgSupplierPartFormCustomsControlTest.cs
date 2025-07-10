using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class OrgSupplierPartFormCustomsControlTest : TestCaseWithFactory
	{
		public void TestInitializeGrid()
		{
			using (var control = new OrgSupplierPartFormCustomsControl())
			{
				var pivotGrid = control.FindSingle<ZGrid>("PivotGrid");
				var columns = pivotGrid.ColumnStyles.Cast<ZGridColumnInfo>();

				var secondQtyColumn = columns.Single(x => x.ColumnName == "CI_SecondQty") as ZTextBoxColumnStyleInfo;
				AssertNotNull(secondQtyColumn);

				var secondUnitQtyColumn = columns.Single(x => x.ColumnName == "CI_SecondUnitQty");
				AssertNotNull(secondUnitQtyColumn);

				var fourthQtyColumn = columns.Single(x => x.ColumnName == "CI_FourthQty") as ZCalcEditColumnStyleInfo;
				AssertNotNull(fourthQtyColumn);

				var fifthQtyColumn = columns.Single(x => x.ColumnName == "CI_FifthQty") as ZCalcEditColumnStyleInfo;
				AssertNotNull(fifthQtyColumn);
			}
		}

		public void TestInitializeAdditionalCalcEdit()
		{
			using (var control = new OrgSupplierPartFormCustomsControl())
			{
				var detailTabPage = control.FindSingle<ZTabPage>("DetailsTabPage");
				var secondQtyCalcEdit = detailTabPage.FindSingle<ZCalcEdit>("secondCalcEdit");
				AssertNotNull(secondQtyCalcEdit);

				var fourthCalcEdit = detailTabPage.FindSingle<ZCalcEdit>("fourthCalcEdit");
				AssertNotNull(fourthCalcEdit);

				var fifthCalcEdit = detailTabPage.FindSingle<ZCalcEdit>("fifthCalcEdit");
				AssertNotNull(fifthCalcEdit);
			}
		}

		public void TestUserControlType()
		{
			using (var form = new ZForm())
			using (var control = new OrgSupplierPartFormCustomsControl())
			{
				form.Controls.Add(control);
				form.Show();

				control.FindSingle<ZTabPage>("supportingDocsTabPage").Show();
				var supportingDocumentsUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("SupportingDocumentsUserControl");
				AssertEquals(typeof(SupportingDocumentsUCWrapperForOrgSupplierPart), supportingDocumentsUserControl.UserControlType);

				control.FindSingle<ZTabPage>("additionalInfosTabPage").Show();
				var additionalInfosUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("additionalInfosUserControl1");
				AssertEquals(typeof(AdditionalInfosUCWrapperForOrgSupplierPart), additionalInfosUserControl.UserControlType);

				control.FindSingle<ZTabPage>("previousDocsTabPage").Show();
				var previousDocumentsUserControl = control.FindSingle<ZDynamicControlCreationUserControl>("PreviousDocumentsUserControl");
				AssertEquals(typeof(PreviousDocumentsUCWrapperForOrgSupplierPart), previousDocumentsUserControl.UserControlType);
			}
		}

		public void TestImportControlIsVisibleIfImportRecordExists()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Common.ClassificationType.IMP;

			using (var form = new ZForm())
			using (var control = new OrgSupplierPartFormCustomsControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(part, "");

				CombineAssertions(() =>
				{
					control.FindSingle<ZTabPage>("supportingDocsTabPage").Show();
					var supportingDocumentsUcWrapper = control.FindSingle<ZDynamicControlCreationUserControl>("SupportingDocumentsUserControl").HostedControl as SupportingDocumentsUCWrapperForOrgSupplierPart;
					AssertEquals("ImportSupportingDocumentsUserControl is visible", expected: true, supportingDocumentsUcWrapper.ImportSupportingDocumentsUserControl.Visible);
					AssertEquals("ExportSupportingDocumentsUserControl is invisible", expected: false, supportingDocumentsUcWrapper.ExportSupportingDocumentsUserControl.Visible);

					control.FindSingle<ZTabPage>("additionalInfosTabPage").Show();
					var additionalInfosUcWrapper = control.FindSingle<ZDynamicControlCreationUserControl>("additionalInfosUserControl1").HostedControl as AdditionalInfosUCWrapperForOrgSupplierPart;
					AssertEquals("ImportAdditionalInfosUserControl is visible", expected: true, additionalInfosUcWrapper.ImportAdditionalInfosUserControl.Visible);
					AssertEquals("ExportAdditionalInfosUserControl is invisible", expected: false, additionalInfosUcWrapper.ExportAdditionalInfosUserControl.Visible);

					AssertNull("previousDocsTabPage", control.FindSingleOrDefault<ZTabPage>("previousDocsTabPage"));
				});
			}
		}

		public void TestExportControlIsVisibleIfExportRecordExists()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Common.ClassificationType.EXP;

			using (var form = new ZForm())
			using (var control = new OrgSupplierPartFormCustomsControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(part, "");

				control.FindSingle<ZTabPage>("supportingDocsTabPage").Show();
				var supportingDocumentsUcWrapper = control.FindSingle<ZDynamicControlCreationUserControl>("SupportingDocumentsUserControl").HostedControl as SupportingDocumentsUCWrapperForOrgSupplierPart;
				AssertEquals("ImportSupportingDocumentsUserControl is visible", expected: false, supportingDocumentsUcWrapper.ImportSupportingDocumentsUserControl.Visible);
				AssertEquals("ExportSupportingDocumentsUserControl is invisible", expected: true, supportingDocumentsUcWrapper.ExportSupportingDocumentsUserControl.Visible);

				control.FindSingle<ZTabPage>("additionalInfosTabPage").Show();
				var additionalInfosUcWrapper = control.FindSingle<ZDynamicControlCreationUserControl>("additionalInfosUserControl1").HostedControl as AdditionalInfosUCWrapperForOrgSupplierPart;
				AssertEquals("ImportAdditionalInfosUserControl is visible", expected: false, additionalInfosUcWrapper.ImportAdditionalInfosUserControl.Visible);
				AssertEquals("ExportAdditionalInfosUserControl is invisible", expected: true, additionalInfosUcWrapper.ExportAdditionalInfosUserControl.Visible);

				control.FindSingle<ZTabPage>("previousDocsTabPage").Show();
				var previousDocumentsUcWrapper = control.FindSingle<ZDynamicControlCreationUserControl>("PreviousDocumentsUserControl").HostedControl as PreviousDocumentsUCWrapperForOrgSupplierPart;
				AssertEquals("ImportPreviousDocumentsUserControl is visible", expected: false, previousDocumentsUcWrapper.ImportPreviousDocumentsUserControl.Visible);
				AssertEquals("ExportPreviousDocumentsUserControl is invisible", expected: true, previousDocumentsUcWrapper.ExportPreviousDocumentsUserControl.Visible);
			}
		}

		public void TestAdditionalInfosUserControl_Caption()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Common.ClassificationType.EXP;

			using (var form = new ZForm())
			using (var control = new OrgSupplierPartFormCustomsControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(part, "");

				CombineAssertions(() =>
				{
					var additionalInfosTabPage = control.FindSingle<ZTabPage>("additionalInfosTabPage");
					AssertEquals("EXP", "[44] Additional Documents", additionalInfosTabPage.CaptionResourceString.Caption);

					pivot.CI_ChildType = Common.ClassificationType.IMP;
					AssertEquals("IMP", "[44] Additional Infos", additionalInfosTabPage.CaptionResourceString.Caption);
				});
			}
		}
	}
}
