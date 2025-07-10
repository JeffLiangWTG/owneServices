using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.EMCS.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class ED801ConsignorMessageProcessor : EmcsMessageProcessor<EmcsInboundEDIMessage<IED801>, IED801>
	{
		public ED801ConsignorMessageProcessor(LoggingInformation logger)
		: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("DD275ECE-E9B6-4D09-AAC2-B569853B2594", "EMCS ED801 Consignor Message Processor");

		protected override BusinessObject GetLinkedObject(EmcsInboundEDIMessage<IED801> message)
		{
			BusinessObject result = null;
			var provider = message.DataProvider;
			if (provider != null)
			{
				result = GetDeclarationFromLocalReference(message, provider.LocalReferenceNumber, provider.MessageGroup);
			}
			return result;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, EmcsInboundEDIMessage<IED801> message)
		{
			var provider = message.DataProvider;
			messageGroup = provider.MessageGroup;

			var emcsDeclaration = (EMCSJobDeclaration)message.EM_LinkedObject;
			emcsDeclaration.JE_MessageStatus = EDIMessage.Status.Received;
			emcsDeclaration.JE_EntryStatus = EU.EMCS.Business.EntryStatusList.Codes.REG;
			message.EM_Status = EDIMessage.Status.ProcessedOK;

			UpdateInvoiceLines(emcsDeclaration);
			AddEADNumber(emcsDeclaration, provider);

			GenerateHtmlEmailAndSendToOriginalOrGroup(factory
				, emcsDeclaration
				, Res.GetString("C7EEC891-5912-4EDD-ACE8-AE18C7F45A86", "EMCS e-AD registered")
				, GetEmailBodyHeader(emcsDeclaration, provider)
				, false
				, message.Branch
				, emcsDeclaration
				, () => emcsDeclaration.Messages.LastOutgoingMessage);

			message.SetLogbookLocalReferenceNumber(provider.LocalReferenceNumber);
			message.SetLogbookRegistrationNumber(provider.ExciseMovement.AdministrativeReferenceCode);
		}

		ZString GetEmailBodyHeader(EMCSJobDeclaration emcsDeclaration, IED801 dataProvider)
		{
			var htmlBody = new StringBuilder();

			htmlBody.Append(Res.GetString("80B8E38F-D5B5-4CD4-A068-970BF3E15268", "Your EMCS Declaration for Job {0} has been registered. For details please follow the Link to the Job.", emcsDeclaration.JE_DeclarationReference));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			htmlBody.Append(Res.GetString("946BA292-ECC9-4D4D-81F3-D3F8C0FBF4CB", "ARC: {0}", dataProvider.ExciseMovement.AdministrativeReferenceCode));

			return htmlBody.ToString();
		}

		void AddEADNumber(EMCSJobDeclaration emcsDeclaration, IED801 provider)
		{
			var entryNumber = emcsDeclaration.LoadCusEntryNumber(true);
			entryNumber.CE_EntryNum = provider.ExciseMovement.AdministrativeReferenceCode;
			entryNumber.CE_EntryLineReference = provider.ExciseMovement.SequenceNumber;
			entryNumber.CE_IssueDate = provider.DateAndTimeOfValidationOfEadEsad;
		}

		void UpdateInvoiceLines(EMCSJobDeclaration emcsDeclaration)
		{
			foreach (var line in emcsDeclaration.InvoiceLines.Cast<EU.EMCS.Business.EMCSJobComInvoiceLine>().ToArray())
			{
				line.ZG_DeclaredValue = line.JI_CustomsQuantity;
			}
		}
	}
}
