using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Delivery
{
	sealed class DocumentPrintTask : PrintTask
	{
		public DocumentPrintTask(IEventBroker broker = null, string emailSubject = null)
		{
			this.broker = broker;
			this.emailSubject = emailSubject;
		}

		readonly IEventBroker broker;
		readonly string emailSubject;

		protected override void SetDraftOptions(DeliveryInstructions instructions)
		{
			if (instructions != null)
			{
				instructions.IsDraft_ReadOnly = false;
			}
		}

		public override void Run(DeliveryInstructions deliveryInstructions, INotifications notifications = null)
		{
			Argument.NotNull(deliveryInstructions, nameof(deliveryInstructions));

			if (deliveryInstructions.Destination != DeliveryInstructionDestination.UserCancelled)
			{
				if (deliveryInstructions.Recipients.Count == 0
					&& deliveryInstructions.Destination == DeliveryInstructionDestination.Preview)
				{
					Deliver(new DocDeliveryContact(Factory), deliveryInstructions, notifications);
				}
				else
				{
					var documentDeliveries = new Dictionary<IDocument, HashSet<StmPrintJob>>();

					foreach (DocDeliveryContact contact in deliveryInstructions.Recipients)
					{
						var documentDeliveriesForContact = Deliver(contact, deliveryInstructions, notifications);

						foreach (var kvp in documentDeliveriesForContact)
						{
							if (!documentDeliveries.TryGetValue(kvp.Key, out var printJobs))
							{
								printJobs = new HashSet<StmPrintJob>(new StmPrintJobPKEqualityComparer());
								documentDeliveries[kvp.Key] = printJobs;
							}

							foreach (var printJob in kvp.Value)
							{
								printJobs.Add(printJob);
							}
						}
					}

					BroadcastDelivery(documentDeliveries);
				}
			}
		}

		IReadOnlyDictionary<IDocument, HashSet<StmPrintJob>> Deliver(DocDeliveryContact contact, DeliveryInstructions instructions, INotifications notifications = null)
		{
			var factory = instructions.Factory;

			var deliveryInfoMap = new Dictionary<DeliveryInfo, DeliveryLogDocument>();

			var method = DeliveryMethod.FromContact(contact, instructions);

			var deliverables = instructions
				.DeliverablesToBePrinted
				.OfType<DocumentDeliverable>()
				.ToArray();

			var nonProcessedDeliverables = new HashSet<DocumentDeliverable>();
			var preProcessedDeliverables = new List<IPrePrintProcessingResult>();

			foreach (var deliverable in deliverables)
			{
				if (!deliverable.IncludedInPrint)
				{
					nonProcessedDeliverables.Add(deliverable);
					continue;
				}

				if (!System.Enum.TryParse<FileType>(contact.AttachmentType, out var fileType))
				{
					fileType = FileType.XLS;
				}

				if (instructions.Destination == DeliveryInstructionDestination.Print
					|| instructions.Destination == DeliveryInstructionDestination.TakenFromContact
					|| instructions.Destination == DeliveryInstructionDestination.Auto)
				{
					var res = deliverable.DoPrePrintProcessing(factory, instructions.IsDraft);

					if (res != null)
					{
						preProcessedDeliverables.Add(res);
					}
				}

				var deliveryInfo = deliverable.GetDeliveryInfo(instructions.IsDraft, fileType);
				deliveryInfo.Instructions = instructions;

				if (instructions.GetOrCreateDeliveryGroup(contact) is StmDeliveryGroup deliveryGroup)
				{
					deliveryInfo.DeliveryGroupID = deliveryGroup.PK;
				}

				deliveryInfo.EmailSubjectLine = emailSubject ?? deliverable.GetEmailSubjectLine();
				deliveryInfo.EmailSignature = deliverable.GetEmailSignature();
				deliveryInfo.EmailFromAddress = contact.EmailFromAddress;

				method.AddFile(deliveryInfo);

				var logParent = deliverable.LogParent;

				if (logParent != null)
				{
					deliveryInfo.ParentGuid = logParent.Identifier;
					deliveryInfo.ParentTableName = logParent.TableName;
				}

				if (instructions.Destination != DeliveryInstructionDestination.Preview)
				{
					var log = LogDelivery(factory, deliverable, instructions.IsDraft);

					deliveryInfoMap[deliveryInfo] = new DeliveryLogDocument
					{
						Document = deliverable.Document,
						CreatedLog = log
					};

					var eDocsParent = deliverable.EDocsParent;

					if (deliverable.SaveCopyToEDocs
						&& eDocsParent != null)
					{
						var eDocsDeliveryParameters = new EDocsDeliveryParameters
						{
							Factory = factory,
							BusinessObject = eDocsParent as IBusiness,
							DocumentName = deliverable.DocumentName,
							DocumentTitle = deliverable.DocumentTitle,
							FileFormat = deliveryInfo.FileFormat,
							DocumentType = deliveryInfo.DocumentType,
							AttachedFileName = deliveryInfo.AttachedFilename,
							ShowDraftWatermark = deliveryInfo.ShowDraftWatermark
						};

						deliverable.Document.AddCopyToEDocs(eDocsDeliveryParameters);
					}
				}
			}

			// create Print Jobs
			method.Deliver(notifications);

			var documentsScheduledForDelivery = PostProcessPrintJobs(factory, deliveryInfoMap);

			factory.RefreshEnabled = true;
			if (instructions.Destination != DeliveryInstructionDestination.Preview)
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, null);
				PostProcessDocuments(preProcessedDeliverables, nonProcessedDeliverables);
			}

			return documentsScheduledForDelivery;
		}

		void BroadcastDelivery(IReadOnlyDictionary<IDocument, HashSet<StmPrintJob>> delivery)
		{
			if (broker == null
				|| delivery.Count == 0)
			{
				return;
			}

			foreach (var documentScheduledForDelivery in delivery)
			{
				var eventData = new PrintJobsCreatedEvent(
					document: documentScheduledForDelivery.Key,
					printJobs: documentScheduledForDelivery.Value.ToArray());

				broker.Publish(eventData);
			}
		}

		/// <summary>
		/// Creates StmALog templates against created StmPrintJobs and marks related StmDeliveryGroups as processed, so StmPrintJobs can be processed by service tasks
		/// </summary>
		IReadOnlyDictionary<IDocument, HashSet<StmPrintJob>> PostProcessPrintJobs(BusinessObjectFactory factory, IReadOnlyDictionary<DeliveryInfo, DeliveryLogDocument> deliveryInfoMap)
		{
			var res = new Dictionary<IDocument, HashSet<StmPrintJob>>();

			var printJobsLink = PrintJobDeliveryInfosLink.GetInstance(factory);

			foreach (var printJob in printJobsLink.PrintJobs)
			{
				var deliveryGroup = printJob.DeliveryGroup;
				if (deliveryGroup != null)
				{
					deliveryGroup.SB_IsProcessed = true;
				}

				var logs = new List<StmALog>();

				foreach (var deliveryInfo in printJobsLink.Get(printJob))
				{
					if (!deliveryInfoMap.TryGetValue(deliveryInfo, out var map))
					{
						continue;
					}

					if (map.CreatedLog is StmALog log)
					{
						logs.Add(log);
					}

					if (!res.TryGetValue(map.Document, out var printJobs))
					{
						printJobs = new HashSet<StmPrintJob>(new StmPrintJobPKEqualityComparer());
						res[map.Document] = printJobs;
					}

					printJobs.Add(printJob);
				}

				printJob.CreateLogTemplates(logs.ToArray());
			}

			return res;
		}

		void PostProcessDocuments(IReadOnlyCollection<IPrePrintProcessingResult> processedDeliverables, IReadOnlyCollection<DocumentDeliverable> unprocessedDeliverables)
		{
			if (processedDeliverables.Count == 0
				|| unprocessedDeliverables.Count == 0)
			{
				return;
			}

			var unprocessedDocuments = unprocessedDeliverables
				.Select(deliverable => deliverable.Document)
				.Where(document => document?.DataContext != null)
				.ToArray();

			foreach (var processedDeliverable in processedDeliverables)
			{
				if (processedDeliverable?.Document?.DataContext == null
					|| processedDeliverable?.ValuesSet == null)
				{
					continue;
				}

				var unprocessedDocumentsMatchingContext = unprocessedDocuments
					.Where(unprocessedDocument => unprocessedDocument.DataContext.Equals(processedDeliverable.Document.DataContext, StringComparison.OrdinalIgnoreCase))
					.ToArray();

				foreach (var unprocessedDocument in unprocessedDocumentsMatchingContext)
				{
					SetValuesOnDocument(unprocessedDocument, processedDeliverable.ValuesSet);
				}
			}
		}

		void SetValuesOnDocument(IDocument unprocessedDocument, IReadOnlyDictionary<string, object> valuesToSet)
		{
			foreach (var valueToSet in valuesToSet)
			{
				var property = unprocessedDocument.Data?.GetDynamicProperty(valueToSet.Key);

				if (property != null)
				{
					try
					{
						property.SetValue(valueToSet.Value);
						property.AcceptChanges();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
						ex.Data[valueToSet.Key] = valueToSet.Key;
						ex.Data[valueToSet.Value] = valueToSet.Value;
						ex.Data["UnprocessedDocument.DataContext"] = unprocessedDocument.DataContext;

						ErrorReporter.ReportOnce("DocumentPrintTask.PostProcessDocuments.SetValuesOnDocument", ex);
					}
				}
			}
		}

		StmALog LogDelivery(BusinessObjectFactory factory, DocumentDeliverable deliverable, bool isDraft)
		{
			if (isDraft)
			{
				return null;
			}

			StmALog log = null;

			var logParent = deliverable.LogParent;

			if (logParent != null)
			{
				var parameters = deliverable.GetParametersForDocumentDeliveryLog(deliverable.DocumentName);

				log = factory.New<StmALog>();

				using (((IUpdateFieldsLock)log).LockForUpdatingKeyFields())
				{
					log.SL_SE_NKEvent = deliverable.DocumentDeliveredEventCode;
					log.SL_Reference = StmALog.GenerateEventReferenceToFitInReferenceMaxLength(string.Empty, parameters);
					log.SL_Parent = logParent.Identifier;
					log.SL_Table = logParent.TableName;
				}
			}

			return log;
		}

		sealed class DeliveryLogDocument
		{
			public IDocument Document { get; set; }
			public StmALog CreatedLog { get; set; }
		}

		sealed class StmPrintJobPKEqualityComparer : IEqualityComparer<StmPrintJob>
		{
			public bool Equals(StmPrintJob printJob1, StmPrintJob printJob2) => printJob1?.PK == printJob2?.PK;
			public int GetHashCode(StmPrintJob printJob) => printJob.PK.GetHashCode();
		}
	}
}
