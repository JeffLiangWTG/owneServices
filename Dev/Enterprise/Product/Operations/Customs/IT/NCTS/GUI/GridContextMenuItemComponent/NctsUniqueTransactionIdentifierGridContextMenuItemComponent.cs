using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml.UniqueTransactionIdentifier;
using Enterprise.Customs.IT.GUI;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed class NctsUniqueTransactionIdentifierGridContextMenuItemComponent : UniqueTransactionIdentifierGridContextMenuItemComponent<NctsDepartureMovementHeader>
{
	public NctsUniqueTransactionIdentifierGridContextMenuItemComponent(ZGrid grid) : base(grid)
	{
	}

	protected override IUniqueTransactionIdentifierRequestContext GetMessageRequestContext(GlbCertificateProvider certificateProvider, ZString uniqueTransactionIdentifier, ZString messageNum)
	{
		return new NctsUniqueTransactionIdentifierRequestContext(Parent.Header, new GlbCertificateProvider(), uniqueTransactionIdentifier, messageNum);
	}

	protected override void AddMessageToBusinessObject(ITEDIMessage ediMessage)
	{
		Parent.Messages.Add(ediMessage);
	}
}
