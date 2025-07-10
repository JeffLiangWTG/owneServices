using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.CH.Business;

[CodeAlive("Will be used in MessageManagerFactory (WI00714806)")]
public class NE130DataProvider : BasePassarDeclarationMessageDataProvider, INE130
{
	public NE130DataProvider(ExportDeclarationMessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	public IExportOperation ExportOperation => exportOperation ??= ExportOperationDataProvider.New(entryHeader);
	IExportOperation exportOperation;

	public ITrader TraderAtDeparture => traderAtDeparture ??= TraderDataProvider.New(entryHeader.Declaration);
	ITrader traderAtDeparture;

	public ISelectionAndTransit EdecSAT => edecSAT ??= EdecSelectionAndTransitDataProvider.New(entryHeader);
	ISelectionAndTransit edecSAT;
}
