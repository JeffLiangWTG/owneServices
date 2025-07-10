using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using EntrySubStyleList = Enterprise.Customs.ES.Business.Declaration.EntrySubStyleList;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class GoodsNotificationAESSendMessageWrapper : AESCommonSendMessageWrapper, IGoodsNotificationAESMessageDataProvider
{
	public GoodsNotificationAESSendMessageWrapper(CusEntryHeader entryHeader, ICertificateProvider certificate) : base(entryHeader, certificate)
	{
		entryInstruction = entryHeader.EntryInstruction;
	}
	readonly CusEntryInstruction entryInstruction;

	public IGoodsNotificationAESExportOperationLRN ExportOperation => exportOperation ?? (exportOperation = new GoodsNotificationAESExportOperationLRNWrapper(entryHeader));
	GoodsNotificationAESExportOperationLRNWrapper exportOperation;

	public ZString CustomOfficeOfPresentation => GetOfficeOfPresentation(entryInstruction.CEI_SubStyle == EntrySubStyleList.Codes.C);

	public ZString CustomOfficeOfExport => OfficeOfExport;

	public IPartyIdProviderWithContactPerson Declarant => declarant ?? (declarant = AESCommonDeclarantWrapperWithContactPerson.New(declaration));
	AESCommonDeclarantWrapperWithContactPerson declarant;

	public ICommonRepresentativeWithContactPerson Representative => representative ?? (representative = CommonRepresentativeWrapperWithContactPerson.New(declaration));
	CommonRepresentativeWrapperWithContactPerson representative;

	public IGoodsNotificationAESGoodsShipment GoodsShipment => goodsShipment ?? (goodsShipment = new GoodsNotificationAESGoodsShipmentWrapper(entryHeader));
	GoodsNotificationAESGoodsShipmentWrapper goodsShipment;
}
