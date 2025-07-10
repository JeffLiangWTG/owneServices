using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	public class AccountingJournalTaxDetail : NonPersistentBusinessObject, IAccountingJournalTaxDetail
	{
		protected AccountingJournalTaxDetail(BusinessObjectFactory factory, IGLMovementDetails glMovementDetails)
			: base(factory)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(glMovementDetails, nameof(glMovementDetails));

			GLMovementDetails = glMovementDetails;
		}

		public static List<AccountingJournalTaxDetail> Create(BusinessObjectFactory factory, IReadOnlyCollection<IGLMovementDetails> collection)
		{
			var list = new List<AccountingJournalTaxDetail>();
			foreach (var element in collection)
			{
				var detail = new AccountingJournalTaxDetail(factory, element);
				list.Add(detail);
			}

			return list;
		}

		IGLMovementDetails GLMovementDetails { get; }
		public ZString TaxConfiguration => GLMovementDetails.TaxConfiguration;
		public ZString GLAccount => GLMovementDetails.GLAccount;
		public ZString GLAccountDesc => GLMovementDetails.GLAccountDesc;
		public ZString BranchCode => GLMovementDetails.BranchCode;
		public ZString DepartmentCode => GLMovementDetails.DepartmentCode;
		public ZDecimal Amount => GLMovementDetails.Amount;
		public ZDate PostDate => GLMovementDetails.PostDate;
		public virtual ZString PostPeriod => GLMovementDetails.PostPeriod;
		public ZString Basis => GLMovementDetails.Basis;
		public ZDecimal OSAmount => GLMovementDetails.OSAmount;
		ZString CurrencyCode => GLMovementDetails.CurrencyCode;
		public RefCurrency Currency => Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, CurrencyCode);
		ZString LocalCurrencyCode => GlbCompany.CurrentCompany.LocalCurrency.RX_Code;
		public RefCurrency LocalCurrency => Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, LocalCurrencyCode);
		public virtual ZString AlternateAccountNum => ZString.Empty;
		public virtual ZString AlternateAccountDesc => ZString.Empty;
		public virtual ZBool IsMissingPeriodForReportingBook => false;
	}
}
