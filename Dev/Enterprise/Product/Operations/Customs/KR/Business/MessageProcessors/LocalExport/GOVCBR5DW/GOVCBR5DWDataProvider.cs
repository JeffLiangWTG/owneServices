using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5DW;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5DWDataProvider
	{
		public IGOVCBR5DWMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5DWMessageData();

			result.AcceptNumber = response.Declaration.FunctionalReferenceId.Value;

			if (DateTime.TryParseExact(response.Declaration.AcceptanceDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.AcceptDate = new ZDate(dt1);
			}

			result.DeclarationNumber = response.Declaration.Id.Value;
			result.DeclarantCompanyName = response.Declaration.Submitter.Name.Value;
			result.ManufacturerCompanyName = response.Declaration.Manufacturer?.Name?.Value ?? ZString.Empty;
			result.ImporterCompanyName = response.Declaration.Agent.Name.Value;

			if (DateTime.TryParseExact(response.Control.InspectionEndDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt2))
			{
				result.ProductConfirmationDate = new ZDate(dt2);
			}

			result.ProductConfirmationManagerID = response.Certifier?.Id?.Value ?? ZString.Empty;
			result.ProductConfirmationManagerName = response.Certifier?.Name?.Value ?? ZString.Empty;
			result.TotalEntryLineCount = (ZInt)response.Declaration.LoadingListQuantity.Value;
			result.TotalPackages = (ZInt)response.Declaration.TotalPackageQuantity.Value;
			result.TotalDeclarationAmount = response.Declaration.InvoiceAmount.Value;
			result.TotalGrossWeight = response.Declaration.TotalGrossMassMeasure.Value;
			result.CustomsOfficeContent = response.Declaration.AdditionalInformation?.Content?.Value ?? ZString.Empty;

			return result;
		}
	}
}
