using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043CSealProvider
	{
		public CC043CSealProvider(SealType04 seal)
		{
			sealType = Argument.NotNull(seal, nameof(seal));
		}
		readonly SealType04 sealType;

		public ZShort SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCached, () => ZShort.ParseSafe(sealType.SequenceNumber, ZShort.Zero));
		CachedValue<ZShort> sequenceNumberCached;

		public ZString Identifier => sealType.Identifier ?? ZString.Empty;
	}
}
