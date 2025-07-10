using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR106;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR106DataProvider
	{
		public IGOVCBR106MessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR106MessageData();

			result.ImportDeclarationNumber = response.Declaration.Id.Value;
			result.ResultType = response.Status.NameCode.Value;
			result.CustomsDepartment = response.Authenticator?.Contact?.DepartmentName?.Value ?? ZString.Empty;
			result.CustomsPersonName = response.Authenticator?.Name?.Value ?? ZString.Empty;
			result.CustomsPersonPhoneNumber = response.Authenticator?.Communication?.Id?.Value ?? ZString.Empty;

			if (DateTime.TryParseExact(response.Status.EffectiveDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.ApprovalDate = new ZDate(dt1);
			}
			if (DateTime.TryParseExact(response.Declaration.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt2))
			{
				result.DeclarationDate = new ZDate(dt2);
			}
			result.ContentDescription = response.Status.Description?.Value ?? ZString.Empty;

			return result;
		}
	}
}
