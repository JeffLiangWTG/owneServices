using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageDefinitions.ZHub;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business
{
	public class DEICustomsAcknowledgementProcessor : IInboundMessageCreator
	{
		public DEICustomsAcknowledgementProcessor()
		{
			logger = new BatchProcessor.LoggingInformation();
		}
		readonly BatchProcessor.LoggingInformation logger;

		public void CreateMessagesForInterchange(EDIInterchange interchange)
		{
			var status = EDIInterchange.Status.Error;

			var bodyText = interchange.EI_BodyText;
			if (bodyText.IsEmpty)
			{
				interchange.Logs.AddNew(Events.ErrorReport, "NO DE CUSTOMS DATA");
			}
			else
			{
				using (var textReader = interchange.GetEI_BodyTextReader())
				{
					var deCustomsData = new DECustomsDataProvider<CUSINF>(typeof(CUSINF).GetEmbeddedResourcePath(), textReader);
					if (deCustomsData != null)
					{
						var cusinf = deCustomsData.Message;
						var referencedMessageIdentifier = cusinf.ReferencedMessageIdentifier;
						var appCode = cusinf.AppCode;
						var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, appCode);
						query.AddToFilter(EDIMessageSchema.EM_MessageNum, referencedMessageIdentifier);
						query.AddToFilter(EDIMessageSchema.EM_GB, interchange.Company.Branches.GetPKs());
						query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
						var relatedMessage = interchange.Factory.Load<EDIMessage>(query).OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
						if (relatedMessage != null)
						{
							status = SetInterchangeDeliverTime(deCustomsData.LogbookTime, cusinf, relatedMessage.Interchange, interchange);
							if (status != EDIInterchange.Status.Error)
							{
								var linkedObject = relatedMessage.EM_LinkedObject;
								if (linkedObject != null)
								{
									SubscribeAttachedDocuments(new DocumentLinking(logger), linkedObject, deCustomsData.AttachedDocuments);
									UpdateMessageStatus(linkedObject, relatedMessage, cusinf);
									DeleteCusReconEntry(linkedObject, cusinf);
								}
							}
						}
						else
						{
							interchange.Logs.AddNew(Events.ErrorReport, $"Interchange processing failed because related EDIMessage couldn't be located. Message Number: {referencedMessageIdentifier}. Application Code: {appCode}");
						}
					}
				}
			}
			interchange.EI_Status = status;
		}

		ZString SetInterchangeDeliverTime(ZDateTimeOffset logbookTime, CUSINF cusinf, EDIInterchange relatedInterchange, EDIInterchange interchange)
		{
			var status = EDIInterchange.Status.Received;
			if (!logbookTime.IsEmpty)
			{
				if (!cusinf.ReferencedMessageStatusSpecified && relatedInterchange != null)
				{
					relatedInterchange.EI_DeliveredTime = logbookTime;
				}
			}
			else
			{
				interchange.Logs.AddNew(Events.ErrorReport, FormattableString.Invariant($"Invalid LogBookTime. Interchange Number: {interchange.EI_InterchangeNum}."));
				status = EDIInterchange.Status.Error;
			}
			return status;
		}

		void SubscribeAttachedDocuments(DocumentLinking documentLinking, BusinessObject linkedObject, List<AttachedDocument> attachedDocuments)
		{
			using (linkedObject.Factory.AddDisposableService())
			{
				var documentLinkingObject = GetDocumentLinkingObject(linkedObject);
				documentLinking.Subscribe(documentLinkingObject);
				documentLinking.DoLink(attachedDocuments);
				var docManagerSupport = (IDocManagerSupport)documentLinkingObject;
				docManagerSupport.DocManagerInfoCore().Save();
			}
		}

		BusinessObject GetDocumentLinkingObject(BusinessObject linkingObject)
		{
			var result = linkingObject;
			if (linkingObject is CusTempStorageDec tempStorageDec)
			{
				result = tempStorageDec.StorageHeader;
			}
			else if (linkingObject is Integration.Customs.DE.IDepartureMovementHeader departureMovement)
			{
				result = (BusinessObject)linkingObject.Factory.Load<Integration.Customs.DE.ICusInBondHeader>(departureMovement.BM_BH);
			}
			return result;
		}

		void UpdateMessageStatus(BusinessObject linkedObject, EDIMessage relatedMessage, CUSINF cusinf)
		{
			const string rejectedMessageStatus = nameof(CUSINFReferencedMessageStatus.REJ);
			if (cusinf.ReferencedMessageStatusSpecified && cusinf.ReferencedMessageStatus == CUSINFReferencedMessageStatus.REJ)
			{
				if (linkedObject is CusTempStorageDec cusTempStorageDec)
				{
					cusTempStorageDec.STH_MessageStatus = rejectedMessageStatus;
				}
				else if (linkedObject is Customs.Business.BaseJobDeclaration jobDeclaration)
				{
					jobDeclaration.JE_MessageStatus = rejectedMessageStatus;
				}
				else if (linkedObject is CusEntryHeader cusEntryHeader)
				{
					cusEntryHeader.CH_Status = rejectedMessageStatus;
					cusEntryHeader.Logs.AddNew(Events.MessageRejected, UniversalReferenceConstants.EntryStatus.ERR, ZDateTime.Now.ToOffset());
				}
				else if (linkedObject is Integration.Customs.DE.ICusInBondHeader nctsHeader)
				{
					nctsHeader.EffectiveMessageStatus = rejectedMessageStatus;
				}
				else if (linkedObject is Integration.Customs.DE.IDepartureMovementHeader departureMovement)
				{
					var header = linkedObject.Factory.Load<Integration.Customs.DE.ICusInBondHeader>(departureMovement.BM_BH);
					header.EffectiveMessageStatus = rejectedMessageStatus;
					departureMovement.DeleteGuaranteeTransactions();
				}
				else if (linkedObject is CusReconDeclaration cusReconDeclaration)
				{
					cusReconDeclaration.CRD_MessageStatus = rejectedMessageStatus;
				}
				relatedMessage.EM_Status = rejectedMessageStatus;
			}
		}

		void DeleteCusReconEntry(BusinessObject linkedObject, CUSINF cusinf)
		{
			if (linkedObject is CusEntryHeader entryHeader && cusinf.ReferencedMessageStatusSpecified && cusinf.ReferencedMessageStatus == CUSINFReferencedMessageStatus.REJ && entryHeader.CusEntryNumber == null)
			{
				entryHeader.GetCusReconEntry()?.Delete();
			}
		}
	}
}
