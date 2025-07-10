using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class SupportingDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestUserControlDataSourceType()
		{
			using (var control = new SupportingDocumentsUserControl())
			{
				AssertEquals(typeof(NctsCommonCargoDesc), control.BindingSource.DataSourceType);
			}
		}

		public void TestGridColumnSizes()
		{
			using (var control = new SupportingDocumentsUserControl())
			{
				var supportingDocumentsGrid = control.FindSingle<ZGrid>("SupportingDocumentsGrid");
				CombineAssertions(() =>
				{
					AssertEquals("CSI_ReferenceNumber", 272, supportingDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_ReferenceNumber).Width);
					AssertEquals("CSI_Code", 56, supportingDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_Code).Width);
					AssertEquals("CSI_Description", 680, supportingDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_Description).Width);
				});
			}
		}

		public void TestGridColumCharcterCasing()
		{
			using (var control = new SupportingDocumentsUserControl())
			{
				var supportingDocumentsGrid = control.FindSingle<ZGrid>("SupportingDocumentsGrid");
				CombineAssertions(() =>
				{
					AssertEquals("CSI_ReferenceNumber", CharacterCasing.Normal, supportingDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_ReferenceNumber).CharacterCasing);
					AssertEquals("CSI_Code", CharacterCasing.Upper, supportingDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_Code).CharacterCasing);
					AssertEquals("CSI_Description", CharacterCasing.Normal, supportingDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_Description).CharacterCasing);
				});
			}
		}

		public void TestSupDocReferenceTextBoxAllowsNormalCase()
		{
			using (var control = new SupportingDocumentsUserControl())
			{
				var editControl = control.FindSingle<ZTextBox>("SupDocReferenceTextBox");
				AssertEquals("normal casing", CharacterCasing.Normal, editControl.CharacterCasing);
			}
		}

		public void TestSupDocReasonTextBoxAllowsNormalCase()
		{
			using (var control = new SupportingDocumentsUserControl())
			{
				var editControl = control.FindSingle<ZTextBox>("SupDocReasonTextBox");
				AssertEquals("normal casing", CharacterCasing.Normal, editControl.CharacterCasing);
			}
		}

		public void TestCSI_CodeFindBoxColumnStyle()
		{
			using (var control = new SupportingDocumentsUserControl())
			{
				var grid = control.FindSingle<ZGrid>("SupportingDocumentsGrid");
				var columnInfo = grid.GetColumnStyle(NctsSupportingDocument.Schema.CSI_Code);
				AssertEquals("ColumnStyleType", typeof(ZCodeFindBoxColumnStyle), columnInfo.ColumnStyleType);
			}
		}

		public void TestSupDocTypeFindBox()
		{
			using (var control = new SupportingDocumentsUserControl())
			{
				var supDocTypeFindBox = control.FindSingleOrDefault<ZCodeFindBox>("SupDocTypeFindBox");
				AssertNotNull("SupDocTypeFindBox", supDocTypeFindBox);
			}
		}
	}
}
