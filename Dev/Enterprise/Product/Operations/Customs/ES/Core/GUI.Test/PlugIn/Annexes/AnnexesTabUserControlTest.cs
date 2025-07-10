using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using EntrySubStyleList = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList;

namespace Enterprise.Customs.ES.GUI.Testing
{
	public class AnnexesTabUserControlTest : TestCaseWithFactory
	{
		public void TestSetUpAnnexGridColumns()
		{
			var testDec = Factory.NewWithValidTestData<JobDeclaration>();
			testDec.JE_ApplicationCode = "BLT";
			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			testDec.CustomsEntryInstructions.RemoveAndDeleteAll();
			var entryInstruction = testDec.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

			var invoiceHeader = testDec.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;

			CombineAssertions(() =>
			{
				var mergeResult = testDec.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Merge done", true, mergeResult);

				using (var form = new ZForm(testDec))
				using (var userControl = new AnnexesTabUserControl())
				{
					form.Controls.Add(userControl);
					form.SetDataBinding(testDec, ".");
					form.Show();

					var annexGrid = userControl.Controls.Find("AnnexGrid", true).First() as ZGrid;
					var storageDocReferenceColumn = FindColumnByName(annexGrid, "CSD_StorageDocReference");
					AssertNotNull("User control should have CSD_StorageDocReference column", storageDocReferenceColumn);
					AssertEquals("CSD_StorageDocReference column name is correct", "eDoc", storageDocReferenceColumn.CaptionResourceString.Caption);

					var descriptionColumn = FindColumnByName(annexGrid, "CSD_Description");
					AssertNotNull("User control should have CSD_Description column", descriptionColumn);
					AssertEquals("CSD_Description column name is correct", "Description", descriptionColumn.CaptionResourceString.Caption);

					var documentExtensionColumn = FindColumnByName(annexGrid, "DocumentExtension");
					AssertNotNull("User control should have DocumentExtension column", documentExtensionColumn);
					AssertEquals("DocumentExtension column name is correct", "Document Extension", documentExtensionColumn.CaptionResourceString.Caption);

					var documentSizeColumn = FindColumnByName(annexGrid, "DocumentSize");
					AssertNotNull("User control should have DocumentSize column", documentSizeColumn);
					AssertEquals("DocumentSize column name is correct", "Document Size", documentSizeColumn.CaptionResourceString.Caption);

					var messageStatusColumn = FindColumnByName(annexGrid, "MessageStatus");
					AssertNotNull("User control should have MessageStatus column", messageStatusColumn);
					AssertEquals("MessageStatus column name is correct", "Message Status", messageStatusColumn.CaptionResourceString.Caption);
				}
			});
		}

		ZGridColumnInfo FindColumnByName(ZGrid grid, string columnName)
			=> grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == $"{columnName}");
	}
}
