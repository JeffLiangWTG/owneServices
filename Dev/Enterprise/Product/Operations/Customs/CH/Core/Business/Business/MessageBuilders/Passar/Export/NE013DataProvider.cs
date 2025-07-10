using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;

namespace Enterprise.Customs.CH.Business;

public class NE013DataProvider : BasePassarExportDeclarationDataProvider, INE013
{
	public NE013DataProvider(ExportDeclarationMessageSendingObject sendingObject) : base(sendingObject)
	{
	}

	ExportDeclarationMessageSendingObject SendingObject => (ExportDeclarationMessageSendingObject)base.sendingObject;

	public IJustification Justification => justification ??= JustificationDataProvider.New(SendingObject);
	IJustification justification;

	public IReadOnlyCollection<IOverrideConfirmation> OverrideConfirmations => null;
}
