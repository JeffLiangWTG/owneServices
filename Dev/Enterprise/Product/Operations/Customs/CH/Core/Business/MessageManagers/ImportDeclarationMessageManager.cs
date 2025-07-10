using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class ImportDeclarationMessageManager : DeclarationMessageManager
{
	public ImportDeclarationMessageManager(DeclarationMessageSendingObject messageSender) : base(messageSender)
	{
	}

	protected override BusinessObject GetBusinessObjectInNewFactory(BusinessObject bizo)
	{
		if (bizo is DeclarationMessageSendingObject messageSendingObject)
		{
			var entryHeader = base.GetBusinessObjectInNewFactory(messageSendingObject.Header) as CusEntryHeader;
			return new ImportDeclarationMessageSendingObject(entryHeader);
		}
		return base.GetBusinessObjectInNewFactory(bizo);
	}
}
