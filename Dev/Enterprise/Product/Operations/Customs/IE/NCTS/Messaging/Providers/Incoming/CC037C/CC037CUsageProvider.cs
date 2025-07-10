using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC037CUsageProvider
	{
		public CC037CUsageProvider(UsageType usageType)
		{
			this.usageType = Argument.NotNull(usageType, nameof(usageType));
		}
		readonly UsageType usageType;

		public ZInt SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCached, () => int.TryParse(usageType.SequenceNumber, out var tryParseIntResult) ? tryParseIntResult : 0);
		CachedValue<int> sequenceNumberCached;

		public ZString MRN => usageType.Mrn;

		public ZDecimal CoveredAmount => usageType.CoveredAmount;

		public ZString Currency => usageType.Currency;

		public ZDate LockDate => new ZDate(usageType.LockDate);

		public ZDateTime ArrivalDateAndTime => (usageType.ArrivalDateAndTime).ConvertToZDateTime();

		public ZDate ReleaseDate => new ZDate(usageType.ReleaseDate);
	}
}
