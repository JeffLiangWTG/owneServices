using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.TemporaryStorage.Business;

public class G5TemporaryStorageMessageSendingObject : EU.Business.CusTempStorage.TemporaryStorageMessageSendingObject
{
	public G5TemporaryStorageMessageSendingObject(TemporaryStorageHeader header) : base(header)
	{
		SetDefaultMessageType();
	}

	public new TemporaryStorageHeader Header => (TemporaryStorageHeader)base.Header;

	public new sealed class Schema : EU.Business.CusTempStorage.TemporaryStorageMessageSendingObject.Schema
	{
		public const string LRN = "LRN";
		public const string MRN = "MRN";
		public const string MessageSubType = "MessageSubType";
	}

	[CargoWiseOne.ResourceStrings.ResourceStringData("ES.TemporaryStorage.Business.G5TemporaryStorageMessageSendingObject|LRN", ShortCaption = "LRN", Caption = "LRN")]
	public ZString LRN => Header.LRN;

	public ZPropertyInfo LRNInfo => GetZPropertyInfo(Schema.LRN);

	[CargoWiseOne.ResourceStrings.ResourceStringData("ES.TemporaryStorage.Business.G5TemporaryStorageMessageSendingObject|MRN", ShortCaption = "MRN", Caption = "MRN")]
	public ZString MRN => Header.MRN;

	public ZPropertyInfo MRNInfo => GetZPropertyInfo(Schema.MRN);

	[CargoWiseOne.ResourceStrings.ResourceStringData("ES.TemporaryStorage.Business.G5TemporaryStorageMessageSendingObject|MessageSubType", ShortCaption = "Msg. Sub Type", Caption = "Message Sub Type")]
	public ZString MessageSubType => DeclarationMessageSubTypeList.Codes.OriginalDeclaration;

	public ZPropertyInfo MessageSubTypeInfo => GetZPropertyInfo(Schema.MessageSubType);

	[CargoWiseOne.ResourceStrings.ResourceStringData("ES.TemporaryStorage.Business.G5TemporaryStorageMessageSendingObject|EntryStatus", ShortCaption = "Cus. Status", Caption = "Customs Status")]
	public override ZString EntryStatus => base.EntryStatus;

	[CargoWiseOne.ResourceStrings.ResourceStringData("ES.TemporaryStorage.Business.G5TemporaryStorageMessageSendingObject|MessageType", ShortCaption = "Msg. Type", Caption = "Message Type")]
	[List(nameof(MessageTypesList))]
	[ReadOnlyMember(nameof(MessageType_ReadOnly))]
	public override ZString MessageType { get => base.MessageType; set => base.MessageType = value; }

	bool MessageType_ReadOnly => !ShouldSend
								|| Header.AMA_MessageType == G5MessageTypeCodeList.Codes.G5v1Reception
								|| (Header.AMA_MessageType == G5MessageTypeCodeList.Codes.G5v1Expedition && Header.CustomsStatus != EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance);

	void SetDefaultMessageType()
	{
		MessageType = Header.AMA_MessageType == G5MessageTypeCodeList.Codes.G5v1Reception
											? DeclarationMessageTypeList.Codes.G5v1Reception
											: Header.CustomsStatus != EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance
																? DeclarationMessageTypeList.Codes.G5v1Expedition
																: ZString.Empty;
	}

	#region Lookup Lists

	public CodeDescriptionPairList MessageTypesList
	{
		get
		{
			var customsStatus = Header.CustomsStatus;
			var messageMode = Header.AMA_MessageType;

			return Header.Factory.GetCachedValue("ES.TemporaryStorage.Business.G5TemporaryStorageMessageSendingObject.MessageTypesList_" + customsStatus + "_" + messageMode, () =>
			{
				var list = new CodeDescriptionPairList();
				if (messageMode == G5MessageTypeCodeList.Codes.G5v1Expedition && customsStatus == EU.Business.UniversalReferenceConstants.PNTS.CustomsStatus.Clearance)
				{
					list.AddPair(DeclarationMessageTypeList.Codes.G5v1ExpeditionAmendment, DeclarationMessageTypeList.Descriptions.G5v1ExpeditionAmendment);
					list.AddPair(DeclarationMessageTypeList.Codes.G5v1ExpeditionCancellation, DeclarationMessageTypeList.Descriptions.G5v1ExpeditionCancellation);
				}
				else if (messageMode == G5MessageTypeCodeList.Codes.G5v1Expedition)
				{
					list.AddPair(DeclarationMessageTypeList.Codes.G5v1Expedition, DeclarationMessageTypeList.Descriptions.G5v1Expedition);
				}
				else if (messageMode == G5MessageTypeCodeList.Codes.G5v1Reception)
				{
					list.AddPair(DeclarationMessageTypeList.Codes.G5v1Reception, DeclarationMessageTypeList.Descriptions.G5v1Reception);
				}
				return list;
			});
		}
	}

	#endregion
}
