using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRCINImportResponseMessageProcessor : ApplicationTypeMessageProcessor
	{
		public FRCINImportResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"France Customs CIN Message Response";
		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.FRCustomsMessage;
		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeList.Codes.CIN };

		protected override void ProcessMessageCore(EDIMessage message)
		{
			if (message is FREDIMessage frEDIMessage)
			{
				var dataProvider = frEDIMessage.MessageDataObject as ICINResponseDataProvider;
				if (dataProvider != null)
				{
					var dec = FindDeclaration(frEDIMessage.Factory, dataProvider);

					if (dec != null)
					{
						// TODO - Apply correct business logic to declaration based on response content
						if (dataProvider.Success)
						{
							dec.STH_MessageStatus = CusTempStorageDec.DeclarationStatusForCorrectionMessage;
						}

						dec.Messages.Add(frEDIMessage);
						message.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;//Received;
					}
				}
			}
		}

		CusTempStorageDec FindDeclaration(BusinessObjectFactory factory, ICINResponseDataProvider dataProvider)
		{
			var originalMessage = factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageNum, dataProvider.MessageID)
																.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.FRCustomsMessage)
																.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.CIN));

			if (originalMessage != null)
			{
				return factory.Load<CusTempStorageDec>(originalMessage.EM_LinkUniqueID);
			}

			return null;
		}
	}
}
