using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Accounting.TaxFramework
{
	public class DocAccountingJournalTaxDetail : DocBaseWrapper
	{
		DocAccountingJournalTaxDetail(IAccountingJournalTaxDetail taxDetail, BusinessObjectFactory factoryToWrap)
			: base(taxDetail, factoryToWrap)
		{
		}

		public static DocAccountingJournalTaxDetail New(IAccountingJournalTaxDetail taxDetail, BusinessObjectFactory factoryToWrap)
		{
			return new DocAccountingJournalTaxDetail(taxDetail, factoryToWrap);
		}
		IAccountingJournalTaxDetail TaxDetail => (IAccountingJournalTaxDetail)WrappedObject;

		public ZString ConfigurationCode => TaxDetail.TaxConfiguration;
		public ZString Basis => TaxDetail.Basis;
		public ZString GLAccount => TaxDetail.GLAccount;
		public ZString GLDescription => TaxDetail.GLAccountDesc;
		public ZDate PostDate => TaxDetail.PostDate;
		public ZString PostPeriod => TaxDetail.PostPeriod;
		public ZString BranchCode => TaxDetail.BranchCode;
		public ZString DepartmentCode => TaxDetail.DepartmentCode;
		ZString DebitCreditSign => TaxDetail.Amount > 0M ? DebitCreditDataEntry.DR : DebitCreditDataEntry.CR;
		public ZDecimal DebitAmountDecimal => DebitCreditSign == DebitCreditDataEntry.DR ? (ZDecimal)Math.Abs(TaxDetail.Amount) : ZDecimal.Zero;
		public ZString DebitAmountString => DebitAmountDecimal != ZDecimal.Zero ? (ZString)FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(DebitAmountDecimal, LocalCurrency) : ZString.Empty;
		public ZDecimal CreditAmountDecimal => DebitCreditSign == DebitCreditDataEntry.CR ? (ZDecimal)Math.Abs(TaxDetail.Amount) : ZDecimal.Zero;
		public ZString CreditAmountString => CreditAmountDecimal != ZDecimal.Zero ? (ZString)FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(CreditAmountDecimal, LocalCurrency) : ZString.Empty;
		public ZDecimal OSAmountDecimal => Math.Abs(TaxDetail.OSAmount);
		public ZString OSAmountString => TaxDetail.LocalCurrency != TaxDetail.Currency ? (ZString)$"{FormatNumberUtil.FormatAmountWithCurrentCompanysCulture(OSAmountDecimal, Currency)} {DebitCreditSign}" : ZString.Empty;
		public ZString AlternateAccountNum => TaxDetail.AlternateAccountNum;
		public ZString AlternateAccountDesc => TaxDetail.AlternateAccountDesc;
		public ZBool IsMissingPeriodForReportingBook => TaxDetail.IsMissingPeriodForReportingBook;
		public DocCurrency Currency
		{
			get { return DocCurrency.New(TaxDetail.Currency, Factory); }
		}
		DocCurrency LocalCurrency
		{
			get { return DocCurrency.New(TaxDetail.LocalCurrency, Factory); }
		}
	}

	public class DocAccountingJournalTaxDetailCollection : DocumentWrapperCollection<DocAccountingJournalTaxDetail>
	{
		public DocAccountingJournalTaxDetailCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public static DocAccountingJournalTaxDetailCollection New(BusinessObjectFactory factory)
		{
			return new DocAccountingJournalTaxDetailCollection(factory);
		}
	}
}
