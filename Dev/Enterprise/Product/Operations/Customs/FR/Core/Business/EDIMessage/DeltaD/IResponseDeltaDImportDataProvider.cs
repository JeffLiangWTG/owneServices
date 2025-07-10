using System.Collections.ObjectModel;
using CargoWise.Customs.FR.MessageDefinitions.DeltaG2.Response.Import;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	interface IResponseDeltaDImportDataProvider : IResponseDataProvider
	{
		Collection<TLiquidationArt> Liquidation { get; }
	}
}
