using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface INctsLineMessageProvider
	{
		ZInt GoodsItemNumber { get; }
		ZString GoodsCustomsProcedureCategory1 { get; }
		ZString GoodsDescription { get; }
		ZDecimal GrossWeightInKG { get; }
		IExternalPackagesInfoCommon ExternalPackages { get; }
	}
}
