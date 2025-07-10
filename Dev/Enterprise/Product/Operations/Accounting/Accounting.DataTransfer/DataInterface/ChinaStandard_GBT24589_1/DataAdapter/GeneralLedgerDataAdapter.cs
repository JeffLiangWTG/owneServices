using System;
using System.Globalization;
using System.Linq;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.DataTransfer.DataInterface;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

#region SuppressResourceStringsCheckRegion

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public class GeneralLedgerDataAdapter : BaseAccountingDataAdapter<BusinessObjectThatDoesntSaveForCN, XSDs.总账>
	{
		#region Data Adapter Overrides

		public override string RootCollectionElementName
		{
			get { return "GeneralLedger"; }
		}

		public override string RootElementName
		{
			get { return "总账"; }
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
		{ }

		protected override void ImportFromValueObjectCore(BusinessObjectThatDoesntSaveForCN bizObj, XSDs.总账 value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		BusinessObjectFactory currentFactory;

		#endregion

		protected override void ExportToValueObjectCore(BusinessObjectThatDoesntSaveForCN bizObj, XSDs.总账 constructedValueObject, IValueObjectExportContext context)
		{
			currentFactory = bizObj.Factory;
			SetGeneralLedgerBaseInformation(constructedValueObject.总账基础信息);//General Ledger Base Information
			SetGLChartAccountValue(constructedValueObject.会计科目, bizObj.ChartType);//  GL Chart Account
			SetGLAccountAssistedValue(constructedValueObject.科目辅助核算);//Account Assisted
			SetCashFlowItemsValue(constructedValueObject.现金流量项目, bizObj);//Cash Flow Items
			SetGLAccountBalancesAndMovementsValue(constructedValueObject.科目余额及发生额, bizObj);//GL Account Balances And Movements
			SetVoucherValue(constructedValueObject.记账凭证, bizObj);//Voucher
			SetCashFlowItemsDataValue(constructedValueObject.现金流量凭证项目数据, bizObj);//Cash Flow Items Data
			SetReportsValue(constructedValueObject.报表集, bizObj);//Reports
			SetReportItemsValue(constructedValueObject.报表项数据, bizObj);//Report Items
		}

		void SetGeneralLedgerBaseInformation(XSDs.总账基础信息 gLBaseInformation)
		{
			gLBaseInformation.会计科目编号规则.Value = AccountingMasterFilesRegistry.Instance.LocalNumberFormats.Value.GetGLLocalNumberFormat(SharedConstants.Languages.ChineseSimplified, Constants.CountryCodes.China).NumberFormat;
			gLBaseInformation.凭证头可扩展字段结构.Value = "";
			gLBaseInformation.凭证头可扩展结构对应档案.Value = "";
			gLBaseInformation.分录行可扩展字段对应档案.Value = "";
			gLBaseInformation.分录行可扩展字段结构.Value = "";
			gLBaseInformation.现金流量项目编码规则.Value = "";
			gLBaseInformation.结构分隔符.Value = "-";
		}

		void SetGLChartAccountValue(XSDs.会计科目Collection gLChartAccounts, ZBool isOldType)
		{
			ZQuery query = new ZQuery(AccGLAccountDescriptorSchema.AJ_Language, SharedConstants.Languages.ChineseSimplified);
			query.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, Constants.CountryCodes.China);
			query.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, AccGLAccountDescriptor.ReportTypeCOA);
			query.OrderBy = AccGLAccountDescriptorSchema.Constants.AJ_LocalAccountNumber;
			AccGLAccountDescriptor[] accGLAccountDescs = currentFactory.Load<AccGLAccountDescriptor>(query);

			foreach (AccGLAccountDescriptor accGLAccountDesc in accGLAccountDescs)
			{
				XSDs.会计科目 localGLaccount = gLChartAccounts.AddNew();
				localGLaccount.余额方向.Value = ChineseUtils.ConvertDebitCreditToChinese(accGLAccountDesc.AJ_DebitCredit);
				localGLaccount.科目名称.Value = accGLAccountDesc.AJ_AccountDescription;
				localGLaccount.科目类型.Value = ChineseUtils.GetGLAccountTypeFromNumber(ZInt.Parse(accGLAccountDesc.AJ_LocalAccountNumber.Left(1)), isOldType);
				localGLaccount.科目级次.Value = (ZShort)GetGlAccountLevel(accGLAccountDesc.AJ_LocalAccountNumber);
				localGLaccount.科目编号.Value = accGLAccountDesc.AJ_LocalAccountNumber;
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

		ZString ControlAccountNumber(ZGuid controlAccountPK)
		{
			var query = new ZDBOnlyQuery(typeof(AccGLAccountDescriptor));
			query.AddToFilter(AccGLAccountDescriptorSchema.AJ_Language, SharedConstants.Languages.ChineseSimplified);
			query.AddToFilter(AccGLAccountDescriptorSchema.AJ_RN_NKCountryOfCompliance, Constants.CountryCodes.China);
			query.AddToFilter(AccGLAccountDescriptorSchema.AJ_ReportType, AccGLAccountDescriptor.ReportTypeCOA);

			var accGLDescriptorPivotSubQuery = new ZDBOnlySubQuery(typeof(AutoAccGLDescriptorPivot), AccGLDescriptorPivotSchema.YJ_AJ);
			accGLDescriptorPivotSubQuery.AddToFilter(AccGLDescriptorPivotSchema.YJ_AG, controlAccountPK);
			query.AddSubQuery(AccGLAccountDescriptorSchema.PK, accGLDescriptorPivotSubQuery, JoinCondition.And);

			var accGLAccountDescs = currentFactory.LoadTop1<AccGLAccountDescriptor>(query);
			if (accGLAccountDescs == null)
			{
				return "";
			}
			return accGLAccountDescs.AJ_LocalAccountNumber;
		}

		void SetGLAccountAssistedValue(XSDs.科目辅助核算Collection gLAccountAssisteds)
		{
			XSDs.科目辅助核算 gLAccountAssisted = gLAccountAssisteds.AddNew();
			gLAccountAssisted.对应档案.Value = "科目余额及发生额";
			gLAccountAssisted.科目编号.Value = ControlAccountNumber(AccountingConfigurationRegistry.Instance.ARControlAccount.Value);
			gLAccountAssisted.辅助项名称.Value = "客户";
			gLAccountAssisted.辅助项编号.Value = "客户";
			gLAccountAssisted.辅助项描述.Value = "客户明细核算";

			gLAccountAssisted = gLAccountAssisteds.AddNew();
			gLAccountAssisted.对应档案.Value = "科目余额及发生额";
			gLAccountAssisted.科目编号.Value = ControlAccountNumber(AccountingConfigurationRegistry.Instance.APControlAccount.Value);
			gLAccountAssisted.辅助项名称.Value = "供应商";
			gLAccountAssisted.辅助项编号.Value = "供应商";
			gLAccountAssisted.辅助项描述.Value = "供应商明细核算";
		}

		void SetCashFlowItemsValue(XSDs.现金流量项目Collection cashFlowItems, BusinessObjectThatDoesntSaveForCN bizObj)
		{
			var reportsDataCollection = new ReportsDataCollection(currentFactory, bizObj.Period, AccountingMasterFilesConstants.ReportCodeOfLocalReport.CashFlowStatement, bizObj.BranchPK);

			ReportsData[] reportsDataArry = reportsDataCollection.ToArray<ReportsData>();

			foreach (var reportsData in reportsDataArry.OrderBy(x => ConvertToZInt(x.ReportItemNumber)))
			{
				if (reportsData.ReportNumber != "8")
				{
					continue;
				}

				if (reportsData.ReportItemName.EndsWith("本月数", StringComparison.CurrentCultureIgnoreCase))
				{
					XSDs.现金流量项目 cashFlowItem = cashFlowItems.AddNew();
					if (reportsData.ReportItemValue == -1m || reportsData.ReportItemNumber == "25" || reportsData.ReportItemNumber == "27")
					{
						if (reportsData.ReportItemName.StartsWith("一", StringComparison.CurrentCultureIgnoreCase))
						{
							cashFlowItem.现金流量项目编码.Value = "O";
						}

						if (reportsData.ReportItemName.StartsWith("二", StringComparison.CurrentCultureIgnoreCase))
						{
							cashFlowItem.现金流量项目编码.Value = "I";
						}

						if (reportsData.ReportItemName.StartsWith("三", StringComparison.CurrentCultureIgnoreCase))
						{
							cashFlowItem.现金流量项目编码.Value = "F";
						}

						if (reportsData.ReportItemName.StartsWith("四", StringComparison.CurrentCultureIgnoreCase))
						{
							cashFlowItem.现金流量项目编码.Value = "E";
						}

						if (reportsData.ReportItemName.StartsWith("五", StringComparison.CurrentCultureIgnoreCase))
						{
							cashFlowItem.现金流量项目编码.Value = "X";
						}

						if (reportsData.ReportItemName.StartsWith("六", StringComparison.CurrentCultureIgnoreCase))
						{
							cashFlowItem.现金流量项目编码.Value = "U";
						}

						cashFlowItem.是否末级.Value = "0";
						cashFlowItem.现金流量项目级次.Value = "1";
						cashFlowItem.现金流量项目父节点.Value = "";
					}
					else
					{
						if (Convert.ToInt16(reportsData.ReportItemNumber, new CultureInfo("en-US")) < 8)
						{
							cashFlowItem.现金流量项目编码.Value = reportsData.CashFlowCode;
							cashFlowItem.现金流量项目父节点.Value = "O";
						}
						else if (Convert.ToInt16(reportsData.ReportItemNumber, new CultureInfo("en-US")) < 14)
						{
							cashFlowItem.现金流量项目编码.Value = reportsData.CashFlowCode;
							cashFlowItem.现金流量项目父节点.Value = "I";
						}
						else if (Convert.ToInt16(reportsData.ReportItemNumber, new CultureInfo("en-US")) < 23)
						{
							cashFlowItem.现金流量项目编码.Value = reportsData.CashFlowCode;
							cashFlowItem.现金流量项目父节点.Value = "F";
						}
						else if (Convert.ToInt16(reportsData.ReportItemNumber, new CultureInfo("en-US")) < 25)
						{
							cashFlowItem.现金流量项目编码.Value = reportsData.CashFlowCode;
							cashFlowItem.现金流量项目父节点.Value = "E";
						}

						if (Convert.ToInt16(reportsData.ReportItemNumber, new CultureInfo("en-US")) == 26m)
						{
							cashFlowItem.现金流量项目编码.Value = reportsData.CashFlowCode;
							cashFlowItem.现金流量项目父节点.Value = "X";
						}
						cashFlowItem.是否末级.Value = "1";
						cashFlowItem.现金流量项目级次.Value = "2";
					}

					cashFlowItem.现金流量项目名称.Value = reportsData.ReportItemName.Replace("本月数", ZString.Empty);
					cashFlowItem.现金流量项目描述.Value = " ";

					cashFlowItem.现金流量数据来源.Value = "1";
					cashFlowItem.现金流量项目属性.Value = "2";
				}
			}
		}

		void SetGLAccountBalancesAndMovementsValue(XSDs.科目余额及发生额Collection gLAccBalancesAndMovements, BusinessObjectThatDoesntSaveForCN bizObj)
		{
			GLAccountBalancesAndMovementsCollection gLAccountBalancesAndMovementsCollection = new GLAccountBalancesAndMovementsCollection(currentFactory, bizObj.Period, bizObj.BranchPK);
			foreach (GLAccountBalancesAndMovements glAccountBalancesAndMovements in gLAccountBalancesAndMovementsCollection)
			{
				XSDs.科目余额及发生额 accBalancesAndMovement = gLAccBalancesAndMovements.AddNew();
				accBalancesAndMovement.科目编号.Value = glAccountBalancesAndMovements.GLAccountNumber;
				accBalancesAndMovement.期末余额方向.Value = glAccountBalancesAndMovements.EndBalanceDRCR;
				accBalancesAndMovement.期初余额方向.Value = glAccountBalancesAndMovements.OpenBalanceDRCR;
				accBalancesAndMovement.币种编码.Value = glAccountBalancesAndMovements.CurrencyCode;
				accBalancesAndMovement.计量单位.Value = glAccountBalancesAndMovements.Unit;

				accBalancesAndMovement.会计年度.Value = glAccountBalancesAndMovements.FinancialYear.ToString();
				accBalancesAndMovement.会计期间号.Value = glAccountBalancesAndMovements.Period.ToString();

				accBalancesAndMovement.期初数量.IsSpecified = true;
				accBalancesAndMovement.期初原币余额.IsSpecified = true;
				accBalancesAndMovement.期初本币余额.IsSpecified = true;
				accBalancesAndMovement.期初数量.Value = (double)glAccountBalancesAndMovements.OpenQuantity;
				accBalancesAndMovement.期初原币余额.Value = (double)glAccountBalancesAndMovements.OpenBalanceCurrency;
				accBalancesAndMovement.期初本币余额.Value = (double)glAccountBalancesAndMovements.OpenBalanceLocalCurrency;

				accBalancesAndMovement.借方数量.IsSpecified = true;
				accBalancesAndMovement.借方原币金额.IsSpecified = true;
				accBalancesAndMovement.借方本币金额.IsSpecified = true;
				accBalancesAndMovement.借方数量.Value = (double)glAccountBalancesAndMovements.DebitQuantity;
				accBalancesAndMovement.借方原币金额.Value = (double)glAccountBalancesAndMovements.DebitCurrencyAmount;
				accBalancesAndMovement.借方本币金额.Value = (double)glAccountBalancesAndMovements.DebitAmountLocalCurrency;

				accBalancesAndMovement.贷方数量.IsSpecified = true;
				accBalancesAndMovement.贷方原币金额.IsSpecified = true;
				accBalancesAndMovement.贷方本币金额.IsSpecified = true;
				accBalancesAndMovement.贷方数量.Value = (double)glAccountBalancesAndMovements.CreditQuantity;
				accBalancesAndMovement.贷方原币金额.Value = (double)glAccountBalancesAndMovements.CreditCurrencyAmount;
				accBalancesAndMovement.贷方本币金额.Value = (double)glAccountBalancesAndMovements.CreditAmountLocalCurrency;

				accBalancesAndMovement.期末数量.IsSpecified = true;
				accBalancesAndMovement.期末原币余额.IsSpecified = true;
				accBalancesAndMovement.期末本币余额.IsSpecified = true;
				accBalancesAndMovement.期末数量.Value = (double)glAccountBalancesAndMovements.EndQuantity;
				accBalancesAndMovement.期末原币余额.Value = (double)glAccountBalancesAndMovements.EndBalanceCurrency;
				accBalancesAndMovement.期末本币余额.Value = (double)glAccountBalancesAndMovements.EndBalanceLocalCurrency;

				accBalancesAndMovement.辅助项1编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber1;
				accBalancesAndMovement.辅助项2编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber2;
				accBalancesAndMovement.辅助项3编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber3;
				accBalancesAndMovement.辅助项4编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber4;
				accBalancesAndMovement.辅助项5编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber5;
				accBalancesAndMovement.辅助项6编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber6;
				accBalancesAndMovement.辅助项7编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber7;
				accBalancesAndMovement.辅助项8编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber8;
				accBalancesAndMovement.辅助项9编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber9;
				accBalancesAndMovement.辅助项10编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber10;
				accBalancesAndMovement.辅助项11编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber11;
				accBalancesAndMovement.辅助项12编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber12;
				accBalancesAndMovement.辅助项13编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber13;
				accBalancesAndMovement.辅助项14编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber14;
				accBalancesAndMovement.辅助项15编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber15;
				accBalancesAndMovement.辅助项16编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber16;
				accBalancesAndMovement.辅助项17编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber17;
				accBalancesAndMovement.辅助项18编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber18;
				accBalancesAndMovement.辅助项19编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber19;
				accBalancesAndMovement.辅助项20编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber20;
				accBalancesAndMovement.辅助项21编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber21;
				accBalancesAndMovement.辅助项22编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber22;
				accBalancesAndMovement.辅助项23编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber23;
				accBalancesAndMovement.辅助项24编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber24;
				accBalancesAndMovement.辅助项25编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber25;
				accBalancesAndMovement.辅助项26编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber26;
				accBalancesAndMovement.辅助项27编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber27;
				accBalancesAndMovement.辅助项28编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber28;
				accBalancesAndMovement.辅助项29编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber29;
				accBalancesAndMovement.辅助项30编号.Value = glAccountBalancesAndMovements.GLAccountAssistedNumber30;
			}
		}

		void SetVoucherValue(XSDs.记账凭证Collection vouchers, BusinessObjectThatDoesntSaveForCN bizObj)
		{
			VoucherCollection voucherCollection = new VoucherCollection(currentFactory);
			voucherCollection.AddElements(bizObj.FromDate, bizObj.ToDate, bizObj.BranchCode);
			foreach (Voucher voucher in voucherCollection)
			{
				XSDs.记账凭证 voucher1 = vouchers.AddNew();
				voucher1.会计年度.Value = voucher.FinancialYear.ToString();
				voucher1.会计期间号.Value = voucher.Period.ToString();
				voucher1.作废标志.Value = voucher.VoidFlag.ToString();
				voucher1.借方原币金额.IsSpecified = true;
				voucher1.借方数量.IsSpecified = true;
				voucher1.借方本币金额.IsSpecified = true;
				voucher1.借方原币金额.Value = (double)voucher.DebitCurrencyAmount;
				voucher1.借方数量.Value = (double)voucher.DebitQuantity;
				voucher1.借方本币金额.Value = (double)voucher.DebitAmountLocalCurrency;
				voucher1.凭证头可扩展字段结构值.Value = voucher.VoucherHeaderExtendedFieldSchemasValue;
				voucher1.凭证来源系统.Value = voucher.VoucherSourceSystem;
				voucher1.分录行可扩展字段结构值.Value = voucher.EntryLineExtendedFieldSchemas;
				voucher1.制单人.Value = voucher.PreparedBy;
				voucher1.单价.IsSpecified = true;
				voucher1.单价.Value = (double)voucher.UnitPrice;
				voucher1.审核人.Value = voucher.Reviwer;
				voucher1.币种编码.Value = voucher.CurrencyCode;
				voucher1.汇率.IsSpecified = true;
				voucher1.汇率.Value = (double)voucher.ExRate;
				voucher1.汇率类型编号.Value = voucher.ExRateTypeNumber;
				voucher1.票据号.Value = voucher.VoucherDocNumber;
				voucher1.票据日期.Value = voucher.VoucherDocDate;
				voucher1.票据类型.Value = voucher.VoucherType;
				voucher1.科目编号.Value = voucher.GLAccountNumber;
				voucher1.结算方式编码.Value = voucher.PaymentTypeCode;
				voucher1.计量单位.Value = voucher.Unit;
				voucher1.记账人.Value = voucher.EnteredBy;
				voucher1.记账凭证摘要.Value = voucher.VoucherDescription;
				voucher1.记账凭证日期.Value = voucher.VoucherDate;
				voucher1.记账凭证类型编号.Value = voucher.VoucherTypeNumber;
				voucher1.记账凭证编号.Value = voucher.VoucherNumber;
				voucher1.记账凭证行号.Value = voucher.VoucherLineNumber;
				voucher1.记账标志.Value = voucher.VoidFlag.ToString();
				voucher1.贷方原币金额.IsSpecified = true;
				voucher1.贷方数量.IsSpecified = true;
				voucher1.贷方本币金额.IsSpecified = true;
				voucher1.贷方原币金额.Value = (double)voucher.CreditCurrencyAmount;
				voucher1.贷方数量.Value = (double)voucher.CreditQuantity;
				voucher1.贷方本币金额.Value = (double)voucher.CreditAmountLocalCurrency;
				voucher1.附件数.IsSpecified = true;
				voucher1.附件数.Value = voucher.Attachments;
				voucher1.辅助项1编号.Value = voucher.GLAccountAssistedNumber1;
				voucher1.辅助项2编号.Value = voucher.GLAccountAssistedNumber2;
				voucher1.辅助项3编号.Value = voucher.GLAccountAssistedNumber3;
				voucher1.辅助项4编号.Value = voucher.GLAccountAssistedNumber4;
				voucher1.辅助项5编号.Value = voucher.GLAccountAssistedNumber5;
				voucher1.辅助项6编号.Value = voucher.GLAccountAssistedNumber6;
				voucher1.辅助项7编号.Value = voucher.GLAccountAssistedNumber7;
				voucher1.辅助项8编号.Value = voucher.GLAccountAssistedNumber8;
				voucher1.辅助项9编号.Value = voucher.GLAccountAssistedNumber9;
				voucher1.辅助项10编号.Value = voucher.GLAccountAssistedNumber10;
				voucher1.辅助项11编号.Value = voucher.GLAccountAssistedNumber11;
				voucher1.辅助项12编号.Value = voucher.GLAccountAssistedNumber12;
				voucher1.辅助项13编号.Value = voucher.GLAccountAssistedNumber13;
				voucher1.辅助项14编号.Value = voucher.GLAccountAssistedNumber14;
				voucher1.辅助项15编号.Value = voucher.GLAccountAssistedNumber15;
				voucher1.辅助项16编号.Value = voucher.GLAccountAssistedNumber16;
				voucher1.辅助项17编号.Value = voucher.GLAccountAssistedNumber17;
				voucher1.辅助项18编号.Value = voucher.GLAccountAssistedNumber18;
				voucher1.辅助项19编号.Value = voucher.GLAccountAssistedNumber19;
				voucher1.辅助项20编号.Value = voucher.GLAccountAssistedNumber20;
				voucher1.辅助项21编号.Value = voucher.GLAccountAssistedNumber21;
				voucher1.辅助项22编号.Value = voucher.GLAccountAssistedNumber22;
				voucher1.辅助项23编号.Value = voucher.GLAccountAssistedNumber23;
				voucher1.辅助项24编号.Value = voucher.GLAccountAssistedNumber24;
				voucher1.辅助项25编号.Value = voucher.GLAccountAssistedNumber25;
				voucher1.辅助项26编号.Value = voucher.GLAccountAssistedNumber26;
				voucher1.辅助项27编号.Value = voucher.GLAccountAssistedNumber27;
				voucher1.辅助项28编号.Value = voucher.GLAccountAssistedNumber28;
				voucher1.辅助项29编号.Value = voucher.GLAccountAssistedNumber29;
				voucher1.辅助项30编号.Value = voucher.GLAccountAssistedNumber30;
			}
		}

		void SetCashFlowItemsDataValue(XSDs.现金流量凭证项目数据Collection cashFlowItemsDatas, BusinessObjectThatDoesntSaveForCN bizObj)
		{
			var voucherCollection = new VoucherCollection(currentFactory);
			voucherCollection.FillCashFlowVoucherCollection(bizObj.FromDate, bizObj.ToDate, bizObj.BranchCode);
			foreach (Voucher voucher in voucherCollection)
			{
				XSDs.现金流量凭证项目数据 itemsData = cashFlowItemsDatas.AddNew();
				itemsData.记账凭证类型编号.Value = "1";
				itemsData.记账凭证编号.Value = voucher.VoucherNumber;
				itemsData.币种编码.Value = voucher.CurrencyCode;
				itemsData.现金流量行号.Value = voucher.VoucherLineNumber;
				itemsData.现金流量摘要.Value = voucher.VoucherDescription;
				itemsData.现金流量项目编码.Value = voucher.CashFlowCode;
				itemsData.现金流量项目属性.Value = voucher.CashFlowItemAttribute;
				itemsData.现金流量原币金额.IsSpecified = true;
				itemsData.现金流量本币金额.IsSpecified = true;
				itemsData.现金流量原币金额.Value = (double)voucher.DebitCurrencyAmount + (double)voucher.CreditCurrencyAmount;
				itemsData.现金流量本币金额.Value = (double)voucher.DebitAmountLocalCurrency + (double)voucher.CreditAmountLocalCurrency;
			}
		}

		void SetReportsValue(XSDs.报表集Collection reports, BusinessObjectThatDoesntSaveForCN bizObj)
		{
			ReportCollection reportCollection = new ReportCollection(currentFactory, bizObj.Period);
			foreach (Report report in reportCollection)
			{
				XSDs.报表集 report1 = reports.AddNew();
				report1.报表名称.Value = report.ReportName;
				report1.报表报告日.Value = report.ReportDate;
				report1.报表报告期.Value = report.ReportPeriod;
				report1.报表编号.Value = report.ReportNumber;
				report1.编制单位.Value = report.ReportCompanyName;
				report1.货币单位.Value = report.CurrencyUnit;
			}
		}

		void SetReportItemsValue(XSDs.报表项数据Collection reportItems, BusinessObjectThatDoesntSaveForCN bizObj)
		{
			ReportsDataCollection reportsDataCollection = new ReportsDataCollection(currentFactory, bizObj.Period, ZString.Empty, bizObj.BranchCode);
			foreach (ReportsData reportsData in reportsDataCollection)
			{
				XSDs.报表项数据 reportItem = reportItems.AddNew();
				reportItem.报表编号.Value = reportsData.ReportNumber;
				reportItem.报表项公式.Value = reportsData.ReportItemFormula;
				reportItem.报表项名称.Value = reportsData.ReportItemName;
				reportItem.报表项数值.IsSpecified = true;
				reportItem.报表项数值.Value = (double)reportsData.ReportItemValue;
				reportItem.报表项编号.Value = reportsData.ReportItemNumber;
				if (reportsData.ReportNumber == "7")
				{
					reportItem.报表项编号.Value += reportsData.ReportItemName.EndsWith("本年实际") ? "A" : "B";
				}
			}
		}
	}
}

#endregion
