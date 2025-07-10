using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Journal
{
	public class JournalSubAccountCollection : AccTransactionHeaderSubAccountCollection<JournalSubAccount, Journal>, ISupportSubAccountCollection
	{
		public JournalSubAccountCollection(Journal master) : base(master)
		{
		}

		protected override string FkColumnName => AccTransactionHeaderSubAccountSchema.AHS_AH.Name;

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

		#region SubAccountCacheKey

		string FirstSubAccountCacheKey => FormattableString.Invariant($"FirstSubAccount-{SubAccountCacheKey}");

		string SecondSubAccountCacheKey => FormattableString.Invariant($"SecondSubAccount-{SubAccountCacheKey}");

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

		public void UpdateSubAccountCacheKey()
		{
			Factory.ClearCachedValue<TransactionLineSubAccount>(FirstSubAccountCacheKey);
			Factory.ClearCachedValue<TransactionLineSubAccount>(SecondSubAccountCacheKey);
			fSubAccountCacheKey = ZGuid.Empty;
		}

		#endregion

		#region  First / Second SubAccount

		public JournalSubAccount FirstSubAccount
		{
			get
			{
				return Factory.GetCachedValue(FirstSubAccountCacheKey,
					() =>
					{
						return this.Cast<JournalSubAccount>().OrderBy(x => x.AHS_Calc_Sequence).FirstOrDefault();
					});
			}
		}

		public JournalSubAccount SecondSubAccount
		{
			get
			{
				return Factory.GetCachedValue(SecondSubAccountCacheKey,
					() =>
					{
						return this.Cast<JournalSubAccount>().OrderBy(x => x.AHS_Calc_Sequence).ElementAtOrDefault(1);
					});
			}
		}

		#endregion

		#region ISupportSubAccountCollection Implementation

		ZString ISupportSubAccountCollection.SortPropertyName => JournalSubAccount.Schema.AHS_Calc_Sequence;

		ISupportSubAccount ISupportSubAccountCollection.AddNew() => AddNew();

		IEnumerable<ISupportSubAccount> ISupportSubAccountCollection.SubAccountElements => this.Cast<JournalSubAccount>();

		#endregion
	}
}
