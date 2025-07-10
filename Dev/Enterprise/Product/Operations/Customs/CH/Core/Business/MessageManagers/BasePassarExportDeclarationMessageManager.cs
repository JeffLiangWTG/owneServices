using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public abstract class BasePassarExportDeclarationMessageManager : DeclarationMessageManager
{
	protected BasePassarExportDeclarationMessageManager(DeclarationMessageSendingObject messageSender) : base(messageSender)
	{
	}

	protected override BusinessObject GetBusinessObjectInNewFactory(BusinessObject bizo)
	{
		if (bizo is ExportDeclarationMessageSendingObject messageSendingObject)
		{
			var entryHeader = base.GetBusinessObjectInNewFactory(messageSendingObject.Header) as CusEntryHeader;
			return new ExportDeclarationMessageSendingObject(messageSendingObject.SendingObjectParent, entryHeader);
		}
		return base.GetBusinessObjectInNewFactory(bizo);
	}
}
