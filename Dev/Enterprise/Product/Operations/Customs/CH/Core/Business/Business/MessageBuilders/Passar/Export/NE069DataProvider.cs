using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("active code")]
public class NE069DataProvider : BasePassarExportDeclarationDataProvider, INE069
{
	public NE069DataProvider(ExportDeclarationMessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	ExportDeclarationMessageSendingObject SendingObject => (ExportDeclarationMessageSendingObject)base.sendingObject;

	public IJustification Justification => justification ??= JustificationDataProvider.New(SendingObject);
	IJustification justification;
}
