using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business
{
	public interface IDEBranchCustomsApplicationTypeMessageProcessorProvider
	{
		BranchCustomsApplicationTypeMessageProcessor GetProcessor(EDIMessage message);
	}

	public abstract class DEBranchCustomsApplicationTypeMessageProcessor<TEDIMessage> : BranchCustomsApplicationTypeMessageProcessor
		where TEDIMessage : EDIMessage
	{
		readonly DocumentLinking documentLinking;

		protected TEDIMessage CurrentMessage { get; private set; }

		protected DEBranchCustomsApplicationTypeMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
			documentLinking = new DocumentLinking(logger);
		}

		protected sealed override void PreProcessMessageCore(EDIMessage baseMessage)
		{
			var message = (TEDIMessage)baseMessage;
			CurrentMessage = message;
			var status = EDIMessage.Status.PreProcessedOK;

			var messageIdentifier = GetMessageIdentifier(message);
			if (!messageIdentifier.IsEmpty)
			{
				message.EM_MessageNum = messageIdentifier;
			}
			if (ProcessedPreviously(message.Factory, messageIdentifier, message))
			{
				Logger.LogWarning(Res.GetString("{9E7BF502-C081-487D-BA12-E638D8C1943C}", "Message with message number {0} exists already.(Type:{1}, Sub:{2}, Ref:{3}); message status set to DISCARDED.", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference));
				status = EDIMessage.Status.Discarded;
			}
			else
			{
				var linkedObject = GetLinkedObject(message);
				if (linkedObject == null && MustHaveLinkedObject)
				{
					if (DelayStatusError)
					{
						if (message.EM_SystemCreateTimeUtc.AddMinutes(5) > ZDateTime.UtcNow || message.EM_HeldUntilDate.IsEmpty)
						{
							Logger.LogWarning(Res.GetString("{44550B60-CED5-43E0-B404-F59F78C9E996}", "Unable to find business object for message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message created shorter than 5 minutes ago and will be processed again.", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference));
							status = EDIMessage.Status.Queued;
							message.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(1);
						}
						else
						{
							Logger.LogWarning(Res.GetString("{5BFCB7DA-4E8C-4F50-9032-0BD908FC37A5}", "Unable to find business object for message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to ERROR.", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference));
							status = EDIMessage.Status.Error;
						}
					}
					else
					{
						Logger.LogWarning(Res.GetString("{5BFCB7DA-4E8C-4F50-9032-0BD908FC37A5}", "Unable to find business object for message (Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to ERROR.", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference));
						status = EDIMessage.Status.Error;
					}
				}
				else
				{
					message.EM_LinkedObject = linkedObject;
					var branchPK = GetCorrectBranchPK(linkedObject);
					if (branchPK.IsValid)
					{
						message.EM_GB = branchPK;
					}
				}
			}
			message.EM_Status = status;
		}

		protected void SubscribeDocumentLinking(BusinessObject businessObject)
		{
			documentLinking.Subscribe(GetDocumentLinkingObject(businessObject));
		}

		protected virtual BusinessObject GetDocumentLinkingObject(BusinessObject businessObject) => businessObject;

		protected abstract List<AttachedDocument> GetAttachedDocuments(TEDIMessage message);

		protected sealed override void ProcessMessageCore(EDIMessage baseMessage)
		{
			var message = (TEDIMessage)baseMessage;
			CurrentMessage = message;
			if (NeedAttachDocumentsToMessage)
			{
				SubscribeDocumentLinking(message);
			}

			if (message.EM_Status == EDIMessage.Status.PreProcessedOK)
			{
				ProcessMessageCore(message.Factory, message);
				if (NeedAttachDocumentsToLinkedObject)
				{
					SubscribeDocumentLinking(message.EM_LinkedObject);
				}

				if (message.EM_Status == EDIMessage.Status.ProcessedOK && UpdateWarehouse && WarehouseIntegrationSupporter is { } warehouseIntegrationSupporter)
				{
					UpdateBondedWarehouseIfApplicable(warehouseIntegrationSupporter, message);
				}
			}
			var attachedDocuments = GetAttachedDocuments(message);
			documentLinking.DoLink(attachedDocuments);
			attachedDocuments.ForEach(x => x.Dispose());
		}

		protected virtual bool NeedAttachDocumentsToMessage => false;

		protected virtual bool NeedAttachDocumentsToLinkedObject => true;

		public bool UpdateWarehouse => UpdateWarehouseCore;

		protected virtual bool UpdateWarehouseCore => false;

		protected virtual IWarehouseIntegrationSupporter WarehouseIntegrationSupporter => CurrentMessage.EM_LinkedObject as IWarehouseIntegrationSupporter;

		protected abstract void ProcessMessageCore(BusinessObjectFactory factory, TEDIMessage message);

		protected virtual bool MustHaveLinkedObject => true;

		protected virtual bool DelayStatusError => false;

		protected abstract BusinessObject GetLinkedObject(TEDIMessage message);

		protected abstract ZGuid GetCorrectBranchPK(BusinessObject linkedObject);

		protected abstract ZString GetMessageIdentifier(TEDIMessage message);

		protected BusinessObject GetLinkedObjectFromOriginalMessage(BusinessObjectFactory factory, string messageIdentifier)
		{
			var originalMessage = GetOriginalMessage(factory, messageIdentifier);
			if (originalMessage == null && MustHaveLinkedObject)
			{
				Logger.LogWarning(Res.GetString("7D7F6C1E-5EDC-4C46-9227-E1D008D4BCA3", "Unable to find the original message (Number:{0}).", messageIdentifier));
			}

			var linkedObject = originalMessage?.EM_LinkedObject;
			if (originalMessage != null && linkedObject == null)
			{
				Logger.LogWarning(Res.GetString("FFD44B33-A845-4B53-84A8-92F0281274C5", "Unable to get linked object from the original message (Table:{0}, ID:{1}).", originalMessage.EM_LinkTable, originalMessage.EM_LinkUniqueID));
			}
			return linkedObject;
		}

		protected CusEntryHeader GetEntryHeaderFromMRN(BusinessObjectFactory factory, string movementReferenceNumber, string atlasReferenceNumber = null)
		{
			var references = new List<ZString> { };
			if (!string.IsNullOrWhiteSpace(movementReferenceNumber))
			{
				references.Add(movementReferenceNumber);
			}
			if (!string.IsNullOrWhiteSpace(atlasReferenceNumber))
			{
				references.Add(atlasReferenceNumber);
			}

			var cusEntryNumber = (references.Count > 0) ? CusEntryNumber.Load<CusEntryNumber>(factory, CusEntryNumberTypes.Standard.MovementReferenceNumber, references.ToArray(), Core.Constants.CountryCodes.Germany)
				.FirstOrDefault(en => en.CE_ParentTable == CusEntryHeaderSchema.Constants.TableName) : null;

			return cusEntryNumber?.Parent is CusEntryHeader cusEntryHeader && cusEntryHeader.Declaration != null && cusEntryHeader.Declaration.Branch?.GB_GC == GlbCompany.CurrentCompany.PK ? cusEntryHeader : null;
		}

		protected T GetLinkedObjectFromMRN<T>(BusinessObjectFactory factory, string movementReferenceNumber, string atlasReferenceNumber = null) where T : BusinessObject
		{
			var references = new List<ZString>();
			if (!string.IsNullOrWhiteSpace(movementReferenceNumber))
			{
				references.Add(movementReferenceNumber);
			}
			if (!string.IsNullOrWhiteSpace(atlasReferenceNumber))
			{
				references.Add(atlasReferenceNumber);
			}

			var cusEntryNumber = references.Count > 0
				? CusEntryNumber.Load<CusEntryNumber>(factory, CusEntryNumberTypes.Standard.MovementReferenceNumber, references.ToArray(), Core.Constants.CountryCodes.Germany).FirstOrDefault(x => x.CE_ParentTable == BusinessObjectFactory.GetTableNameFromType(typeof(T)))
				: null;
			return cusEntryNumber != null ? factory.Load<T>(cusEntryNumber.CE_ParentID) : null;
		}

		protected CusReconDeclaration GetCusReconDeclarationFromMRN(BusinessObjectFactory factory, ZGuid companyPK, string movementReferenceNumber, string atlasReferenceNumber = null)
		{
			CusReconDeclaration result = null;
			var references = new List<ZString>();
			if (!string.IsNullOrWhiteSpace(movementReferenceNumber))
			{
				references.Add(movementReferenceNumber);
			}
			if (!string.IsNullOrWhiteSpace(atlasReferenceNumber))
			{
				references.Add(atlasReferenceNumber);
			}
			var cusEntryNumbers = CusEntryNumber.Load<CusEntryNumber>(factory, CusEntryNumberTypes.Standard.MovementReferenceNumber, references.ToArray(), Core.Constants.CountryCodes.Germany).Where(x => x.CE_ParentTable == CusReconDeclaration.Schema.TableName);

			foreach (var cusEntryNumber in cusEntryNumbers)
			{
				var cusReconDeclaration = factory.Load<CusReconDeclaration>(cusEntryNumber.CE_ParentID);
				if (cusReconDeclaration != null && cusReconDeclaration.Branch.GB_GC == companyPK)
				{
					result = cusReconDeclaration;
					break;
				}
			}
			return result;
		}

		protected CusEntryHeader GetLinkedObjectFromOriginalMessageOrMrn(BusinessObjectFactory factory, string messageIdentifier, ZString movementReferenceNumber)
		{
			var originalMessage = GetOriginalMessage(factory, messageIdentifier);
			var linkedObject = originalMessage?.EM_LinkedObject as CusEntryHeader ?? GetEntryHeaderFromMRN(factory, movementReferenceNumber);

			if (linkedObject is null)
			{
				Logger.LogWarning(Res.GetString("96EF01EA-E913-4C6A-A45D-5556398E080A", "Unable to get linked object from the original message or MRN (Message Number:{0}, MRN:{1}).", messageIdentifier, movementReferenceNumber));
			}

			return linkedObject;
		}

		protected void GenerateHtmlEmailAndSendToOriginalOrGroup(BusinessObjectFactory factory, IRelatedJob relatedJob, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo, BusinessObject sourceBusinessObject, ZString originalMessageNumber)
		{
			GenerateHtmlEmailAndSendToOriginalOrGroup(factory, relatedJob, messageTypeInSubject, body, isFailure, branchForEmailLogo, sourceBusinessObject, () => GetOriginalMessages(factory, originalMessageNumber).FirstOrDefault());
		}

		protected void GenerateHtmlEmailAndSendToOriginalOrGroup(BusinessObjectFactory factory, IRelatedJob relatedJob, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo, BusinessObject sourceBusinessObject, Func<EDIMessage> getOriginalMessage)
		{
			GenerateHtmlEmailAndSendToOriginalOrGroup(factory, relatedJob, messageTypeInSubject, body, isFailure, branchForEmailLogo, sourceBusinessObject, () => GetEmailAddressToSendTo());
			ZString GetEmailAddressToSendTo() => GetEmailAddressToSendToFromQueuedUser(getOriginalMessage());
		}

		protected ZString GetErrorsEmailTable(IEnumerable<IERRNCKError> errors)
		{
			var result = ZString.Empty;
			if (errors.Any())
			{
				var emailTable = new HtmlTableCreator(new string[] {
					Res.GetString("432C5A1C-316F-4675-A62B-8F3D65CABFBD", "Error Code"),
					Res.GetString("656F0886-106D-4593-A4CC-92A801380F3D", "Pointer"),
					Res.GetString("846301DC-AC91-42CA-BC54-B93614CC2F75", "Text"),
					Res.GetString("864ADF7F-A167-403F-800A-B367069D5614", "Original Value")
				});
				foreach (var error in errors)
				{
					emailTable.WriteRow(new string[] { error.Code, error.Pointer, error.Text, error.OriginalValue });
				}
				result = emailTable.ToHtml();
			}
			return result;
		}

		bool ProcessedPreviously(BusinessObjectFactory factory, ZString messageIdentifier, TEDIMessage messageToProcess)
		{
			bool result = false;
			if (!messageIdentifier.IsEmpty)
			{
				var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCode);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
				query.AddToFilter(EDIMessageSchema.EM_MessageNum, messageIdentifier);
				query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.ProcessedOK);
				query.AddToFilter(EDIMessageSchema.EM_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
				query.AddToFilter(EDIMessageSchema.PK, SQLComparisonOperator.NotEqual, messageToProcess.PK);
				query.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, messageToProcess.EM_SystemCreateTimeUtc);
				const string indexName = "NR_RX__EM_MessageNum"; // there is no existing constant for this index name
				query.TableIndexHints.Add(new TableIndexHint(indexName));
				result = factory.Exists(typeof(TEDIMessage), query);
			}
			return result;
		}

		protected EDIMessage[] GetOriginalMessages(BusinessObjectFactory factory, ZString referencedMessageIdentifier)
		{
			EDIMessage[] result = null;
			if (!referencedMessageIdentifier.IsEmpty)
			{
				var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCode);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Sent);
				query.AddToFilter(EDIMessageSchema.EM_MessageNum, referencedMessageIdentifier);
				query.AddToFilter(EDIMessageSchema.EM_GB, GlbCompany.CurrentCompany.Branches.GetPKs());
				const string indexName = "NR_RX__EM_MessageNum"; // there is no existing constant for this index name
				query.TableIndexHints.Add(new TableIndexHint(indexName));
				result = factory.Load<EDIMessage>(query).OrderBy(x => x.EM_SystemCreateTimeUtc).ToArray();
			}
			return result ?? Array.Empty<EDIMessage>();
		}

		protected EDIMessage GetOriginalMessage(BusinessObjectFactory factory, ZString referencedMessageIdentifier)
		{
			return factory.GetCachedValue("Enterprise.Customs.DE.Business.DEBranchCustomsApplicationTypeMessageProcessor.GetOriginalMessage_" + referencedMessageIdentifier,
				() => referencedMessageIdentifier.IsEmpty ? null : GetOriginalMessages(factory, referencedMessageIdentifier).FirstOrDefault());
		}

		protected void AttachDocumentsToEmail(EmailDef email, IEnumerable<AttachedDocument> attachedDocuments)
		{
			attachedDocuments.ForEach(x => email.Attachments.Add(new AttachmentDef(x.FileName, AttachmentDef.StreamToByteArray(x.ImageData))));
		}

		#region Bonded Warehouse
		void UpdateBondedWarehouseIfApplicable(IWarehouseIntegrationSupporter supporter, EDIMessage message)
		{
			var entry = supporter as CusEntryHeader;

			var declarationIsImportedFromOrder = entry?.Declaration.IsOutwardOrderImported ?? false;

			if (supporter.SupportsBondedWarehousing || declarationIsImportedFromOrder)
			{
				bwhMessagePK = message.PK;

				if (entry != null)
				{
					if (declarationIsImportedFromOrder)
					{
						if (!entry.EntryNumber.IsEmpty)
						{
							entry.Factory.Saved -= UpdateBondedWarehouseAttributesOnFactorySaved;
							entry.Factory.Saved += UpdateBondedWarehouseAttributesOnFactorySaved;
						}
					}
					else if (entry.IsImport && entry.IsOutOfWarehouseWarehousing)
					{
						entry.Factory.Saved -= DeclarationOutboundSaved;
						entry.Factory.Saved += DeclarationOutboundSaved;
					}
					else if (entry.Declaration.IsWarehouseAdjustment)
					{
						entry.Factory.Saved -= UpdateBondedWarehouseAdjustmentOnFactorySaved;
						entry.Factory.Saved += UpdateBondedWarehouseAdjustmentOnFactorySaved;
					}
					else
					{
						entry.Factory.Saved -= BondedWhsMsgProcessorCreator.ProcessBondedWarehouseOnFactorySaved;
						entry.Factory.Saved += BondedWhsMsgProcessorCreator.ProcessBondedWarehouseOnFactorySaved;
					}
				}
				else
				{
					supporter.Factory.Saved -= BondedWhsMsgProcessorCreator.ProcessBondedWarehouseOnFactorySaved;
					supporter.Factory.Saved += BondedWhsMsgProcessorCreator.ProcessBondedWarehouseOnFactorySaved;
				}
			}
		}

		void DeclarationOutboundSaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				factory.Saved -= DeclarationOutboundSaved;

				var message = factory.Load<EDIMessage>(bwhMessagePK);
				var entryHeader = message.EM_LinkedObject as CusEntryHeader;
				var declaration = entryHeader?.Declaration;
				if (declaration != null)
				{
					var publishAcceptEvent = ShouldPublishWhsOutwardAcceptEvent(entryHeader);
					var result = BondedWarehousingHelper.PublishShipmentForWHSOutward(declaration, publishAcceptEvent);
					if (!result.IsEmpty)
					{
						Logger.LogWarning(result);
					}
				}
				factory.Save();
			}
		}

		void UpdateBondedWarehouseAdjustmentOnFactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				factory.Saved -= UpdateBondedWarehouseAdjustmentOnFactorySaved;

				var message = factory.Load<EDIMessage>(bwhMessagePK);
				if (message.EM_LinkedObject is CusEntryHeader cusEntryHeader)
				{
					var result = cusEntryHeader.PublishShipmentForWHSOutward(true);
					if (!result.ErrorMessage.IsEmpty)
					{
						Logger.LogWarning(result.ErrorMessage);
					}
				}
				factory.Save();
			}
		}

		void UpdateBondedWarehouseAttributesOnFactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				factory.Saved -= UpdateBondedWarehouseAttributesOnFactorySaved;

				var message = factory.Load<EDIMessage>(bwhMessagePK);
				var entryHeader = message.EM_LinkedObject as CusEntryHeader;
				if (entryHeader != null)
				{
					var order = GetWhsOrder(entryHeader);

					foreach (var line in entryHeader.AllEntryLines.Cast<CusEntryLine>())
					{
						UpdateWarehouseOrder(line, order);
					}
				}
				factory.Save();
			}

			void UpdateWarehouseOrder(CusEntryLine entryLine, IWhsOrder order)
			{
				if (UpdateWarehouseOrderForCurrentEntryLine(entryLine))
				{
					var entryHeader = entryLine.Header;

					var declaration = entryLine.Declaration;

					var invoiceLine = entryLine.RandomLine; // we have only one invoice line per entry line, RandomLine is fine here
					if (invoiceLine != null && order.WD_DocketID == invoiceLine.JI_BondedWHSOrderNumber)
					{
						var orderLineQuery = new ZQuery(WhsDocketLineSchema.WE_LineNo, invoiceLine.JI_BondedWHSOrderLineNumber)
													.AddToFilter(WhsDocketLineSchema.WE_WD, order.PK);

						var orderLine = declaration.Factory.LoadTop1<IWhsDocketLine>(orderLineQuery);
						if (orderLine != null)
						{
							var orderAttributeQuery = new ZQuery(WhsBondedWarehouseAttributeSchema.WB_ParentID, orderLine.PK);

							var orderAttribute = declaration.Factory.LoadTop1<IWhsBondedWarehouseAttribute>(orderAttributeQuery);
							if (orderAttribute != null)
							{
								orderAttribute.WB_EntryKey = entryHeader.EntryNumber;
								orderAttribute.WB_EntryLineNo = entryLine.CL_LineNumber;
							}
						}
					}
				}
			}

			IWhsOrder GetWhsOrder(CusEntryHeader entryHeader)
			{
				IWhsOrder result = null;
				var declaration = entryHeader.Declaration;
				if (declaration.IsOutwardOrderImported)
				{
					var orderPivotQuery = new ZQuery(WhsDocketJobPivotSchema.WV_ParentId, declaration.PK)
												.AddToFilter(WhsDocketJobPivotSchema.WV_ParentTableCode, declaration.TablePrefix);

					var orderPivot = declaration.Factory.LoadTop1<IWhsDocketJobPivot>(orderPivotQuery);

					if (orderPivot != null)
					{
						result = declaration.Factory.Load<IWhsOrder>(orderPivot.WV_WD_Docket);
					}
				}
				return result;
			}
		}

		protected virtual bool UpdateWarehouseOrderForCurrentEntryLine(CusEntryLine entryLine) => true;

		public bool ShouldPublishWhsOutwardAcceptEvent(CusEntryHeader entryHeader) => ShouldPublishWhsOutwardAcceptEventCore(entryHeader);

		protected virtual bool ShouldPublishWhsOutwardAcceptEventCore(CusEntryHeader entryHeader) => false;

		BondedWarehouseMessageProcessorCreator BondedWhsMsgProcessorCreator => bondedWhsMsgProcessorCreator ?? (bondedWhsMsgProcessorCreator = new BondedWarehouseMessageProcessorCreator(Logger, GetNewBondedWarehouseMessageProcessor, GetBondedWarehouseNotificationGroupPK));
		BondedWarehouseMessageProcessorCreator bondedWhsMsgProcessorCreator;

		protected ZGuid BwhMessagePk => bwhMessagePK;
		ZGuid bwhMessagePK;
		protected virtual Customs.Business.MessageProcessors.BondedWarehouseMessageProcessor GetNewBondedWarehouseMessageProcessor(Action<EmailDef, EDIMessage> sendEmail) => new BondedWarehouseMessageProcessor(bwhMessagePK, null, sendEmail);

		Guid GetBondedWarehouseNotificationGroupPK(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			// own group for BWH mails fallback
			return DECustomsDataRegistry.Instance.SendImportAcknowledgements.GetFallBackValueAtAllLevels(companyPK, branchPK, departmentPK).PK.ToGuid();
		}

		#endregion
	}
}
