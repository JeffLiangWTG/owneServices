using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionLineSubAccountCollection : AccTransactionLineSubAccountCollection<TransactionLineSubAccount, DependentTransactionLine>, ISupportSubAccountCollection
	{
		public TransactionLineSubAccountCollection(DependentTransactionLine master) : base(master)
		{
		}

		protected override string FkColumnName => AccTransactionLineSubAccountSchema.AL1_AL.Name;

		protected override bool AllowNewCore => false;

		protected override bool AllowSort => false;

		protected override BusinessObject AddNewCore()
		{
			var businessObject = base.AddNewCore();
			UpdateSubAccountCacheKey();
			return businessObject;
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			UpdateSubAccountCacheKey();
		}

		string FirstSubAccountCacheKey => FormattableString.Invariant($"FirstSubAccount-{SubAccountCacheKey}");

		string SecondSubAccountCacheKey => FormattableString.Invariant($"SecondSubAccount-{SubAccountCacheKey}");

		public TransactionLineSubAccount FirstSubAccount
		{
			get
			{
				return Factory.GetCachedValue(FirstSubAccountCacheKey,
				() =>
				{
					return Master.SubAccounts.Cast<TransactionLineSubAccount>().OrderBy(x => x.AL1_Calc_Sequence).FirstOrDefault();
				});
			}
		}

		public TransactionLineSubAccount SecondSubAccount
		{
			get
			{
				return Factory.GetCachedValue(SecondSubAccountCacheKey,
				() =>
				{
					return Master.SubAccounts.Cast<TransactionLineSubAccount>().OrderBy(x => x.AL1_Calc_Sequence).ElementAtOrDefault(1);
				});
			}
		}

		public void UpdateSubAccountCacheKey()
		{
			Factory.ClearCachedValue<TransactionLineSubAccount>(FirstSubAccountCacheKey);
			Factory.ClearCachedValue<TransactionLineSubAccount>(SecondSubAccountCacheKey);
			fSubAccountCacheKey = ZGuid.Empty;
		}

		ZGuid SubAccountCacheKey
		{
			get
			{
				if (fSubAccountCacheKey.IsEmpty)
				{
					fSubAccountCacheKey = ZGuid.NewZGuid();
				}
				return fSubAccountCacheKey;
			}
		}
		ZGuid fSubAccountCacheKey;

		#region ISupportSubAccountCollection Implementation

		public ZString SortPropertyName => TransactionLineSubAccount.Schema.AL1_Calc_Sequence;

		ISupportSubAccount ISupportSubAccountCollection.AddNew() => AddNew();

		public IEnumerable<ISupportSubAccount> SubAccountElements => this.Cast<TransactionLineSubAccount>();

		#endregion
	}
}
