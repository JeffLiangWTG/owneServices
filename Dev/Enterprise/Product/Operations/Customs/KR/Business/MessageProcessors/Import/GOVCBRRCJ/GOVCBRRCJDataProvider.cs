using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRRCJ;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBRRCJDataProvider
	{
		public IGOVCBRRCJMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBRRCJMessageData();

			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.NoticeDateTime = dt;
			}
			result.SuspendedNumber = response.Id.Value;
			result.ImportDeclarationNumber = response.Declaration.Id.Value;
			result.OriginalDeclarationNumber = response.Declaration.AdditionalDocument.Id?.Value ?? ZString.Empty;
			result.SuspendedType = response.Declaration.TypeCode.Value;
			result.SuspendedReason = response.Declaration.AdditionalInformation?.Content?.Value ?? ZString.Empty;
			if (DateTime.TryParseExact(response.Declaration.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt2))
			{
				result.DeclarationDate = new ZDate(dt2);
			}

			if (DateTime.TryParseExact(response.Declaration.AdditionalDocument.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt3))
			{
				result.SuspendedDate = new ZDate(dt3);
			}
			result.SuspendedCode = response.Declaration.ReasonCode.Value;
			result.SuspendedSolutionCode = response.Declaration.AdditionalInformation?.StatementCode?.Value ?? ZString.Empty;
			result.DeclarantCompanyName = response.Declaration.Submitter.Contact.Name.Value;
			result.ImportCompanyName = response.Declaration.Payer.Name.Value;
			result.ImportRepresentativeName = response.Declaration.Payer.Contact.Name.Value;
			result.DeclarationOffice = response.Declaration.DeclarationOfficeId.Value + response.Declaration.SubsequentDeclarationOfficeId.Value;
			result.CustomsManagerName = response.Declaration.Authenticator.Name.Value;
			result.CustomsManagerPhoneNumber = response.Declaration.Authenticator.Communication?.Id?.Value ?? ZString.Empty;
			result.CustomsPersonFaxNumber = response.Declaration.Authenticator.Contact?.Communication?.Id?.Value ?? ZString.Empty;

			var consignmentList = new List<IConsignment>();
			foreach (var security in response.Declaration.Consignment)
			{
				var consignment = new Consignment();

				consignment.EntryLineNo = new ZInt(security.SequenceNumeric.ToString());
				consignment.HSCode = security.Commodity.Classification.Id.Value;
				consignment.InvoiceDescription = security.Commodity.CargoDescription?.Value ?? ZString.Empty;
				consignment.BrandName = security.Commodity.Description?.Value ?? ZString.Empty;

				consignmentList.Add(consignment);

				if (consignmentList.Count >= 11)
				{
					break;
				}
			}
			result.Consignment = consignmentList.ToArray();

			return result;
		}
	}
}
