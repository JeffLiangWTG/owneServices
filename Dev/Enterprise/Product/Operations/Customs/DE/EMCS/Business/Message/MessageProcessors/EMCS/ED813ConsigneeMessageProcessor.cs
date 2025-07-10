using System.Linq;
using System.Text;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED813ConsigneeMessageProcessor : EmcsMessageProcessor<EmcsInboundEDIMessage<IED813>, IED813>
	{
		public ED813ConsigneeMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("A09D5888-F627-4883-BDA5-EE33436D1B4F", "EMCS ED813 Consignee Message Processor");

		protected override BusinessObject GetLinkedObject(EmcsInboundEDIMessage<IED813> message)
		{
			BusinessObject result = null;
			var provider = message.DataProvider;
			if (provider != null)
			{
				var updateEad = provider.UpdateEadEsad;
				var previousSequenceNumber = (ZShort.ParseSafe(updateEad.SequenceNumber, 1) - 1).ToString();
				result = GetDeclarationFromEADNumber(message, updateEad.AdministrativeReferenceCode, provider.MessageGroup, previousSequenceNumber);
			}
			return result;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EmcsInboundEDIMessage<IED813> message)
		{
			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			var provider = message.DataProvider;
			messageGroup = provider.MessageGroup;
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.CHG;
			UpdateDeclarationSequenceNumberIfNecessary(provider, emcsDeclaration);
			message.EM_Status = EDIMessage.Status.ProcessedOK;

			UpdateDeclarationFromMessage(provider, emcsDeclaration);

			GenerateHtmlEmailAndSendToOriginalOrGroup(factory
				, emcsDeclaration
				, Res.GetString("3C0C61A5-C323-497C-ABFD-E63CF103CEC1", "EMCS Change of Destination")
				, GetEmailBody(emcsDeclaration, provider)
				, false
				, message.Branch
				, emcsDeclaration
				, () => emcsDeclaration.Messages.LastOutgoingMessage);

			message.SetLogbookRegistrationNumber(provider.UpdateEadEsad.AdministrativeReferenceCode);
		}

		void UpdateDeclarationSequenceNumberIfNecessary(IED813 provider, EMCSJobDeclaration emcsDeclaration)
		{
			var newConsigneeTraderId = provider.Consignee?.TraderId ?? ZString.Empty;
			var newConsigneeCountry = provider.Consignee?.Country ?? string.Empty;
			var importer = emcsDeclaration.ImporterDocumentaryAddress.Organisation;
			if (newConsigneeTraderId.IsEmpty()
				|| (importer?.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, newConsigneeCountry)?.Any(x => x.OK_CustomsRegNo == newConsigneeTraderId) ?? false))
			{
				emcsDeclaration.SequenceNumber = provider.UpdateEadEsad.SequenceNumber;
			}
		}

		void UpdateDeclarationFromMessage(IED813 provider, EMCSJobDeclaration emcsDeclaration)
		{
			var journeyTime = provider.JourneyTime;
			if (!journeyTime.IsEmpty)
			{
				emcsDeclaration.ZG_JourneyTime = journeyTime;
			}

			var transportModeCode = provider.TransportModeCode;
			if (!transportModeCode.IsEmpty)
			{
				var cwTransportMode = new EU.EMCS.Business.TransportModeTranslator().TranslateToCargoWiseCode(transportModeCode);
				emcsDeclaration.JE_TransportMode = cwTransportMode;
				emcsDeclaration.SpecialInstructions = cwTransportMode.IsEmpty ? provider.ComplementaryInfo : ZString.Empty;
			}

			var transportArrangement = provider.TransportArrangement;
			if (!transportArrangement.IsEmpty)
			{
				emcsDeclaration.ZG_TransportArrangement = transportArrangement;
			}

			var invoiceNumber = provider.InvoiceNumber;
			if (!invoiceNumber.IsEmpty)
			{
				emcsDeclaration.InvoiceNumber = invoiceNumber;
			}

			var invoiceDate = provider.InvoiceDate;
			if (!invoiceDate.IsEmpty)
			{
				emcsDeclaration.InvoiceDate = invoiceDate;
			}

			emcsDeclaration.JE_MessageSubType = provider.DestinationTypeCode;

			var guarantorTypeCode = provider.GuarantorTypeCode;
			if (!guarantorTypeCode.IsEmpty)
			{
				emcsDeclaration.ZG_GuarantorType = guarantorTypeCode;
				emcsDeclaration.CreateOrUpdateGuarantor(provider.Guarantor);
			}

			emcsDeclaration.EADNumber = provider.UpdateEadEsad.AdministrativeReferenceCode;

			var deliveryPlace = provider.DeliveryPlace;
			if (deliveryPlace != null)
			{
				emcsDeclaration.CreateOrUpdateDeliveryPlace(deliveryPlace);
			}

			if (!provider.TransportArrangement.IsEmpty)
			{
				emcsDeclaration.CreateOrUpdateCarrierAgent(provider.NewTransportArranger);
			}

			var newTransporter = provider.NewTransporter;
			if (newTransporter != null)
			{
				emcsDeclaration.CreateOrUpdateTransporter(newTransporter);
			}

			if (provider.TransportDetails.Any())
			{
				emcsDeclaration.CreateOrUpdateTransportDetails(provider.TransportDetails);
			}
		}

		ZString GetEmailBody(EMCSJobDeclaration declaration, IED813 dataProvider)
		{
			var newDestinationCode = dataProvider.NewDestinationCode;
			var emailBody = new StringBuilder();
			emailBody.Append(Res.GetString("D9651EA9-947F-48E3-B04D-5AA107BDBD0F", "Your EMCS Declaration for Job {0} received a notification of a changed destination. For details please follow the link to the Job.", declaration.JE_DeclarationReference));
			emailBody.Append("<br />");
			emailBody.Append("<br />");
			emailBody.Append(Res.GetString("CE3602FA-0608-42FD-94A6-3C58250A9796", "ARC: {0}", dataProvider.UpdateEadEsad.AdministrativeReferenceCode));
			emailBody.Append("<br />");
			emailBody.Append("<br />");
			emailBody.Append(Res.GetString("D5B95158-97BD-4968-B11E-3AAEC103DB76", "New Destination Code: {0}", newDestinationCode + " " + declaration.Factory.GetCachedValue<EU.EMCS.Business.EMCSDestinationTypeList>().GetDescriptionFromCode(newDestinationCode)));
			return emailBody.ToString();
		}
	}
}
