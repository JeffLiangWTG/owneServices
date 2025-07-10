using System;
using System.Collections;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentScanning.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentEngine
{
	[Serializable]
	public class AutoDocumentDeliveryJob : System.Runtime.Serialization.ISerializable, IJsonSerializable
	{
		public AutoDocumentDeliveryJob(IDocumentSupportable businessObject, bool isFactoryPopulateButDoNotSave, ZGuid documentCommandPK)
			: this(businessObject, isFactoryPopulateButDoNotSave, documentCommandPK, ZGuid.Empty)
		{
		}

		public AutoDocumentDeliveryJob(IDocumentSupportable businessObject, ZGuid documentCommandPK, bool onlySendToDocManager)
			: this(businessObject, documentCommandPK, ZGuid.Empty, onlySendToDocManager, false)
		{
		}

		public AutoDocumentDeliveryJob(IDocumentSupportable businessObject, bool isFactoryPopulateButDoNotSave, ZGuid documentCommandPK, ZGuid printerQueuePK)
			: this(businessObject, documentCommandPK, printerQueuePK, false, false)
		{
			this.IsFactoryPopulateButDoNotSave = isFactoryPopulateButDoNotSave;
		}

		public AutoDocumentDeliveryJob(IDocumentSupportable businessObject, ZGuid documentCommandPK, bool onlySendToDocManager, bool isFactoryPopulateButDoNotSave)
			: this(businessObject, documentCommandPK, ZGuid.Empty, onlySendToDocManager, false)
		{
			this.IsFactoryPopulateButDoNotSave = isFactoryPopulateButDoNotSave;
		}

		public AutoDocumentDeliveryJob(IDocumentSupportable businessObject, ZGuid documentCommandPK, ZGuid printerQueuePK, bool onlySendToDocManager)
			: this(businessObject, documentCommandPK, printerQueuePK, onlySendToDocManager, false)
		{
		}

		public AutoDocumentDeliveryJob(IDocumentSupportable businessObject, ZGuid documentCommandPK, ZGuid printerQueuePK, bool onlySendToDocManager, bool sendToEDocs)
		{
			this.documentSupportable = businessObject;
			this.printerQueuePK = printerQueuePK.IsValid ? printerQueuePK.ToGuid() : Guid.Empty;
			this.documentCommandPK = documentCommandPK.IsValid ? documentCommandPK.ToGuid() : Guid.Empty;
			this.onlySendToDocManager = onlySendToDocManager;
			this.sendToEDocs = sendToEDocs;

			BusinessObject bizo = businessObject as BusinessObject;

			if (bizo != null)
			{
				this.factory = bizo.Factory;
				this.businessObjectPK = bizo.PK;
				this.businessObjectType = bizo.GetType();
			}
		}

		//TODO: WI00738741 - Confirm if serialization is required for AutoDocumentDeliveryJob
		#region Constructor For IJsonSerializable
		internal AutoDocumentDeliveryJob(AutoDocumentDeliveryJobJsonData data)
		{
			businessObjectType = Type.GetType(data.BusinessObjectType);
			businessObjectPK = data.BusinessObjectPK;
			printerQueuePK = data.PrinterQueuePK;
			documentCommandPK = data.DocumentCommandPK;
			onlySendToDocManager = data.SendToDocManager;
			sendToEDocs = data.SendToEDocs;
		}
		#endregion

#if NETFRAMEWORK
		protected AutoDocumentDeliveryJob(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			businessObjectType = (Type)info.GetValue("businessObjectType", typeof(Type));
			businessObjectPK = (Guid)info.GetValue("businessObjectPK", typeof(Guid));
			printerQueuePK = (Guid)info.GetValue("printerQueuePK", typeof(Guid));
			documentCommandPK = (Guid)info.GetValue("documentCommandPK", typeof(Guid));
			onlySendToDocManager = info.GetBoolean("sendToDocManager");
			sendToEDocs = info.GetBoolean("sendToEDocs");
		}
#endif

		protected class DummyDocumentEvents : IDocumentEvents
		{
			public event DocumentCancelEventHandler DocumentPrintRequested;
			public event DocumentPrintedEventHandler DocumentPrePreviewed;
			public event DocumentPrintedEventHandler DocumentPrePrinted;
			public event DocumentPrintedEventHandler DocumentPrinted;

			public bool onDocumentPrintRequested(object sender, DocumentCancelEventArgs e)
			{
				if (DocumentPrinted != null)
				{
					DocumentPrintRequested(sender, e);
				}

				return e.Cancel;
			}

			public void onDocumentPrePreviewed(object sender, DocumentPrintedEventArgs e)
			{
				DocumentPrePreviewed?.Invoke(sender, e);
			}

			public void onDocumentPrePrinted(object sender, DocumentPrintedEventArgs e)
			{
				if (DocumentPrinted != null)
				{
					DocumentPrePrinted(sender, e);
				}
			}

			public void onDocumentPrinted(object sender, DocumentPrintedEventArgs e)
			{
				if (DocumentPrinted != null)
				{
					DocumentPrinted(sender, e);
				}
			}
		}

		public void Deliver(INotifications notifications)
		{
			try
			{
				DeliverCore(notifications);
			}
			catch (TemplateGeneratingException ex)
			{
				notifications.AddError(ex.GetErrorMessage(DocumentCommand));
			}
			catch (ExternalStorageException ex)
			{
				ex.ReportExceptionForDeveloper();
				notifications.AddError(ex.UnableToAccessStorageFriendlyMessage);
			}
		}

		protected void DeliverCore(INotifications notifications, bool shouldSetIsAutoDocumentDeliveryFlag = true)
		{
			using (var documentPrintSet = GetNewDocumentPrintSetForDelivery())
			{
				var dummyDocumentEvents = GetNewDummyDocumentEvents(notifications);

				documentPrintSet.IsAutoDocumentDelivery = shouldSetIsAutoDocumentDeliveryFlag;

				var documentSupporter = DocumentSupportable.DocumentSupporter;
				if (documentSupporter != null)
				{
					documentSupporter.SetupDeliveryForAutoDocumentDeliveryJob(documentPrintSet);

					documentSupporter.Initialise(dummyDocumentEvents);
				}

				for (int i = 0; i < documentPrintSet.Count; i++)
				{
					var docPack = documentPrintSet[i];
					var instructions = GetDeliveryInstructions(docPack);
					using (instructions.ResetEDocsToBeDeliveredIfNeeded())
					{
						bool wasBusinessObjectDeleted = BusinessObject == null;

						if (!wasBusinessObjectDeleted)
						{
							if (instructions.Recipients.Count > 0 || IsDeliverToPrinter || OnlySendToDocManager)
							{
								instructions.RunPreSaveValidation();
								if (instructions.HasErrors && !OnlySendToDocManager)
								{
									BONotification.AddErrorsFromBusinessObjectValidationIncludingChildren(notifications, instructions);
								}
								else if (DocumentCommand.Parent == null || DocumentCommand.IsApplicable)
								{
									DocumentPrintedEventHandler notifyPrePreviewed = (sender, e) => dummyDocumentEvents.onDocumentPrePreviewed(BusinessObject, new DocumentPrintedEventArgs(instructions.Destination, documentPrintSet.ParentMenuCommand));
									documentPrintSet.DocumentPrePreviewed += notifyPrePreviewed;

									DocumentPrintedEventHandler notifyPrePrinted = (sender, e) => dummyDocumentEvents.onDocumentPrePrinted(BusinessObject, new DocumentPrintedEventArgs(instructions.Destination, documentPrintSet.ParentMenuCommand));
									documentPrintSet.DocumentPrePrinted += notifyPrePrinted;

									documentPrintSet.RunDocumentPack(instructions, docPack);
									dummyDocumentEvents.onDocumentPrinted(BusinessObject, new DocumentPrintedEventArgs(instructions.Destination, documentPrintSet.ParentMenuCommand));

									if (notifyPrePreviewed != null)
									{
										documentPrintSet.DocumentPrePreviewed -= notifyPrePreviewed;
									}

									if (notifyPrePrinted != null)
									{
										documentPrintSet.DocumentPrePrinted -= notifyPrePrinted;
									}
								}
							}
							else
							{
								notifications.AddWarning(MessageOfNoRecipient);
							}
						}
					}
				}

#if DEBUG
				OnDeliveredBeforeDocumentPrintSetDisposed(documentPrintSet);
#endif
			}
		}

		protected virtual DummyDocumentEvents GetNewDummyDocumentEvents(INotifications notifications)
		{
			return new DummyDocumentEvents();
		}

		protected virtual string MessageOfNoRecipient => Res.GetString("8a53242a-2731-45be-96b2-089ade221137", "Document not delivered: No recipient found on delivery instructions");

#if DEBUG
		protected virtual void OnDeliveredBeforeDocumentPrintSetDisposed(DocumentPrintSet documentPrintSet) { }
#endif

		protected virtual DocumentPrintSet GetNewDocumentPrintSetForDelivery()
		{
			return new DocumentPrintSet(DocumentCommand, Note?.UserDefinedFieldList);
		}

		protected virtual DeliveryInstructions GetDeliveryInstructions(DocumentPack pack)
		{
			var result = IsFactoryPopulateButDoNotSave ? new DeliveryInstructions(pack, new FactoryStrategy.PopulateButDoNotSave(Factory)) : new DeliveryInstructions(pack);

			if (IsDeliverToPrinter)
			{
				RemoveAllRecipients(result);
				result.Destination = DeliveryInstructionDestination.Print;
				result.PrinterDelivery.PrintQueuePK = printerQueuePK;
			}
			else if (OnlySendToDocManager)
			{
				SetDocumentPrintCopyTypeToSupportAll(pack);
				RetainOneRecipient(result);
				result.Destination = DeliveryInstructionDestination.DocManager;
			}
			else
			{
				RemovePrintRecipients(result);
				result.Destination = DeliveryInstructionDestination.TakenFromContact;
			}
			result.SendToEDocs = sendToEDocs;

			return result;
		}

		public DocumentCommand DocumentCommand
		{
			get
			{
				if (fDocumentCommand == null)
				{
					fDocumentCommand = GetDocumentCommandCore();

					if (fDocumentCommand != null)
					{
						fDocumentCommand.Parent = DocumentSupportable;
					}
				}

				return fDocumentCommand;
			}
		}

		DocumentCommand fDocumentCommand;

		protected virtual DocumentCommand GetDocumentCommandCore()
		{
			return Factory.Load<DocumentCommand>(documentCommandPK);
		}

		public IDocumentSupportable DocumentSupportable
		{
			get { return documentSupportable ?? (documentSupportable = (IDocumentSupportable)BusinessObject); }
		}

		DocumentNote Note
		{
			get
			{
				if (note == null && DocumentSupportable is IStmNoteParent parent)
				{
					note = DocumentNote.LoadNote(parent);
				}
				return note;
			}
		}
		DocumentNote note;

		public BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}

		public BusinessObject BusinessObject
		{
			get { return businessObjectType == null ? null : Factory.Load(businessObjectType, businessObjectPK); }
		}

		public bool OnlySendToDocManager
		{
			get { return onlySendToDocManager; }
		}

		public ZGuid PrinterQueuePK
		{
			get { return printerQueuePK; }
		}

		public bool SendToEDocs
		{
			get { return sendToEDocs; }
		}

		protected virtual void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue("businessObjectPK", businessObjectPK.IsValid ? businessObjectPK.ToGuid() : Guid.Empty);
			info.AddValue("businessObjectType", businessObjectType);
			info.AddValue("printerQueuePK", printerQueuePK.IsValid ? printerQueuePK.ToGuid() : Guid.Empty);
			info.AddValue("documentCommandPK", documentCommandPK.IsValid ? documentCommandPK.ToGuid() : Guid.Empty);
			info.AddValue("sendToDocManager", onlySendToDocManager);
			info.AddValue("sendToEDocs", sendToEDocs);
		}

		#region Implementation

		bool IsDeliverToPrinter
		{
			get { return printerQueuePK != Guid.Empty; }
		}

		void RemovePrintRecipients(DeliveryInstructions instructions)
		{
			foreach (DocDeliveryContact recipient in new ArrayList(instructions.Recipients))
			{
				if (recipient.DeliveryMethod == Core.Constants.ContactNotifyModes.Print)
				{
					instructions.Recipients.Remove(recipient);
				}
			}
		}

		// Purpose: Address the translation issue of the 'RecipientNameAndAddress' macro.
		void RetainOneRecipient(DeliveryInstructions instructions)
		{
			if (instructions.Recipients.Count == 1)
			{
				instructions.Recipients[0].DeliveryMethod = Core.Constants.ContactNotifyModes.EDoc;
			}
			else if (instructions.Recipients.Count > 1)
			{
				var defaultContact = instructions.DocPack.AutoDocumentDelivery.GetDeliveryDetailsForSystemDefaultContact(instructions.Recipients[0].OrgHeader, instructions.DocPack.StmMenuCommand);
				RemoveAllRecipients(instructions);
				defaultContact.DeliveryMethod = Core.Constants.ContactNotifyModes.EDoc;
				defaultContact.Name = ContactType.Find(instructions.DocPack.StmMenuCommand.SU_ContactType).DefaultName.ToString(string.IsNullOrEmpty(instructions.Language) ? Res.DefaultLanguage : instructions.Language.ToString());
				instructions.Recipients.Add(defaultContact);
			}
		}

		protected void RemoveAllRecipients(DeliveryInstructions instructions)
		{
			foreach (DocDeliveryContact recipient in new ArrayList(instructions.Recipients))
			{
				instructions.Recipients.Remove(recipient);
			}
		}

		void SetDocumentPrintCopyTypeToSupportAll(DocumentPack pack) => pack.OfType<Report>().ForEach(r => r.PrintCopyType = ZArchitecture.Core.PrintCopyType.ALL);

		IDocumentSupportable documentSupportable;
		BusinessObjectFactory factory;

		readonly ZGuid businessObjectPK;
		readonly Type businessObjectType;
		readonly ZGuid printerQueuePK;
		readonly ZGuid documentCommandPK;
		readonly bool onlySendToDocManager;
		readonly bool sendToEDocs;
		protected readonly bool IsFactoryPopulateButDoNotSave;

		#endregion

		#region ISerializable Members

		void System.Runtime.Serialization.ISerializable.GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			GetObjectData(info, context);
		}

		//TODO: WI00738741 - Confirm if serialization is required for AutoDocumentDeliveryJob
		public object GetJsonData() =>
			new AutoDocumentDeliveryJobJsonData()
			{
				BusinessObjectPK = businessObjectPK.IsValid ? businessObjectPK.ToGuid() : Guid.Empty,
				BusinessObjectType = businessObjectType.AssemblyQualifiedName,
				PrinterQueuePK = printerQueuePK.IsValid ? printerQueuePK.ToGuid() : Guid.Empty,
				DocumentCommandPK = documentCommandPK.IsValid ? documentCommandPK.ToGuid() : Guid.Empty,
				SendToDocManager = onlySendToDocManager,
				SendToEDocs = sendToEDocs
			};

		#endregion
	}
}
