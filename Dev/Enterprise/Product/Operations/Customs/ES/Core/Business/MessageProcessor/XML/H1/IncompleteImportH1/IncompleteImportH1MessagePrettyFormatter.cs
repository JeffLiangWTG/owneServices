using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.H1.PDI400V1Sal;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.Business;

public class IncompleteImportH1MessagePrettyFormatter : ImportH1CommonMessagePrettyFormatter<Pdi400V1Sal>
{
	public IncompleteImportH1MessagePrettyFormatter(Pdi400V1Sal response, CusEntryHeader entryHeader) : base(response)
	{
		Argument.NotNull(entryHeader, nameof(entryHeader));
		countryCode = entryHeader.CountryCode;
		factory = entryHeader.Factory;
	}
	readonly ZString countryCode;
	readonly BusinessObjectFactory factory;

	public override ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
	{
		var messageDetails = new StringBuilder();

		messageDetails.Append(AcceptedDeclarationText);

		var ackResponseData = response.Cc415R?.Ack;
		if (ackResponseData != null)
		{
			messageDetails.Append(blankLine);
			AppendDescriptionData(messageDetails, ackResponseData.OperationRegistered);
			AppendDomainData(messageDetails, ackResponseData.ImportOperation?.Domain);
			AppendAcceptanceDate(messageDetails, response.Cc415R.Cc426R);
			AppendCustomsRegistrationNumber(messageDetails, response);
			messageDetails.Append(blankLine);
			AppendCsvElectronicDeclarationDataIfNotEmpty(messageDetails, ackResponseData.EdeclarationCsvId);
			AppendRequiredCertificates(messageDetails, ackResponseData.GoodsItem, factory, countryCode);
			AppendNotifications(messageDetails, ackResponseData.Notification);
		}

		return messageDetails.ToString();
	}

	void AppendCustomsRegistrationNumber(StringBuilder messageDetails, IMRNField mrnField)
	{
		if (string.IsNullOrEmpty(mrnField.MRN)) { return; }

		AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, CustomsRegistrationNumberText, mrnField.MRN);
	}

	string CustomsRegistrationNumberText => ResString.GetMultilingualString("D426389F-B7EB-472E-81D4-400A03D816BC", "Customs Registration Number:");
}
