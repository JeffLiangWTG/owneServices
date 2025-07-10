using System.Collections.ObjectModel;
using CargoWise.Customs.FR.MessageDefinitions.DeltaG1.Response.Export;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	interface IResponseDeltaCExportDataProvider : IResponseDataProvider
	{
		ZString mrnecs { get; }
		Collection<TReponseArticle> Liquidation { get; }
	}
}
