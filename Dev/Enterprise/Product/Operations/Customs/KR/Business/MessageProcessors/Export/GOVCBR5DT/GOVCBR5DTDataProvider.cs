using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5DT;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5DTDataProvider
	{
		public IGOVCBR5DTMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5DTMessageData();
			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.NoticeDateTime = dt;
			}
			result.ExportDeclarationNumber = response.Declaration.Id.Value;
			result.AmendSequence = new ZInt(response.Declaration.VersionId?.Value);
			if (DateTime.TryParseExact(response.Status.EffectiveDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.DecisionDate = (ZDate)dt1;
			}
			result.NoticeType = response.Status.NameCode.Value;
			result.FaultParty = response.Declaration.ReasonCode?.Value ?? ZString.Empty;
			result.FaultPartyChangeReason = response.Declaration.Reason?.Value ?? ZString.Empty;
			result.NoticeDescription = response.Status.Description?.Value ?? ZString.Empty;
			result.ApprovalNo = response.Declaration.AdditionalDocument?.Id?.Value ?? ZString.Empty;
			result.CustomsPersonID = response.Authenticator?.Id?.Value ?? ZString.Empty;
			result.CustomsPersonName = response.Authenticator?.Name?.Value ?? ZString.Empty;
			result.CustomsOfficeAndDivision = response.Declaration.DeclarationOfficeId.Value;
			return result;
		}
	}
}
