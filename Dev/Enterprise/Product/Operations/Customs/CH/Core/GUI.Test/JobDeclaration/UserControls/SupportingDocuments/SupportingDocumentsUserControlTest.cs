using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.GUI.Testing;

public class SupportingDocumentsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSource()
	{
		using (var control = new SupportingDocumentsUserControl())
		{
			AssertEquals(typeof(SupportingDocument), control.BindingSource.DataSourceType);
		}
	}

	public void TestGridColumnStyleProperties()
	{
		var supportingDocument = Factory.New<SupportingDocument>();
		var collection = new SupportingDocumentCollection(supportingDocument);
		using (var control = new SupportingDocumentsUserControl())
		{
			var grid = control.SupportingDocumentsGrid;
			grid.SetDataBinding(collection, "");
			control.Show();

			AssertEquals(4, grid.Columns.Count);
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Code: CharacterCasing", CharacterCasing.Upper, grid.GetColumnStyle(SupportingDocument.Schema.CSI_Code).CharacterCasing);
				AssertEquals("CSI_ReferenceNumber: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber).CharacterCasing);
				AssertEquals("CSI_ReferenceNumber2: CharacterCasing", CharacterCasing.Normal, grid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber2).CharacterCasing);
				AssertEquals("CSI_DateOfIssue: DateTimeFormat", ZDateTimePickerFormat.Short, (grid.GetColumnStyle(SupportingDocument.Schema.CSI_DateOfIssue) as ZDateEditColumnStyleInfo).DateTimeFormat);
			});
		}
	}

	public void TestGridColumnStyleVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();

		using (var control = new SupportingDocumentsUserControl())
		{
			control.JobDeclaration = declaration;

			var typeGridColumn = control.SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_Code);
			var referenceGridColumn = control.SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_Code);
			var additionalInformationGridColumn = control.SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber2);
			var dateOfIssueGridColumn = control.SupportingDocumentsGrid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber2);

			AssertNotNull(nameof(typeGridColumn), typeGridColumn);
			AssertNotNull(nameof(referenceGridColumn), referenceGridColumn);
			AssertNotNull(nameof(additionalInformationGridColumn), additionalInformationGridColumn);
			AssertNotNull(nameof(dateOfIssueGridColumn), dateOfIssueGridColumn);

			declaration.JE_MessageType = MessageTypeCodeList.Codes.Import;
			AssertEquals(nameof(typeGridColumn.IsUnavailable), false, typeGridColumn.IsUnavailable);
			AssertEquals(nameof(referenceGridColumn.IsUnavailable), false, referenceGridColumn.IsUnavailable);
			AssertEquals(nameof(additionalInformationGridColumn.IsUnavailable), false, additionalInformationGridColumn.IsUnavailable);
			AssertEquals(nameof(dateOfIssueGridColumn.IsUnavailable), false, dateOfIssueGridColumn.IsUnavailable);

			declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;
			AssertEquals(nameof(typeGridColumn.IsUnavailable), false, typeGridColumn.IsUnavailable);
			AssertEquals(nameof(referenceGridColumn.IsUnavailable), false, referenceGridColumn.IsUnavailable);
			AssertEquals(nameof(additionalInformationGridColumn.IsUnavailable), true, additionalInformationGridColumn.IsUnavailable);
			AssertEquals(nameof(dateOfIssueGridColumn.IsUnavailable), true, additionalInformationGridColumn.IsUnavailable);
		}
	}
}
