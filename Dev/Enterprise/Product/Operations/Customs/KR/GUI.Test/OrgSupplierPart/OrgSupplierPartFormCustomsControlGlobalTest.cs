using Enterprise.Core.Forms;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class OrgSupplierPartFormCustomsControlGlobalTest : Customs.GUI.Testing.OrgSupplierPartFormCustomsControlGlobalTest
	{
		protected override ZUserControl GetUserControl()
		{
			return new OrgSupplierPartFormCustomsControlGlobal();
		}
		protected override string UserControlName => "OrgSupplierPartFormCustomsControlGlobal";

		public void TestExportControlVisibilityWhenChildTypeIsChanged()
		{
			var orgSupplierPart = Factory.New<OrgSupplierPart>();
			var currentPivot1 = orgSupplierPart.PivotsForBinding.AddNew();
			currentPivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTE;
			var currentPivot2 = orgSupplierPart.PivotsForBinding.AddNew();
			currentPivot2.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
			var currentPivot3 = orgSupplierPart.PivotsForBinding.AddNew();
			currentPivot3.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTB;
			using (var form = new ZForm(orgSupplierPart))
			{
				var userControl = GetUserControl();
				form.Controls.Add(userControl);
				form.Show();
				var tabControl = userControl.FindSingle<ZTabControl>("DetailTabControl");
				tabControl.SelectedTab = userControl.FindSingle<ZTabPage>("DetailsTabPage");
				var grid = userControl.FindSingle<ZGrid>("PivotGrid");

				grid.CurrentRowIndex = 2;
				AssertEquals(false, tabControl.FindSingle<DynamicLayoutPanel>("DynamicCustomsDetailsCommonPanel").Visible);
				AssertEquals(false, tabControl.FindSingle<ZGroupBox>("CertificateOfOriginGroupBox").Visible);

				grid.CurrentRowIndex = 1;
				AssertEquals(false, tabControl.FindSingle<DynamicLayoutPanel>("DynamicCustomsDetailsCommonPanel").Visible);
				AssertEquals(false, tabControl.FindSingle<ZGroupBox>("CertificateOfOriginGroupBox").Visible);

				grid.CurrentRowIndex = 0;
				AssertEquals(true, tabControl.FindSingle<DynamicLayoutPanel>("DynamicCustomsDetailsCommonPanel").Visible);
				AssertEquals(true, tabControl.FindSingle<ZGroupBox>("CertificateOfOriginGroupBox").Visible);
			}
		}

		public void TestGovernmentAgencyInfoBoundGrid()
		{
			using (var userControl = new EXPOtherGovernmentAgencyInfoUserControl())
			{
				userControl.Show();
				var grid = userControl.FindSingle<ZGrid>("GovernmentAgencyInfoBoundGrid");
				int index = 0;
				AssertColumnNameAndReadOnly((ZGridColumnInfo)grid.ColumnStyles[index++], GAApproval.Schema.CSI_SubType, false);
				AssertColumnNameAndReadOnly((ZGridColumnInfo)grid.ColumnStyles[index++], GAApproval.Schema.CSI_Code, false);
				AssertColumnNameAndReadOnly((ZGridColumnInfo)grid.ColumnStyles[index++], GAApproval.Schema.CSI_Procedure, false);
				AssertColumnNameAndReadOnly((ZGridColumnInfo)grid.ColumnStyles[index++], GAApproval.Schema.CSI_Description, false);
				AssertColumnNameAndReadOnly((ZGridColumnInfo)grid.ColumnStyles[index++], nameof(GAApproval.NonGAReasonType), false);
				AssertColumnNameAndReadOnly((ZGridColumnInfo)grid.ColumnStyles[index++], GAApproval.Schema.CSI_AdditionalDescription, false);
				AssertColumnNameAndReadOnly((ZGridColumnInfo)grid.ColumnStyles[index++], nameof(GAApproval.ExportNonGAMandatoryDocument), true);
			}

			using (var userControl = new IMPOtherGovernmentAgencyInfoUserControl())
			{
				userControl.Show();
				var approvalDocumentGrid = userControl.FindSingle<ZGrid>("GovernmentAgencyInfoBoundGrid");
				int index = 0;
				AssertColumnNameAndReadOnly((ZGridColumnInfo)approvalDocumentGrid.ColumnStyles[index++], GAApproval.Schema.CSI_Code, false);
				AssertColumnNameAndReadOnly((ZGridColumnInfo)approvalDocumentGrid.ColumnStyles[index++], GAApproval.Schema.CSI_Procedure, false);
				AssertColumnNameAndReadOnly((ZGridColumnInfo)approvalDocumentGrid.ColumnStyles[index++], GAApproval.Schema.CSI_Description, false);
				AssertColumnNameAndReadOnly((ZGridColumnInfo)approvalDocumentGrid.ColumnStyles[index++], GAApproval.Schema.CSI_SubType, false);

				index = 0;
				var nonApprovalDocumentGrid = userControl.FindSingle<ZGrid>("NonApprovalDocumentGrid");
				AssertColumnNameAndReadOnly((ZGridColumnInfo)nonApprovalDocumentGrid.ColumnStyles[index++], NonGADetail.Schema.CSI_Procedure, false);
				AssertColumnNameAndReadOnly((ZGridColumnInfo)nonApprovalDocumentGrid.ColumnStyles[index++], NonGADetail.Schema.CSI_Code, false);
				AssertColumnNameAndReadOnly((ZGridColumnInfo)nonApprovalDocumentGrid.ColumnStyles[index++], nameof(NonGADetail.NonGAReasonType), false);
				AssertColumnNameAndReadOnly((ZGridColumnInfo)nonApprovalDocumentGrid.ColumnStyles[index++], NonGADetail.Schema.CSI_Description, false);
				AssertColumnNameAndReadOnly((ZGridColumnInfo)nonApprovalDocumentGrid.ColumnStyles[index++], nameof(NonGADetail.ImportNonGAMandatoryDocument), true);
			}

			void AssertColumnNameAndReadOnly(ZGridColumnInfo column, string columnName, bool isReadOnly)
			{
				AssertEquals(column.ColumnName, columnName);
				AssertEquals(column.IsReadOnly, isReadOnly);
			}
		}

		public void TestTabPageVisibility()
		{
			var orgSupplierPart = Factory.New<OrgSupplierPart>();
			var currentPivot = orgSupplierPart.PivotsForBinding.AddNew();
			using (var form = new ZForm(orgSupplierPart))
			{
				var userControl = GetUserControl();
				form.Controls.Add(userControl);
				form.Show();
				var tabControl = userControl.FindSingle<ZTabControl>("DetailTabControl");

				currentPivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTE;
				AssertEquals(3, tabControl.Controls.Count);
				AssertEquals("DetailsTabPage", tabControl.Controls[0].Name);
				AssertEquals("AttributesTabPage", tabControl.Controls[1].Name);
				AssertEquals("GovernmentAgencyInfoTabPage", tabControl.Controls[2].Name);

				currentPivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
				AssertEquals(3, tabControl.Controls.Count);
				AssertEquals("DetailsTabPage", tabControl.Controls[0].Name);
				AssertEquals("AttributesTabPage", tabControl.Controls[1].Name);
				AssertEquals("OtherDetailsTabPage", tabControl.Controls[2].Name);

				currentPivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTB;
				AssertEquals(2, tabControl.Controls.Count);
				AssertEquals("DetailsTabPage", tabControl.Controls[0].Name);
				AssertEquals("AttributesTabPage", tabControl.Controls[1].Name);
			}
		}
	}
}
