using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentVisualizer.Delivery
{
	public abstract class LinkedDocumentMessageProcessor
	{
		public void Process(IBusiness bizObj, IStmALog log, Integration.IBusinessObjectLoader loader = null)
		{
			if (bizObj == null
				|| !(log is StmALog stmALog)
				|| log.SL_SE_NKEvent != Events.DataLinkedCode)
			{
				return;
			}

			loader = loader ?? new DefaultBusinessObjectLoader();

			if (stmALog.IsInDatabase)
			{
				CreateLogAndEDoc(bizObj, stmALog.RelatedEDIMessage?.Message, loader);
			}
			else
			{
				CreateLogAndEDocAfterLogIsSaved(bizObj, stmALog, loader);
			}
		}

		void CreateLogAndEDocAfterLogIsSaved(IBusiness bizObj, StmALog log, Integration.IBusinessObjectLoader loader)
		{
			var processingService = log.Factory.ServiceContainer.GetService<DelayedProcessingService>();

			if (processingService == null)
			{
				processingService = new DelayedProcessingService();
				log.Factory.ServiceContainer.AddService(processingService);

				void OnFactorySaved(BusinessObjectFactory factory, bool savedSuccessfully)
				{
					if (savedSuccessfully)
					{
						foreach (var process in processingService.ProcessList)
						{
							process();
						}

						factory.Saved -= OnFactorySaved;
						factory.ServiceContainer.RemoveService<DelayedProcessingService>();
					}
				}

				log.Factory.Saved += OnFactorySaved;
			}

			processingService.ProcessList.Add(() => CreateLogAndEDoc(bizObj, log.RelatedEDIMessage?.Message, loader));
		}

		void CreateLogAndEDoc(IBusiness bizObj, IEDIMessage message, Integration.IBusinessObjectLoader loader)
		{
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = Globals.IsUserInteractive;

			if (!ShouldProcess(message)
				|| !(factory.Load<StmMenuItemBase>(MenuItemPK) is IStmMenuItem menuItem)
				|| menuItem?.Documents.Count != MenuItemDocumentCount)
			{
				return;
			}

			var document = menuItem.Documents.OfType<IStmMenuTemplatePivot>().FirstOrDefault();
			var docType = factory.Load<RefDocType>(document.SI_RT_DocType);

			if (docType == null)
			{
				return;
			}

			var createdLog = CreateLog(bizObj, factory, docType);
			var createdEDoc = CreateEDoc(bizObj, loader, factory, menuItem, docType);

			if (!createdLog
				&& !createdEDoc)
			{
				return;
			}

			try
			{
				OnBeforeSaveForTest(factory);

				Action recoveryAction = () =>
				{
					factory = new BusinessObjectFactory();
					factory.RefreshEnabled = Globals.IsUserInteractive;
					CreateLog(bizObj, factory, docType);
					CreateEDoc(bizObj, loader, factory, menuItem, docType);
					OnBeforeSaveForTest(factory);
				};

				ZExceptionReporting.ProcessWithSaveExceptionHandling(factory.Save, recoveryAction, attempts: 3);
			}
			catch (ZSaveException exc)
			{
				ErrorReporter.ReportOnce("LinkedDocumentMessageProcessor-CreateLogAndEDoc-FailedAfter3Attempts", "LinkedDocumentMessageProcessor failed to save after 3 attempts.", exc);
			}
		}

		bool CreateEDoc(IBusiness bizObj, Integration.IBusinessObjectLoader loader, BusinessObjectFactory factory, IStmMenuItem menuItem, RefDocType docType)
		{
			var createdEDoc = false;
			if (docType.RT_LogSystemCreatedDocsToEDocs
				&& loader.LoadBusinessObject(factory, bizObj) is IDocumentSupportable documentSupportable)
			{
				createdEDoc = CreateEDoc(factory, menuItem, documentSupportable);
			}

			return createdEDoc;
		}

		bool CreateLog(IBusiness bizObj, BusinessObjectFactory factory, RefDocType docType)
		{
			var createdLog = false;
			if (!docType.RT_SE_NKDocumentReceivedEvent.IsEmpty
				&& bizObj is IStmALogParent logParent)
			{
				var eventCode = docType.RT_SE_NKDocumentReceivedEvent;
				var logReference = docType.EvaluateLogMacro(bizObj)?.Item1;

				createdLog = CreateLog(factory, logParent, eventCode, logReference);
			}

			return createdLog;
		}

		bool CreateLog(BusinessObjectFactory factory, IStmALogParent logParent, ZString eventCode, ZString reference)
		{
			var log = factory.New<StmALog>();

			using (((IUpdateFieldsLockReachAround)log).LockForUpdatingKeyFields(false))
			{
				log.SL_SE_NKEvent = eventCode;
				log.SL_Reference = StmALog.GenerateEventReferenceToFitInReferenceMaxLength(reference, null);
				log.SL_Parent = logParent.Identifier;
				log.SL_Table = logParent.TableName;
				log.SL_FireWorkflow = true;
			}

			return true;
		}

		bool CreateEDoc(BusinessObjectFactory factory, IStmMenuItem menuItem, IDocumentSupportable documentSupportable)
		{
			var deliveriesProvider = new FormDeliverablesProvider();
			var deliverables = deliveriesProvider.GetDeliverables(menuItem, documentSupportable);

			var eDocWasCreated = false;

			foreach (var deliverable in deliverables.OfType<DocumentDeliverable>())
			{
				var eDocDeliveryParameters =  new EDocsDeliveryParameters
				{
					Factory = factory,
					BusinessObject = deliverable.EDocsParent as IBusiness,
					DocumentName = deliverable.DocumentName,
					DocumentTitle = deliverable.DocumentTitle,
					DocumentType = deliverable.DocumentType,
					AttachedFileName = deliverable.DocumentName
				};

				eDocWasCreated |= deliverable.Document.AddCopyToEDocs(eDocDeliveryParameters);
			}

			return eDocWasCreated;
		}

		[Conditional("DEBUG")]
		protected virtual void OnBeforeSaveForTest(BusinessObjectFactory factory)
		{
		}

		protected abstract bool ShouldProcess(IEDIMessage message);
		protected abstract ZGuid MenuItemPK { get; }
		protected abstract ZInt MenuItemDocumentCount { get; }

		sealed class DelayedProcessingService : IService
		{
			public List<Action> ProcessList = new List<Action>();
		}
	}
}
