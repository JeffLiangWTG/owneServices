using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class LocalExportMessageUserControlTest : TestCaseWithFactory
	{
		public class Columns
		{
			public ZString Names;
			public System.Type InfoType;
		}

		Columns CreateColumnsType(ZString names, System.Type infoType)
		{
			var result = new Columns();
			result.Names = names;
			result.InfoType = infoType;
			return result;
		}

		public void TestEntriesGridInEntriesTab()
		{
			using (var control = new LocalExportMessageUserControl())
			{
				var grid = (ZGrid)control.Controls.Find("EntriesBoundGrid", true)[0];

				var columnsList = new List<Columns>();
				columnsList.Add(CreateColumnsType("FormattedEntryNumber", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(CusEntryHeader.Schema.CH_MessageType, typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("MessageTypeDescription", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(CusEntryHeader.Schema.CH_Status, typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("MessageStatusDescription", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(CusEntryHeader.Schema.CH_EntryStatus, typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("EntryHeaderStatusDescription", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(CusEntryHeader.Schema.CH_EntrySubmittedDate, typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("FormattedRefNumber", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("AcceptedDate", typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("MostRecentCustomsReviewDate", typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("TotalGrossWeightInKG", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("TotalPackages", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("CustomsValue", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("KR_ActualDateOfLoading", typeof(ZDateEditColumnStyleInfo)));

				Assert(grid, columnsList);
			}
		}

		public void TestEntryLineColumns()
		{
			using (var control = new LocalExportMessageUserControl())
			{
				var grid = (ZGrid)control.Controls.Find("EntryLineGrid", true)[0];

				var columnsList = new List<Columns>();
				columnsList.Add(CreateColumnsType("CL_LineNumber", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(CusEntryLine.Schema.FormattedTariff, typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("CL_Description", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("SupportingDocumentNo", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("SupportingDocumentType", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("SupportingDocumentTypeDescription", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("NetWeightInKG", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("InvoiceQuantity", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("InvoiceUQ", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("NoOfPacks", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("PackType", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("CL_CustomsValue", typeof(ZCalcEditColumnStyleInfo)));

				Assert(grid, columnsList);
			}
		}

		public void TestEntryAmendmentsColumns()
		{
			using (var control = new LocalExportMessageUserControl())
			{
				var grid = (ZGrid)control.Controls.Find("EntryAmendmentsBoundGrid", true)[0];

				var columnsList = new List<Columns>();
				columnsList.Add(CreateColumnsType("FormattedDeclarationNumber", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("MessageStatus", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("MessageStatusDescription", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("SubmissionDate", typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("AmendType", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("AmendTypeDescription", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("ReasonCode", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("ReasonCodeDescription", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("NoticeType", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("NoticeTypeDescription", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("FormattedCustomsReferenceNumber", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("CustomsReviewDate", typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("CustomerOfficer", typeof(ZTextBoxColumnStyleInfo)));

				Assert(grid, columnsList);
			}
		}

		public void TestEntryAmendmentItemsColumns()
		{
			using (var control = new LocalExportMessageUserControl())
			{
				var grid = (ZGrid)control.Controls.Find("EntryAmendedItemsGrid", true)[0];

				var columnsList = new List<Columns>();
				columnsList.Add(CreateColumnsType("AmendTypeDescriptionForEntry", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("ID", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("DataItemDescription", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("BeforeValue", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("AfterValue", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("DataItemNo", typeof(ZTextBoxColumnStyleInfo)));
				AssertEquals(false, grid.GetColumnStyle("DataItemNo").IsVisible);

				Assert(grid, columnsList);
			}
		}

		void Assert(ZGrid grid, List<Columns> columnsList)
		{
			foreach (Columns col in columnsList)
			{
				var checkColumn = grid.GetColumnStyle(col.Names);
				AssertNotNull(checkColumn);
				AssertEquals(col.InfoType, checkColumn.GetType());
				AssertEquals("IsReadOnly", true, checkColumn.IsReadOnly);
			}
		}

		public void TestEntryDetailsTopArea()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MessagesTabPage;
				AssertEquals(typeof(LocalExportMessageUserControl), brokerageControl.MessageUserControl.GetType());

				using (var control = brokerageControl.MessageUserControl)
				{
					var entryDetailsGroupBox = control.FindSingle<ZGroupBox>("EntryDetailsGroupBox");

					AssertNotNull(entryDetailsGroupBox.FindSingle<ZTextBox>("EntryNumberTextBox"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZDropEdit>("MessageTypeDropEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZDropEdit>("MessageStatusDropEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZDropEdit>("EntryStatusDropEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZDateEdit>("EntrySubmittedDateEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZTextBox>("ReferenceNumberTextBox"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZDateEdit>("AcceptedDateEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZDateEdit>("CustomsReviewDateEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("TotalCustomsValueKRWCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("TotalGrossWeightInKGCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("TotalPackagesCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZTextBox>("CustomerOfficerTextBox"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZTextBox>("CustomsRemarkTextBox"));
				}
			}
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.LocalExport;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.Messages.AddNew();
		}
		JobDeclaration declaration;
	}
}
