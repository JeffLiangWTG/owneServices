using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ComplXAESSendMessageWrapper : AESCommonSendMessageWrapper, IComplXAESMessageDataProvider
	{
		public ComplXAESSendMessageWrapper(CusEntryHeader entryHeader, ICertificateProvider certificate) : base(entryHeader, certificate)
		{
		}

		public IComplXAESExportOperation ExportOperation => exportOperation ?? (exportOperation = new ComplXAESExportOperationWrapper(entryHeader));
		ComplXAESExportOperationWrapper exportOperation;

		public IPartyIdProvider Declarant => declarant ?? (declarant = AESCommonDeclarantWrapper.New(declaration));
		AESCommonDeclarantWrapper declarant;

		public IPartyIdProvider Representative => representative ?? (representative = PartyIdWrapper.New(GetRepresentativeOrgAddress()));
		PartyIdWrapper representative;

		public IComplXAESGoodsShipment GoodsShipment => goodsShipment ?? (goodsShipment = new ComplXAESGoodsShipmentWrapper(entryHeader));
		ComplXAESGoodsShipmentWrapper goodsShipment;

		OrgAddress GetRepresentativeOrgAddress()
		{
			OrgAddress orgAddress = null;
			if (declaration != null && (declaration.JE_DeclarantType == ESRepresentationTypeList.Codes._2Direct || declaration.JE_DeclarantType == ESRepresentationTypeList.Codes._5IndirectATC))
			{
				orgAddress = declaration.Representative ?? declaration.DeclarantAddress;
			}
			return orgAddress;
		}
	}
}
