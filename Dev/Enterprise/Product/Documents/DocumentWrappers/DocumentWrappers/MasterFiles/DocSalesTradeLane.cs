using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocSalesTradeLane : DocumentWrapper
	{
		DocSalesTradeLane(ViewDocTradeLane tradeLane, BusinessObjectFactory factoryToWrap)
			: base(tradeLane, factoryToWrap)
		{
		}

		public static DocSalesTradeLane New(ViewDocTradeLane tradeLane, BusinessObjectFactory factoryToWrap)
		{
			return (tradeLane != null) ? new DocSalesTradeLane(tradeLane, factoryToWrap) : null;
		}

		ViewDocTradeLane ViewTradeLane
		{
			get { return (ViewDocTradeLane)WrappedObject; }
		}

		public override string ToString()
		{
			return Summary;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		public ZString Summary => $"{ProductName} {OriginCode} {DestinationCode} {TradeMode} {TradeType} {TradingStatus}";

		#region Sales Trade Lane Properties

		public ZString ProductName => ViewTradeLane.ProductName;

		public ZString OriginCode => !ViewTradeLane.WarehouseUnloco.IsEmpty ? ViewTradeLane.WarehouseUnloco : ViewTradeLane.OriginCode;

		public ZString DestinationCode => ViewTradeLane.DestinationCode;

		public ZString TradeMode => !ViewTradeLane.Service.IsEmpty ? ViewTradeLane.Service : ViewTradeLane.TradeMode;

		public ZString TradeType => ViewTradeLane.TradeType;

		public ZBool IsTraded => ViewTradeLane.IsTraded;

		public ZString TradingStatus => ViewTradeLane.IsTraded ? Res.GetString("087B11F3-4779-4D83-AA2C-35B0D75F20A9", "Traded") : Res.GetString("529D08AB-C1C8-4622-9C7A-2A9D4B8A7D5C", "Estimate");

		public ZDateTime ActivityDate => ViewTradeLane.ActivityDate;

		public ZDecimal Weight => ViewTradeLane.Weight;

		public ZString WeightUQ => ViewTradeLane.WeightUQ;

		public ZDecimal Volume => ViewTradeLane.Volume;

		public ZString VolumeUQ => ViewTradeLane.VolumeUQ;

		public ZDecimal TEU => ViewTradeLane.TEU;

		public ZString Currency => ViewTradeLane.Currency;

		public ZDecimal EstimatedRevenue => ViewTradeLane.EstimatedRevenue;

		#endregion
	}
}
