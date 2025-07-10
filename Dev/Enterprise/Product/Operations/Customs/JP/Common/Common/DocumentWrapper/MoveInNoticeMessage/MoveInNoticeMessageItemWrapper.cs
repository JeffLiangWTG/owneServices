using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common
{
	public sealed class MoveInNoticeMessageItemWrapper(CarryInDocumentItemProvider item) : DocumentEngineCore.DocWrappers.DocumentWrapper
	{
		readonly CarryInDocumentItemProvider item = Argument.NotNull(item, nameof(item));

		#region Item Fields
		public ZString I_6 => item.AWBNumber;

		public ZString I_7 => item.MoveInQuantity.FormatNumberInDocument();

		public ZString I_8 => item.TotalQuantity.FormatNumberInDocument();

		public ZString I_9 => item.MoveInWeight.FormatNumberInDocument();

		public ZString I_10 => item.PortOfLoadingIATACode;

		public ZString I_11 => item.FinalDestinationIATACode;

		public ZString I_12 => item.GoodsType;

		public ZString I_13 => item.AirCargoAgentLocation;

		public ZString I_14 => item.SpecialCargoCode;

		public ZString I_15 => item.GoodsDescription;

		public ZString I_16 => item.AirlineCode;
		#endregion
	}
}
