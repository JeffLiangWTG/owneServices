using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.Messaging
{
	public interface IDepartureGoodsItem : ICommonGoodsItem
	{
		ZString CountryOfDispatchExportCode { get; }
		ZString CountryOfDestinationCode { get; }
		ZString TransportChargesMethodOfPayment { get; }
		ZString CommercialReferenceNumber { get; }
		ZString UNDangerousGoodsCode { get; }
		ZDecimal BillValue { get; }
		ZInt DeclarationItemNumber { get; }
		ZShort BillSequenceNumber { get; }
		IReadOnlyCollection<IPreviousAdministrativeReference> PreviousAdministrativeReferences { get; }
		IReadOnlyCollection<IStatement> SpecialMentions { get; }
		ITrader Consignor { get; }
		ITrader Consignee { get; }
		ITrader ConsignorSecurity { get; }
		ITrader ConsigneeSecurity { get; }
	}
}
