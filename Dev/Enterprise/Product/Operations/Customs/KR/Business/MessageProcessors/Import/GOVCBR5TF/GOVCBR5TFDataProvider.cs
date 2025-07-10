using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5TF;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5TFDataProvider
	{
		public IGOVCBR5TFMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);
			var result = new GOVCBR5TFMessageData();

			result.ImportDeclarationNumber = response.Declaration.Id.Value;
			if (DateTime.TryParseExact(response.Declaration.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.DeclarationDate = dt;
			}
			if (DateTime.TryParseExact(response.Declaration.PreviousDocument?.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.EntryReleaseDate = dt1;
			}
			result.DeclarantCompanyName = response.Declaration.Submitter.Name.Value;
			result.ImportCompanyName = response.Declaration.Importer?.Name.Value ?? ZString.Empty;
			return result;
		}
	}
}
