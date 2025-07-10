using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IT.GUI;

sealed class EntryUniqueTransactionIdentifierGridContextMenuItemComponent : UniqueTransactionIdentifierGridContextMenuItemComponent<CusEntryHeader>
{
	public EntryUniqueTransactionIdentifierGridContextMenuItemComponent(ZGrid grid) : base(grid)
	{
	}

	protected override IUniqueTransactionIdentifierRequestContext GetMessageRequestContext(GlbCertificateProvider certificateProvider, ZString uniqueTransactionIdentifier, ZString messageNum)
	{
		return new EntryHeaderUniqueTransactionIdentifierRequestContext(Parent, new GlbCertificateProvider(), uniqueTransactionIdentifier, messageNum);
	}

	protected override void AddMessageToBusinessObject(ITEDIMessage ediMessage)
	{
		Parent.Messages.Add(ediMessage);
	}
}
