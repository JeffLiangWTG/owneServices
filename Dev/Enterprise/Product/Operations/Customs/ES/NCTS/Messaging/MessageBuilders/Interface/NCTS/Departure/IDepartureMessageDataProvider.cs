using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface IDepartureMessageDataProvider : INctsBaseDepartureMessageProvider
	{
		#region Fields For CST

		ZString CustomsProcedureCategory3 { get; }

		#endregion

		#region Fields For LOC

		ZString CountryOfDeparture { get; }
		IReadOnlyCollection<INctsCustomsTransitOfficeProvider> CustomsOfficesOfTransit { get; }
		INctsCustomsTransitOfficeProvider CustomsOfficeOfDestination { get; }

		#endregion

		#region Fields For RFF

		IReadOnlyCollection<IGuaranteeNumber> GuaranteeNumbers { get; }

		#endregion

		#region Fields For TDT

		ITransportMediumInfoCommon TransitTransportMedium { get; }

		#endregion

		#region Fields For NAD

		IPartyProvider Principal { get; }
		IPartyProvider Representative { get; }

		#endregion

		#region Fields For Goods

		IReadOnlyCollection<IDepartureLine> Lines { get; }

		#endregion

		#region Fields For CNT

		ZString NationalSimplificationIndicator { get; }

		#endregion
	}

	public interface IGuaranteeNumber
	{
		ZString Type { get; }
		ZString AccessCode { get; }
	}

	public interface IDepartureLine : INctsBaseDepartureLineMessageProvider
	{
		ZString CountryOfDeparture { get; }

		ZString CountryOfDestination { get; }

		#region Fields For CST

		ZString GoodsCustomsProcedureCategory5 { get; }

		#endregion

		#region Fields For LOC

		ZString GoodsCountryOfOrigin { get; }
		ZString GoodsCountryOfDestination { get; }

		#endregion

		#region Fields For MEA

		ZDecimal NetWeightInKG { get; }
		ZDecimal FiscalUnitsNumber { get; }
		ZString FiscalUnitsQualifier { get; }

		#endregion

		#region Fields For NAD

		IPartyProvider GoodsConsignor { get; }
		IPartyProvider GoodsConsignee { get; }
		IPartyProvider SecurityGoodsConsignor { get; }
		IPartyProvider SecurityGoodsConsignee { get; }

		#endregion

		#region Fields For PAC
		IDepartureInternalPackagesInfo InternalPackages { get; }
		IVehiclePackagesInfoCommon VehiclePackages { get; }

		#endregion

		#region Fields For MOA

		ZDecimal TotalGoodValueInEuros { get; }

		#endregion

		#region Fields For DOC

		IReadOnlyCollection<IDepartureDocuments> Documents { get; }

		#endregion

		#region Fields For TOD

		ZString GoodsTransportMethodOfPayment { get; }
		ZString GoodsCountryCode { get; }

		#endregion
	}

	public interface IDepartureInternalPackagesInfo : IInternalPackagesInfoCommon
	{
		ZBool IsVehiclePackage { get; }
	}

	public interface IDepartureDocuments : IDocumentsCommon
	{
		ZString Source { get; }
	}
}
