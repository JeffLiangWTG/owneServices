using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public class ReportsData : NonPersistentBusinessObject, IObsoleteValidation
	{
		public const string LocID = "T209";
		public ZString ReportNumber { get; set; }
		public ZString ReportItemNumber { get; set; }
		public ZString ReportItemName { get; set; }
		public ZString ReportItemFormula { get; set; }
		public ZDecimal ReportItemValue { get; set; }
		public ZString CashFlowCode { get; set; }
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CN specific report strings.")]
	public static class ReportsDataCNStrings
	{
		public const string EndAmt = "期末数";
		public const string BeginAmt = "年初数";
		public const string CurrMonthAmt = "本月数";
		public const string YearToCurrAmt = "本年累计数";
		public const string YearEndBalance = "年末金额";
		public const string YearBeginBalance = "年初余额";
		public const string CurrYearAmt = "本年增加数";
		public const string CurrYearReturnAmt = "本年转回数";
		public const string LastYearActual = "上年实际";
		public const string CurrYearActual = "本年实际";

		public const string NetCashFlow = "的现金流量净额";
		public const string CashFlow = "的现金流量";
		public const string FiveNetIncreaseInCash = "五、现金净增加额";
		public const string AddOpeningCashBalance = "加：期初现金余额";
		public const string ClosingCashBalance = "六、期末现金余额";

		public const string One = "一、";
		public const string Two = "二、";
		public const string Three = "三、";
		public const string Four = "四、";
	}

	public class ReportsDataCollection : NonPersistentBusinessObjectCollection<ReportsData>	{
		public ReportsDataCollection(BusinessObjectFactory factory, ZInt period) : this(factory, period, ZString.Empty)
		{
		}

		public ReportsDataCollection(BusinessObjectFactory factory, ZInt period, ZString reportCode) : this(factory, period, reportCode, ZString.Empty)
		{
		}

		public ReportsDataCollection(BusinessObjectFactory factory, ZInt period, ZString reportCode, ZString branchCode) : base(factory)
		{
			BranchCode = branchCode;
			AddElements(period, reportCode);
		}

		public ReportsDataCollection(BusinessObjectFactory factory, ZInt period, ZString reportCode, ZGuid branchPK)
			: base(factory)
		{
			if (reportCode == AccountingMasterFilesConstants.ReportCodeOfLocalReport.CashFlowStatement)
			{
				SetCashFlowStatement(period, branchPK);
			}
		}

		readonly ZString BranchCode;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ReportsData();
		}

		void AddElements(ZInt period, ZString reportCode)
		{
			if (period <= 0)
			{
				return;
			}

			if (reportCode == ZString.Empty || reportCode == AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet)
			{
				SetBalanceSheet(period);
			}

			if (reportCode == AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLoss)
			{
				SetProfitAndLoss(period);
			}

			if (reportCode == ZString.Empty || reportCode == AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLossMonthly)
			{
				SetProfitAndLossMonthly(period);
			}

			if (reportCode == AccountingMasterFilesConstants.ReportCodeOfLocalReport.VATDetailed)
			{
				SetVATDetailedReport(period);
			}

			if (reportCode == AccountingMasterFilesConstants.ReportCodeOfLocalReport.AssetProvision)
			{
				SetAssetProvisionReport(period);
			}

			if (reportCode == AccountingMasterFilesConstants.ReportCodeOfLocalReport.P_LAppropriation)
			{
				SetPNLAppropriationReport(period);
			}

			if (reportCode == ZString.Empty || reportCode == AccountingMasterFilesConstants.ReportCodeOfLocalReport.EquityMovement)
			{
				SetEquityMovementReport(period);
			}
		}

		decimal EquityMovementReportColumnItemValue(DataRow row, int startColumn, int endColumn, string columnSuffix)
		{
			decimal columnItemValue = 0;
			for (var i = startColumn; i <= endColumn; i++)
			{
				var columnName = "B" + (i + 100).ToString().Substring(1, 2) + columnSuffix;
				if (row.Table.Columns.Contains(columnName))
				{
					columnItemValue += row[columnName] == DBNull.Value ? 0 : Convert.ToDecimal(row[columnName]);
				}
			}
			return columnItemValue;
		}

		void SetCashFlowStatement(ZInt period, ZGuid branchPK)
		{
			ZString repotType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.CashFlowStatement;
			ComplianceReportsSetupCategoryCollection reportCategories = GetReportCategories(repotType);

			string sqlText = GetCashflowSQL(period, GlbCompany.CurrentCompany.PK, branchPK.IsValid ? branchPK.ToString() : string.Empty);

			DataTable results = RunScript(sqlText);
			if (results.Rows.Count > 0)
			{
				ZInt i = 1;
				ZInt displayOrder = 0;
				ZDecimal totalAmt = 0m;
				ZDecimal yTDAmt = 0m;
				ZDecimal subTotalAmt = 0m;
				ZDecimal subYTDAmt = 0m;
				ZString activityDescription1 = ZString.Empty;
				ZString activityDescription2 = ZString.Empty;
				ZString activityDescription3 = ZString.Empty;
				ZString activityDescription4 = ZString.Empty;
				ZString activityDescription5 = ZString.Empty;
				ZString cashFlowCodes = ZString.Empty;

				foreach (DataRow row in results.Rows)
				{
					if (displayOrder < 5 && displayOrder < Convert.ToInt16(row["DisplayOrder"]))
					{
						switch (displayOrder)
						{
							case 0:
								activityDescription1 = TranslateRegistry("CashFlowActivityConfiguration", row["ActivityDescription"].ToString());
								AddNewReportData("8", "0", ReportsDataCNStrings.One + activityDescription1 + ReportsDataCNStrings.CashFlow + ReportsDataCNStrings.CurrMonthAmt, -1m, row["CashFlowCodes"].ToString());
								break;
							case 1:
								AddNewReportData("8", i.ToString(), activityDescription1 + ReportsDataCNStrings.NetCashFlow + (NoResString)"：" + ReportsDataCNStrings.CurrMonthAmt, 0m, cashFlowCodes);
								AddNewReportData("8", i.ToString(), activityDescription1 + ReportsDataCNStrings.NetCashFlow + (NoResString)"：" + ReportsDataCNStrings.YearToCurrAmt, subYTDAmt, cashFlowCodes);

								i++;
								activityDescription2 = TranslateRegistry("CashFlowActivityConfiguration", row["ActivityDescription"].ToString());
								AddNewReportData("8", i.ToString(), ReportsDataCNStrings.Two + activityDescription2 + ReportsDataCNStrings.CashFlow + ReportsDataCNStrings.CurrMonthAmt, -1m, row["CashFlowCodes"].ToString());
								break;
							case 2:
								AddNewReportData("8", i.ToString(), activityDescription2 + ReportsDataCNStrings.NetCashFlow + (NoResString)"：" + ReportsDataCNStrings.CurrMonthAmt, 0m, cashFlowCodes);
								AddNewReportData("8", i.ToString(), activityDescription2 + ReportsDataCNStrings.NetCashFlow + (NoResString)"：" + ReportsDataCNStrings.YearToCurrAmt, subYTDAmt, cashFlowCodes);

								i++;
								activityDescription3 = TranslateRegistry("CashFlowActivityConfiguration", row["ActivityDescription"].ToString());
								AddNewReportData("8", i.ToString(), ReportsDataCNStrings.Three + activityDescription3 + ReportsDataCNStrings.CashFlow + ReportsDataCNStrings.CurrMonthAmt, -1m, row["CashFlowCodes"].ToString());
								break;
							case 3:
								AddNewReportData("8", i.ToString(), activityDescription3 + ReportsDataCNStrings.NetCashFlow + (NoResString)"：" + ReportsDataCNStrings.CurrMonthAmt, 0m, cashFlowCodes);
								AddNewReportData("8", i.ToString(), activityDescription3 + ReportsDataCNStrings.NetCashFlow + (NoResString)"：" + ReportsDataCNStrings.YearToCurrAmt, subYTDAmt, cashFlowCodes);

								i++;
								activityDescription4 = TranslateRegistry("CashFlowActivityConfiguration", row["ActivityDescription"].ToString());
								AddNewReportData("8", i.ToString(), ReportsDataCNStrings.Four + activityDescription4 + ReportsDataCNStrings.CashFlow + ReportsDataCNStrings.CurrMonthAmt, -1m, row["CashFlowCodes"].ToString());
								break;
							case 4:
								AddNewReportData("8", i.ToString(), activityDescription4 + ReportsDataCNStrings.NetCashFlow + (NoResString)"：" + ReportsDataCNStrings.CurrMonthAmt, 0m, cashFlowCodes);
								AddNewReportData("8", i.ToString(), activityDescription4 + ReportsDataCNStrings.NetCashFlow + (NoResString)"：" + ReportsDataCNStrings.YearToCurrAmt, subYTDAmt, cashFlowCodes);

								i++;
								activityDescription5 = TranslateRegistry("CashFlowActivityConfiguration", row["ActivityDescription"].ToString());
								AddNewReportData("8", i.ToString(), activityDescription5 + ReportsDataCNStrings.NetCashFlow + (NoResString)"：" + ReportsDataCNStrings.CurrMonthAmt, -1m, row["CashFlowCodes"].ToString());
								break;
						}
						subTotalAmt = 0m;
						subYTDAmt = 0m;
					}
					var reportsData = AddNewReportData("8", i.ToString(), TranslateRegistry("CashFlowActivityConfiguration", row["CashFlowDescription"].ToString()) + ReportsDataCNStrings.CurrMonthAmt, row["TotalAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["TotalAmount"]), row["CashFlowCodes"].ToString());
					totalAmt += reportsData.ReportItemValue;
					subTotalAmt += reportsData.ReportItemValue;

					var reportsData1 = AddNewReportData("8", i.ToString(), TranslateRegistry("CashFlowActivityConfiguration", row["CashFlowDescription"].ToString()) + ReportsDataCNStrings.YearToCurrAmt, row["YTDAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(row["YTDAmount"]), row["CashFlowCodes"].ToString());
					yTDAmt += reportsData1.ReportItemValue;
					subYTDAmt += reportsData1.ReportItemValue;

					i++;
					displayOrder = Convert.ToInt16(row["DisplayOrder"]);
					cashFlowCodes = row["CashFlowCodes"].ToString();
				}

				if (displayOrder == 4)
				{
					AddNewReportData("8", i.ToString(), activityDescription4 + ReportsDataCNStrings.NetCashFlow + (NoResString)"：" + ReportsDataCNStrings.CurrMonthAmt, 0m, cashFlowCodes);
					AddNewReportData("8", i.ToString(), activityDescription4 + ReportsDataCNStrings.NetCashFlow + (NoResString)"：" + ReportsDataCNStrings.YearToCurrAmt, subYTDAmt, cashFlowCodes);
					i++;
				}

				if (displayOrder == 5)
				{
					AddNewReportData("8", i.ToString(), activityDescription5 + ReportsDataCNStrings.NetCashFlow + (NoResString)"：" + ReportsDataCNStrings.CurrMonthAmt, 0m, ZString.Empty);
					AddNewReportData("8", i.ToString(), activityDescription5 + ReportsDataCNStrings.NetCashFlow + (NoResString)"：" + ReportsDataCNStrings.YearToCurrAmt, subYTDAmt, ZString.Empty);
					i++;
				}

				AddNewReportData("8", i.ToString(), ReportsDataCNStrings.FiveNetIncreaseInCash + ReportsDataCNStrings.CurrMonthAmt, 0m, ZString.Empty);
				AddNewReportData("8", i.ToString(), ReportsDataCNStrings.FiveNetIncreaseInCash + ReportsDataCNStrings.YearToCurrAmt, yTDAmt, ZString.Empty);
				i++;

				AddNewReportData("8", i.ToString(), ReportsDataCNStrings.AddOpeningCashBalance + ReportsDataCNStrings.CurrMonthAmt, 0m, ZString.Empty);
				var reportsDataT6Y = AddNewReportData("8", i.ToString(), ReportsDataCNStrings.AddOpeningCashBalance + ReportsDataCNStrings.YearToCurrAmt, 0m, ZString.Empty);
				sqlText = GetCashAtBeginningSQL(period, GlbCompany.CurrentCompany.PK, branchPK.IsValid ? branchPK.ToString() : string.Empty);
				DataTable cashAtBeginningY = RunScript(sqlText);
				if (cashAtBeginningY.Rows.Count > 0)
				{
					reportsDataT6Y.ReportItemValue = Convert.ToDecimal(cashAtBeginningY.Rows[0][0]);
				}
				i++;

				AddNewReportData("8", i.ToString(), ReportsDataCNStrings.ClosingCashBalance + ReportsDataCNStrings.CurrMonthAmt, 0m, ZString.Empty);
				AddNewReportData("8", i.ToString(), ReportsDataCNStrings.ClosingCashBalance + ReportsDataCNStrings.YearToCurrAmt, yTDAmt + reportsDataT6Y.ReportItemValue, ZString.Empty);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Strings.")]
		static ZString GetCashflowSQL(ZInt period, ZGuid companyPK, string branchPK)
		{
			return ZString.Format(@"SELECT 'X' AS TAG, CashFlowCodes, CashFlowDescription, ActivityType,
												ActivityDescription, TotalAmount, YTDAmount, 
												CASE
													WHEN ActivityType = 'O' THEN 1
													WHEN ActivityType = 'I' THEN 2
													WHEN ActivityType = 'F' THEN 3
													WHEN ActivityType = 'E' THEN 4
													WHEN ActivityType = 'X' THEN 5
													ELSE 10
												END As DisplayOrder FROM Report_CashFlowStatementChina({0},'{1}','{2}') ORDER BY DisplayOrder ",
									period, companyPK, branchPK);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Strings.")]
		static ZString GetCashAtBeginningSQL(ZInt period, ZGuid companyPK, string branchPK)
		{
			return ZString.Format(@"SELECT [Value] FROM dbo.CashAtBeginningOfPeriod ({0}, '{1}', '{2}', 'Y') AS CashAtBeginning",
									period, companyPK, branchPK);
		}

		ZString TranslateRegistry(ZString registryId, ZString englishText)
		{
			ZString result = englishText.TrimEnd();
			var registryItem = AccountingMasterFilesRegistry.Instance.FindByName(registryId);
			if (registryItem != null && registryItem is ICustomizableDataCaptionSource)
			{
				var dataString = CustomizableDataResourceStrings.GetMultilingualString((ICustomizableDataCaptionSource)registryItem, null, englishText.TrimEnd());
				if (dataString != null )
				{
					result = dataString.ToString(Core.SharedConstants.Languages.ChineseSimplified);
				}
			}
			return result;
		}

		ReportsData AddNewReportData(ZString reportNumber, ZString reportItemNumber, ZString reportItemName, ZDecimal reportItemValue, ZString cashFlowCode)
		{
			var reportData = AddNew();
			reportData.ReportNumber = reportNumber;
			reportData.ReportItemNumber = reportItemNumber;
			reportData.ReportItemName = reportItemName;
			reportData.ReportItemValue = reportItemValue;
			reportData.CashFlowCode = cashFlowCode;

			return reportData;
		}

		void SetEquityMovementReport(ZInt period)
		{
			ZString reportNumber = "7";
			ZString repotType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.EquityMovement;
			ComplianceReportsSetupCategoryCollection reportCategories = GetReportCategories(repotType);
			string sqlText = GetChinaStatementOfShareholdersEquitySQL(GlbCompany.CurrentCompany.PK, period, BranchCode);

			DataTable results = RunScript(sqlText);

			if (results.Rows.Count > 0)
			{
				DataRow row = results.Rows[0];

				for (int i = 1; i <= 81; i++)
				{
					for (int j = 0; j < 2; j++)
					{
						ReportsData reportsData = AddNew();
						reportsData.ReportNumber = reportNumber;
						reportsData.ReportItemNumber = i.ToString();
						if ((i >= 3 && i <= 6) || (i >= 18 && i <= 23) || i == 30 || (i >= 49 && i <= 54) || i == 69)
						{
							var columnName = "B" + (i + 100).ToString().Substring(1, 2);
							reportsData.ReportItemName = reportCategories.GetDescriptionFromCode(columnName);

							columnName += j == 0 ? "_Credit" : "_Credit_LY";
							reportsData.ReportItemValue = row[columnName] == DBNull.Value ? 0 : Convert.ToDecimal(row[columnName]);
						}

						if (i == 41 || (i >= 56 && i <= 59) || i == 72)
						{
							var columnName = "B" + i;
							reportsData.ReportItemName = reportCategories.GetDescriptionFromCode(columnName);

							columnName += j == 0 ? "_Debit" : "_Debit_LY";
							reportsData.ReportItemValue = row[columnName] == DBNull.Value ? 0 : Convert.ToDecimal(row[columnName]);
						}
						if (i >= 64 && i <= 66)
						{
							var columnName = "B" + i;
							reportsData.ReportItemName = reportCategories.GetDescriptionFromCode(columnName);

							columnName += j == 0 ? "_Debit" : "_Debit_LY";
							reportsData.ReportItemValue = row[columnName] == DBNull.Value ? 0 : Convert.ToDecimal(row[columnName]);

							columnName = "B" + i;
							columnName += j == 0 ? "_Credit" : "_Credit_LY";
							reportsData.ReportItemValue += row[columnName] == DBNull.Value ? 0 : Convert.ToDecimal(row[columnName]);
						}
						switch (i)
						{
							case 1:
								reportsData.ReportItemName = ReportItems.Descriptions.B01;
								reportsData.ReportItemValue = row["B0306"] == DBNull.Value ? 0 : Convert.ToDecimal(row["B0306"]);
								if (j == 0)
								{
									reportsData.ReportItemValue += EquityMovementReportColumnItemValue(row, 3, 6, "_Credit_LY");
									reportsData.ReportItemValue -= EquityMovementReportColumnItemValue(row, 3, 6, "_Debit_LY");
								}
								break;
							case 2:
								reportsData.ReportItemName = ReportItems.Descriptions.B02;
								reportsData.ReportItemValue = EquityMovementReportColumnItemValue(row, 3, 6, j == 0 ? "_Credit" : "_Credit_LY");
								break;
							case 10:
								reportsData.ReportItemName = ReportItems.Descriptions.B10;
								reportsData.ReportItemValue = EquityMovementReportColumnItemValue(row, 3, 6, j == 0 ? "_Debit" : "_Debit_LY");
								break;
							case 15:
								reportsData.ReportItemName = ReportItems.Descriptions.B15;
								reportsData.ReportItemValue = row["B0306"] == DBNull.Value ? 0 : Convert.ToDecimal(row["B0306"]);
								reportsData.ReportItemValue += EquityMovementReportColumnItemValue(row, 3, 6, "_Credit_LY");
								reportsData.ReportItemValue -= EquityMovementReportColumnItemValue(row, 3, 6, "_Debit_LY");
								if (j == 0)
								{
									reportsData.ReportItemValue += EquityMovementReportColumnItemValue(row, 3, 6, "_Credit");
									reportsData.ReportItemValue -= EquityMovementReportColumnItemValue(row, 3, 6, "_Debit");
								}
								break;

							case 16:
								reportsData.ReportItemName = ReportItems.Descriptions.B16;
								reportsData.ReportItemValue = row["B1841"] == DBNull.Value ? 0 : Convert.ToDecimal(row["B1841"]);
								if (j == 0)
								{
									reportsData.ReportItemValue += EquityMovementReportColumnItemValue(row, 18, 41, "_Credit_LY");
									reportsData.ReportItemValue -= EquityMovementReportColumnItemValue(row, 18, 41, "_Debit_LY");
								}
								break;
							case 17:
								reportsData.ReportItemName = ReportItems.Descriptions.B17;
								reportsData.ReportItemValue = EquityMovementReportColumnItemValue(row, 18, 41, j == 0 ? "_Credit" : "_Credit_LY");
								break;
							case 40:
								reportsData.ReportItemName = ReportItems.Descriptions.B40;
								reportsData.ReportItemValue = EquityMovementReportColumnItemValue(row, 18, 41, j == 0 ? "_Debit" : "_Debit_LY");
								break;
							case 45:
								reportsData.ReportItemName = ReportItems.Descriptions.B45;
								reportsData.ReportItemValue = row["B1841"] == DBNull.Value ? 0 : Convert.ToDecimal(row["B1841"]);
								reportsData.ReportItemValue += EquityMovementReportColumnItemValue(row, 18, 41, "_Credit_LY");
								reportsData.ReportItemValue -= EquityMovementReportColumnItemValue(row, 18, 41, "_Debit_LY");
								if (j == 0)
								{
									reportsData.ReportItemValue += EquityMovementReportColumnItemValue(row, 18, 41, "_Credit");
									reportsData.ReportItemValue -= EquityMovementReportColumnItemValue(row, 18, 41, "_Debit");
								}
								break;

							case 46:
								reportsData.ReportItemName = ReportItems.Descriptions.B46;
								reportsData.ReportItemValue = row["B4959"] == DBNull.Value ? 0 : Convert.ToDecimal(row["B4959"]);
								if (j == 0)
								{
									reportsData.ReportItemValue += EquityMovementReportColumnItemValue(row, 49, 59, "_Credit_LY");
									reportsData.ReportItemValue -= EquityMovementReportColumnItemValue(row, 49, 59, "_Debit_LY");
								}
								break;
							case 47:
								reportsData.ReportItemName = ReportItems.Descriptions.B47;
								reportsData.ReportItemValue = EquityMovementReportColumnItemValue(row, 49, 59, j == 0 ? "_Credit" : "_Credit_LY");
								break;
							case 48:
								reportsData.ReportItemName = ReportItems.Descriptions.B48;
								reportsData.ReportItemValue = EquityMovementReportColumnItemValue(row, 49, 53, j == 0 ? "_Credit" : "_Credit_LY");
								break;
							case 55:
								reportsData.ReportItemName = ReportItems.Descriptions.B55;
								reportsData.ReportItemValue = EquityMovementReportColumnItemValue(row, 49, 59, j == 0 ? "_Debit" : "_Debit_LY");
								break;
							case 63:
								reportsData.ReportItemName = ReportItems.Descriptions.B63;
								reportsData.ReportItemValue = row["B4959"] == DBNull.Value ? 0 : Convert.ToDecimal(row["B4959"]);
								reportsData.ReportItemValue += EquityMovementReportColumnItemValue(row, 49, 59, "_Credit_LY");
								reportsData.ReportItemValue -= EquityMovementReportColumnItemValue(row, 49, 59, "_Debit_LY");
								if (j == 0)
								{
									reportsData.ReportItemValue += EquityMovementReportColumnItemValue(row, 49, 59, "_Credit");
									reportsData.ReportItemValue -= EquityMovementReportColumnItemValue(row, 49, 59, "_Debit");
								}
								break;

							case 67:
								reportsData.ReportItemName = ReportItems.Descriptions.B67;
								reportsData.ReportItemValue = row["B6972"] == DBNull.Value ? 0 : Convert.ToDecimal(row["B6972"]);
								if (j == 0)
								{
									reportsData.ReportItemValue += EquityMovementReportColumnItemValue(row, 69, 72, "_Credit_LY");
									reportsData.ReportItemValue -= EquityMovementReportColumnItemValue(row, 69, 72, "_Debit_LY");
								}
								break;
							case 68:
								reportsData.ReportItemName = ReportItems.Descriptions.B68;
								reportsData.ReportItemValue = EquityMovementReportColumnItemValue(row, 69, 72, j == 0 ? "_Credit" : "_Credit_LY");
								break;
							case 71:
								reportsData.ReportItemName = ReportItems.Descriptions.B71;
								reportsData.ReportItemValue = EquityMovementReportColumnItemValue(row, 69, 72, j == 0 ? "_Debit" : "_Debit_LY");
								break;
							case 76:
								reportsData.ReportItemName = ReportItems.Descriptions.B76;
								reportsData.ReportItemValue = row["B6972"] == DBNull.Value ? 0 : Convert.ToDecimal(row["B6972"]);
								reportsData.ReportItemValue += EquityMovementReportColumnItemValue(row, 69, 72, "_Credit_LY");
								reportsData.ReportItemValue -= EquityMovementReportColumnItemValue(row, 69, 72, "_Debit_LY");
								if (j == 0)
								{
									reportsData.ReportItemValue += EquityMovementReportColumnItemValue(row, 69, 72, "_Credit");
									reportsData.ReportItemValue -= EquityMovementReportColumnItemValue(row, 69, 72, "_Debit");
								}
								break;

							case 77:
								reportsData.ReportItemName = ReportItems.Descriptions.B77;
								reportsData.ReportItemValue = row["B79"] == DBNull.Value ? 0 : Convert.ToDecimal(row["B79"]);
								if (j == 0)
								{
									reportsData.ReportItemValue += row["LastPNL"] == DBNull.Value ? 0 : Convert.ToDecimal(row["LastPNL"]);
									reportsData.ReportItemValue -= row["B79_Debit_LY"] == DBNull.Value ? 0 : Convert.ToDecimal(row["B79_Debit_LY"]);
									reportsData.ReportItemValue += row["B79_Credit_LY"] == DBNull.Value ? 0 : Convert.ToDecimal(row["B79_Credit_LY"]);
								}
								break;
							case 78:
								reportsData.ReportItemName = ReportItems.Descriptions.B78;
								reportsData.ReportItemValue = j == 0
									? (row["CurrPNL"] == DBNull.Value ? 0 : Convert.ToDecimal(row["CurrPNL"]))
									: (row["LastPNL"] == DBNull.Value ? 0 : Convert.ToDecimal(row["LastPNL"]));
								break;
							case 79:
								reportsData.ReportItemName = reportCategories.GetDescriptionFromCode("B79");
								if (j == 0)
								{
									reportsData.ReportItemValue = row["B79_Debit"] == DBNull.Value ? 0 : Convert.ToDecimal(row["B79_Debit"]);
									reportsData.ReportItemValue -= row["B79_Credit"] == DBNull.Value ? 0 : Convert.ToDecimal(row["B79_Credit"]);
								}
								else
								{
									reportsData.ReportItemValue = row["B79_Debit_LY"] == DBNull.Value ? 0 : Convert.ToDecimal(row["B79_Debit_LY"]);
									reportsData.ReportItemValue -= row["B79_Credit_LY"] == DBNull.Value ? 0 : Convert.ToDecimal(row["B79_Credit_LY"]);
								}
								break;
							case 81:
								reportsData.ReportItemName = ReportItems.Descriptions.B81;
								reportsData.ReportItemValue = row["B79"] == DBNull.Value ? 0 : Convert.ToDecimal(row["B79"]);
								reportsData.ReportItemValue += row["LastPNL"] == DBNull.Value ? 0 : Convert.ToDecimal(row["LastPNL"]);
								reportsData.ReportItemValue -= row["B79_Debit_LY"] == DBNull.Value ? 0 : Convert.ToDecimal(row["B79_Debit_LY"]);
								reportsData.ReportItemValue += row["B79_Credit_LY"] == DBNull.Value ? 0 : Convert.ToDecimal(row["B79_Credit_LY"]);

								if (j == 0)
								{
									reportsData.ReportItemValue += row["CurrPNL"] == DBNull.Value ? 0 : Convert.ToDecimal(row["CurrPNL"]);
									reportsData.ReportItemValue -= row["B79_Debit"] == DBNull.Value ? 0 : Convert.ToDecimal(row["B79_Debit"]);
									reportsData.ReportItemValue += row["B79_Credit"] == DBNull.Value ? 0 : Convert.ToDecimal(row["B79_Credit"]);
								}
								break;
						}

						if (reportsData.ReportItemName.IsEmpty)
						{
							RemoveAndDelete(reportsData);
						}
						else
						{
							reportsData.ReportItemName = j == 0
								? reportsData.ReportItemName + ReportsDataCNStrings.CurrYearActual
								: reportsData.ReportItemName + ReportsDataCNStrings.LastYearActual;
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Strings.")]
		static ZString GetChinaStatementOfShareholdersEquitySQL(ZGuid companyPK, ZInt period, ZString branchCode)
		{
			return ZString.Format("EXEC ChinaStatementOfShareholdersEquity '{0}',{1},'{2}'",
									companyPK, period, branchCode);
		}

		void SetPNLAppropriationReport(ZInt period)
		{
			ZString reportNumber = "6";
			ZString repotType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.P_LAppropriation;
			ComplianceReportsSetupCategoryCollection reportCategories = GetReportCategories(repotType);
			DataTable results = RunScript(BSHPNLSQL(period, repotType));
			DataTable pNLData = RunScript(BSHPNLSQL(period, AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLoss));

			if (results.Rows.Count > 0 || pNLData.Rows.Count > 0)
			{
				ReportsData reportsDataPL1E = AddNew();
				reportsDataPL1E.ReportNumber = reportNumber;
				reportsDataPL1E.ReportItemNumber = "10";
				reportsDataPL1E.ReportItemName = ReportItems.Descriptions.PL1 + ReportsDataCNStrings.LastYearActual;
				reportsDataPL1E.ReportItemValue = 0;

				ReportsData reportsDataPL1F = AddNew();
				reportsDataPL1F.ReportNumber = reportNumber;
				reportsDataPL1F.ReportItemNumber = "10";
				reportsDataPL1F.ReportItemName = ReportItems.Descriptions.PL1 + ReportsDataCNStrings.CurrYearActual;
				reportsDataPL1F.ReportItemValue = 0;
				if (pNLData.Rows.Count > 0)
				{
					DataRow pNLrow = pNLData.Rows[0];
#pragma warning disable CW1161 // SQL columns
					ZString categories = "D02, D03, D05, D06, D07, D08, D09, D12, D16, D17, D18, D19, H22, H27, H32, H33";
#pragma warning restore CW1161 // SQL columns

					foreach (DataColumn column in pNLData.Columns.Cast<DataColumn>().Where(column => categories.Contains(column.ColumnName.Substring(0, 3))))
					{
						if (column.ColumnName.Length == 3)
						{
							reportsDataPL1F.ReportItemValue += Convert.ToDecimal(pNLrow[column] == DBNull.Value ? 0 : pNLrow[column]);
						}
						else
						{
							reportsDataPL1E.ReportItemValue += Convert.ToDecimal(pNLrow[column] == DBNull.Value ? 0 : pNLrow[column]);
						}
					}
				}

				DataRow row = null;
				if (results.Rows.Count > 0)
				{
					row = results.Rows[0];
				}
				ReportsData reportsDataPL2E = AddNew();
				reportsDataPL2E.ReportNumber = reportNumber;
				reportsDataPL2E.ReportItemNumber = "31";
				reportsDataPL2E.ReportItemName = ReportItems.Descriptions.PL2 + ReportsDataCNStrings.LastYearActual;
				reportsDataPL2E.ReportItemValue = reportsDataPL1E.ReportItemValue;

				ReportsData reportsDataPL2F = AddNew();
				reportsDataPL2F.ReportNumber = reportNumber;
				reportsDataPL2F.ReportItemNumber = "31";
				reportsDataPL2F.ReportItemName = ReportItems.Descriptions.PL2 + ReportsDataCNStrings.CurrYearActual;
				reportsDataPL2F.ReportItemValue = reportsDataPL1F.ReportItemValue;

				for (int i = 2; i <= 3; i++)
				{
					SetPNLAppropriationReportItemValue(reportsDataPL2E, reportsDataPL2F, row, reportCategories, reportNumber, "A0" + i, i);
				}

				ReportsData reportsDataPL3E = AddNew();
				reportsDataPL3E.ReportNumber = reportNumber;
				reportsDataPL3E.ReportItemNumber = "91";
				reportsDataPL3E.ReportItemName = ReportItems.Descriptions.PL3 + ReportsDataCNStrings.LastYearActual;
				reportsDataPL3E.ReportItemValue += reportsDataPL2E.ReportItemValue;

				ReportsData reportsDataPL3F = AddNew();
				reportsDataPL3F.ReportNumber = reportNumber;
				reportsDataPL3F.ReportItemNumber = "91";
				reportsDataPL3F.ReportItemName = ReportItems.Descriptions.PL3 + ReportsDataCNStrings.CurrYearActual;
				reportsDataPL3F.ReportItemValue += reportsDataPL2F.ReportItemValue;

				for (int i = 4; i <= 9; i++)
				{
					SetPNLAppropriationReportItemValue(reportsDataPL3E, reportsDataPL3F, row, reportCategories, reportNumber, "A0" + i, i);
				}

				ReportsData reportsDataPL4E = AddNew();
				reportsDataPL4E.ReportNumber = reportNumber;
				reportsDataPL4E.ReportItemNumber = "131";
				reportsDataPL4E.ReportItemName = ReportItems.Descriptions.PL4 + ReportsDataCNStrings.LastYearActual;
				reportsDataPL4E.ReportItemValue += reportsDataPL3E.ReportItemValue;

				ReportsData reportsDataPL4F = AddNew();
				reportsDataPL4F.ReportNumber = reportNumber;
				reportsDataPL4F.ReportItemNumber = "131";
				reportsDataPL4F.ReportItemName = ReportItems.Descriptions.PL4 + ReportsDataCNStrings.CurrYearActual;
				reportsDataPL4F.ReportItemValue += reportsDataPL3F.ReportItemValue;
				for (int i = 10; i <= 13; i++)
				{
					SetPNLAppropriationReportItemValue(reportsDataPL4E, reportsDataPL4F, row, reportCategories, reportNumber, "A" + i, i);
				}
			}
		}

		void SetPNLAppropriationReportItemValue(ReportsData reportsDataE, ReportsData reportsDataF, DataRow row, ComplianceReportsSetupCategoryCollection reportCategories, ZString reportNumber, ZString columnName, int i)
		{
			ReportsData reportsData = AddNew();
			reportsData.ReportNumber = reportNumber;
			reportsData.ReportItemNumber = (i * 10).ToString();
			reportsData.ReportItemName = reportCategories.GetDescriptionFromCode(columnName) + ReportsDataCNStrings.LastYearActual;
			reportsData.ReportItemValue = row == null ? 0 : row[columnName + "_YED"] == DBNull.Value ? 0 : Convert.ToDecimal(row[columnName + "_YED"]);
			reportsDataE.ReportItemValue += reportsData.ReportItemValue;
			if (i > 3)
			{
				reportsData.ReportItemValue *= -1;
			}

			reportsData = AddNew();
			reportsData.ReportNumber = reportNumber;
			reportsData.ReportItemNumber = (i * 10).ToString();
			reportsData.ReportItemName = reportCategories.GetDescriptionFromCode(columnName) + ReportsDataCNStrings.CurrYearActual;
			reportsData.ReportItemValue = row == null ? 0 : row[columnName] == DBNull.Value ? 0 : Convert.ToDecimal(row[columnName]);
			reportsDataF.ReportItemValue += reportsData.ReportItemValue;
			if (i > 3)
			{
				reportsData.ReportItemValue *= -1;
			}
		}

		decimal AssetProvisionReportColumnItemValue(DataRow row, int baseColumn, string columnSuffix)
		{
			decimal columnItemValue = 0;
			string columnName = "P" + (baseColumn + 101).ToString().Substring(1, 2) + columnSuffix;
			columnItemValue = row[columnName] == DBNull.Value ? 0 : Convert.ToDecimal(row[columnName]);

			columnName = "P" + (baseColumn + 102).ToString().Substring(1, 2) + columnSuffix;
			columnItemValue += row[columnName] == DBNull.Value ? 0 : Convert.ToDecimal(row[columnName]);

			return columnItemValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Strings.")]
		static ZString GetChinaAssetsProvisionSQL(ZGuid companyPK, ZInt period, ZString branchCode)
		{
			return ZString.Format("EXEC ChinaAssetsProvision '{0}',{1},'{2}'",
									companyPK, period, branchCode);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1066:DoNotUseGotoCaseOrDefault", Justification = "Baseline")]

		void SetAssetProvisionReport(ZInt period)
		{
			ZString repotType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.AssetProvision;
			ComplianceReportsSetupCategoryCollection reportCategories = GetReportCategories(repotType);

			string sqlText = GetChinaAssetsProvisionSQL(GlbCompany.CurrentCompany.PK, period, BranchCode);

			DataTable results = RunScript(sqlText);

			if (results.Rows.Count > 0)
			{
				DataRow row = results.Rows[0];

				for (int i = 1; i <= 20; i++)
				{
					decimal lastColumnItemValue = 0;

					for (int j = 1; j <= 4; j++)
					{
						ReportsData reportsData = AddNew();
						reportsData.ReportNumber = "5";
						reportsData.ReportItemNumber = i.ToString();
						switch (i)
						{
							case 1:
								reportsData.ReportItemName = ReportItems.Descriptions.A01;
								goto case 16;
							case 4:
								reportsData.ReportItemName = ReportItems.Descriptions.A04;
								goto case 16;
							case 7:
								reportsData.ReportItemName = ReportItems.Descriptions.A07;
								goto case 16;
							case 10:
								reportsData.ReportItemName = ReportItems.Descriptions.A10;
								goto case 16;
							case 13:
								reportsData.ReportItemName = ReportItems.Descriptions.A13;
								goto case 16;
							case 16:
								if (i == 16)
								{
									reportsData.ReportItemName = ReportItems.Descriptions.A16;
								}

								switch (j)
								{
									case 1:
										reportsData.ReportItemName += ReportsDataCNStrings.YearBeginBalance;
										reportsData.ReportItemValue = AssetProvisionReportColumnItemValue(row, i, "");
										lastColumnItemValue = reportsData.ReportItemValue;
										break;
									case 2:
										reportsData.ReportItemName += ReportsDataCNStrings.CurrYearAmt;
										reportsData.ReportItemValue = AssetProvisionReportColumnItemValue(row, i, "_Credit");
										lastColumnItemValue += reportsData.ReportItemValue;
										break;
									case 3:
										reportsData.ReportItemName += ReportsDataCNStrings.CurrYearReturnAmt;
										reportsData.ReportItemValue = AssetProvisionReportColumnItemValue(row, i, "_Debit");
										lastColumnItemValue -= reportsData.ReportItemValue;
										break;
									case 4:
										reportsData.ReportItemName += ReportsDataCNStrings.YearEndBalance;
										reportsData.ReportItemValue = lastColumnItemValue;
										break;
								}
								break;

							default:

								string columnName = "P" + (i + 100).ToString().Substring(1, 2);

								reportsData.ReportItemName = reportCategories.GetDescriptionFromCode(columnName);
								switch (j)
								{
									case 1:
										reportsData.ReportItemName += ReportsDataCNStrings.YearBeginBalance;
										reportsData.ReportItemValue = row[columnName] == DBNull.Value ? 0 : Convert.ToDecimal(row[columnName]);
										lastColumnItemValue = reportsData.ReportItemValue;
										break;
									case 2:
										reportsData.ReportItemName += ReportsDataCNStrings.CurrYearAmt;
										reportsData.ReportItemValue = row[columnName + "_Credit"] == DBNull.Value ? 0 : Convert.ToDecimal(row[columnName + "_Credit"]);
										lastColumnItemValue += reportsData.ReportItemValue;
										break;
									case 3:
										reportsData.ReportItemName += ReportsDataCNStrings.CurrYearReturnAmt;
										reportsData.ReportItemValue = row[columnName + "_Debit"] == DBNull.Value ? 0 : Convert.ToDecimal(row[columnName + "_Debit"]);
										lastColumnItemValue -= reportsData.ReportItemValue;
										break;
									case 4:
										reportsData.ReportItemName += ReportsDataCNStrings.YearEndBalance;
										reportsData.ReportItemValue = lastColumnItemValue;
										break;
								}
								break;
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Strings.")]
		static ZString GetChinaVATDetailedReportSQL(ZGuid companyPK, ZString branchCode, ZInt period)
		{
			return ZString.Format("EXEC ChinaVATDetailedReport '{0}','{1}',{2}",
									companyPK, branchCode, period);
		}

		void SetVATDetailedReport(ZInt period)
		{
			ZString repotType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.VATDetailed;
			ComplianceReportsSetupCategoryCollection reportCategories = GetReportCategories(repotType);

			string sqlText = GetChinaVATDetailedReportSQL(GlbCompany.CurrentCompany.PK, BranchCode, period);

			DataTable results = RunScript(sqlText);
			if (results.Rows.Count > 0)
			{
				DataRow row = results.Rows[0];

				ReportsData reportsDataD1 = AddNew();
				reportsDataD1.ReportNumber = "4";
				reportsDataD1.ReportItemNumber = "1";
				reportsDataD1.ReportItemName = ReportItems.Descriptions.V01 + ReportsDataCNStrings.CurrMonthAmt;
				reportsDataD1.ReportItemValue = 0;

				ReportsData reportsDataE1 = AddNew();
				reportsDataE1.ReportNumber = "4";
				reportsDataE1.ReportItemNumber = "101";
				reportsDataE1.ReportItemName = ReportItems.Descriptions.V01 + ReportsDataCNStrings.YearToCurrAmt;
				reportsDataE1.ReportItemValue = row["VTT"] == DBNull.Value ? 0 : 0 - Convert.ToDecimal(row["VTT"]);

				ReportsData reportsDataD15 = AddNew();
				reportsDataD15.ReportNumber = "4";
				reportsDataD15.ReportItemNumber = "15";
				reportsDataD15.ReportItemName = ReportItems.Descriptions.V15 + ReportsDataCNStrings.CurrMonthAmt;
				reportsDataD15.ReportItemValue = 0;

				ReportsData reportsDataE15 = AddNew();
				reportsDataE15.ReportNumber = "4";
				reportsDataE15.ReportItemNumber = "151";
				reportsDataE15.ReportItemName = ReportItems.Descriptions.V15 + ReportsDataCNStrings.YearToCurrAmt;
				reportsDataE15.ReportItemValue = reportsDataE1.ReportItemValue;

				ReportsData reportsDataD16 = AddNew();
				reportsDataD16.ReportNumber = "4";
				reportsDataD16.ReportItemNumber = "16";
				reportsDataD16.ReportItemName = ReportItems.Descriptions.V16 + ReportsDataCNStrings.CurrMonthAmt;
				reportsDataD16.ReportItemValue = 0;

				ReportsData reportsDataE16 = AddNew();
				reportsDataE16.ReportNumber = "4";
				reportsDataE16.ReportItemNumber = "161";
				reportsDataE16.ReportItemName = ReportItems.Descriptions.V16 + ReportsDataCNStrings.YearToCurrAmt;
				reportsDataE16.ReportItemValue = row["T18"] == DBNull.Value ? 0 : 0 - Convert.ToDecimal(row["T18"]);

				ReportsData reportsDataD17 = AddNew();
				reportsDataD17.ReportNumber = "4";
				reportsDataD17.ReportItemNumber = "17";
				reportsDataD17.ReportItemName = ReportItems.Descriptions.V17 + ReportsDataCNStrings.CurrMonthAmt;
				reportsDataD17.ReportItemValue = 0;

				ReportsData reportsDataE17 = AddNew();
				reportsDataE17.ReportNumber = "4";
				reportsDataE17.ReportItemNumber = "171";
				reportsDataE17.ReportItemName = ReportItems.Descriptions.V17 + ReportsDataCNStrings.YearToCurrAmt;
				reportsDataE17.ReportItemValue = 0;

				ReportsData reportsDataD18 = AddNew();
				reportsDataD18.ReportNumber = "4";
				reportsDataD18.ReportItemNumber = "18";
				reportsDataD18.ReportItemName = ReportItems.Descriptions.V18 + ReportsDataCNStrings.CurrMonthAmt;
				reportsDataD18.ReportItemValue = 0;

				ReportsData reportsDataE18 = AddNew();
				reportsDataE18.ReportNumber = "4";
				reportsDataE18.ReportItemNumber = "181";
				reportsDataE18.ReportItemName = ReportItems.Descriptions.V18 + ReportsDataCNStrings.YearToCurrAmt;
				reportsDataE18.ReportItemValue = 0;

				ReportsData reportsDataD20 = AddNew();
				reportsDataD20.ReportNumber = "4";
				reportsDataD20.ReportItemNumber = "20";
				reportsDataD20.ReportItemName = ReportItems.Descriptions.V20 + ReportsDataCNStrings.CurrMonthAmt;
				reportsDataD20.ReportItemValue = 0;

				ReportsData reportsDataE20 = AddNew();
				reportsDataE20.ReportNumber = "4";
				reportsDataE20.ReportItemNumber = "201";
				reportsDataE20.ReportItemName = ReportItems.Descriptions.V20 + ReportsDataCNStrings.YearToCurrAmt;
				reportsDataE20.ReportItemValue = reportsDataE16.ReportItemValue;

				foreach (DataColumn column in results.Columns)
				{
					if (reportCategories.ContainsCategory(column.ColumnName.Substring(0, 3)))
					{
						ZInt sequence = reportCategories[column.ColumnName.Substring(0, 3)].Sequence.ToZInt();
						if (sequence == 18)
						{
							decimal itemValue = Convert.ToDecimal(row[column] == DBNull.Value ? 0 : row[column]);
							if (column.ColumnName.Length == 3)
							{
								reportsDataD18.ReportItemValue += itemValue;
							}
							else
							{
								reportsDataE18.ReportItemValue += itemValue;
								reportsDataE20.ReportItemValue -= itemValue;
							}
						}
						else
						{
							ReportsData reportsData = AddNew();
							reportsData.ReportNumber = "4";
							reportsData.ReportItemNumber = sequence.ToString();
							reportsData.ReportItemValue = Convert.ToDecimal(row[column] == DBNull.Value ? 0 : row[column]);

							if (column.ColumnName.Length == 3)
							{
								reportsData.ReportItemName = reportCategories.GetDescriptionFromCode(column.ColumnName) + ReportsDataCNStrings.CurrMonthAmt;

								if (sequence < 14)
								{
									reportsDataD15.ReportItemValue -= reportsData.ReportItemValue;
									if (sequence < 7)
									{
										reportsData.ReportItemValue = 0 - reportsData.ReportItemValue;
									}
								}

								switch (sequence)
								{
									case 5:
										reportsDataD17.ReportItemValue -= reportsData.ReportItemValue;
										reportsDataD18.ReportItemValue -= reportsData.ReportItemValue;
										break;
									case 12:
										reportsDataD17.ReportItemValue += reportsData.ReportItemValue;
										reportsDataD18.ReportItemValue += reportsData.ReportItemValue;
										break;
								}
							}
							else
							{
								reportsData.ReportItemName = reportCategories.GetDescriptionFromCode(column.ColumnName.Substring(0, 3)) +
															 ReportsDataCNStrings.YearToCurrAmt;
								reportsData.ReportItemNumber = sequence.ToString() + "1";

								if (sequence < 14)
								{
									reportsDataE15.ReportItemValue -= reportsData.ReportItemValue;
									if (sequence < 7)
									{
										reportsData.ReportItemValue = 0 - reportsData.ReportItemValue;
									}
								}

								switch (sequence)
								{
									case 5:
										reportsDataE17.ReportItemValue -= reportsData.ReportItemValue;
										reportsDataE18.ReportItemValue -= reportsData.ReportItemValue;
										break;
									case 12:
										reportsDataE17.ReportItemValue += reportsData.ReportItemValue;
										reportsDataE18.ReportItemValue += reportsData.ReportItemValue;
										break;
								}
							}
						}
					}
				}
			}
		}

		void SetBalanceSheet(ZInt period)
		{
			ZString repotType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet;
			ComplianceReportsSetupCategoryCollection reportCategories = GetReportCategories(repotType);
			DataTable results = RunScript(BSHPNLSQL(period, repotType));
			if (results.Rows.Count > 0)
			{
				ReportsData reportsDataT101 = AddNew();
				reportsDataT101.ReportNumber = "1";
				reportsDataT101.ReportItemNumber = "101";
				reportsDataT101.ReportItemName = ReportItems.Descriptions.T101 + ReportsDataCNStrings.EndAmt;
				reportsDataT101.ReportItemValue = 0;

				ReportsData reportsDataT103 = AddNew();
				reportsDataT103.ReportNumber = "1";
				reportsDataT103.ReportItemNumber = "103";
				reportsDataT103.ReportItemName = ReportItems.Descriptions.T103 + ReportsDataCNStrings.EndAmt;
				reportsDataT103.ReportItemValue = 0;

				ReportsData reportsDataT105 = AddNew();
				reportsDataT105.ReportNumber = "1";
				reportsDataT105.ReportItemNumber = "105";
				reportsDataT105.ReportItemName = ReportItems.Descriptions.T105 + ReportsDataCNStrings.EndAmt;
				reportsDataT105.ReportItemValue = 0;

				ReportsData reportsDataT106 = AddNew();
				reportsDataT106.ReportNumber = "1";
				reportsDataT106.ReportItemNumber = "106";
				reportsDataT106.ReportItemName = ReportItems.Descriptions.T106 + ReportsDataCNStrings.EndAmt;
				reportsDataT106.ReportItemValue = 0;

				ReportsData reportsDataT16 = AddNew();
				reportsDataT16.ReportNumber = "1";
				reportsDataT16.ReportItemNumber = "16";
				reportsDataT16.ReportItemName = ReportItems.Descriptions.T16 + ReportsDataCNStrings.EndAmt;
				reportsDataT16.ReportItemValue = 0;

				ReportsData reportsDataT27 = AddNew();
				reportsDataT27.ReportNumber = "1";
				reportsDataT27.ReportItemNumber = "27";
				reportsDataT27.ReportItemName = ReportItems.Descriptions.T27 + ReportsDataCNStrings.EndAmt;
				reportsDataT27.ReportItemValue = 0;

				ReportsData reportsDataT29 = AddNew();
				reportsDataT29.ReportNumber = "1";
				reportsDataT29.ReportItemNumber = "29";
				reportsDataT29.ReportItemName = ReportItems.Descriptions.T29 + ReportsDataCNStrings.EndAmt;
				reportsDataT29.ReportItemValue = 0;

				ReportsData reportsDataT45 = AddNew();
				reportsDataT45.ReportNumber = "1";
				reportsDataT45.ReportItemNumber = "45";
				reportsDataT45.ReportItemName = ReportItems.Descriptions.T45 + ReportsDataCNStrings.EndAmt;
				reportsDataT45.ReportItemValue = 0;

				ReportsData reportsDataT53 = AddNew();
				reportsDataT53.ReportNumber = "1";
				reportsDataT53.ReportItemNumber = "53";
				reportsDataT53.ReportItemName = ReportItems.Descriptions.T53 + ReportsDataCNStrings.EndAmt;
				reportsDataT53.ReportItemValue = 0;

				ReportsData reportsDataT71 = AddNew();
				reportsDataT71.ReportNumber = "1";
				reportsDataT71.ReportItemNumber = "71";
				reportsDataT71.ReportItemName = ReportItems.Descriptions.T71 + ReportsDataCNStrings.EndAmt;
				reportsDataT71.ReportItemValue = 0;

				ReportsData reportsDataT82 = AddNew();
				reportsDataT82.ReportNumber = "1";
				reportsDataT82.ReportItemNumber = "82";
				reportsDataT82.ReportItemName = ReportItems.Descriptions.T82 + ReportsDataCNStrings.EndAmt;
				reportsDataT82.ReportItemValue = 0;

				ReportsData reportsDataT83 = AddNew();
				reportsDataT83.ReportNumber = "1";
				reportsDataT83.ReportItemNumber = "83";
				reportsDataT83.ReportItemName = ReportItems.Descriptions.T83 + ReportsDataCNStrings.EndAmt;
				reportsDataT83.ReportItemValue = 0;

				ReportsData reportsDataT85 = AddNew();
				reportsDataT85.ReportNumber = "1";
				reportsDataT85.ReportItemNumber = "85";
				reportsDataT85.ReportItemName = ReportItems.Descriptions.T85 + ReportsDataCNStrings.EndAmt;
				reportsDataT85.ReportItemValue = 0;

				ReportsData reportsDataT101Y = AddNew();
				reportsDataT101Y.ReportNumber = "1";
				reportsDataT101Y.ReportItemNumber = "301";
				reportsDataT101Y.ReportItemName = ReportItems.Descriptions.T101 + ReportsDataCNStrings.BeginAmt;
				reportsDataT101Y.ReportItemValue = 0;

				ReportsData reportsDataT103Y = AddNew();
				reportsDataT103Y.ReportNumber = "1";
				reportsDataT103Y.ReportItemNumber = "303";
				reportsDataT103Y.ReportItemName = ReportItems.Descriptions.T103 + ReportsDataCNStrings.BeginAmt;
				reportsDataT103Y.ReportItemValue = 0;

				ReportsData reportsDataT105Y = AddNew();
				reportsDataT105Y.ReportNumber = "1";
				reportsDataT105Y.ReportItemNumber = "305";
				reportsDataT105Y.ReportItemName = ReportItems.Descriptions.T105 + ReportsDataCNStrings.BeginAmt;
				reportsDataT105Y.ReportItemValue = 0;

				ReportsData reportsDataT106Y = AddNew();
				reportsDataT106Y.ReportNumber = "1";
				reportsDataT106Y.ReportItemNumber = "306";
				reportsDataT106Y.ReportItemName = ReportItems.Descriptions.T106 + ReportsDataCNStrings.BeginAmt;
				reportsDataT106Y.ReportItemValue = 0;

				ReportsData reportsDataT16Y = AddNew();
				reportsDataT16Y.ReportNumber = "1";
				reportsDataT16Y.ReportItemNumber = "216";
				reportsDataT16Y.ReportItemName = ReportItems.Descriptions.T16 + ReportsDataCNStrings.BeginAmt;
				reportsDataT16Y.ReportItemValue = 0;

				ReportsData reportsDataT27Y = AddNew();
				reportsDataT27Y.ReportNumber = "1";
				reportsDataT27Y.ReportItemNumber = "227";
				reportsDataT27Y.ReportItemName = ReportItems.Descriptions.T27 + ReportsDataCNStrings.BeginAmt;
				reportsDataT27Y.ReportItemValue = 0;

				ReportsData reportsDataT29Y = AddNew();
				reportsDataT29Y.ReportNumber = "1";
				reportsDataT29Y.ReportItemNumber = "229";
				reportsDataT29Y.ReportItemName = ReportItems.Descriptions.T29 + ReportsDataCNStrings.BeginAmt;
				reportsDataT29Y.ReportItemValue = 0;

				ReportsData reportsDataT45Y = AddNew();
				reportsDataT45Y.ReportNumber = "1";
				reportsDataT45Y.ReportItemNumber = "245";
				reportsDataT45Y.ReportItemName = ReportItems.Descriptions.T45 + ReportsDataCNStrings.BeginAmt;
				reportsDataT45Y.ReportItemValue = 0;

				ReportsData reportsDataT53Y = AddNew();
				reportsDataT53Y.ReportNumber = "1";
				reportsDataT53Y.ReportItemNumber = "253";
				reportsDataT53Y.ReportItemName = ReportItems.Descriptions.T53 + ReportsDataCNStrings.BeginAmt;
				reportsDataT53Y.ReportItemValue = 0;

				ReportsData reportsDataT71Y = AddNew();
				reportsDataT71Y.ReportNumber = "1";
				reportsDataT71Y.ReportItemNumber = "271";
				reportsDataT71Y.ReportItemName = ReportItems.Descriptions.T71 + ReportsDataCNStrings.BeginAmt;
				reportsDataT71Y.ReportItemValue = 0;

				ReportsData reportsDataT82Y = AddNew();
				reportsDataT82Y.ReportNumber = "1";
				reportsDataT82Y.ReportItemNumber = "282";
				reportsDataT82Y.ReportItemName = ReportItems.Descriptions.T82 + ReportsDataCNStrings.BeginAmt;
				reportsDataT82Y.ReportItemValue = 0;

				ReportsData reportsDataT83Y = AddNew();
				reportsDataT83Y.ReportNumber = "1";
				reportsDataT83Y.ReportItemNumber = "283";
				reportsDataT83Y.ReportItemName = ReportItems.Descriptions.T83 + ReportsDataCNStrings.BeginAmt;
				reportsDataT83Y.ReportItemValue = 0;

				ReportsData reportsDataT85Y = AddNew();
				reportsDataT85Y.ReportNumber = "1";
				reportsDataT85Y.ReportItemNumber = "285";
				reportsDataT85Y.ReportItemName = ReportItems.Descriptions.T85 + ReportsDataCNStrings.BeginAmt;
				reportsDataT85Y.ReportItemValue = 0;

				DataRow row = results.Rows[0];

				foreach (DataColumn column in results.Columns)
				{
					if (reportCategories.ContainsCategory(column.ColumnName.Substring(0, 3)))
					{
						ReportsData reportsData = AddNew();
						ZInt sequence = reportCategories[column.ColumnName.Substring(0, 3)].Sequence.ToZInt();
						reportsData.ReportNumber = "1";
						reportsData.ReportItemNumber = sequence.ToString();
						reportsData.ReportItemValue = Convert.ToDecimal(row[column] == DBNull.Value ? 0 : row[column]);
						if (column.ColumnName.Length == 3)
						{
							reportsData.ReportItemName = reportCategories.GetDescriptionFromCode(column.ColumnName) + ReportsDataCNStrings.EndAmt;
							switch (sequence)
							{
								case 25:
									reportsDataT27.ReportItemValue += reportsData.ReportItemValue;
									break;
								case 26:
									reportsDataT27.ReportItemValue -= reportsData.ReportItemValue;
									break;
								case 28:
									reportsDataT29.ReportItemValue -= reportsData.ReportItemValue;
									break;
								case 94:
									reportsDataT101.ReportItemValue -= reportsData.ReportItemValue;
									break;
								case 102:
									reportsDataT103.ReportItemValue = reportsData.ReportItemValue;
									break;
								case 104:
									reportsDataT105.ReportItemValue -= reportsData.ReportItemValue;
									break;
								default:
									if ((sequence > 1 && sequence < 12) || (sequence == 14 || sequence == 15))
									{
										reportsDataT16.ReportItemValue += reportsData.ReportItemValue;
									}

									if (sequence != 36 && (sequence > 17 && sequence < 25) || (sequence > 29 && sequence < 44))
									{
										reportsDataT45.ReportItemValue += reportsData.ReportItemValue;
									}

									if (sequence == 64 || (sequence > 54 && sequence < 62) || (sequence > 65 && sequence < 71))
									{
										reportsDataT71.ReportItemValue += reportsData.ReportItemValue;
									}

									if (sequence > 72 && sequence < 81)
									{
										reportsDataT82.ReportItemValue += reportsData.ReportItemValue;
									}

									if ((sequence > 85 && sequence < 89) || (sequence == 91 || sequence == 92))
									{
										reportsDataT85.ReportItemValue += reportsData.ReportItemValue;
									}

									if ((sequence > 94 && sequence < 99) || sequence == 93 || sequence == 100)
									{
										reportsDataT101.ReportItemValue += reportsData.ReportItemValue;
									}

									break;
							}
						}
						else
						{
							reportsData.ReportItemNumber = (sequence + 200M).ToString("0");
							reportsData.ReportItemName = reportCategories.GetDescriptionFromCode(column.ColumnName.Substring(0, 3)) + ReportsDataCNStrings.BeginAmt;
							switch (sequence)
							{
								case 25:
									reportsDataT27Y.ReportItemValue += reportsData.ReportItemValue;
									break;
								case 26:
									reportsDataT27Y.ReportItemValue -= reportsData.ReportItemValue;
									break;
								case 28:
									reportsDataT29Y.ReportItemValue -= reportsData.ReportItemValue;
									break;
								case 94:
									reportsDataT101Y.ReportItemValue -= reportsData.ReportItemValue;
									break;
								case 102:
									reportsDataT103Y.ReportItemValue = reportsData.ReportItemValue;
									break;
								case 104:
									reportsDataT105Y.ReportItemValue -= reportsData.ReportItemValue;
									break;
								default:
									if ((sequence > 1 && sequence < 12) || (sequence == 14 || sequence == 15))
									{
										reportsDataT16Y.ReportItemValue += reportsData.ReportItemValue;
									}

									if (sequence != 36 && (sequence > 17 && sequence < 25) || (sequence > 29 && sequence < 44))
									{
										reportsDataT45Y.ReportItemValue += reportsData.ReportItemValue;
									}

									if (sequence == 64 || (sequence > 54 && sequence < 62) || (sequence > 65 && sequence < 71))
									{
										reportsDataT71Y.ReportItemValue += reportsData.ReportItemValue;
									}

									if (sequence > 72 && sequence < 81)
									{
										reportsDataT82Y.ReportItemValue += reportsData.ReportItemValue;
									}

									if ((sequence > 85 && sequence < 89) || (sequence == 91 || sequence == 92))
									{
										reportsDataT85Y.ReportItemValue += reportsData.ReportItemValue;
									}

									if ((sequence > 94 && sequence < 99) || sequence == 93 || sequence == 100)
									{
										reportsDataT101Y.ReportItemValue += reportsData.ReportItemValue;
									}

									break;
							}
						}
					}
				}
				reportsDataT29.ReportItemValue += reportsDataT27.ReportItemValue;
				reportsDataT45.ReportItemValue += reportsDataT29.ReportItemValue;
				reportsDataT53.ReportItemValue = reportsDataT45.ReportItemValue + reportsDataT16.ReportItemValue;
				reportsDataT83.ReportItemValue = reportsDataT71.ReportItemValue + reportsDataT82.ReportItemValue;
				reportsDataT101.ReportItemValue += reportsDataT85.ReportItemValue;
				reportsDataT103.ReportItemValue += reportsDataT101.ReportItemValue;
				reportsDataT105.ReportItemValue += reportsDataT103.ReportItemValue;
				reportsDataT106.ReportItemValue = reportsDataT83.ReportItemValue + reportsDataT105.ReportItemValue;

				reportsDataT29Y.ReportItemValue += reportsDataT27Y.ReportItemValue;
				reportsDataT45Y.ReportItemValue += reportsDataT29Y.ReportItemValue;
				reportsDataT53Y.ReportItemValue = reportsDataT45Y.ReportItemValue + reportsDataT16Y.ReportItemValue;
				reportsDataT83Y.ReportItemValue = reportsDataT71Y.ReportItemValue + reportsDataT82Y.ReportItemValue;
				reportsDataT101Y.ReportItemValue += reportsDataT85Y.ReportItemValue;
				reportsDataT103Y.ReportItemValue += reportsDataT101Y.ReportItemValue;
				reportsDataT105Y.ReportItemValue += reportsDataT103Y.ReportItemValue;
				reportsDataT106Y.ReportItemValue = reportsDataT83Y.ReportItemValue + reportsDataT105Y.ReportItemValue;
			}
		}

		void SetProfitAndLoss(ZInt period)
		{
			ZString repotType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLoss;
			ComplianceReportsSetupCategoryCollection reportCategories = GetReportCategories(repotType);
			DataTable results = RunScript(BSHPNLSQL(period, repotType));

			ReportsData reportsDataP01Y = AddNew();
			reportsDataP01Y.ReportNumber = "2";
			reportsDataP01Y.ReportItemNumber = "101";
			reportsDataP01Y.ReportItemName = ReportItems.Descriptions.P01 + ReportsDataCNStrings.BeginAmt;
			reportsDataP01Y.ReportItemValue = 0;

			ReportsData reportsDataP04Y = AddNew();
			reportsDataP04Y.ReportNumber = "2";
			reportsDataP04Y.ReportItemNumber = "104";
			reportsDataP04Y.ReportItemName = ReportItems.Descriptions.P04 + ReportsDataCNStrings.BeginAmt;
			reportsDataP04Y.ReportItemValue = 0;

			ReportsData reportsDataP21Y = AddNew();
			reportsDataP21Y.ReportNumber = "2";
			reportsDataP21Y.ReportItemNumber = "121";
			reportsDataP21Y.ReportItemName = ReportItems.Descriptions.P21 + ReportsDataCNStrings.BeginAmt;
			reportsDataP21Y.ReportItemValue = 0;

			ReportsData reportsDataP31Y = AddNew();
			reportsDataP31Y.ReportNumber = "2";
			reportsDataP31Y.ReportItemNumber = "131";
			reportsDataP31Y.ReportItemName = ReportItems.Descriptions.P31 + ReportsDataCNStrings.BeginAmt;
			reportsDataP31Y.ReportItemValue = 0;

			ReportsData reportsDataP34Y = AddNew();
			reportsDataP34Y.ReportNumber = "2";
			reportsDataP34Y.ReportItemNumber = "134";
			reportsDataP34Y.ReportItemName = ReportItems.Descriptions.P34 + ReportsDataCNStrings.BeginAmt;
			reportsDataP34Y.ReportItemValue = 0;

			ReportsData reportsDataP36Y = AddNew();
			reportsDataP36Y.ReportNumber = "2";
			reportsDataP36Y.ReportItemNumber = "136";
			reportsDataP36Y.ReportItemName = ReportItems.Descriptions.P36 + ReportsDataCNStrings.BeginAmt;
			reportsDataP36Y.ReportItemValue = 0;

			ReportsData reportsDataP01 = AddNew();
			reportsDataP01.ReportNumber = "2";
			reportsDataP01.ReportItemNumber = "01";
			reportsDataP01.ReportItemName = ReportItems.Descriptions.P01 + ReportsDataCNStrings.EndAmt;
			reportsDataP01.ReportItemValue = 0;

			ReportsData reportsDataP04 = AddNew();
			reportsDataP04.ReportNumber = "2";
			reportsDataP04.ReportItemNumber = "04";
			reportsDataP04.ReportItemName = ReportItems.Descriptions.P04 + ReportsDataCNStrings.EndAmt;
			reportsDataP04.ReportItemValue = 0;

			ReportsData reportsDataP21 = AddNew();
			reportsDataP21.ReportNumber = "2";
			reportsDataP21.ReportItemNumber = "21";
			reportsDataP21.ReportItemName = ReportItems.Descriptions.P21 + ReportsDataCNStrings.EndAmt;
			reportsDataP21.ReportItemValue = 0;

			ReportsData reportsDataP31 = AddNew();
			reportsDataP31.ReportNumber = "2";
			reportsDataP31.ReportItemNumber = "31";
			reportsDataP31.ReportItemName = ReportItems.Descriptions.P31 + ReportsDataCNStrings.EndAmt;
			reportsDataP31.ReportItemValue = 0;

			ReportsData reportsDataP34 = AddNew();
			reportsDataP34.ReportNumber = "2";
			reportsDataP34.ReportItemNumber = "34";
			reportsDataP34.ReportItemName = ReportItems.Descriptions.P34 + ReportsDataCNStrings.EndAmt;
			reportsDataP34.ReportItemValue = 0;

			ReportsData reportsDataP36 = AddNew();
			reportsDataP36.ReportNumber = "2";
			reportsDataP36.ReportItemNumber = "36";
			reportsDataP36.ReportItemName = ReportItems.Descriptions.P36 + ReportsDataCNStrings.EndAmt;
			reportsDataP36.ReportItemValue = 0;

			if (results.Rows.Count > 0)
			{
				DataRow row = results.Rows[0];

				foreach (DataColumn column in results.Columns)
				{
					if (reportCategories.ContainsCategory(column.ColumnName.Substring(0, 3)))
					{
						ReportsData reportsData = AddNew();
						ZInt sequence = Convert.ToInt16(reportCategories[column.ColumnName.Substring(0, 3)].Category.Substring(1));
						reportsData.ReportNumber = "2";
						reportsData.ReportItemNumber = sequence.ToString();
						if ((sequence > 4 && sequence < 18) || (sequence > 26 && sequence < 33) || sequence == 35)
						{
							reportsData.ReportItemValue -= Convert.ToDecimal(row[column] == DBNull.Value ? 0 : row[column]);
						}
						else
						{
							reportsData.ReportItemValue = Convert.ToDecimal(row[column] == DBNull.Value ? 0 : row[column]);
						}

						if (column.ColumnName.Length == 3)
						{
							reportsData.ReportItemName = reportCategories.GetDescriptionFromCode(column.ColumnName) + ReportsDataCNStrings.EndAmt;
							if (sequence > 1 && sequence < 4)
							{
								reportsDataP01.ReportItemValue += reportsData.ReportItemValue;
							}

							if (sequence > 4 && sequence < 7)
							{
								reportsDataP04.ReportItemValue += reportsData.ReportItemValue;
							}

							if (sequence == 2 || sequence == 3 || sequence == 12 ||
								(sequence > 4 && sequence < 10) || (sequence > 15 && sequence < 20))
							{
								reportsDataP21.ReportItemValue += Convert.ToDecimal(row[column] == DBNull.Value ? 0 : row[column]);
								reportsDataP31.ReportItemValue += Convert.ToDecimal(row[column] == DBNull.Value ? 0 : row[column]);
							}

							switch (sequence)
							{
								case 22:
									reportsDataP31.ReportItemValue += reportsData.ReportItemValue;
									break;
								case 27:
									reportsDataP31.ReportItemValue -= reportsData.ReportItemValue;
									break;
								case 32:
									reportsDataP34.ReportItemValue -= reportsData.ReportItemValue;
									break;
								case 33:
									reportsDataP34.ReportItemValue += reportsData.ReportItemValue;
									break;
								case 35:
									reportsDataP36.ReportItemValue -= reportsData.ReportItemValue;
									break;
							}
						}
						else
						{
							reportsData.ReportItemName = reportCategories.GetDescriptionFromCode(column.ColumnName.Substring(0, 3)) + ReportsDataCNStrings.BeginAmt;
							reportsData.ReportItemNumber = (reportCategories[column.ColumnName.Substring(0, 3)].Sequence + 100m).ToString("0");

							if (sequence > 1 && sequence < 4)
							{
								reportsDataP01Y.ReportItemValue += reportsData.ReportItemValue;
							}

							if (sequence > 4 && sequence < 7)
							{
								reportsDataP04Y.ReportItemValue += reportsData.ReportItemValue;
							}

							if (sequence == 2 || sequence == 3 || sequence == 12 ||
								(sequence > 4 && sequence < 10) || (sequence > 15 && sequence < 20))
							{
								reportsDataP21Y.ReportItemValue += Convert.ToDecimal(row[column] == DBNull.Value ? 0 : row[column]);
								reportsDataP31Y.ReportItemValue += Convert.ToDecimal(row[column] == DBNull.Value ? 0 : row[column]);
							}

							switch (sequence)
							{
								case 22:
									reportsDataP31Y.ReportItemValue += reportsData.ReportItemValue;
									break;
								case 27:
									reportsDataP31Y.ReportItemValue -= reportsData.ReportItemValue;
									break;
								case 32:
									reportsDataP34Y.ReportItemValue -= reportsData.ReportItemValue;
									break;
								case 33:
									reportsDataP34Y.ReportItemValue += reportsData.ReportItemValue;
									break;
								case 35:
									reportsDataP36Y.ReportItemValue -= reportsData.ReportItemValue;
									break;
							}
						}
					}
				}
				reportsDataP34.ReportItemValue += reportsDataP31.ReportItemValue;
				reportsDataP36.ReportItemValue += reportsDataP34.ReportItemValue;

				reportsDataP34Y.ReportItemValue += reportsDataP31Y.ReportItemValue;
				reportsDataP36Y.ReportItemValue += reportsDataP34Y.ReportItemValue;
			}
		}

		void SetProfitAndLossMonthly(ZInt period)
		{
			ZString repotType = AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLossMonthly;
			ComplianceReportsSetupCategoryCollection reportCategories = GetReportCategories(repotType);
			DataTable results = RunScript(BSHPNLSQL(period, repotType));
			ReportsData reportsDataD10 = AddNew();
			reportsDataD10.ReportNumber = "201";
			reportsDataD10.ReportItemNumber = "10";
			reportsDataD10.ReportItemName = ReportItems.Descriptions.D10 + ReportsDataCNStrings.CurrMonthAmt;
			reportsDataD10.ReportItemValue = 0;

			ReportsData reportsDataD18 = AddNew();
			reportsDataD18.ReportNumber = "201";
			reportsDataD18.ReportItemNumber = "18";
			reportsDataD18.ReportItemName = ReportItems.Descriptions.D18 + ReportsDataCNStrings.CurrMonthAmt;
			reportsDataD18.ReportItemValue = 0;

			ReportsData reportsDataD27 = AddNew();
			reportsDataD27.ReportNumber = "201";
			reportsDataD27.ReportItemNumber = "27";
			reportsDataD27.ReportItemName = ReportItems.Descriptions.D27 + ReportsDataCNStrings.CurrMonthAmt;
			reportsDataD27.ReportItemValue = 0;

			ReportsData reportsDataD32 = AddNew();
			reportsDataD32.ReportNumber = "201";
			reportsDataD32.ReportItemNumber = "32";
			reportsDataD32.ReportItemName = ReportItems.Descriptions.D32 + ReportsDataCNStrings.CurrMonthAmt;
			reportsDataD32.ReportItemValue = 0;

			ReportsData reportsDataD10Y = AddNew();
			reportsDataD10Y.ReportNumber = "201";
			reportsDataD10Y.ReportItemNumber = "110";
			reportsDataD10Y.ReportItemName = ReportItems.Descriptions.D10 + ReportsDataCNStrings.YearToCurrAmt;
			reportsDataD10Y.ReportItemValue = 0;

			ReportsData reportsDataD18Y = AddNew();
			reportsDataD18Y.ReportNumber = "201";
			reportsDataD18Y.ReportItemNumber = "118";
			reportsDataD18Y.ReportItemName = ReportItems.Descriptions.D18 + ReportsDataCNStrings.YearToCurrAmt;
			reportsDataD18Y.ReportItemValue = 0;

			ReportsData reportsDataD27Y = AddNew();
			reportsDataD27Y.ReportNumber = "201";
			reportsDataD27Y.ReportItemNumber = "127";
			reportsDataD27Y.ReportItemName = ReportItems.Descriptions.D27 + ReportsDataCNStrings.YearToCurrAmt;
			reportsDataD27Y.ReportItemValue = 0;

			ReportsData reportsDataD32Y = AddNew();
			reportsDataD32Y.ReportNumber = "201";
			reportsDataD32Y.ReportItemNumber = "132";
			reportsDataD32Y.ReportItemName = ReportItems.Descriptions.D32 + ReportsDataCNStrings.YearToCurrAmt;
			reportsDataD32Y.ReportItemValue = 0;

			if (results.Rows.Count > 0)
			{
				DataRow row = results.Rows[0];

				foreach (DataColumn column in results.Columns)
				{
					if (reportCategories.ContainsCategory(column.ColumnName.Substring(0, 3)))
					{
						ZInt sequence = reportCategories[column.ColumnName.Substring(0, 3)].Sequence.ToZInt();

						ReportsData reportsData = AddNew();
						reportsData.ReportNumber = "201";
						reportsData.ReportItemNumber = sequence.ToString();
						reportsData.ReportItemValue = Convert.ToDecimal(row[column] == DBNull.Value ? 0 : row[column]);
						if (column.ColumnName.Length == 3)
						{
							reportsData.ReportItemName = reportCategories.GetDescriptionFromCode(column.ColumnName) + ReportsDataCNStrings.CurrMonthAmt;
							switch (sequence)
							{
								case 1:
									reportsDataD10.ReportItemValue += reportsData.ReportItemValue;
									break;
								case 4:
								case 5:
									reportsData.ReportItemValue *= -1;
									reportsDataD10.ReportItemValue -= reportsData.ReportItemValue;
									break;
								case 11:
									reportsDataD18.ReportItemValue += reportsData.ReportItemValue;
									break;
								case 14:
								case 15:
								case 16:
									reportsData.ReportItemValue *= -1;
									reportsDataD18.ReportItemValue -= reportsData.ReportItemValue;
									break;
								case 19:
								case 22:
								case 23:
									reportsDataD27.ReportItemValue += reportsData.ReportItemValue;
									break;
								case 25:
									reportsData.ReportItemValue *= -1;
									reportsDataD27.ReportItemValue -= reportsData.ReportItemValue;
									break;
								case 28:
								case 29:
									reportsData.ReportItemValue *= -1;
									reportsDataD32.ReportItemValue -= reportsData.ReportItemValue;
									break;
								case 30:
									reportsDataD32.ReportItemValue += reportsData.ReportItemValue;
									break;
							}
						}
						else
						{
							reportsData.ReportItemName = reportCategories.GetDescriptionFromCode(column.ColumnName.Substring(0, 3)) + ReportsDataCNStrings.YearToCurrAmt;
							reportsData.ReportItemNumber = (sequence + 100m).ToString("0");
							switch (sequence)
							{
								case 1:
									reportsDataD10Y.ReportItemValue += reportsData.ReportItemValue;
									break;
								case 4:
								case 5:
									reportsData.ReportItemValue *= -1;
									reportsDataD10Y.ReportItemValue -= reportsData.ReportItemValue;
									break;
								case 11:
									reportsDataD18Y.ReportItemValue += reportsData.ReportItemValue;
									break;
								case 14:
								case 15:
								case 16:
									reportsData.ReportItemValue *= -1;
									reportsDataD18Y.ReportItemValue -= reportsData.ReportItemValue;
									break;
								case 19:
								case 22:
								case 23:
									reportsDataD27Y.ReportItemValue += reportsData.ReportItemValue;
									break;
								case 25:
									reportsData.ReportItemValue *= -1;
									reportsDataD27Y.ReportItemValue -= reportsData.ReportItemValue;
									break;
								case 28:
								case 29:
									reportsData.ReportItemValue *= -1;
									reportsDataD32Y.ReportItemValue -= reportsData.ReportItemValue;
									break;
								case 30:
									reportsDataD32Y.ReportItemValue += reportsData.ReportItemValue;
									break;
							}
						}
					}
				}
				reportsDataD18.ReportItemValue += reportsDataD10.ReportItemValue;
				reportsDataD27.ReportItemValue += reportsDataD18.ReportItemValue;
				reportsDataD32.ReportItemValue += reportsDataD27.ReportItemValue;

				reportsDataD18Y.ReportItemValue += reportsDataD10Y.ReportItemValue;
				reportsDataD27Y.ReportItemValue += reportsDataD18Y.ReportItemValue;
				reportsDataD32Y.ReportItemValue += reportsDataD27Y.ReportItemValue;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL Strings.")]
		ZString BSHPNLSQL(ZInt period, string repotType)
		{
			return ZString.Format(
				@"EXEC BalanceSheetOrProfitAndLossWithReportCategories '{0}',{1},'{2}','','{3}','{4}','ZH-CN','CN'",
				repotType == "P&L" ? "PNL" : repotType,
				period,
				GlbCompany.CurrentCompany.PK,
				BranchCode,
				ReportTypeCategories(repotType)
				);
		}

		DataTable RunScript(ZString sqlText)
		{
			DataTable table = DataUtils.GetDataTableFromQuery(Db.Connection, sqlText);

			table.PrimaryKey = new[] { table.Columns["Period"] };

			return table;
		}

		ComplianceReportsSetupCategoryCollection GetReportCategories(ZString reportType)
		{
			return AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsCN.Value.GetReportTypeCategoriesFromCode(reportType);
		}

		ZString ReportTypeCategories(ZString reportType)
		{
			ComplianceReportsSetupCategoryCollection reportCategories = GetReportCategories(reportType);
			ZString categories = reportCategories.CodesAsString.Replace(AccountingMasterFilesConstants.DefaultReportCategory.Undefined + ",", "").Replace(AccountingMasterFilesConstants.DefaultReportCategory.Undefined, "").Trim(',', ' ');

			ZStringBuilder builder = new ZStringBuilder();
			foreach (ComplianceReportsSetupCategory element in reportCategories.Cast<ComplianceReportsSetupCategory>().Where(element => element.Category != AccountingMasterFilesConstants.DefaultReportCategory.Undefined))
			{
				builder.Append(element.Category);
			}

			ZString result = ZString.Empty;
			if (!categories.IsEmpty)
			{
				result = categories + ", " + builder.ToStringWithDelimiterBetweenAppends("_YED, ") + "_YED";
			}
			return result;
		}
	}
}

