using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.TD11;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.ES.Business.Declaration.CodeDescriptionList;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.Business;

public abstract class ImportH1CommonMessagePrettyFormatter<TResponse> : CommonMessagePrettyFormatter, IMessagePrettyFormatter
	where TResponse : ICommonErrors
{
	public ImportH1CommonMessagePrettyFormatter(TResponse response)
	{
		this.response = Argument.NotNull(response, nameof(response));
	}
	protected readonly TResponse response;

	public abstract ZString CreateMessageDetailsAccepted(string extraDataFromProcessing);

	public ZString CreateMessageDetailsRejected()
	{
		var messageDetails = new StringBuilder();

		messageDetails.Append(RejectedDeclarationText);
		messageDetails.Append(ListOfErrorsText);

		if (response.FunctionalErrors != null && response.FunctionalErrors.Any())
		{
			SetMessageDetailsForRejectedMessageCommon(messageDetails, response.FunctionalErrors);
		}
		else if (response.XMLErrors != null && response.XMLErrors.Any())
		{
			SetMessageDetailsForErrorMessageCommon(messageDetails, response.XMLErrors);
		}

		return messageDetails.ToString();
	}

	protected void AppendDescriptionData(StringBuilder messageDetails, ZString descriptionCode)
	{
		if (string.IsNullOrEmpty(descriptionCode)) { return; }
		AppendDataInNewTableIfNotEmpty(messageDetails, DescriptionText, GetDescription(descriptionCode));

		ZString GetDescription(string descriptionCode)
		{
			var description = descriptionCode switch
			{
				ImportH1OperationRegisteredCodeList.Codes.AcceptedDac => ImportH1OperationRegisteredCodeList.Descriptions.AcceptedDac,
				ImportH1OperationRegisteredCodeList.Codes.DacPendingOnCciPciValidation => ImportH1OperationRegisteredCodeList.Descriptions.DacPendingOnCciPciValidation,
				ImportH1OperationRegisteredCodeList.Codes.AcceptedDpa => ImportH1OperationRegisteredCodeList.Descriptions.AcceptedDpa,
				ImportH1OperationRegisteredCodeList.Codes.DpaPendingOnCciPciValidation => ImportH1OperationRegisteredCodeList.Descriptions.DpaPendingOnCciPciValidation,
				ImportH1OperationRegisteredCodeList.Codes.AmendedDpa => ImportH1OperationRegisteredCodeList.Descriptions.AmendedDpa,
				ImportH1OperationRegisteredCodeList.Codes.DpaAmendmentPendingOnCciPciValidation => ImportH1OperationRegisteredCodeList.Descriptions.DpaAmendmentPendingOnCciPciValidation,
				ImportH1OperationRegisteredCodeList.Codes.AcceptedDacOnTimeDueToSacCompletion => ImportH1OperationRegisteredCodeList.Descriptions.AcceptedDacOnTimeDueToSacCompletion,
				ImportH1OperationRegisteredCodeList.Codes.AcceptedDacOutOfTimeDueToSacCompletion => ImportH1OperationRegisteredCodeList.Descriptions.AcceptedDacOutOfTimeDueToSacCompletion,
				ImportH1OperationRegisteredCodeList.Codes.AcceptedDacOnTimeDueToSacCompletionPendingOnCciPciValidation => ImportH1OperationRegisteredCodeList.Descriptions.AcceptedDacOnTimeDueToSacCompletionPendingOnCciPciValidation,
				ImportH1OperationRegisteredCodeList.Codes.AcceptedDacOutOfTimeDueToSacCompletionPendingOnCciPciValidation => ImportH1OperationRegisteredCodeList.Descriptions.AcceptedDacOutOfTimeDueToSacCompletionPendingOnCciPciValidation,
				ImportH1OperationRegisteredCodeList.Codes.AcceptedPdi => ImportH1OperationRegisteredCodeList.Descriptions.AcceptedPdi,
				ImportH1OperationRegisteredCodeList.Codes.AmendedPdi => ImportH1OperationRegisteredCodeList.Descriptions.AmendedPdi,
				ImportH1OperationRegisteredCodeList.Codes.AcceptedAmendedSpa => ImportH1OperationRegisteredCodeList.Descriptions.AcceptedAmendedSpa,
				ImportH1OperationRegisteredCodeList.Codes.AcceptedSac => ImportH1OperationRegisteredCodeList.Descriptions.AcceptedSac,
				ImportH1OperationRegisteredCodeList.Codes.ActivationSpaGreaterThanSac => ImportH1OperationRegisteredCodeList.Descriptions.ActivationSpaGreaterThanSac,
				_ => ZString.Empty
			};
			return descriptionCode + " - " + description;
		}
	}

	protected void AppendDomainData(StringBuilder messageDetails, ZString domainCode)
	{
		if (string.IsNullOrEmpty(domainCode)) { return; }
		AppendDataInNewTableIfNotEmpty(messageDetails, DomainText, GetDomainDescription(domainCode));

		ZString GetDomainDescription(string domainCode)
		{
			var domainDescription = domainCode switch
			{
				ImportH1DomainCodeList.Codes.NoCciNoNationalCentralized => ImportH1DomainCodeList.Descriptions.NoCciNoNationalCentralized,
				ImportH1DomainCodeList.Codes.NoCciNationalCentralized => ImportH1DomainCodeList.Descriptions.NoCciNationalCentralized,
				ImportH1DomainCodeList.Codes.Cci => ImportH1DomainCodeList.Descriptions.Cci,
				_ => ZString.Empty
			};
			return domainCode + " - " + domainDescription;
		}
	}

	protected void AppendAcceptanceDate(StringBuilder messageDetails, Cc426RTypeD cc426R)
	{
		if (cc426R == null) { return; }
		ZString acceptanceDateField = cc426R.DeclarationRegistrationDateAndTime;

		if (string.IsNullOrEmpty(acceptanceDateField)) { return; }

		ZDateTime.TryParseExact(acceptanceDateField, out var acceptanceDate, CustomsDateTimeExtension.DateTimeFormatyyyyMMddTHHmmss);
		AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, AcceptanceText, acceptanceDate.ToCustomsFormatDateStringddMMyyyyHHmmssWithDash());
	}

	protected void AppendRequiredCertificates(StringBuilder messageDetails, Collection<GoodsItemTypeD01> goodsItems, BusinessObjectFactory factory, ZString countryCode)
	{
		if (goodsItems == null || !goodsItems.Any()) { return; }

		messageDetails.Append(blankLine);
		messageDetails.Append(GetH3Text(RequiredCertificatesText));

		var tableCreator = GetNewTableCreator();
		tableCreator.WriteRow(ItemColumnText, GetStrongText(MeasureText), GetStrongText(AgencyText), GetStrongText(DocumentsText));

		foreach (var goodItem in goodsItems)
		{
			var pca = goodItem.Pca;
			if (pca == null && !pca.Any()) { return; }

			foreach (var cert in pca)
			{
				AppendItemCertificateRow(goodItem.DeclarationGoodsItemNumber, cert, tableCreator);
			}
		}
		messageDetails.Append(tableCreator.ToHtml());

		void AppendItemCertificateRow(ZString itemDepartureNumber, PcaTypeD certificate, HtmlTableCreator tableCreator)
		{
			var agencyCode = certificate.Code;
			var agencyName = certificate.Name;

			var measures = certificate.TaricMeasure;
			if (measures == null && !measures.Any()) { return; }

			foreach (var measure in measures)
			{
				var taricMeasure = measure.TaricMeasureCode;

				var documents = measure.DocumentRequired;
				if (documents == null || !documents.Any()) { return; }

				var tableCreatorCert = new HtmlTableCreator(TableInterpretation.Attributes.FullWidth) { EnableHTMLEncoding = false };
				foreach (var document in documents)
				{
					var docType = document.Type;
					var documentDescription = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, docType, countryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection, ZDateTime.Today)?.ZZD_Description;
					tableCreatorCert.WriteRow(docType + " - " + documentDescription);
				}
				tableCreator.WriteRow(itemDepartureNumber, taricMeasure, agencyCode + " " + agencyName, tableCreatorCert.ToHtml());
			}
		}
	}

	protected void AppendNotifications(StringBuilder messageDetails, Collection<NotificationTypeD> notifications)
	{
		if (notifications == null || !notifications.Any()) { return; }

		messageDetails.Append(blankLine);
		messageDetails.Append(GetH3Text(NotificationText));

		var tableCreator = GetNewTableCreator();
		tableCreator.WriteRow(ErrorCodeColumnText, GetStrongText(TextColumnText));

		foreach (var notification in notifications)
		{
			tableCreator.WriteRow(notification.Code, notification.Text);
		}
		messageDetails.Append(tableCreator.ToHtml());
	}

	string DomainText => ResString.GetMultilingualString("65535805-CBE0-48B5-B28D-0A9C564B4365", "Domain:");
	string NotificationText => ResString.GetMultilingualString("D0440639-D44A-4F81-A5F7-F9B15D457760", "Notifications");
	string TextColumnText => ResString.GetMultilingualString("30F4A3B1-5717-424A-A672-81CC8D4EEC25", "Text");
}
