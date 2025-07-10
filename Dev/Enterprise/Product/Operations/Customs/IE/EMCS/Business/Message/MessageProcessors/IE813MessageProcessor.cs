using System;
using System.Linq;
using System.Text;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.IE.EMCS.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class IE813MessageProcessor : EMCSMessageProcessor<IIE813>
	{
		public IE813MessageProcessor(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("48682222-12a4-4313-a62a-67b39cfc5fc1", "EMCS IE813 Message Processor");

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EMCSInboundEDIMessage message, IIE813 provider)
		{
			if (message.EM_LinkedObject is EMCSJobDeclaration emcsDeclaration)
			{
				emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.CHG;
				message.EM_Status = EDIMessage.Status.ProcessedOK;
				linkedEMCSDeclaration = emcsDeclaration;

				if (emcsDeclaration.IsConsignee)
				{
					UpdateDeclarationSequenceNumberIfNecessary(provider, emcsDeclaration);
					UpdateDeclarationFromMessage(provider, emcsDeclaration);
				}

				if (emcsDeclaration.IsConsignor)
				{
					emcsDeclaration.JE_MessageStatus = EDIMessage.Status.Received;
					emcsDeclaration.SequenceNumber = provider.UpdateEad.SequenceNumber;
				}

				SendEmailNotification(message, Res.GetString("2c0ade6c-613f-4f5b-81e3-c09cb073ec76", "EMCS Change of Destination"), false, provider, null, GetEmailBody);
			}
		}

		protected override ZString ReplaceExtraXMLElementIfNeeded(ZString text)
		{
			return text.Replace(":UpdateEad>", ":UpdateEadEsad>");
		}

		void UpdateDeclarationSequenceNumberIfNecessary(IIE813 provider, EMCSJobDeclaration emcsDeclaration)
		{
			var newConsigneeTraderId = provider.Consignee?.TraderId ?? ZString.Empty;
			var newConsigneeCountry = provider.Consignee?.Country ?? string.Empty;
			var importer = emcsDeclaration.ImporterDocumentaryAddress.Organisation;
			if (newConsigneeTraderId.IsEmpty()
				|| (importer?.CustomsCodes.GetOrgCusCodesForCodeAndCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, newConsigneeCountry)?.Any(x => x.OK_CustomsRegNo == newConsigneeTraderId) ?? false))
			{
				emcsDeclaration.SequenceNumber = provider.UpdateEad.SequenceNumber;
			}
		}

		void UpdateDeclarationFromMessage(IIE813 provider, EMCSJobDeclaration emcsDeclaration)
		{
			var journeyTime = provider.JourneyTime;
			if (!journeyTime.IsEmpty)
			{
				emcsDeclaration.ZG_JourneyTime = journeyTime;
			}

			var transportModeCode = provider.TransportModeCode;
			if (!transportModeCode.IsEmpty)
			{
				var cwTransportMode = new TransportModeTranslator().TranslateToCargoWiseCode(transportModeCode);
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

			emcsDeclaration.EADNumber = provider.UpdateEad.AdministrativeReferenceCode;

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

		string GetEmailBody(EMCSJobDeclaration declaration, IEMCSInboundProvider dataProvider, IEMCSEvent iEvent)
		{
			var provider = (IIE813)dataProvider;
			var newDestinationCode = provider.NewDestinationCode;
			var emailBody = new StringBuilder();
			emailBody.Append(Res.GetString("eec58d7c-1a7a-4acb-967a-1090568bc75c", "Your EMCS Declaration for Job {0} received a notification of a changed destination. For details please follow the link to the Job.", declaration.JE_DeclarationReference));
			emailBody.Append("<br />");
			emailBody.Append("<br />");
			emailBody.Append(Res.GetString("9389b698-6680-45bf-a544-92de445dc55e", "ARC: {0}", provider.AdministrativeReferenceCode));
			emailBody.Append("<br />");
			emailBody.Append("<br />");
			emailBody.Append(Res.GetString("91c43410-eb91-4210-b6ac-4a1fef4799fb", "New Destination Code: {0}", newDestinationCode + " " + declaration.Factory.GetCachedValue<EMCSDestinationTypeList>().GetDescriptionFromCode(newDestinationCode)));

			return emailBody.ToString();
		}
	}
}
