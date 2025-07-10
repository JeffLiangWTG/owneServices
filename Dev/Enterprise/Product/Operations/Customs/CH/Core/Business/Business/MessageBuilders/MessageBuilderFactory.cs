using CargoWise.Customs.CH.MessageContracts.Chartera.Outgoing;
using CargoWise.Customs.CH.MessageContracts.Ebd.Version0_2;
using CargoWise.Customs.CH.MessageContracts.Edec.Bordereau.Version1_0;
using CargoWise.Customs.CH.MessageContracts.Edec.ECom.Version1_0;
using CargoWise.Customs.CH.MessageContracts.Edec.Evv.Version3_0;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations.Version4_0;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.CH.Business;

public static class MessageBuilderFactory
{
	public static IXmlMessageBuilder NewMessageBuilder(IMessageSendingObject objectToSend)
	{
		return objectToSend switch
		{
			ImportDeclarationMessageSendingObject sendingImportObject => new EdecMessageBuilder(new EdecIMPDeclarationDataProvider(sendingImportObject)),

			ExportDeclarationMessageSendingObject sendingExportObject => sendingExportObject.MessageType.ToString() switch
			{
				PassarMessageTypeList.Codes.NC016 => new NC016_v1MessageBuilder(new NC016DataProvider(sendingExportObject)),
				PassarMessageTypeList.Codes.NE013 when FuncsHelper.IsCHNE015V3Active => new NE013_v3MessageBuilder(new NE013DataProvider(sendingExportObject)),
				PassarMessageTypeList.Codes.NE013 => new NE013_v4MessageBuilder(new NE013DataProvider(sendingExportObject)),
				PassarMessageTypeList.Codes.NE014 => new NE014_v3MessageBuilder(new NE014DataProvider(sendingExportObject)),
				PassarMessageTypeList.Codes.NE015 when FuncsHelper.IsCHNE015V3Active => new NE015_v3MessageBuilder(new NE015DataProvider(sendingExportObject)),
				PassarMessageTypeList.Codes.NE015 => new NE015_v4MessageBuilder(new NE015DataProvider(sendingExportObject)),
				PassarMessageTypeList.Codes.NE069 => new NE069_v1MessageBuilder(new NE069DataProvider(sendingExportObject)),
				PassarMessageTypeList.Codes.NE130 => new NE130_v1MessageBuilder(new NE130DataProvider(sendingExportObject)),
				PassarMessageTypeList.Codes.NC123 => new NC123_v1MessageBuilder(new NC123DataProvider(sendingExportObject)),
				_ => null
			},

			SupportingDocSendingObject supportingDocSendingObject => new EbdMessageBuilder(new EbdDocumentImportDataProvider(supportingDocSendingObject)),

			EComplaintMessageSendingObject complaintSendingObject => new EdecComplaintRequestBuilder(new EdecComplaintRequestDataProvider(complaintSendingObject)),

			EvvRequestSendingObject evvSendingObject => new EdecEvvRequestMessageBuilder(new EdecEvvRequestDataProvider(evvSendingObject)),

			CharteraOutputDocumentSearchSendingObject charteraOutputDocumentSearchSendingObject => new DocumentSearchRequestV1MessageBuilder(charteraOutputDocumentSearchSendingObject),

			BordereauListRequestSendingObject bordereauListRequestSendingObject => new EdecBordereauListRequestMessageBuilder(new BordereauListRequestDataProvider(bordereauListRequestSendingObject)),

			BordereauRequestSendingObject bordereauRequestSendingObject => new EdecBordereauRequestMessageBuilder(new BordereauRequestDataProvider(bordereauRequestSendingObject)),

			_ => null
		};
	}
}
