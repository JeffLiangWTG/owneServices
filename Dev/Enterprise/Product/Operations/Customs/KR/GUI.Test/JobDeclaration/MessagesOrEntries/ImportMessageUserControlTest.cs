using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class ImportMessageUserControlTest : TestCaseWithFactory
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
			using (var control = new ImportMessageUserControl())
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
				columnsList.Add(CreateColumnsType("AcceptedDate", typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(CusEntryHeader.Schema.CH_EntryReleaseDate, typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("CustomsValue", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("CustomsValueUSD", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Freight", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Insurance", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("AdditionalAmount", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("DeductedAmount", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("TotalAmountPayable", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("FormattedTotalDutyAmount", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("TotalVAT", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("TotalValueForVAT", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("TotalVATExemptionValue", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("TotalSpecialConsumptionTax", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("TotalTransportationTax", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("TotalLiquorTax", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("TotalEducationTax", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("TotalAgricultureTax", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("PenaltyForLateDeclaration", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("PenaltyForMissedDeclaration", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("TotalInvoiceAmount", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("InvoiceAmountCurrency", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("TotalGrossWeightInKG", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("TotalPackages", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("PackagesUQ", typeof(ZTextBoxColumnStyleInfo)));
				Assert(grid, columnsList);
			}
		}

		public void TestEntriesLineGrid()
		{
			using (var control = new ImportMessageUserControl())
			{
				var grid = (ZGrid)control.Controls.Find("EntryLineGrid", true)[0];

				var columnsList = new List<Columns>();
				columnsList.Add(CreateColumnsType(CusEntryLine.Schema.CL_LineNumber, typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(CusEntryLine.Schema.FormattedTariff, typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(CusEntryLine.DutyRateCodeDescription), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(CusEntryLine.PreferenceCodeDescription), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(CusEntryLine.DutyRate), typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(CusEntryLine.TariffDescription), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(CusEntryLine.CountryOfOriginCode), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(CusEntryLine.NetWeightInKG), typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(CusEntryLine.Schema.CustomsQuantity, typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(CusEntryLine.CustomsUnitQty), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(CusEntryLine.Schema.CL_CustomsValue, typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(CusEntryLine.CustomsValueUSD), typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(CusEntryLine.DutyFeeAmount), typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(CusEntryLine.DutyReductionRate), typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(CusEntryLine.CL_DutyReductionAmount), typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(CusEntryLine.VATAmount), typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(CusEntryLine.DomesticTaxAmount), typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(CusEntryLine.EducationTaxAmount), typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(CusEntryLine.AgricultureTaxAmount), typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(CusEntryLine.Schema.CL_ValueForVAT, typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(CusEntryLine.CL_ValueExemptForVAT), typeof(ZCalcEditColumnStyleInfo)));

				Assert(grid, columnsList);
			}
		}

		public void TestSubsequentMessages()
		{
			using (var control = new ImportMessageUserControl())
			{
				var subsequentTabControl = control.Controls.Find("SubsequentTabControl", true)[0];
				AssertNotNull(subsequentTabControl);
				AssertNotNull(subsequentTabControl.Controls.Find("SubsequentMessageDetailsUserControl", true)[0]);
				AssertNotNull(subsequentTabControl.Controls.Find("TaxExemptionOrSpecificUseDutyRateUserControl", true)[0]);
				AssertNotNull(subsequentTabControl.Controls.Find("ApplicationOfFTARateUserControl", true)[0]);
				AssertNotNull(subsequentTabControl.Controls.Find("RequestToExtendReExportDateUserControl", true)[0]);
				AssertNotNull(subsequentTabControl.Controls.Find("ExemptionRequestOfPenaltyUserControl", true)[0]);
				AssertNotNull(subsequentTabControl.Controls.Find("AgreedRateForAllLinesUserControl", true)[0]);
				AssertNotNull(subsequentTabControl.Controls.Find("SubsequentMessageRefundRequestUserControl", true)[0]);
			}
		}

		public void TestEntryAmendmentsColumns()
		{
			using (var control = new ImportMessageUserControl())
			{
				var grid = (ZGrid)control.Controls.Find("EntryAmendmentsBoundGrid", true)[0];

				var columnsList = new List<Columns>();
				columnsList.Add(CreateColumnsType("SubmissionDate", typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("AmendSequenceNo", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("MessageStatus", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("MessageStatusDescription", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("AmendType1Description", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("AmendType2Description", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("FaultPartyDescription", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("ReasonCodeDescription", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("NoticeTypeDescription", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("DecisionDate", typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("CustomerOfficer", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("PaymentAmount", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("DelayPaymentAmount", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("AmendPenaltyPayable", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("BeforeTotalDutyTax", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("AfterTotalDutyTax", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("DutyTaxDifference", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("BeforeCustomsValue", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("AfterCustomsValue", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("CustomsValueDifference", typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("AmendType1", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("AmendType2", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("FaultParty", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("ReasonCode", typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("NoticeType", typeof(ZTextBoxColumnStyleInfo)));

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

				if (col.Names == nameof(ImportAmendmentDetails.AmendType1) || col.Names == nameof(ImportAmendmentDetails.AmendType2)
					|| col.Names == nameof(ImportAmendmentDetails.FaultParty) || col.Names == nameof(ImportAmendmentDetails.ReasonCode)
					|| col.Names == nameof(ImportAmendmentDetails.NoticeType))
				{
					Assert(!checkColumn.IsVisible);
				}
				else
				{
					Assert(checkColumn.IsVisible);
				}
			}
		}

		public void TestEntryDetailsTopArea()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MessagesTabPage;
				AssertEquals(typeof(ImportMessageUserControl), brokerageControl.MessageUserControl.GetType());

				using (var importMessageUserControl = brokerageControl.MessageUserControl)
				{
					var entryDetailsPanel = importMessageUserControl.FindSingle<DynamicLayoutPanel>("EntryDetailsInnerPanel");

					AssertNotNull(entryDetailsPanel.FindSingle<ZTextBox>("EntryNumberTextBox"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZDropEdit>("MessageTypeDropEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZDropEdit>("MessageStatusDropEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZDropEdit>("EntryStatusDropEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZDateEdit>("EntrySubmittedDateEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZDateEdit>("AcceptedDateEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZDateEdit>("ClearedDateEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZTextBox>("IncotermTextBox"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZCalcEdit>("TotalInvoiceAmountCalcEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZCalcEdit>("TotalCustomsValueKRWCalcEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZCalcEdit>("TotalCustomsValueUSDCalcEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZCalcEdit>("TotalValueForVATCalcEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZCalcEdit>("TotalVATExemptionValueCalcEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZGroupBox>("CustomsDisbursementBillGroupBox"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZCalcEdit>("TotalDutyAmountCalcEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZCalcEdit>("TotalSpecialConsumptionTaxCalcEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZCalcEdit>("TotalTransportationTaxCalcEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZCalcEdit>("TotalLiquorTaxCalcEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZCalcEdit>("TotalEducationTaxCalcEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZCalcEdit>("TotalAgricultureTaxCalcEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZCalcEdit>("TotalVATCalcEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZCalcEdit>("TotalPayableAmountCalcEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZCalcEdit>("PenaltyForLateDeclarationCalcEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZCalcEdit>("PenaltyForMissedDeclarationCalcEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZCalcEdit>("TotalGrossWeightInKGCalcEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZCalcDropEdit>("TotalPackagesDropEdit"));
					AssertNotNull(entryDetailsPanel.FindSingle<ZTextBox>("CustomerOfficerTextBox"));
					AssertNotNull(entryDetailsPanel.FindSingle<LongTextControl>("CustomsRemarkLongTextBox"));

					var customsDisbursementBillGrid = (ZGrid)entryDetailsPanel.FindSingle<ZGroupBox>("CustomsDisbursementBillGroupBox").Controls.Find("CustomsDisbursementBillGrid", true)[0];
					var columnsList = new List<Columns>();
					columnsList.Add(CreateColumnsType(CusStatementHeader.Schema.B2_StatementNumber, typeof(ZCodeFindBoxColumnStyleInfo)));
					columnsList.Add(CreateColumnsType(CusStatementHeader.Schema.B2_ProcessDate, typeof(ZDateEditColumnStyleInfo)));
					columnsList.Add(CreateColumnsType(CusStatementHeader.Schema.B2_PrintDate, typeof(ZDateEditColumnStyleInfo)));
					columnsList.Add(CreateColumnsType(CusStatementHeader.Schema.B2_DueDate, typeof(ZDateEditColumnStyleInfo)));
					columnsList.Add(CreateColumnsType( CusStatementHeader.Schema.B2_StatementAmount, typeof(ZCalcEditColumnStyleInfo)));
					columnsList.Add(CreateColumnsType( nameof(CusStatementHeader.B2_StatusName), typeof(ZTextBoxColumnStyleInfo)));
					columnsList.Add(CreateColumnsType(CusStatementHeader.Schema.B2_PaymentAuthorizationDate, typeof(ZDateEditColumnStyleInfo)));
					Assert(customsDisbursementBillGrid, columnsList);

					var entryDetailsGroupBox = importMessageUserControl.FindSingle<ZGroupBox>("ExtendedInfoGroupBox");
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZTextBox>("EntryLineNumberTextBox"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZTextBox>("TariffTextBox"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZDropEdit>("DutyRateTypeDropEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZDropEdit>("DutyRateCodeDropEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("DutyRateCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZTextBox>("DescriptionTextBox"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCodeFindBox>("GoodsOriginCodeFindBox"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("NetWeightCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("CustomsQtyCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZTextBox>("CustomsQtyUQTextBox"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("CustomsValueKRWCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("CustomsValueUSDCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("DutyAmountCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("DutyReductionRateCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("DutyReductionAmountCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("VATAmountCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("DomesticTaxAmountCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("EducationTaxAmountCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("AgricultureTaxAmountCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("ValueForVATCalcEdit"));
					AssertNotNull(entryDetailsGroupBox.FindSingle<ZCalcEdit>("VATExemptionValueCalcEdit"));
				}
			}
		}

		public void TestSubsequentMessages_FTA()
		{
			using (var control = new ImportMessageUserControl())
			{
				var subsequentTabControl = control.FindSingle<ZTabControl>("SubsequentTabControl");
				using (var userControl = subsequentTabControl.FindSingle<ZUserControl>("ApplicationOfFTARateUserControl"))
				{
					var applicationOfFTARate = userControl.FindSingle<ZGroupBox>("ApplicationOfFTARateGroupBox");
					AssertNotNull(applicationOfFTARate.FindSingle<ZDropEdit>("MessageStatusDropEdit"));
					AssertNotNull(applicationOfFTARate.FindSingle<ZDateEdit>("AcceptedDateEdit"));
					AssertNotNull(applicationOfFTARate.FindSingle<ZTextBox>("LawCodeTextBox"));

					var grid = userControl.FindSingle<ZGrid>("AmendmentOfApplicationOfFTARateGrid");

					var columnsList = new List<Columns>();
					columnsList.Add(CreateColumnsType("SequenceNo", typeof(ZCalcEditColumnStyleInfo)));
					columnsList.Add(CreateColumnsType("MessageStatus", typeof(ZTextBoxColumnStyleInfo)));
					columnsList.Add(CreateColumnsType("MessageStatusDescription", typeof(ZTextBoxColumnStyleInfo)));
					columnsList.Add(CreateColumnsType("AcceptedDate", typeof(ZDateEditColumnStyleInfo)));
					columnsList.Add(CreateColumnsType("DecisionDate", typeof(ZDateEditColumnStyleInfo)));
					columnsList.Add(CreateColumnsType("NoticeTypeDescription", typeof(ZTextBoxColumnStyleInfo)));
					columnsList.Add(CreateColumnsType("AmendmentReason", typeof(ZTextBoxColumnStyleInfo)));
					columnsList.Add(CreateColumnsType("AmendmentType", typeof(ZTextBoxColumnStyleInfo)));

					Assert(grid, columnsList);
				}
			}
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = Common.KR.KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.Messages.AddNew();
		}
		JobDeclaration declaration;
	}
}
