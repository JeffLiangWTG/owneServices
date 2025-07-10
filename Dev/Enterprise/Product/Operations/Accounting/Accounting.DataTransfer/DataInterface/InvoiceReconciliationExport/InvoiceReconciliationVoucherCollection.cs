using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile;
using Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.DataInterface
{
	public class InvoiceReconciliationVoucherCollection : VoucherCollection
	{
		public InvoiceReconciliationVoucherCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public InvoiceReconciliationVoucherCollection(BusinessObjectFactory factory, ZString complianceSubType, ZString exportStatus)
			: base(factory)
		{
			this.ComplianceSubType = complianceSubType;
			this.ExportStatus = exportStatus;
		}

		readonly ZString ComplianceSubType;
		readonly ZString ExportStatus;

		public override ZQuery ExtraFilter
		{
			get
			{
				if (extraFilter != null)
				{
					return extraFilter;
				}
				else
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(AccTransactionHeader));
					query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType,
						new ZString[] { TransactionTypes.Invoice, TransactionTypes.CreditNote, TransactionTypes.AdjustmentNote });
					switch (ComplianceSubType)
					{
						case "TAX":
							query.AddToFilter(AccTransactionHeaderSchema.AH_ComplianceSubType, new ZString[] { "TXA", "TXB" });
							break;
						case "TXA":
							query.AddToFilter(AccTransactionHeaderSchema.AH_ComplianceSubType, "TXA");
							break;
						case "TXB":
							query.AddToFilter(AccTransactionHeaderSchema.AH_ComplianceSubType, "TXB");
							break;
						case "NTX":
							query.AddToFilter(AccTransactionHeaderSchema.AH_ComplianceSubType, ZString.Empty);
							break;
					}

					ZDBOnlySubQuery subQuery = null;
					ZQuery filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DataExportCode);
					filter.AddToFilter(StmALogSchema.SL_Table, AccTransactionHeaderSchema.Constants.TableName);
					switch (ExportStatus)
					{
						case "NEX":
							subQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent, true);
							break;
						case "PEX":
							subQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
							break;
					}
					if (subQuery != null)
					{
						subQuery.AddToFilter(filter, JoinCondition.And);
						query.AddSubQuery(subQuery, JoinCondition.And);
					}
					extraFilter = query;
					return query;
				}
			}
		}
		ZQuery extraFilter;

		protected override void SetVoucherValue(ZInt year, AccTransactionHeader transaction, VoucherProvider provider)
		{
			base.SetVoucherValue(year, transaction, provider);
			if (transaction != null && ListTransactionHeader.All(c => c.PK != transaction.PK))
			{
				ListTransactionHeader.Add(transaction);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "China local client specific string.")]
		protected override void SetVoucherLineValue(ZInt year, AccTransactionHeader transaction, VoucherProvider provider, Voucher baseVoucher, VoucherLine voucherLine, ZInt i)
		{
			base.SetVoucherLineValue(year, transaction, provider, baseVoucher, voucherLine, i);

			if (transaction != null)
			{
				var voucher = (VoucherKingDeeK3)baseVoucher;

				voucher.CalculatedCurrencyCode = voucher.CurrencyCode;
				voucher.CalculatedExchangeRate = voucher.ExRate;
				voucher.CalculatedAmount = voucher.DebitCurrencyAmount > 0 ? voucher.DebitCurrencyAmount : voucher.CreditCurrencyAmount;

				if (voucherLine.AccountPK != null && voucherLine.AccountPK != ZGuid.Empty)
				{
					if (provider.Ledger == LedgerTypes.AccountsPayable && voucherLine.AccountPK == AccountingConfigurationRegistry.Instance.APControlAccount.Value)
					{
						voucher.AccountingItem = string.Format(CultureInfo.InvariantCulture, "供应商---{0}---{1}", provider.OrganisationCode, provider.OrganisationName);
					}
					else if (provider.Ledger == LedgerTypes.AccountsReceivable && voucherLine.AccountPK == AccountingConfigurationRegistry.Instance.ARControlAccount.Value)
					{
						voucher.AccountingItem = string.Format(CultureInfo.InvariantCulture, "客户---{0}---{1}", provider.OrganisationCode, provider.OrganisationName);
					}

					if ((provider.Ledger == LedgerTypes.AccountsPayable && voucherLine.AccountPK == AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value) ||
						(provider.Ledger == LedgerTypes.AccountsReceivable && voucherLine.AccountPK == AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value))
					{
						voucher.CalculatedCurrencyCode = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
						voucher.CalculatedExchangeRate = VoucherKingDeeK3.InvalidExchangeRate;
						voucher.CalculatedAmount = voucher.DebitAmountLocalCurrency > 0 ? voucher.DebitAmountLocalCurrency : voucher.CreditAmountLocalCurrency;
					}
				}

				voucher.PostDate = provider.PostDate;

				voucher.InvoiceDate = provider.InvoiceDate;

				if (!TransactionNumberMap.ContainsKey(transaction.PK))
				{
					TransactionNumberIndex++;
					TransactionNumberMap.Add(transaction.PK, TransactionNumberIndex);
				}
				voucher.TransactionIndex = TransactionNumberMap[transaction.PK];
			}
		}

		#region Cash Flow

		protected override bool AllowCashFlow
		{
			get { return false; }
		}

		#endregion

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new VoucherKingDeeK3();
		}

		int TransactionNumberIndex;
		readonly Dictionary<ZGuid, int> TransactionNumberMap = new Dictionary<ZGuid, int>();

		public List<AccTransactionHeader> ListTransactionHeader
		{
			get
			{
				if (listTransactionHeader == null)
				{
					listTransactionHeader = new List<AccTransactionHeader>();
				}
				return listTransactionHeader;
			}
		}
		List<AccTransactionHeader> listTransactionHeader;
	}
}
