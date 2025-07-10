using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G3.G3PresV1Sal;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class G3DeclarationMessagePrettyFormatter : G3CommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public G3DeclarationMessagePrettyFormatter(G3PresV1Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}

		readonly G3PresV1Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();
			messageDetails.Append(AcceptedDeclarationText);

			AppendAcceptedDeclarationDetails(messageDetails);
			AppendMastersConsignmentDetails(messageDetails);

			return messageDetails.ToString();
		}

		public ZString CreateMessageDetailsRejected()
		{
			var messageDetails = new StringBuilder();
			messageDetails.Append(RejectedText);

			AppendRejectedDeclarationDetails(messageDetails);
			AppendListOfErrors(response, messageDetails);

			return messageDetails.ToString();
		}

		void AppendAcceptedDeclarationDetails(StringBuilder messageDetails)
		{
			var declarationDate = response?.Accepted?.Header?.DeclarationDate;
			var presentationDate = response?.Accepted?.Header?.PresentationDate;
			if (!string.IsNullOrEmpty(declarationDate)
				|| !string.IsNullOrEmpty(presentationDate))
			{
				messageDetails.Append(blankLine);
				AppendDataInNewTableIfNotEmpty(messageDetails, DeclarationDateTimeText, declarationDate);
				AppendDataInNewTableIfNotEmpty(messageDetails, PresentationDateTimeText, presentationDate);
			}

			var lrn = response?.Accepted?.Header?.Lrn;
			var mrn = response?.Accepted?.Mrn;
			var csv = response?.Accepted?.Csv;
			var releaseCode = response?.Accepted?.ReleaseCode;
			var effPresentationDate = response?.Accepted?.EffPresentationDate;
			if (!string.IsNullOrEmpty(lrn)
				|| !string.IsNullOrEmpty(mrn)
				|| !string.IsNullOrEmpty(csv)
				|| !string.IsNullOrEmpty(releaseCode)
				|| !string.IsNullOrEmpty(effPresentationDate))
			{
				messageDetails.Append(blankLine);
				AppendDataInNewTableIfNotEmpty(messageDetails, LrnText, lrn);
				AppendDataInNewTableIfNotEmpty(messageDetails, MrnText, mrn);
				AppendDataInNewTableIfNotEmpty(messageDetails, CsvIdText, csv);
				messageDetails.Append(blankLine);
				AppendDataInNewTableIfNotEmpty(messageDetails, ReleaseCodeText, releaseCode);
				AppendDataInNewTableIfNotEmpty(messageDetails, ReleaseCodeEffDateText, effPresentationDate);
			}
		}

		void AppendMastersConsignmentDetails(StringBuilder messageDetails)
		{
			messageDetails.Append(blankLine);
			messageDetails.Append(MastersConsignmentText);
			messageDetails.Append(blankLine);

			var masterConsignment = response.Accepted.Header.MasterConsignment.First();
			var previousDocument = masterConsignment.PreviousDocument.FirstOrDefault();
			var masterTransportDocument = masterConsignment.TransportDocument;

			if (previousDocument != null)
			{
				AppendDataInNewTableIfNotEmpty(messageDetails, PreviousDocumentTypeText, previousDocument.PrevDocType + ": " + previousDocument.PrevDocRefNum);
			}

			if (masterTransportDocument != null)
			{
				AppendDataInNewTableIfNotEmpty(messageDetails, TransportDocumentTypeText, masterTransportDocument.TransDocType + ": " + masterTransportDocument.TransDocRefNum);
			}

			AppendDataInNewTableIfNotEmpty(messageDetails, ReceptacleText, masterConsignment.Receptacle);
			AppendDataInNewTableIfNotEmpty(messageDetails, ReleaseCodeText, masterConsignment.ReleaseCode);

			var houseConsignments = masterConsignment.HouseConsignment.Take(20);
			foreach (var houseConsignment in houseConsignments)
			{
				var transportDocument = houseConsignment.TransportDocument;
				messageDetails.Append(blankLine);
				messageDetails.Append(HouseConsignmentText);
				messageDetails.Append(blankLine);

				if (transportDocument != null)
				{
					AppendDataInNewTableIfNotEmpty(messageDetails, TransportDocumentText, transportDocument.TransDocType + ": " + transportDocument.TransDocRefNum);
				}

				AppendDataInNewTableIfNotEmpty(messageDetails, ReleaseCodeText, houseConsignment.ReleaseCode);
			}

			if (masterConsignment.HouseConsignment.Count > 20)
			{
				messageDetails.Append(blankLine);
				messageDetails.Append(HouseConsignmentNoteText);
			}
		}

		void AppendRejectedDeclarationDetails(StringBuilder messageDetails)
		{
			if (!string.IsNullOrEmpty(response?.Rejected.Lrn))
			{
				messageDetails.Append(blankLine);
				AppendDataInNewTableIfNotEmpty(messageDetails, LrnText, response.Rejected.Lrn);
			}
		}

		string DeclarationDateTimeText => ResString.GetMultilingualString("D44CC34E-D3BE-46AF-A7EF-689F97E89F57", "Declaration Date Time");
		string PresentationDateTimeText => ResString.GetMultilingualString("537C3350-9B44-419E-8C93-C063065AE4CA", "Presentation Date Time");
		string ReleaseCodeText => ResString.GetMultilingualString("C27C46C7-69D4-434B-B907-A6B8EFB48B5D", "Release Code");
		string ReleaseCodeEffDateText => ResString.GetMultilingualString("C93B4F89-F26D-4AD9-BE4B-BB2916AA290F", "Release Code Effective Date Time");
		string MastersConsignmentText => GetStrongText(ResString.GetMultilingualString("FCA90F37-9D53-42FD-BFA0-E35BFA0ECDDF", "Master Consignment"));
		string HouseConsignmentText => GetStrongText(ResString.GetMultilingualString("B948FFC3-8D9D-406D-B32A-20C502CABC6D", "House Consignment"));
		string PreviousDocumentTypeText => ResString.GetMultilingualString("AC841DBA-4ED0-4BA2-94ED-E2166FA0D8B4", "Previous Document Type");
		string TransportDocumentTypeText => ResString.GetMultilingualString("D9B4E621-F69D-4DAB-8FB3-E574F17AE9C4", "Transport Document Type");
		string ReceptacleText => ResString.GetMultilingualString("B49576D5-74C0-4434-A987-B23CAEE8A0E1", "Receptacle");
		string TransportDocumentText => ResString.GetMultilingualString("7151AEF2-3F97-47D1-BEB6-73D05D20CD9A", "Transport Document");
		string HouseConsignmentNoteText => ResString.GetMultilingualString("8DF2F8D5-B4E3-4A7D-8259-DE6446219034", "Note: only displaying details of first 20 House Consignments. Refer to 'Text' tab for details of all House Consignments.");
	}
}
