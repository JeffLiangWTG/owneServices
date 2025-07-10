using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(StatementForm))]
	sealed class StatementFormTest : ZFormBasherTest
	{
		public void TestReadOnly()
		{
			var header = Factory.New<CusStatementHeader>();

			using (var frm = new StatementForm(header))
			{
				frm.Show();
				Application.DoEvents();

				Assert("Should be readonly.", header.ReadOnly);
			}
		}

		public void TestVisible()
		{
			var header = Factory.New<CusStatementHeader>();
			header.B2_StatementNumber = "DN-123456789RM2345-1";

			void AssertVisible(string[] expectedLineColumns, string[] expectedLineGroupColumns, string[] expectedLineGroupFinancialColumns)
			{
				using (var frm = new StatementForm(header))
				{
					frm.Show();
					Application.DoEvents();

					var grid = (ZGrid)frm.Controls.Find("StatementLinesGrid", true).First();
					var actualColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(c => !c.IsUnavailable).Select(c => c.ColumnName);

					AssertContainsExactElementsInAnyOrder(expectedLineColumns, actualColumns);

					grid = (ZGrid)frm.Controls.Find("LineGroupGrid", true).First();
					actualColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(c => !c.IsUnavailable).Select(c => c.ColumnName);

					AssertContainsExactElementsInAnyOrder(expectedLineGroupColumns, actualColumns);

					grid = (ZGrid)frm.Controls.Find("LineGroupFinancialDetailGrid", true).First();
					actualColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Where(c => !c.IsUnavailable).Select(c => c.ColumnName);
					AssertContainsExactElementsInAnyOrder(expectedLineGroupFinancialColumns, actualColumns);

					var transactionsTabControl = (ZTabControl)frm.Controls.Find("TransactionsTabControl", true).First();

					var dailyStatementTabPage = transactionsTabControl.GetTabPage("DailyStatementTabPage");

					if (header.B2_IsMonthlyStatement)
					{
						transactionsTabControl.SelectedTab = dailyStatementTabPage;
						AssertEquals("Should only be visible when the statement header is SOA.", header.B2_IsMonthlyStatement, dailyStatementTabPage.TabVisible);
					}
					else
					{
						AssertNull("Should be hidden when the statement header is DN.", dailyStatementTabPage);
					}

					var transactionsTabPage = transactionsTabControl.GetTabPage("TransactionsTabPage");
					transactionsTabControl.SelectedTab = transactionsTabPage;

					var chargesGroupBox = (ZGroupBox)frm.Controls.Find("ChargesGroupBox", true).First();

					AssertEquals("Should only be visible when the statement header is Daily Statement.", !header.B2_IsMonthlyStatement, chargesGroupBox.Visible);
				}
			}

			var lineColumns = new[]
			{
				CusStatementLineSchema.Constants.B3_BrokerReference,
				CusStatementLineSchema.Constants.B3_ImporterCustomsID,
				CusStatementLineSchema.Constants.B3_EntryType,
				CusStatementLineSchema.Constants.B3_EntryNum,
				CusStatementLineSchema.Constants.B3_EntryDate,
				CusStatementLineSchema.Constants.B3_EntryProcessPort,
				CusStatementLineSchema.Constants.B3_Status,
				CusStatementLineSchema.Constants.B3_CustomsFeesTotal,
				CusStatementLineSchema.Constants.B3_ScheduledProcessDate,
				CusStatementLineSchema.Constants.B3_AssociatedEntry,
				CusStatementLine.Schema.B4_ChargeAmountDTY,
				CusStatementLine.Schema.B4_ChargeAmountEXS,
				CusStatementLine.Schema.B4_ChargeAmountGSTOrGSD,
				CusStatementLine.Schema.B4_ChargeAmountSIM,
				CusStatementLine.Schema.B4_ChargeAmountOTH,
				nameof(CusStatementLine.PaymentMethod),
				CusStatementLine.Schema.ImporterCode,
				CusStatementLine.Schema.ImporterName
			};

			var lineColumnsForCARMDN = new[]
			{
				CusStatementLineSchema.Constants.B3_BrokerReference,
				CusStatementLineSchema.Constants.B3_ImporterCustomsID,
				CusStatementLineSchema.Constants.B3_EntryType,
				CusStatementLineSchema.Constants.B3_EntryNum,
				CusStatementLineSchema.Constants.B3_EntryDate,
				CusStatementLineSchema.Constants.B3_EntryProcessPort,
				CusStatementLineSchema.Constants.B3_Status,
				CusStatementLineSchema.Constants.B3_CustomsFeesTotal,
				CusStatementLineSchema.Constants.B3_ScheduledProcessDate,
				CusStatementLineSchema.Constants.B3_AssociatedEntry,
				CusStatementLine.Schema.B4_CARMDNChargeAmount_Duties,
				CusStatementLine.Schema.B4_CARMDNChargeAmount_ExciseTax,
				CusStatementLine.Schema.B4_CARMDNChargeAmount_ExciseDuties,
				CusStatementLine.Schema.B4_CARMDNChargeAmount_GSTAndHSTAndPST,
				CusStatementLine.Schema.B4_CARMDNChargeAmount_Interests,
				CusStatementLine.Schema.B4_CARMDNChargeAmount_Others,
				CusStatementLine.Schema.B4_CARMDNChargeAmount_SIMA,
				nameof(CusStatementLine.PaymentMethod),
				CusStatementLine.Schema.ImporterCode,
				CusStatementLine.Schema.ImporterName
			};

			var lineGroupColumns = new[]
			{
				CusStatementLineGroupSchema.Constants.B10_OH_Importer,
				CusStatementLineGroupSchema.Constants.B10_ImporterCustomsID,
				nameof(CusStatementLineGroup.B10_TotalPaymentReceived),
				nameof(CusStatementLineGroup.PaymentMethod),
				nameof(CusStatementLineGroup.TotalPayableByBrokerOnDailyStatement),
				nameof(CusStatementLineGroup.TotalPayableByImporterOnDailyStatement),
				nameof(CusStatementLineGroup.B10_ImporterName),
				nameof(CusStatementLineGroup.Total),

				nameof(CusStatementLineGroup.TotalDuties),
				nameof(CusStatementLineGroup.TotalSIMA),
				nameof(CusStatementLineGroup.TotalExcise),
				nameof(CusStatementLineGroup.TotalGSTOrGSD),
				nameof(CusStatementLineGroup.TotalOthers),
			};

			var lineGroupColumnsForCARMDN = new[]
			{
				CusStatementLineGroupSchema.Constants.B10_OH_Importer,
				CusStatementLineGroupSchema.Constants.B10_ImporterCustomsID,
				nameof(CusStatementLineGroup.B10_TotalPaymentReceived),
				nameof(CusStatementLineGroup.PaymentMethod),
				nameof(CusStatementLineGroup.TotalPayableByBrokerOnDailyStatement),
				nameof(CusStatementLineGroup.TotalPayableByImporterOnDailyStatement),
				nameof(CusStatementLineGroup.B10_ImporterName),
				nameof(CusStatementLineGroup.Total),

				nameof(CusStatementLineGroup.TotalDuties),
				nameof(CusStatementLineGroup.TotalSIMA),
				nameof(CusStatementLineGroup.TotalExcise),
				nameof(CusStatementLineGroup.TotalExciseDuties),
				nameof(CusStatementLineGroup.TotalGSTOrGSD),
				nameof(CusStatementLineGroup.TotalOthers),
				nameof(CusStatementLineGroup.TotalInterests),
			};

			var lineGroupFinancialColumns = new[]
			{
				nameof(CusStatementLineGroupFinancialDetail.ChargeTypeDescription),
				CusStatementLineGroupFinancialDetail.Schema.B11_Amount
			};

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				header.B2_IsMonthlyStatement = false;
				AssertVisible(lineColumns, lineGroupColumns, lineGroupFinancialColumns);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				header.B2_RMNumber = "1";
				AssertVisible(lineColumnsForCARMDN, lineGroupColumnsForCARMDN, lineGroupFinancialColumns);
			}

			lineColumns = new[]
			{
				CusStatementLineSchema.Constants.B3_ImporterCustomsID,
				CusStatementLineSchema.Constants.B3_EntryType,
				CusStatementLineSchema.Constants.B3_EntryNum,
				CusStatementLine.Schema.EntryStatusDescription,
				CusStatementLineSchema.Constants.B3_Status,
				CusStatementLineSchema.Constants.B3_CustomsFeesTotal,
				CusStatementLineSchema.Constants.B3_EIIndicator,
				CusStatementLineSchema.Constants.B3_AssociatedEntry,
				CusStatementLineSchema.Constants.B3_CreditNote,
				CusStatementLineSchema.Constants.B3_CreditNoteDate,
			};

			lineGroupColumns = new[]
			{
				CusStatementLineGroupSchema.Constants.B10_OH_Importer,
				CusStatementLineGroupSchema.Constants.B10_ImporterCustomsID,
				nameof(CusStatementLineGroup.B10_PreviousMonthlyStatementTotal),
				nameof(CusStatementLineGroup.B10_PaymentReceivedSinceLastMonthlyStatement),
				nameof(CusStatementLineGroup.B10_UnpaidBalanceForward),
				nameof(CusStatementLineGroup.B10_ArrearsInterest),
				nameof(CusStatementLineGroup.B10_TransactionTotal),
				nameof(CusStatementLineGroup.B10_OtherCharges),
				nameof(CusStatementLineGroup.B10_TotalCredits),
				nameof(CusStatementLineGroup.B10_InterestAmount),
				nameof(CusStatementLineGroup.B10_Refund),
				nameof(CusStatementLineGroup.B10_TotalPayableForBrokerSoAStatement),
				nameof(CusStatementLineGroup.B10_TotalPayableForImporterSoAStatement),
				nameof(CusStatementLineGroup.B10_GIPastTotal),
				nameof(CusStatementLineGroup.B10_GICurrentTotal),
				nameof(CusStatementLineGroup.TotalDuties),
				nameof(CusStatementLineGroup.TotalSIMA),
				nameof(CusStatementLineGroup.TotalExcise),
				nameof(CusStatementLineGroup.TotalGSTOrGSD),
				nameof(CusStatementLineGroup.TotalOthers),
			};

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				header.B2_IsMonthlyStatement = true;
				AssertVisible(lineColumns, lineGroupColumns, lineGroupFinancialColumns);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				header.B2_RMNumber = "2";
				AssertVisible(lineColumns, lineGroupColumns, lineGroupFinancialColumns);
			}
		}

		public void TestDecimals()
		{
			var header = Factory.New<CusStatementHeader>();

			using (var frm = new StatementForm(header))
			{
				frm.Show();
				Application.DoEvents();

				var columnsWithWrongDecimals = new List<string>();

				IEnumerable<string> FindColumns(string gridName)
				{
					var grid = (ZGrid)frm.Controls.Find(gridName, true).First();
					var columns = grid.ColumnStyles.Cast<ZGridColumnInfo>();

					foreach (var column in columns.OfType<ZCalcEditColumnStyleInfo>())
					{
						if (column.Decimals != 2)
						{
							yield return $"{grid.Name} - {column.ColumnName}";
						}
					}
				}

				columnsWithWrongDecimals.AddRange(FindColumns("LineGroupGrid"));
				columnsWithWrongDecimals.AddRange(FindColumns("LineChargeGrid"));
				columnsWithWrongDecimals.AddRange(FindColumns("StatementLinesGrid"));
				columnsWithWrongDecimals.AddRange(FindColumns("LineGroupFinancialDetailGrid"));

				var messsage = $@"The Decimals of these columns should be 2, please be careful when you working in a design mode.
{string.Join(System.Environment.NewLine, columnsWithWrongDecimals)}";

				Assert(messsage, columnsWithWrongDecimals.Count == 0);
			}
		}

		public void TestConstructor()
		{
			var header = Factory.New<CusStatementHeader>();
			header.B2_IsMonthlyStatement = true;

			using (var form = new StatementForm(header))
			{
				AssertNull("Not support DocDataPlugIn for ARL", form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}

			header.B2_IsMonthlyStatement = false;

			using (var form = new StatementForm(header))
			{
				AssertNotNull("Support DocDataPlugIn for DN", form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<CusStatementHeader>();

			var result = new StatementForm(header)
			{
				ControllerID = ControllerIDs.Customs.CA.CADailyNoticeReconciliation
			};

			return result;
		}
	}
}
