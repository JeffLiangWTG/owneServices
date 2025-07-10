using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC060CTypeOfControlProvider
	{
		public CC060CTypeOfControlProvider(TypeOfControlsType typeOfControlsType)
		{
			this.typeOfControlsType = Argument.NotNull(typeOfControlsType, nameof(typeOfControlsType));
		}
		readonly TypeOfControlsType typeOfControlsType;

		public ZInt SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCached, () => int.TryParse(typeOfControlsType.SequenceNumber, out var tryParseIntResult) ? tryParseIntResult : 0);
		CachedValue<int> sequenceNumberCached;

		public ZString Type => typeOfControlsType.Type;

		public ZString Text => typeOfControlsType.Text;
	}
}
