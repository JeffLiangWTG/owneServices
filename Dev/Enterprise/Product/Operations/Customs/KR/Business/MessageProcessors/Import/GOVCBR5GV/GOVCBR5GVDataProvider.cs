using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5GV;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5GVDataProvider
	{
		public GOVCBR5GVMessageData GetMessageData(BusinessObjectFactory factory, TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5GVMessageData(factory);

			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.NoticeDate = new ZDate(dt);
			}
			result.ImportDeclarationNumber = response.Declaration.PreviousDocument.Id.Value;
			result.ComplementReasonCode = response.Declaration.TransactionNatureCode.Value;
			result.ComplementReasonName = response.Declaration.Note.Value;
			if (DateTime.TryParseExact(response.Declaration.AdditionalInformation.LimitDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.ComplementByDate = new ZDate(dt1);
			}
			if (DateTime.TryParseExact(response.Declaration.PreviousDocument.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt2))
			{
				result.DocumentNoticeDate = new ZDate(dt2);
			}
			result.CustomsManagerName = response.Authenticator.Name.Value;
			result.CustomsManagerPhoneNumber = response.Authenticator.Communication.Id.Value;
			result.ComplementDescription = response.Declaration.Reason.Value;
			result.ComplementNumber = response.Declaration.Id.Value;
			result.DeclarationOffice = MessageFunctions.GetCustomsOfficeAndDivision(factory, response.Declaration.DeclarationOfficeId.Value);
			result.PrimaryOfficialName = response.Authenticator.Contact?.PrimaryOfficial?.Value ?? ZString.Empty;

			if (response.Declaration.GoodsShipmentSpecified)
			{
				GOVCBR5GVLineMessageData lineMessageData = null;
				foreach (var goodsShipment in response.Declaration.GoodsShipment)
				{
					if (lineMessageData == null)
					{
						lineMessageData = result.Lines.AddNew();
						lineMessageData.FirstLineNo = GetLineNumber(goodsShipment);
						lineMessageData.FirstLineDataItemID = GetDataItemID(goodsShipment);
					}
					else
					{
						lineMessageData.SecondLineNo = GetLineNumber(goodsShipment);
						lineMessageData.SecondLineDataItemID = GetDataItemID(goodsShipment);
						lineMessageData = null;
					}
				}
			}

			if (response.Declaration.AdditionalDocumentSpecified)
			{
				var documentName = new ZStringBuilder();
				var issuingPartyName = new ZStringBuilder();

				foreach (var document in response.Declaration.AdditionalDocument)
				{
					documentName.Append(document.Name.Value);
					issuingPartyName.Append(document.IssuingPartyName.Value);
				}

				result.DocumentName = documentName.ToStringWithNewLineBetweenAppends();
				result.IssuingPartyName = issuingPartyName.ToStringWithNewLineBetweenAppends();
			}
			return result;
		}

		static ZShort GetLineNumber(ResponseDeclarationGoodsShipment goodsShipment)
		{
			return ZShort.ParseSafe(goodsShipment.SequenceNumeric.ToString(), ZShort.Zero);
		}

		static ZString GetDataItemID(ResponseDeclarationGoodsShipment goodsShipment)
		{
			return goodsShipment.AdditionalInformation.Pointer.TagId.Value;
		}
	}
}
