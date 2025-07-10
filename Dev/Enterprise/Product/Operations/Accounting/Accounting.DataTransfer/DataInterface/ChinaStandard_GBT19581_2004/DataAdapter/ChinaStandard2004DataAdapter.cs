using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT19581_2004
{
	public class ChinaStandard2004DataAdapter : BaseAccountingDataAdapter<BizObjThatDoesntSaveForCN2004, XSDs.会计核算软件数据>
	{
		#region Data Adapter Overrides

		public override string RootCollectionElementName
		{
			get { return "ChinaStandard2004"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Element name")]
		public override string RootElementName
		{
			get { return "会计核算软件数据"; }
		}

		public override XmlSchema Schema
		{
			get { return new ZXmlSchema(); }
		}

		public override XmlSchema CollectionSchema
		{
			get { return new ZXmlSchema(); }
		}

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		protected override void ExportToValueObjectCore(BizObjThatDoesntSaveForCN2004 bizObj,
														XSDs.会计核算软件数据 constructedValueObject, IValueObjectExportContext context)
		{
			currentFactory = bizObj.Factory;

			List<ZString> exportTypes = bizObj.ExportFiles;
			if (exportTypes.Contains("AccountBook"))
			{
				SetAccountBookValue(constructedValueObject.电子账簿, bizObj);
			}

			if (exportTypes.Contains("ChartOfAccounts"))
			{
				SetGLChartAccountValue(constructedValueObject.会计科目);
			}

			if (exportTypes.Contains("SupplementaryAccounts"))
			{
				constructedValueObject.辅助核算项档案.AddNew();
				SetDepartmentInformationValue(constructedValueObject.辅助核算项档案[0].部门); //DepartmentInformation
				SetStaffInformationValue(constructedValueObject.辅助核算项档案[0].人员); //StaffInformation
				SetClientValue(constructedValueObject.辅助核算项档案[0].单位); //ClientInformation
			}

			if (exportTypes.Contains("Department"))
			{
				constructedValueObject.辅助核算项档案.AddNew();
				SetDepartmentInformationValue(constructedValueObject.辅助核算项档案[0].部门); //DepartmentInformation
			}

			if (exportTypes.Contains("Staff"))
			{
				constructedValueObject.辅助核算项档案.AddNew();
				SetStaffInformationValue(constructedValueObject.辅助核算项档案[0].人员); //StaffInformation
			}

			if (exportTypes.Contains("Client"))
			{
				constructedValueObject.辅助核算项档案.AddNew();
				SetClientValue(constructedValueObject.辅助核算项档案[0].单位);
			}

			if (exportTypes.Contains("AccountingVouchers"))
			{
				SetVoucherValue(constructedValueObject.记账凭证, bizObj);
			}

			if (exportTypes.Contains("TrialBalance"))
			{
				SetGLTrialBalanceValue(constructedValueObject.科目余额及发生额, bizObj);
			}

			if (exportTypes.Contains("BalanceSheet"))
			{
				SetBalanceSheetReport(constructedValueObject.企业资产负债表, bizObj);
			}

			if (exportTypes.Contains("ProfitAndLoss"))
			{
				SetProfitAndLossReport(constructedValueObject.企业利润表, bizObj);
			}

			if (exportTypes.Contains("VATDetailed"))
			{
				SetVATDetailedReport(constructedValueObject.企业应交增值税明细表, bizObj);
			}

			if (exportTypes.Contains("AssetProvision"))
			{
				SetAssetProvisionReport(constructedValueObject.企业资产减值准备明细表, bizObj);
			}

			if (exportTypes.Contains("PNLAppropriation"))
			{
				SetPNLAppropriationReport(constructedValueObject.企业利润分配表, bizObj);
			}

			if (exportTypes.Contains("EquityMovement"))
			{
				SetEquityMovementReport(constructedValueObject.企业股东权益增减变动表, bizObj);
			}

			if (exportTypes.Contains("CashFlowStatement"))
			{
				setCashFlowStatement(constructedValueObject.企业现金流量表, bizObj);
			}
		}

		#region Cash Flow Statement

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "China standard special word.")]
		void setCashFlowStatement(XSDs.企业现金流量表Collection reports, BusinessObjectThatDoesntSaveForCN bizObj)
		{
			var reportsDataCollection = new ReportsDataCollection(currentFactory, bizObj.Period, AccountingMasterFilesConstants.ReportCodeOfLocalReport.CashFlowStatement, bizObj.BranchPK);

			ReportsData[] reportsDataArry = reportsDataCollection.ToArray<ReportsData>();

			foreach (var reportsData in reportsDataArry.OrderBy(x => ConvertToZInt(x.ReportItemNumber)))
			{
				if (reportsData.ReportNumber != "8")
				{
					continue;
				}

				if (reportsData.ReportItemName.EndsWith("本月数"))
				{
					var report = reports.AddNew();
					report.报告期.Value = GetLastDayFromPeriod(bizObj.Period);
					report.报表编号.Value = "会企03";
					report.编制单位.Value = companyName;
					report.货币单位.Value = currUnit;
					report.项目.Value = reportsData.ReportItemName.Replace("本月数", ZString.Empty);
					report.金额.IsSpecified = true;

					if (reportsData.ReportItemValue == 0m)
					{
						report.行次.Value = reportsData.ReportItemNumber;
						report.金额.Value = 0;
					}

					if (reportsData.ReportItemValue == -1m)
					{
						report.行次.Value = ZString.Empty;
					}
				}
			}

			foreach (ReportsData reportsData in reportsDataCollection)
			{
				if (reportsData.ReportNumber != "8")
				{
					continue;
				}

				var data = reportsData;

				if (reportsData.ReportItemName.EndsWith("本年累计数"))
				{
					foreach (var report in reports.Cast<XSDs.企业现金流量表>().Where(report => report.项目.Value == data.ReportItemName.Replace("本年累计数", ZString.Empty)))
					{
						report.金额.Value = reportsData.ReportItemValue == -0m ? 0 : (double)reportsData.ReportItemValue;
					}
				}
			}
		}

		#endregion

		#region Statement of Shareholder Equity

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "China standard special word.")]
		void SetEquityMovementReport(XSDs.企业股东权益增减变动表Collection reports, BusinessObjectThatDoesntSaveForCN bizObj)
		{
			var reportsDataCollection = new ReportsDataCollection(currentFactory, bizObj.Period, AccountingMasterFilesConstants.ReportCodeOfLocalReport.EquityMovement, bizObj.BranchCode);

			ReportsData[] reportsDataArry = reportsDataCollection.ToArray<ReportsData>();

			foreach (var reportsData in reportsDataArry.OrderBy(x => ConvertToZInt(x.ReportItemNumber)))
			{
				if (reportsData.ReportNumber != "7")
				{
					continue;
				}

				if (reportsData.ReportItemName.EndsWith("上年实际"))
				{
					var report = reports.AddNew();
					report.报告期.Value = GetLastDayFromPeriod(bizObj.Period);
					report.报表编号.Value = "会企01表附表2";
					report.编制单位.Value = companyName;
					report.货币单位.Value = currUnit;
					report.项目.Value = reportsData.ReportItemName.Replace("上年实际", ZString.Empty);
					report.行次.Value = reportsData.ReportItemNumber;
					report.上年数.IsSpecified = true;
					report.本年数.IsSpecified = true;
					report.本年数.Value = 0;
					report.上年数.Value = reportsData.ReportItemValue == -0m ? 0 : (double)reportsData.ReportItemValue;
				}
			}

			foreach (ReportsData reportsData in reportsDataCollection)
			{
				if (reportsData.ReportNumber != "7")
				{
					continue;
				}

				var data = reportsData;

				if (reportsData.ReportItemName.EndsWith("本年实际"))
				{
					foreach (var report in reports.Cast<XSDs.企业股东权益增减变动表>().Where(report => report.项目.Value == data.ReportItemName.Replace("本年实际", ZString.Empty)))
					{
						report.本年数.Value = reportsData.ReportItemValue == -0m ? 0 : (double)reportsData.ReportItemValue;
					}
				}
			}
		}

		#endregion

		#region P&L Appropriation Report

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "China standard special word.")]
		void SetPNLAppropriationReport(XSDs.企业利润分配表Collection reports, BusinessObjectThatDoesntSaveForCN bizObj)
		{
			var reportsDataCollection = new ReportsDataCollection(currentFactory, bizObj.Period, AccountingMasterFilesConstants.ReportCodeOfLocalReport.P_LAppropriation, bizObj.BranchCode);

			ReportsData[] reportsDataArry = reportsDataCollection.ToArray<ReportsData>();

			foreach (var reportsData in reportsDataArry.OrderBy(x => ConvertToZInt(x.ReportItemNumber)))
			{
				if (reportsData.ReportNumber != "6")
				{
					continue;
				}

				if (reportsData.ReportItemName.EndsWith("上年实际"))
				{
					var report = reports.AddNew();
					report.报告期.Value = GetLastDayFromPeriod(bizObj.Period);
					report.报表编号.Value = "会企02表附表2";
					report.编制单位.Value = companyName;
					report.货币单位.Value = currUnit;
					report.项目.Value = reportsData.ReportItemName.Replace("上年实际", ZString.Empty);
					ZInt itemNumber = ConvertToZInt(reportsData.ReportItemNumber);
					report.行次.Value = itemNumber % 10 > 0 ? "" : (itemNumber / 10).ToString();
					report.上年实际.IsSpecified = true;
					report.本年实际.IsSpecified = true;
					report.本年实际.Value = 0;
					report.上年实际.Value = reportsData.ReportItemValue == -0m ? 0 : (double)reportsData.ReportItemValue;
				}
			}

			foreach (ReportsData reportsData in reportsDataCollection)
			{
				if (reportsData.ReportNumber != "6")
				{
					continue;
				}

				var data = reportsData;

				if (reportsData.ReportItemName.EndsWith("本年实际"))
				{
					foreach (var report in reports.Cast<XSDs.企业利润分配表>().Where(report => report.项目.Value == data.ReportItemName.Replace("本年实际", ZString.Empty)))
					{
						report.本年实际.Value = reportsData.ReportItemValue == -0m ? 0 : (double)reportsData.ReportItemValue;
					}
				}
			}
		}

		#endregion

		#region Asset Provision Report

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "China standard special word.")]
		void SetAssetProvisionReport(XSDs.企业资产减值准备明细表Collection reports, BusinessObjectThatDoesntSaveForCN bizObj)
		{
			var reportsDataCollection = new ReportsDataCollection(currentFactory, bizObj.Period, AccountingMasterFilesConstants.ReportCodeOfLocalReport.AssetProvision, bizObj.BranchCode);

			ReportsData[] reportsDataArry = reportsDataCollection.ToArray<ReportsData>();

			foreach (var reportsData in reportsDataArry.OrderBy(x => ConvertToZInt(x.ReportItemNumber)))
			{
				if (reportsData.ReportNumber != "5")
				{
					continue;
				}

				if (reportsData.ReportItemName.EndsWith("本年增加数"))
				{
					var report = reports.AddNew();
					report.报告期.Value = GetLastDayFromPeriod(bizObj.Period);
					report.报表编号.Value = "会企01表附表1";
					report.编制单位.Value = companyName;
					report.货币单位.Value = currUnit;
					report.项目.Value = reportsData.ReportItemName.Replace("本年增加数", ZString.Empty);
					report.年初余额.IsSpecified = true;
					report.本年增加数.IsSpecified = true;
					report.本年转回数.IsSpecified = true;
					report.年末金额.IsSpecified = true;
					report.年初余额.Value = 0;
					report.本年转回数.Value = 0;
					report.年末金额.Value = 0;
					report.本年增加数.Value = reportsData.ReportItemValue == -0m ? 0 : (double)reportsData.ReportItemValue;
				}
			}

			foreach (ReportsData reportsData in reportsDataCollection)
			{
				if (reportsData.ReportNumber != "5")
				{
					continue;
				}

				var data = reportsData;

				if (reportsData.ReportItemName.EndsWith("年初余额"))
				{
					foreach (var report in reports.Cast<XSDs.企业资产减值准备明细表>().Where(report => report.项目.Value == data.ReportItemName.Replace("年初余额", ZString.Empty)))
					{
						report.年初余额.Value = reportsData.ReportItemValue == -0m ? 0 : (double)reportsData.ReportItemValue;
					}
				}

				if (reportsData.ReportItemName.EndsWith("本年转回数"))
				{
					foreach (var report in reports.Cast<XSDs.企业资产减值准备明细表>().Where(report => report.项目.Value == data.ReportItemName.Replace("本年转回数", ZString.Empty)))
					{
						report.本年转回数.Value = reportsData.ReportItemValue == -0m ? 0 : (double)reportsData.ReportItemValue;
					}
				}

				if (reportsData.ReportItemName.EndsWith("年末金额"))
				{
					foreach (var report in reports.Cast<XSDs.企业资产减值准备明细表>().Where(report => report.项目.Value == data.ReportItemName.Replace("年末金额", ZString.Empty)))
					{
						report.年末金额.Value = reportsData.ReportItemValue == -0m ? 0 : (double)reportsData.ReportItemValue;
					}
				}
			}
		}

		#endregion

		#region VAT Detailed Report

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "China standard special word.")]
		void SetVATDetailedReport(XSDs.企业应交增值税明细表Collection reports, BusinessObjectThatDoesntSaveForCN bizObj)
		{
			var reportsDataCollection = new ReportsDataCollection(currentFactory, bizObj.Period, AccountingMasterFilesConstants.ReportCodeOfLocalReport.VATDetailed, bizObj.BranchCode);

			ReportsData[] reportsDataArry = reportsDataCollection.ToArray<ReportsData>();

			foreach (var reportsData in reportsDataArry.OrderBy(x => ConvertToZInt(x.ReportItemNumber)))
			{
				if (reportsData.ReportNumber != "4")
				{
					continue;
				}

				if (reportsData.ReportItemName.EndsWith("本月数"))
				{
					var report = reports.AddNew();
					report.报告期.Value = GetLastDayFromPeriod(bizObj.Period);
					report.报表编号.Value = "会企01表附表3";
					report.编制单位.Value = companyName;
					report.货币单位.Value = currUnit;
					report.项目.Value = reportsData.ReportItemName.Replace("本月数", ZString.Empty);
					report.行次.Value = reportsData.ReportItemNumber;
					report.本月数.IsSpecified = true;
					report.本年累计数.IsSpecified = true;
					report.本年累计数.Value = 0;
					report.本月数.Value = reportsData.ReportItemValue == -0m ? 0 : (double)reportsData.ReportItemValue;
				}
			}

			foreach (ReportsData reportsData in reportsDataCollection)
			{
				if (reportsData.ReportNumber != "4" || !reportsData.ReportItemName.EndsWith("本年累计数"))
				{
					continue;
				}

				var data = reportsData;
				foreach (var report in reports.Cast<XSDs.企业应交增值税明细表>().Where(report => report.项目.Value == data.ReportItemName.Replace("本年累计数", ZString.Empty)))
				{
					report.本年累计数.Value = reportsData.ReportItemValue == -0m ? 0 : (double)reportsData.ReportItemValue;
				}
			}
		}

		#endregion

		#region Profit And Loss Report

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "China standard special word.")]
		void SetProfitAndLossReport(XSDs.企业利润表Collection reports, BusinessObjectThatDoesntSaveForCN bizObj)
		{
			var reportsDataCollection = new ReportsDataCollection(currentFactory, bizObj.Period, AccountingMasterFilesConstants.ReportCodeOfLocalReport.ProfitAndLossMonthly, bizObj.BranchCode);

			ReportsData[] reportsDataArry = reportsDataCollection.ToArray<ReportsData>();

			foreach (var reportsData in reportsDataArry.OrderBy(x => ConvertToZInt(x.ReportItemNumber)))
			{
				if (reportsData.ReportNumber != "201")
				{
					continue;
				}

				if (reportsData.ReportItemName.EndsWith("本月数"))
				{
					var report = reports.AddNew();
					report.报告期.Value = GetLastDayFromPeriod(bizObj.Period);
					report.报表编号.Value = "会企地月02表";
					report.编制单位.Value = companyName;
					report.货币单位.Value = currUnit;
					report.项目.Value = reportsData.ReportItemName.Replace("本月数", ZString.Empty);
					report.行次.Value = reportsData.ReportItemNumber;
					report.本月数.IsSpecified = true;
					report.本年累计数.IsSpecified = true;
					report.本年累计数.Value = 0;
					report.本月数.Value = reportsData.ReportItemValue == -0m ? 0 : (double)reportsData.ReportItemValue;
				}
			}

			foreach (ReportsData reportsData in reportsDataCollection)
			{
				if (reportsData.ReportNumber != "201" || !reportsData.ReportItemName.EndsWith("本年累计数"))
				{
					continue;
				}

				var data = reportsData;
				foreach (var report in reports.Cast<XSDs.企业利润表>().Where(report => report.项目.Value == data.ReportItemName.Replace("本年累计数", ZString.Empty)))
				{
					report.本年累计数.Value = reportsData.ReportItemValue == -0m ? 0 : (double)reportsData.ReportItemValue;
				}
			}
		}

		#endregion

		#region Balance Sheet Report

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "this is China standard., China standard special word.")]
		void SetBalanceSheetReport(XSDs.企业资产负债表Collection reports, BusinessObjectThatDoesntSaveForCN bizObj)
		{
			var reportsDataCollection = new ReportsDataCollection(currentFactory, bizObj.Period, AccountingMasterFilesConstants.ReportCodeOfLocalReport.BalanceSheet, bizObj.BranchCode);
			ReportsData reportsDataHeader = reportsDataCollection.AddNew();
			reportsDataHeader.ReportNumber = "1";
			reportsDataHeader.ReportItemNumber = "1";
			reportsDataHeader.ReportItemName = "流动资产:";
			reportsDataHeader.ReportItemValue = 0;

			reportsDataHeader = reportsDataCollection.AddNew();
			reportsDataHeader.ReportNumber = "1";
			reportsDataHeader.ReportItemNumber = "17";
			reportsDataHeader.ReportItemName = "非流动资产:";
			reportsDataHeader.ReportItemValue = 0;

			reportsDataHeader = reportsDataCollection.AddNew();
			reportsDataHeader.ReportNumber = "1";
			reportsDataHeader.ReportItemNumber = "54";
			reportsDataHeader.ReportItemName = "流动负债:";
			reportsDataHeader.ReportItemValue = 0;

			reportsDataHeader = reportsDataCollection.AddNew();
			reportsDataHeader.ReportNumber = "1";
			reportsDataHeader.ReportItemNumber = "72";
			reportsDataHeader.ReportItemName = "非流动负债:";
			reportsDataHeader.ReportItemValue = 0;

			reportsDataHeader = reportsDataCollection.AddNew();
			reportsDataHeader.ReportNumber = "1";
			reportsDataHeader.ReportItemNumber = "84";
			reportsDataHeader.ReportItemName = "所有者权益（或股东权益）:";
			reportsDataHeader.ReportItemValue = 0;
			ReportsData[] reportsDataArry = reportsDataCollection.ToArray<ReportsData>();

			foreach (var reportsData in reportsDataArry.OrderBy(x => ConvertToZInt(x.ReportItemNumber)))
			{
				if (reportsData.ReportNumber != "1")
				{
					continue;
				}

				if (reportsData.ReportItemNumber == "1" || reportsData.ReportItemNumber == "17" ||
					reportsData.ReportItemNumber == "54" || reportsData.ReportItemNumber == "72" ||
					reportsData.ReportItemNumber == "84" || reportsData.ReportItemName.EndsWith("期末数"))
				{
					var report = reports.AddNew();
					report.报告日.Value = GetLastDayFromPeriod(bizObj.Period);
					report.报表编号.Value = "会企 01 表";
					report.编制单位.Value = companyName;
					report.货币单位.Value = currUnit;
					report.项目.Value = reportsData.ReportItemName.Replace("期末数", ZString.Empty);
					if (reportsData.ReportItemName.EndsWith("期末数"))
					{
						report.行次.Value = reportsData.ReportItemNumber;
						report.年初数.IsSpecified = true;
						report.期末数.IsSpecified = true;
						report.年初数.Value = 0;
						report.期末数.Value = (double)reportsData.ReportItemValue;
					}
				}
			}

			foreach (ReportsData reportsData in reportsDataCollection)
			{
				if (reportsData.ReportNumber != "1" || !reportsData.ReportItemName.EndsWith("年初数"))
				{
					continue;
				}

				var data = reportsData;
				foreach (var report in reports.Cast<XSDs.企业资产负债表>().Where(report => report.项目.Value == data.ReportItemName.Replace("年初数", ZString.Empty)))
				{
					report.年初数.Value = (double)reportsData.ReportItemValue;
				}
			}
		}

		#endregion

		#region Chart of Account

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID., China standard special word.")]
		void SetGLChartAccountValue(XSDs.会计科目Collection gLChartAccounts)
		{
			AccGLAccountDescriptor[] accGLAccountDescs = currentFactory.Load<AccGLAccountDescriptor>(Filter);

			foreach (AccGLAccountDescriptor accGLAccountDesc in accGLAccountDescs)
			{
				if (accGLAccountDesc.AJ_ReportCategory != "BSH" && accGLAccountDesc.AJ_ReportCategory != "P&L")
				{
					continue;
				}

				XSDs.会计科目 localGLaccount = gLChartAccounts.AddNew();
				localGLaccount.余额方向.IsSpecified = true;
				localGLaccount.余额方向.Value = accGLAccountDesc.AJ_DebitCredit == Constants.DebitCredit.Credit ? XSDs.余额方向类型.贷 : XSDs.余额方向类型.借;
				localGLaccount.科目名称.Value = accGLAccountDesc.AJ_AccountDescription;
				localGLaccount.科目类型.Value = accGLAccountDesc.AJ_ReportCategory;

				ZString gLAccountType = ChineseUtils.GetGLAccountTypeFromNumber(ZInt.Parse(accGLAccountDesc.AJ_LocalAccountNumber.Left(1)));
				if (gLAccountType != ZString.Empty)
				{
					localGLaccount.科目类型.Value = gLAccountType;
				}

				localGLaccount.科目级次.Value = (ZShort)GetGlAccountLevel(accGLAccountDesc.AJ_LocalAccountNumber);
				localGLaccount.科目编号.Value = accGLAccountDesc.AJ_LocalAccountNumber;
				localGLaccount.辅助核算标志.IsSpecified = true;
				localGLaccount.辅助核算标志.Value = 0;
				localGLaccount.辅助核算项.Value = "";
				if (AccountingConfigurationRegistry.Instance.ARControlAccount.Value == accGLAccountDesc.ParentGLHeader.PK || AccountingConfigurationRegistry.Instance.APControlAccount.Value == accGLAccountDesc.ParentGLHeader.PK)
				{
					localGLaccount.辅助核算标志.Value = 1;
					localGLaccount.辅助核算项.Value = "单位";
				}
				localGLaccount.计量单位.Value = "";
			}
		}

		ZQuery Filter
		{
			get
			{
				var zQuery = new ZQuery(AccGLAccountDescriptorSchema.AJ_Language, SharedConstants.Languages.ChineseSimplified);
				zQuery.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, Constants.CountryCodes.China);
				zQuery.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, AccGLAccountDescriptor.ReportTypeCOA);
				zQuery.OrderBy = AccGLAccountDescriptorSchema.Constants.AJ_LocalAccountNumber;
				return zQuery;
			}
		}

		ZInt GetGlAccountLevel(ZString localAccountNumber)
		{
			ZString glLocalNumberFormat = AccountingMasterFilesRegistry.Instance.LocalNumberFormats.Value.GetGLLocalNumberFormat(SharedConstants.Languages.ChineseSimplified, Constants.CountryCodes.China).NumberFormat;

			ZString[] registryFormat = glLocalNumberFormat.Split(new[] { '-' });
			ZString localAccountNumber1 = localAccountNumber;

			while (localAccountNumber1.EndsWith("0"))
			{
				localAccountNumber1 = localAccountNumber1.RemoveSafe(localAccountNumber1.Length - 1, 1);
			}

			ZInt cnt = 0;
			ZInt len = 0;
			foreach (var variable in registryFormat)
			{
				len += ConvertToZInt(variable);
				cnt++;
				if (localAccountNumber.Length == len || localAccountNumber1.Length <= len)
				{
					break;
				}
			}
			return cnt;
		}

		ZInt ConvertToZInt(ZString str)
		{
			ZInt result;
			if (!ZInt.TryParse(str.Replace(".", ""), out result))
			{
				result = 0;
			}

			return result;
		}

		#endregion

		#region Account Book

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "China standard special word.")]
		void SetAccountBookValue(XSDs.电子账簿Collection eAccountBooks, BizObjThatDoesntSaveForCN2004 bizObj)
		{
			AccountingEBook accountingEBook = new AccountingEBook();
			XSDs.电子账簿 eAccountBook = eAccountBooks.AddNew();
			eAccountBook.电子账簿编号.Value = AccountingEBook.BookNumber;
			eAccountBook.电子账簿名称.Value = accountingEBook.BookName;
			eAccountBook.会计核算单位.Value = accountingEBook.CompanyName;
			eAccountBook.组织机构代码.Value = accountingEBook.CompanyRegistrationCode;
			eAccountBook.单位性质.IsSpecified = true;
			eAccountBook.单位性质.Value = XSDs.单位性质类型.企业单位;
			if (GlbCompany.CurrentCompany.LicenceEnterpriseCode == "EDI")
			{
				eAccountBook.行业.Value = "软件和信息技术服务业";
			}
			else
			{
				eAccountBook.行业.Value = AccountingEBook.Industry;
			}
			eAccountBook.开发单位.Value = AccountingEBook.DevelopmentCompany;
			eAccountBook.版本号.Value = AccountingEBook.Version;
			eAccountBook.本位币.Value = accountingEBook.BaseCurrency;
			eAccountBook.年度.Value = bizObj.FromDate.Year.ToString("D");
			eAccountBook.科目结构.Value = AccountingMasterFilesRegistry.Instance.LocalNumberFormats.Value.GetGLLocalNumberFormat(SharedConstants.Languages.ChineseSimplified, Constants.CountryCodes.China).NumberFormat.Replace("-", ",");
		}

		#endregion

		#region GL Account Assisted

		void SetDepartmentInformationValue(XSDs.部门Collection departmentInformations)
		{
			DepartmentInformationCollection collection = new DepartmentInformationCollection(currentFactory);
			foreach (DepartmentInformation departmentInformation in collection)
			{
				XSDs.部门 departmentInformationXSD = departmentInformations.AddNew();
				departmentInformationXSD.部门编号.Value = departmentInformation.DepartmentCode;
				departmentInformationXSD.部门名称.Value = departmentInformation.DepartmentName;
				departmentInformationXSD.上级部门编号.Value = departmentInformation.ParentDepartmentCode;
			}
		}

		void SetStaffInformationValue(XSDs.人员Collection staffInformations)
		{
			StaffInformationCollection collection = new StaffInformationCollection(currentFactory);
			foreach (StaffInformation staffInformation in collection)
			{
				XSDs.人员 staffInformationXSD = staffInformations.AddNew();
				staffInformationXSD.人员编号.Value = staffInformation.StaffCode; //StaffCode
				staffInformationXSD.人员姓名.Value = staffInformation.StaffName; //StaffName
				staffInformationXSD.证件类别.Value = staffInformation.IDType; //IDType
				staffInformationXSD.证件号码.Value = staffInformation.IDNumber; //IDNumber
				staffInformationXSD.性别.Value = staffInformation.Gender; //Gender
				staffInformationXSD.出生日期.Value = staffInformation.BirthDate; //BirthDate
				staffInformationXSD.部门编号.Value = staffInformation.DepartmentCode; //DepartmentCode
				staffInformationXSD.入职日期.Value = staffInformation.EmploymentDate; //EmploymentDate
				staffInformationXSD.离职日期.Value = staffInformation.LeaveDate; //LeaveDate
			}
		}

		void SetClientValue(XSDs.单位Collection clientInformations)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
			subQuery.AddToFilter(JoinCondition.Or, OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
			subQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
			query.AddSubQuery(subQuery, JoinCondition.And);
			query.OrderBy = AutoOrgHeader.Schema.OH_Code;

			OrgHeaderCollection collection = new OrgHeaderCollection(currentFactory, query);
			collection.Load();
			foreach (OrgHeader client in collection)
			{
				XSDs.单位 supplierInformationXSD = clientInformations.AddNew();
				supplierInformationXSD.单位编号.Value = client.OH_Code;
				supplierInformationXSD.单位名称.Value = LocalCompanyName.GetLocalCompanyName(client, OrgConstants.AddressType.Receivables);
				supplierInformationXSD.类别编号.Value = "0101";// May be an identifier or GUID.
				var localAddress = GetLocalAddress(client);
				if (localAddress != null)
				{
					supplierInformationXSD.地区.Value = localAddress.OA_City;
					supplierInformationXSD.地址.Value = localAddress.OA_Address1 + " " + localAddress.OA_Address2;
					supplierInformationXSD.电话.Value = localAddress.OA_Phone;
				}
			}
		}

		OrgHeader fOrgHeader;
		OrgAddress fAddress;

		OrgAddress GetLocalAddress(OrgHeader orgHeader)
		{
			if (fOrgHeader != orgHeader)
			{
				fOrgHeader = orgHeader;
				OrgAddressList orgAddress = orgHeader.Addresses.AddressesOfType(OrgConstants.AddressType.Receivables);
				fAddress = null;
				foreach (OrgAddress address in orgAddress)
				{
					if (address.AddressCapability.GetIsMainAddress(OrgConstants.AddressType.Receivables))
					{
						fAddress = address;
						break;
					}
				}
			}
			return fAddress;
		}

		#endregion

		#region Trial Balance

		ZString GetAccountNumberCRDR(ZString gLAccountNumber)
		{
			ZQuery filter = Filter;
			filter.AddToFilter(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, gLAccountNumber + AccGLAccountRegistryStrFormat);
			AccGLAccountDescriptor[] accGLAccountDescs = currentFactory.Load<AccGLAccountDescriptor>(filter);
			return accGLAccountDescs.Length > 0 ? accGLAccountDescs[0].AJ_DebitCredit : ZString.Empty;
		}

		ZString AccGLAccountRegistryStrFormat
		{
			get
			{
				ZString[] registryFormat = AccountingMasterFilesRegistry.Instance.LocalNumberFormats.Value.GetGLLocalNumberFormat(SharedConstants.Languages.ChineseSimplified, Constants.CountryCodes.China).NumberFormat.Split(new[] { '-' });
				ZInt len = 0;
				len = registryFormat.Aggregate(len, (current, variable) => (ZInt)(current + ConvertToZInt(variable)));
				ZString strFormat = "";
				return strFormat.PadRight(len - ConvertToZInt(registryFormat[0]), Convert.ToChar("0"));
			}
		}

		void SetGLTrialBalanceValue(XSDs.科目余额及发生额Collection gLTrialBalances, BusinessObjectThatDoesntSaveForCN bizObj)
		{
			GLAccountBalancesAndMovementsCollection gLTrialBalancesCollection = new GLAccountBalancesAndMovementsCollection(currentFactory, bizObj.Period, bizObj.BranchPK);
			foreach (GLAccountBalancesAndMovements glAccountBalancesAndMovements in gLTrialBalancesCollection)
			{
				XSDs.科目余额及发生额 gLTrialBalance = gLTrialBalances.AddNew();

				ZString gLAccDRCR = ChineseUtils.ConvertDebitCreditToChinese(GetAccountNumberCRDR(glAccountBalancesAndMovements.GLAccountNumber));
				int debitCredit = 1;
				if (glAccountBalancesAndMovements.OpenBalanceDRCR != gLAccDRCR)
				{
					debitCredit = -1;
				}

				gLTrialBalance.科目编号.Value = glAccountBalancesAndMovements.GLAccountNumber;
				gLTrialBalance.币种.Value = GetCurrencyNameFromCode(glAccountBalancesAndMovements.CurrencyCode);

				gLTrialBalance.辅助核算组.Value = ZString.Empty;
				gLTrialBalance.会计月度.Value = glAccountBalancesAndMovements.Period.ToString().Substring(4);

				gLTrialBalance.期初数量.IsSpecified = true;
				gLTrialBalance.期初外币余额.IsSpecified = true;
				gLTrialBalance.期初余额.IsSpecified = true;
				gLTrialBalance.期初数量.Value = (double)glAccountBalancesAndMovements.OpenQuantity;
				gLTrialBalance.期初外币余额.Value = glAccountBalancesAndMovements.OpenBalanceCurrency == 0m ? 0 : (double)glAccountBalancesAndMovements.OpenBalanceCurrency * debitCredit;
				gLTrialBalance.期初余额.Value = glAccountBalancesAndMovements.OpenBalanceLocalCurrency == 0m ? 0 : (double)glAccountBalancesAndMovements.OpenBalanceLocalCurrency * debitCredit;

				gLTrialBalance.借方发生数量.IsSpecified = true;
				gLTrialBalance.借方外币发生额.IsSpecified = true;
				gLTrialBalance.借方发生额.IsSpecified = true;
				gLTrialBalance.借方发生数量.Value = (double)glAccountBalancesAndMovements.DebitQuantity;
				gLTrialBalance.借方外币发生额.Value = (double)glAccountBalancesAndMovements.DebitCurrencyAmount;
				gLTrialBalance.借方发生额.Value = (double)glAccountBalancesAndMovements.DebitAmountLocalCurrency;

				gLTrialBalance.贷方发生数量.IsSpecified = true;
				gLTrialBalance.贷方外币发生额.IsSpecified = true;
				gLTrialBalance.贷方发生额.IsSpecified = true;
				gLTrialBalance.贷方发生数量.Value = (double)glAccountBalancesAndMovements.CreditQuantity;
				gLTrialBalance.贷方外币发生额.Value = (double)glAccountBalancesAndMovements.CreditCurrencyAmount;
				gLTrialBalance.贷方发生额.Value = (double)glAccountBalancesAndMovements.CreditAmountLocalCurrency;

				gLTrialBalance.期末数量.IsSpecified = true;
				gLTrialBalance.期末外币余额.IsSpecified = true;
				gLTrialBalance.期末余额.IsSpecified = true;
				debitCredit = 1;
				if (glAccountBalancesAndMovements.EndBalanceDRCR != gLAccDRCR)
				{
					debitCredit = -1;
				}

				gLTrialBalance.期末数量.Value = (double)glAccountBalancesAndMovements.EndQuantity;
				gLTrialBalance.期末外币余额.Value = glAccountBalancesAndMovements.EndBalanceCurrency == 0m ? 0 : (double)glAccountBalancesAndMovements.EndBalanceCurrency * debitCredit;
				gLTrialBalance.期末余额.Value = glAccountBalancesAndMovements.EndBalanceLocalCurrency == 0m ? 0 : (double)glAccountBalancesAndMovements.EndBalanceLocalCurrency * debitCredit;
			}
		}

		#endregion

		#region Vouchers

		void SetVoucherValue(XSDs.记账凭证Collection vouchers, BusinessObjectThatDoesntSaveForCN bizObj)
		{
			var voucherCollection = new VoucherCollection(currentFactory);
			voucherCollection.AddElements(bizObj.FromDate, bizObj.ToDate, bizObj.BranchCode);
			foreach (Voucher voucher in voucherCollection)
			{
				XSDs.记账凭证 voucher1 = vouchers.AddNew();

				voucher1.凭证日期.Value = voucher.VoucherDate;
				voucher1.凭证种类.Value = voucher.VoucherTypeNumber;
				voucher1.凭证编号.Value = voucher.VoucherNumber;
				voucher1.行号.Value = new ZInt(voucher.VoucherLineNumber);
				voucher1.摘要.Value = voucher.VoucherDescription;
				voucher1.科目编号.Value = voucher.GLAccountNumber;
				voucher1.借方金额.IsSpecified = true;
				voucher1.借方金额.Value = (double)voucher.DebitAmountLocalCurrency;
				voucher1.贷方金额.IsSpecified = true;
				voucher1.贷方金额.Value = (double)voucher.CreditAmountLocalCurrency;
				voucher1.币种.Value = GetCurrencyNameFromCode(voucher.CurrencyCode);
				voucher1.借方外币金额.IsSpecified = true;
				voucher1.借方外币金额.Value = (double)voucher.DebitCurrencyAmount;
				voucher1.贷方外币金额.IsSpecified = true;
				voucher1.贷方外币金额.Value = (double)voucher.CreditCurrencyAmount;
				voucher1.汇率.IsSpecified = true;
				voucher1.汇率.Value = (double)voucher.ExRate;
				voucher1.数量.IsSpecified = true;
				voucher1.数量.Value = (double)voucher.DebitQuantity;
				voucher1.单价.IsSpecified = true;
				voucher1.单价.Value = (double)voucher.UnitPrice;
				voucher1.辅助核算组.Value = voucher.EntryLineExtendedFieldSchemas;
				if (AccountingConfigurationRegistry.Instance.ARControlAccount.Value == voucher.AccountPK || AccountingConfigurationRegistry.Instance.APControlAccount.Value == voucher.AccountPK)// China standard special word.
				{
					voucher1.辅助核算组.Value = voucher.OrgCode;
				}
				voucher1.结算方式.Value = voucher.PaymentTypeCode;
				voucher1.票据类型.Value = voucher.VoucherType;
				voucher1.票据号.Value = voucher.VoucherDocNumber;
				voucher1.票据日期.Value = voucher.VoucherDocDate;
				voucher1.附件数.IsSpecified = true;
				voucher1.附件数.Value = voucher.Attachments;
				voucher1.制单人员.Value = voucher.EnteredBy;
				voucher1.审核人员.Value = voucher.Reviwer;
				voucher1.记账人员.Value = voucher.PreparedBy;
				voucher1.出纳人员.Value = voucher.Cashier;
				voucher1.结账标志.IsSpecified = true;
				voucher1.结账标志.Value = voucher.AccountingFlag;
				voucher1.作废标志.IsSpecified = true;
				voucher1.作废标志.Value = voucher.VoidFlag;
			}
		}

		#endregion

		BusinessObjectFactory currentFactory;

		ZString companyName
		{
			get
			{
				if (fCompanyName.IsEmpty)
				{
					fCompanyName = LocalCompanyName.GetCurrentCompanyLocalName();
				}

				return fCompanyName;
			}
		}

		ZString fCompanyName = ZString.Empty;

		ZString currUnit
		{
			get
			{
				if (fCurrUnit.IsEmpty)
				{
					fCurrUnit = new Report().CurrencyUnit;
				}

				return fCurrUnit;
			}
		}

		ZString fCurrUnit = ZString.Empty;

		ZInt fPeriod;
		ZString fLastDay;

		ZString GetLastDayFromPeriod(ZInt period)
		{
			if (fPeriod != period)
			{
				fPeriod = period;
				fLastDay = (new AccountingPeriodCalculator(currentFactory)).GetLastDayForPeriod(period).ToString("yyyyMMdd");
			}
			return fLastDay;
		}

		RefCurrency fCurrency;

		ZString GetCurrencyNameFromCode(ZString currencyCode)
		{
			if (fCurrency == null || fCurrency.RX_Code != currencyCode)
			{
				fCurrency = RefCurrency.LoadFromCurrencyCode(currentFactory, currencyCode);
			}
			return fCurrency.RX_UnitNameMultilingual.ToString(Constants.Languages.ChineseSimplified);
		}

		protected override void ImportFromValueObjectCore(BizObjThatDoesntSaveForCN2004 bizObj, XSDs.会计核算软件数据 value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}
	}
}
