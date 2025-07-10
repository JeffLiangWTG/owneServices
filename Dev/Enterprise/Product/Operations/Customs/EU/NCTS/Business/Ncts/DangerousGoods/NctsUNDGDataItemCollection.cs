using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsUNDGDataItemCollection : UNDGDataItemCollection
	{
		public NctsUNDGDataItemCollection(NctsDepartureCargoDesc master) : base(master)
		{
			var header = master.Header;
			if (header.IsRuleActive(x => x.IsRuleTR0027Active))
			{
				var maxUNDGsAllowed = 99;
				this.EnableMaxCountValidationWithMessageError(maxUNDGsAllowed, warnAtHalfway: false, Res.GetString("0221C586-EBD5-458C-AEC5-785BC80087CD", "[{0}] The maximum number of {1} Dangerous Goods Lines has been exceeded.", ValidationRuleCodeConstants.TR0027, maxUNDGsAllowed));
			}

			if (header.IsRuleActive(x => x.IsRuleE1406Active) && header.IsInPhase5TransitionPeriod)
			{
				var validationRuleConfiguration = header.Configuration.ValidationRuleConfiguration;
				this.EnableMaxCountValidationWithMessageError(1, false, validationRuleConfiguration.Messages.E1406Message);
			}
		}

		public new NctsUNDGDataItem this[int i] => (NctsUNDGDataItem)base[i];

		public new NctsUNDGDataItem AddNew() => (NctsUNDGDataItem)base.AddNew();

		protected override UNDGDataItemStandAloneCollection GetNewStandaloneCollection()
		{
			return new NctsUNDGDataItemStandAloneCollection(Factory, typeof(NctsUNDGDataItem));
		}

		public IReadOnlyCollection<string> UniqueUNNumbers => Factory.GetValue(ref uniqueUNNumbersCached, () => GetUniqueUNNumbers());
		CachedProperty<HashSet<string>> uniqueUNNumbersCached;

		HashSet<string> GetUniqueUNNumbers() => this.Where(x => x.Substance != null).Select(x => (string)x.Substance.DG_UNNO).ToHashSet();

		public class NctsUNDGDataItemStandAloneCollection : UNDGDataItemStandAloneCollection
		{
			public NctsUNDGDataItemStandAloneCollection(BusinessObjectFactory factory, Type type)
				: base(factory, type)
			{
			}

			public new NctsUNDGDataItem this[int index] => (NctsUNDGDataItem)base[index];

			public new NctsUNDGDataItem AddNew() => (NctsUNDGDataItem)base.AddNew();
		}
	}
}
