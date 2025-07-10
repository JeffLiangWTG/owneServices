using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.Services.OperationalActions.Business
{
	abstract class BaseDocumentProcessor
	{
		public BaseDocumentProcessor(OperationalActionRunner runner)
		{
			Runner = runner;
		}

		public void InitializeProcessor(IOperationalActionSectionLog log, ZGuid[] targetsPKs, Type targetType)
		{
			Log = log;
			TargetsPKs = targetsPKs;
			TargetType = targetType;
		}

		public OperationalActionRunner Runner { get; }

		public OperationalAction Action => Runner.Action;

		public IOperationalActionSectionLog Log { get; set; }

		public ZGuid[] TargetsPKs { get; set; }

		public Type TargetType { get; set; }

		BulkDeliveryMethod bulkDeliveryMethod;
		public BulkDeliveryMethod BulkDeliveryMethod
		{
			get
			{
				if (bulkDeliveryMethod == null || bulkDeliveryMethod.Code != Runner.BulkDeliveryMethod)
				{
					bulkDeliveryMethod = Runner.Lookups.BulkDeliveryMethod_List[Runner.BulkDeliveryMethod];
				}
				return bulkDeliveryMethod;
			}
		}

		public abstract void ProcessDocument();

		protected BusinessObjectFactory GetNewFactory()
		{
			BusinessObjectFactory factory;
			if (Runner.Selection is ModuleSelection selection)
			{
				factory = ((IFilterModuleForFactory)selection.Module).GetNewFactoryForFilterModule();
			}
			else
			{
				factory = new BusinessObjectFactory();
			}
			factory.RefreshEnabled = false;
			return factory;
		}

		protected DummyDocumentEvents CreateDummyDocumentEvents(IDocumentSupportable docSupportable)
		{
			var dummyDocumentEvents = new DummyDocumentEvents();
			docSupportable.DocumentSupporter.Initialise(dummyDocumentEvents);
			return dummyDocumentEvents;
		}

		protected DeliveryInstructions CreateDeliveryInstructions(string draftOption, FactoryStrategy strategy)
		{
			var instructions = strategy == null ? new DeliveryInstructions() : new DeliveryInstructions(strategy);
			instructions.AllowAutoDelivery = true;
			instructions.Destination = DeliveryInstructionDestination.Auto;
			instructions.PrinterDelivery.PrintQueuePK = Runner.Printer;
			instructions.Language = Runner.DocumentPrintLanguage;

			if (draftOption != DraftOptionsList.Codes.Both)
			{
				instructions.IsDraft = draftOption == DraftOptionsList.Codes.Draft;
			}

			return instructions;
		}

		protected void SetDeliveryInstructionsCoverNoteIfNeeded(DeliveryInstructions instructions)
		{
			if (Runner.IncludeCoverNote && CanAttachCoverNote(instructions))
			{
				instructions.IncludeCoverNote = true;
				instructions.CoverNote = Runner.CoverNoteText;
			}
		}

		protected bool CanAttachCoverNote(DeliveryInstructions instructions)
		{
			foreach (DocDeliveryContact contact in instructions.Recipients)
			{
				if (contact.DeliveryMethod == ContactNotifyModes.Email || contact.DeliveryMethod == ContactNotifyModes.Fax)
				{
					return true;
				}
			}
			return false;
		}

		protected UserControlProviderList InitializeProviderList(BusinessObject target)
		{
			using (var note = target is IStmNoteParent noteParent ? DocumentNote.RetrieveNote(noteParent) : null)
			{
				return note != null ?
						new UserControlProviderList(note.GetSystemDefinedFieldList(), note.UserDefinedFieldList) :
						new UserControlProviderList();
			}
		}
	}
}
