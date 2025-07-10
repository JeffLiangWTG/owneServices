using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.trader_ctypes;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class TR060CTypeOfControlProvider
	{
		public TR060CTypeOfControlProvider(TypeOfControlsType typeOfControls)
		{
			this.typeOfControlsType = Argument.NotNull(typeOfControls, nameof(typeOfControls));
		}
		readonly TypeOfControlsType typeOfControlsType;

		public ZInt SequenceNumber => CachedValueHelper.GetValue(ref sequenceNumberCached, () => int.TryParse(typeOfControlsType.SequenceNumber, out var tryParseIntResult) ? tryParseIntResult : 0);
		CachedValue<int> sequenceNumberCached;

		public ZString Type => typeOfControlsType.Type;

		public ZString Text => typeOfControlsType.Text;
	}
}
