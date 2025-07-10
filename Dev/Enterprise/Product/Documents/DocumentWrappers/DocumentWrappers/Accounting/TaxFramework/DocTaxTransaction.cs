using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	public class DocTaxTransaction : DocBaseWrapper
	{
		protected DocTaxTransaction(AccTaxTransaction taxTransaction, BusinessObjectFactory factory)
			: base(taxTransaction, factory)
		{
			Argument.NotNull(taxTransaction, "AccTaxTransaction");
		}

		public static DocTaxTransaction New(AccTaxTransaction taxTransaction, BusinessObjectFactory factoryToWrap)
		{
			return new DocTaxTransaction(taxTransaction, factoryToWrap);
		}

		public ZString TaxSuperType => TaxTransaction.ATT_TaxSuperType;
		public ZBool IncludeInInvoiceAmount => TaxTransaction.ATT_AffectsSourceTransactionTotal;
		public ZDecimal OSAmount => TaxTransaction.ATT_OSTaxAmount * Multiplier;
		public ZDecimal LocalAmount => TaxTransaction.ATT_LocalTaxAmount * Multiplier;
		public ZDecimal OSBaseAmount => TaxTransaction.ATT_OSTaxBaseAmount * Multiplier;
		public ZDecimal LocalBaseAmount => TaxTransaction.ATT_LocalTaxBaseAmount * Multiplier;
		public ZBool IsRealized => !TaxTransaction.ATT_RealisationDate.IsEmpty;
		public ZString Currency => TaxTransaction.ATT_RX_NKOSTaxCurrency;
		ZDecimal taxRateDecimal => (ZDecimal)TaxTransaction.ATT_RateNumerator / TaxTransaction.ATT_RateDenominator;
		public ZString TaxRate => FormatNumberUtil.FormatRateWithCurrentCompanysCulture(taxRateDecimal);
		public ZString TaxSystemCode => TaxTransaction.ATT_TaxSystemCode;
		public ZString TaxSystemDescription => TaxSystems.GetDescriptionFromCode(TaxSystemCode);
		public ZString TaxAuthorityCode => TaxTransaction.TaxAuthorityCode;
		public ZString TaxAuthorityDescription => TaxAuthorities.GetDescriptionFromCode(TaxAuthorityCode);
		public ZString TaxAuthorityServiceCode => TaxTransaction.ATT_TaxAuthorityServiceCode;
		public ZString TaxAuthorityServiceCodeDescription => TaxTransaction.ATT_TaxAuthorityServiceCodeDescription;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007:Customizable Data Translation Rule", Justification = "Baseline")]
		public ZString TaxMessageText => TaxTransaction.TaxMessage?.A9_EnglishMsg ?? ZString.Empty;
		public ZString TaxConfigurationDescription => TaxTransaction.TaxConfiguration?.ETC_Description ?? ZString.Empty;

		internal AccTaxTransaction TaxTransaction => WrappedObject as AccTaxTransaction;

		TransactionHeader TransactionHeader => Factory.Load<TransactionHeader>(TaxTransaction.ATT_AH);

		int Multiplier => (TransactionHeader as ITransactionHeader)?.Multiplier ?? 1;

		CodeDescriptionPairList TaxSystems => taxSystems ?? (taxSystems = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().GetTaxSystems(TaxTransaction.Company.GC_RN_NKCountryCode));
		CodeDescriptionPairList taxSystems;
		CodeDescriptionPairList TaxAuthorities => taxAuthorities ?? (taxAuthorities = ObjectFactory.Get<IAccountingMasterFilesDependencyFactory>().GetTaxFrameworkConfigurationHelper().GetTaxAuthorities(TaxTransaction.Company.GC_RN_NKCountryCode));
		CodeDescriptionPairList taxAuthorities;
	}
}
