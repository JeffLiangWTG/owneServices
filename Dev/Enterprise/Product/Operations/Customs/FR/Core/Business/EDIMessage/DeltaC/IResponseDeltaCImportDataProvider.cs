using System.Collections.ObjectModel;
using CargoWise.Customs.FR.MessageDefinitions.DeltaG1.Response.Import;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	interface IResponseDeltaCImportDataProvider : IResponseDataProvider
	{
		Collection<TReponseArticle> Liquidation { get; }
	}
}
