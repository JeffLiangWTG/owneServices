using System.Collections.ObjectModel;
using CargoWise.Customs.FR.MessageDefinitions.DeltaG2.Response.Export;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	interface IResponseDeltaDExportDataProvider : IResponseDataProvider
	{
		ZString mrn { get; }
		Collection<TLiquidationArt> Liquidation { get; }
	}
}
