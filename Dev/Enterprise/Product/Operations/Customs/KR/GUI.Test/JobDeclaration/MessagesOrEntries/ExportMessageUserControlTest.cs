using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using JobMessageTypeList = Enterprise.Customs.Business.JobMessageTypeList;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class ExportMessageUserControlTest : TestCaseWithFactory
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

		public void TestEntryHeaderColumns()
		{
			using (var control = new ExportMessageUserControl())
			{
				var grid = (ZGrid)control.Controls.Find("EntriesBoundGrid", true)[0];

				var columnsList = new List<Columns>();
				columnsList.Add(CreateColumnsType("FormattedEntryNumber", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(CusEntryHeader.Schema.CH_Status, typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("MessageStatusDescription", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(CusEntryHeader.Schema.CH_EntryStatus, typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("EntryHeaderStatusDescription", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(CusEntryHeader.Schema.CH_EntrySubmittedDate, typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("AcceptedDate", typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(CusEntryHeader.Schema.CH_EntryReleaseDate, typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(CusEntryHeader.DueDateofLoading), typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("KR_ActualDateOfLoading", typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("CustomsValue", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("CustomsValueUSD", typeof(ZCalcEditColumnStyleInfo)));

				Assert(grid, columnsList);
			}
		}

		public void TestEntryLineColumns()
		{
			using (var control = new ExportMessageUserControl())
			{
				var grid = (ZGrid)control.Controls.Find("EntryLineGrid", true)[0];

				var columnsList = new List<Columns>();
				columnsList.Add(CreateColumnsType(CusEntryLine.Schema.FormattedTariff, typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("TariffDescription", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("CountryOfOriginCode", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("NetWeightInKG", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("CustomsQuantity", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("CustomsUnitQty", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("NoOfPacks", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("PackType", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("CL_CustomsValue", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("CustomsValueUSD", typeof(ZCalcEditColumnStyleInfo)));

				Assert(grid, columnsList);
			}
		}

		public void TestEntryDetailsTopArea()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MessagesTabPage;
				AssertEquals(typeof(ExportMessageUserControl), brokerageControl.MessageUserControl.GetType());

				using (var exportMessageUserControl = brokerageControl.MessageUserControl)
				{
					var entryDetailsGroupBox = exportMessageUserControl.FindSingle<ZGroupBox>("EntryDetailsGroupBox");

					AssertNotNull(entryDetailsGroupBox.FindSingle<ZTextBox>("EntryNumberTextBox"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZDropEdit>("MessageTypeDropEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZDropEdit>("MessageStatusDropEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZDropEdit>("EntryStatusDropEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZDateEdit>("EntrySubmittedDateEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZDateEdit>("AcceptedDateEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZDateEdit>("ClearedDateEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZDateEdit>("DueDateOfLoadingDateEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZDateEdit>("ActualDateOfLoadingDateEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZTextBox>("IncotermTextBox"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("TotalInvoiceAmountCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZTextBox>("InvoiceAmountCurrencyTextBox"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("TotalCustomsValueKRWCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("TotalCustomsValueUSDCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("FreightCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("InsuranceCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("TotalGrossWeightInKGCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("TotalPackagesCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZTextBox>("PackagesUQTextBox"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZTextBox>("CustomerOfficerTextBox"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZTextBox>("CustomsRemarkTextBox"));
				}
			}
		}

		public void TestEntryAmendmentsColumns()
		{
			using (var control = new ExportMessageUserControl())
			{
				var grid = (ZGrid)control.Controls.Find("EntryAmendmentsBoundGrid", true)[0];

				var columnsList = new List<Columns>();
				columnsList.Add(CreateColumnsType("AmendSequenceNo", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("MessageStatus", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("SubmissionDate", typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("AmendmentType", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("AmendmentTypeDescription", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("FaultParty", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("FaultPartyOtherDescription", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("ReasonCode", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("AmendReasonDescription", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("NoticeType", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("NoticeDescription", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("ApprovalNo", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("DecisionDate", typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("CustomerOfficerIDAndName", typeof(ZTextBoxColumnStyleInfo)));

				Assert(grid, columnsList);
			}
		}

		public void TestEntryAmendmentItemsColumns()
		{
			using (var control = new ExportMessageUserControl())
			{
				var grid = (ZGrid)control.Controls.Find("EntryAmendmentItemGrid", true)[0];

				var columnsList = new List<Columns>();
				columnsList.Add(CreateColumnsType("AmendTypeDescription", typeof(ZTextBoxColumnStyleInfo)));
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

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.Messages.AddNew();
		}
		JobDeclaration declaration;
	}
}
