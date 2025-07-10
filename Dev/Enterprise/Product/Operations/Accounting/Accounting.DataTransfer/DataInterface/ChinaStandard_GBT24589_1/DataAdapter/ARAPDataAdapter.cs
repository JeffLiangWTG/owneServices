using System;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.DataTransfer.DataInterface;
using Enterprise.DataTransfer.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1
{
	public class ARAPDataAdapter : BaseAccountingDataAdapter<BusinessObjectThatDoesntSaveForCN, XSDs.应收应付>
	{
		#region Data Adapter Overrides

		public override string RootCollectionElementName
		{
			get { return "ARAPDatas"; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Element name")]
		public override string RootElementName
		{
			get { return "应收应付"; }
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

		protected override void ImportFromValueObjectCore(BusinessObjectThatDoesntSaveForCN bizObj, XSDs.应收应付 value, IValueObjectImportContext context)
		{
			throw new NotSupportedException();
		}

		BusinessObjectFactory currentFactory;

		#endregion

		protected override void ExportToValueObjectCore(BusinessObjectThatDoesntSaveForCN bizObj, XSDs.应收应付 constructedValueObject, IValueObjectExportContext context)
		{
			currentFactory = bizObj.Factory;
			SetBillsTypeValue(constructedValueObject.单据类型);//Bills Type
			SetTransactionTypeValue(constructedValueObject.交易类型);//Transaction Type
			SetARStatementValue(constructedValueObject.应收明细表, bizObj);//ARStatement
			SetAPStatementValue(constructedValueObject.应付明细表, bizObj);//APStatement
		}

		void SetBillsTypeValue(XSDs.单据类型Collection billsTypes)
		{
			foreach (CodeDescriptionPair arapBillType in new ARAPBillTypes())
			{
				XSDs.单据类型 billType = billsTypes.AddNew();
				billType.单据类型编码.Value = arapBillType.Code;
				billType.单据类型名称.Value = arapBillType.Description;
			}
		}

		void SetTransactionTypeValue(XSDs.交易类型Collection transactionTypes)
		{
			foreach (ARAPTransactionTypes aRAPTransactionType in new ARAPTransactionTypesCollection(currentFactory))
			{
				XSDs.交易类型 transactionType = transactionTypes.AddNew();
				transactionType.交易类型编码.Value = aRAPTransactionType.TransactionTypesCode;
				transactionType.交易类型名称.Value = aRAPTransactionType.TransactionTypesName;
			}
		}

		void SetARStatementValue(XSDs.应收明细表Collection aRStatements, BusinessObjectThatDoesntSaveForCN bizObj)
		{
			ARAPTransactionsCollection aRAPTransactionsCollection = new ARAPTransactionsCollection(currentFactory);
			aRAPTransactionsCollection.BuildTransactions(LedgerTypes.AccountsReceivable, bizObj.FromDate, bizObj.ToDate);
			foreach (ARAPTransactions aRAPTransaction in aRAPTransactionsCollection)
			{
				XSDs.应收明细表 aRTransaction = aRStatements.AddNew();
				aRTransaction.交易类型编码.Value = aRAPTransaction.TransactionTypeCode;
				aRTransaction.付款日期.Value = GetDateSafe(aRAPTransaction.PaymentDate);
				aRTransaction.会计年度.Value = aRAPTransaction.FinancialYear.ToString();
				aRTransaction.会计期间号.Value = aRAPTransaction.Period.ToString();
				aRTransaction.余额方向.Value = aRAPTransaction.CRDR;
				aRTransaction.到期日.Value = GetDateSafe(aRAPTransaction.DueDate);
				aRTransaction.单据类型编码.Value = aRAPTransaction.BillsTypeCode;
				aRTransaction.单据编号.Value = aRAPTransaction.BillsNumber;
				aRTransaction.原币余额.IsSpecified = true;
				aRTransaction.原币余额.Value = Convert.ToDouble(aRAPTransaction.OSBalance);
				aRTransaction.原币发生金额.IsSpecified = true;
				aRTransaction.原币发生金额.Value = Convert.ToDouble(aRAPTransaction.OSTransactionAmount);
				aRTransaction.原币币种.Value = aRAPTransaction.TransactionCurrency;
				aRTransaction.发票号.Value = aRAPTransaction.InvoiceNumber;
				aRTransaction.合同号.Value = aRAPTransaction.ContractNumber;
				aRTransaction.客户编码.Value = aRAPTransaction.ClientCode;
				aRTransaction.摘要.Value = aRAPTransaction.Description;
				aRTransaction.本位币.Value = aRAPTransaction.BaseCurrency;
				aRTransaction.本币余额.IsSpecified = true;
				aRTransaction.本币余额.Value = Convert.ToDouble(aRAPTransaction.LocalBalance);
				aRTransaction.本币发生金额.IsSpecified = true;
				aRTransaction.本币发生金额.Value = Convert.ToDouble(aRAPTransaction.LocalTransactionAmount);
				aRTransaction.核销凭证编号.Value = aRAPTransaction.VerifyMatchVoucherNumber;
				aRTransaction.核销日期.Value = GetDateSafe(aRAPTransaction.VerifyMatchDate);
				aRTransaction.核销标志.Value = aRAPTransaction.VerifyMatchFlag;
				aRTransaction.汇率.IsSpecified = true;
				aRTransaction.汇率.Value = Convert.ToDouble(aRAPTransaction.ExRate);
				aRTransaction.汇票编号.Value = aRAPTransaction.RemittanceDraftNumber;
				aRTransaction.科目编号.Value = aRAPTransaction.GLAccountNumber;
				aRTransaction.结算方式编码.Value = aRAPTransaction.PaymentTypeCode;
				aRTransaction.记账凭证日期.Value = aRAPTransaction.VoucherDate;
				aRTransaction.记账凭证类型编号.Value = aRAPTransaction.VoucherTypeNumber;
				aRTransaction.记账凭证编号.Value = aRAPTransaction.VoucherNumber;
				aRTransaction.记账日期.Value = aRAPTransaction.EnteredDate;
				aRTransaction.项目编码.Value = aRAPTransaction.ItemCode;
			}
		}

		void SetAPStatementValue(XSDs.应付明细表Collection aPStatements, BusinessObjectThatDoesntSaveForCN bizObj)
		{
			ARAPTransactionsCollection aRAPTransactionsCollection = new ARAPTransactionsCollection(currentFactory);
			aRAPTransactionsCollection.BuildTransactions(LedgerTypes.AccountsPayable, bizObj.FromDate, bizObj.ToDate);
			foreach (ARAPTransactions aRAPTransaction in aRAPTransactionsCollection)
			{
				XSDs.应付明细表 aRTransaction = aPStatements.AddNew();
				aRTransaction.交易类型编码.Value = aRAPTransaction.TransactionTypeCode;
				aRTransaction.付款日期.Value = GetDateSafe(aRAPTransaction.PaymentDate);
				aRTransaction.会计年度.Value = aRAPTransaction.FinancialYear.ToString();
				aRTransaction.会计期间号.Value = aRAPTransaction.Period.ToString();
				aRTransaction.余额方向.Value = aRAPTransaction.CRDR;
				aRTransaction.到期日.Value = GetDateSafe(aRAPTransaction.DueDate);
				aRTransaction.单据类型编码.Value = aRAPTransaction.BillsTypeCode;
				aRTransaction.单据编号.Value = aRAPTransaction.BillsNumber;
				aRTransaction.原币余额.IsSpecified = true;
				aRTransaction.原币余额.Value = Convert.ToDouble(aRAPTransaction.OSBalance);
				aRTransaction.原币发生金额.IsSpecified = true;
				aRTransaction.原币发生金额.Value = Convert.ToDouble(aRAPTransaction.OSTransactionAmount);
				aRTransaction.原币币种.Value = aRAPTransaction.TransactionCurrency;
				aRTransaction.发票号.Value = aRAPTransaction.InvoiceNumber;
				aRTransaction.合同号.Value = aRAPTransaction.ContractNumber;
				aRTransaction.供应商编码.Value = aRAPTransaction.ClientCode;
				aRTransaction.摘要.Value = aRAPTransaction.Description;
				aRTransaction.本位币.Value = aRAPTransaction.BaseCurrency;
				aRTransaction.本币余额.IsSpecified = true;
				aRTransaction.本币余额.Value = Convert.ToDouble(aRAPTransaction.LocalBalance);
				aRTransaction.本币发生金额.IsSpecified = true;
				aRTransaction.本币发生金额.Value = Convert.ToDouble(aRAPTransaction.LocalTransactionAmount);
				aRTransaction.核销凭证编号.Value = aRAPTransaction.VerifyMatchVoucherNumber;
				aRTransaction.核销日期.Value = GetDateSafe(aRAPTransaction.VerifyMatchDate);
				aRTransaction.核销标志.Value = aRAPTransaction.VerifyMatchFlag;
				aRTransaction.汇率.IsSpecified = true;
				aRTransaction.汇率.Value = Convert.ToDouble(aRAPTransaction.ExRate);
				aRTransaction.汇票编号.Value = aRAPTransaction.RemittanceDraftNumber;
				aRTransaction.科目编号.Value = aRAPTransaction.GLAccountNumber;
				aRTransaction.结算方式编码.Value = aRAPTransaction.PaymentTypeCode;
				aRTransaction.记账凭证日期.Value = aRAPTransaction.VoucherDate;
				aRTransaction.记账凭证类型编号.Value = aRAPTransaction.VoucherTypeNumber;
				aRTransaction.记账凭证编号.Value = aRAPTransaction.VoucherNumber;
				aRTransaction.记账日期.Value = aRAPTransaction.EnteredDate;
				aRTransaction.项目编码.Value = aRAPTransaction.ItemCode;
			}
		}

		string GetDateSafe(ZString date)
		{
			return date.IsEmpty ? "00000000" : date.ToString();
		}
	}
}
