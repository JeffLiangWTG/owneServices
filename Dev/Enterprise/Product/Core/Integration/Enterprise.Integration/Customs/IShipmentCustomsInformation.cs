using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public interface IShipmentCustomsInformation
		{
			ZString CustomsCargoStatus { get; }
			ZString CustomsMessageStatus { get; }
			ZString CRLStatus { get; }
			ZString SEBillStatus { get; }
			ZString HLDOrEXMStatus { get; }
			ZString ENSStatus { get; }
			ZString EXPStatus { get; }
			ZString ITStatus { get; }

			ZString ISFBillNumber { get; }
			ZString ISFBillStatus { get; }
			ZString ISFBillStatusDescription { get; }

			ZString AFRBillStatus { get; }
			ZString AFRBillStatusDescription { get; }
			ZString EntryStatusDescription { get; }

			ZString ACICargoStatus { get; }
			ZString ACIMessageStatus { get; }

			ZDecimal DestinationGoodsValue { get; }
			ZString DestinationCurrencyCode { get; }
			ZDecimal DestinationExchangeRate { get; }

			ZPropertyInfo DestinationExchangeRateInfo { get; }
		}
	}
}
