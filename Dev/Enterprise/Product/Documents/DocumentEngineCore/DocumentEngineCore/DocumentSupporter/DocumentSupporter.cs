using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupporter.DocumentSupporterHelper;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngineCore.DocumentSupport
{
	public abstract class DocumentSupporter : IDocumentEvents
	{
		protected DocumentSupporter(BusinessObject parentBusinessObject)
		{
			if (object.ReferenceEquals(parentBusinessObject, null))
			{
				throw new ArgumentNullException(nameof(parentBusinessObject));
			}

			BusinessObject = parentBusinessObject;
		}

		#region Business Object Properties

		public readonly BusinessObject BusinessObject;

		public ZGuid PK
		{
			get { return BusinessObject.PK; }
		}

#if DEBUG
		virtual
#endif
		public BusinessObjectFactory Factory
		{
			get { return BusinessObject.Factory; }
		}

		public bool IsInDatabase
		{
			get { return BusinessObject.IsInDatabase; }
		}

		public bool HasChanges
		{
			get { return BusinessObject.HasChanges; }
			set { BusinessObject.HasChanges = value; }
		}

		public bool IsNonPersistent
		{
			get { return GetIsNonPersistent(); }
		}

		// Overridden in Enterprise.Accounting.ARAP.Invoice.InvoiceBaseDummyDocumentSupporter
		// supposedly so the NonPersistentBO there shows a Document Menu anyway. Not sure why.
		// Possibly a hack....
		protected virtual bool GetIsNonPersistent()
		{
			return BusinessObject.IsNull || typeof(NonPersistentBusinessObject).IsAssignableFrom(BusinessObject.GetType());
		}

		#endregion

		#region Overridable Members

		/// <summary>
		/// Logical name for this business object that implements IDocumentSupport, so that when user defined document menus are added to this object
		/// they can be saved in the database against this logical name and can be retrieved correctly later even if the business object class gets renamed.
		/// </summary>
		public abstract BusinessContext BusinessContext { get; }

		/// <summary>
		/// DocumentWrappers are wrappers around business objects for the purpose of rendering documents.
		/// You can return one or more for the purpose of rendering documents for the given data context.
		/// Each DocumentWrapper returned will be rendered as a separate document.
		/// </summary>
		protected abstract DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun);

		/// <summary>
		/// A list of data contexts that are supported by the GetDocBusinessObject method on this interface.
		/// This is called during user document menu customisation to return a list of templates that can be
		/// supported by this IDocumentSupport object.
		/// </summary>
		protected abstract DataContext[] GetSupportedDataContexts();

		public abstract ISecurityCheckpoint CustomisationSecurityCheckpoint { get; }

		protected virtual List<DocumentSupporterQuestion> GenerateQuestionsToAskUsersBeforeRunningDocumentCore(IStmMenuItem commandAboutToBeRun)
		{
			return new List<DocumentSupporterQuestion>();
		}

		public void Initialise(IDocumentEvents documentEventSource)
		{
			documentEventSource.DocumentPrintRequested += new DocumentCancelEventHandler(DocumentEventSource_DocumentPrintRequested);
			documentEventSource.DocumentPrePreviewed += new DocumentPrintedEventHandler(DocumentEventSource_DocumentPrePreviewed);
			documentEventSource.DocumentPrePrinted += new DocumentPrintedEventHandler(DocumentEventSource_DocumentPrePrinted);
			documentEventSource.DocumentPrinted += new DocumentPrintedEventHandler(DocumentEventSource_DocumentPrinted);

			InitialiseCore(documentEventSource);
		}

		protected virtual void InitialiseCore(IDocumentEvents documentEventSource)
		{
		}

		void DocumentEventSource_DocumentPrintRequested(object sender, DocumentCancelEventArgs e)
		{
			foreach (var handler in DocumentEventsHandlers)
			{
				if (handler.CanHandleMenuItem(e.MenuItem))
				{
					handler.HandleDocumentPrintRequested(sender, e);
				}
			}

			if (DocumentPrintRequested != null)
			{
				DocumentPrintRequested(BusinessObject, e);
			}
		}

		public event DocumentCancelEventHandler DocumentPrintRequested;

		void DocumentEventSource_DocumentPrePreviewed(object sender, DocumentPrintedEventArgs e)
		{
			foreach (var handler in DocumentEventsHandlers)
			{
				if (handler.CanHandleMenuItem(e.MenuItem))
				{
					handler.HandleDocumentPrePreviewed(sender, e);
				}
			}

			DocumentPrePreviewed?.Invoke(BusinessObject, e);
		}

		public event DocumentPrintedEventHandler DocumentPrePreviewed;

		void DocumentEventSource_DocumentPrePrinted(object sender, DocumentPrintedEventArgs e)
		{
			foreach (var handler in DocumentEventsHandlers)
			{
				if (handler.CanHandleMenuItem(e.MenuItem))
				{
					handler.HandleDocumentPrePrinted(sender, e);
				}
			}

			if (DocumentPrePrinted != null)
			{
				DocumentPrePrinted(BusinessObject, e);
			}
		}

		public event DocumentPrintedEventHandler DocumentPrePrinted;

		void DocumentEventSource_DocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
			foreach (var handler in DocumentEventsHandlers)
			{
				if (handler.CanHandleMenuItem(e.MenuItem))
				{
					handler.HandleDocumentPrinted(sender, e);
				}
			}

			if (DocumentPrinted != null)
			{
				DocumentPrinted(BusinessObject, e);
			}
		}

		public event DocumentPrintedEventHandler DocumentPrinted;

		IDocumentEventsHandler[] documentEventsHandlers;
		public IDocumentEventsHandler[] DocumentEventsHandlers
		{
			get { return documentEventsHandlers ?? (documentEventsHandlers = GetDocumentEventsHandlers()); }
		}

		protected virtual IDocumentEventsHandler[] GetDocumentEventsHandlers()
		{
			return Array.Empty<IDocumentEventsHandler>();
		}

		public IDisposable InitialiseFetchStrategy()
		{
			IDisposable disposableAction = null;

			if (BusinessObject.FetchStrategy is IDocumentSupporterFetchStrategy fetchStrategy)
			{
				disposableAction = ((IExternalFetchHintSupporter)BusinessObject.Factory).SetupCreator();

				try
				{
					fetchStrategy.AddDocumentSupporterFetchHints();
				}
				catch (Exception)
				{
					disposableAction.Dispose();
					throw;
				}
			}

			return disposableAction;
		}

		/// <summary>
		/// If any of the document menus for this business object actually runs child document menus, then return the collection of
		/// Child objects that implements IDocumentSupport for a given Business Context.
		/// e.g. Consol Pre-Alert may print all Pre-Alerts of the contained shipments, then the Consol
		/// class needs to return a child collection of shipments when the Shipment business Context is passed in.
		/// </summary>
		/// <param name="menuToBeRun">The parent document / report currently being run.</param>
		/// <param name="businessContext">A logical name for the child business object that is requested by the menu item,
		/// so that the GetChildCollection implementation can decide which collection to return. </param>
		/// <param name="childCommandBeingRun">The child command currently being run, if any.</param>
		public virtual IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommandBeingRun)
		{
			return null;
		}

		/// <summary>
		/// An array of child BusinessContexts that are supported by this IDocumentSupport object, i.e. GetChildCollection method will
		/// be able to return the correct collection of IDocumentSupport objects based on these BusinessContexts.
		/// The BusinessContext of this IDocumentSupport object is always supported, you can return null by default.
		/// </summary>
		public virtual BusinessContext[] SupportedChildBusinessContexts
		{
			get { return null; }
		}

		/// <summary>
		/// Filter strings can be defined for each document menu, e.g. certain menus will only show if the transport mode is AIR.
		/// When running a document menu that has filter string defined, the document engine will ask the business object that
		/// implements IDocumentSupport as to what the current value is for a particular type of Filter. e.g. What is the value for
		/// the TransportMode (TRN) filter. The document engine will compare returned value with what's been defined in the filter and decide
		/// whether to show a menu or not.
		/// </summary>
		public virtual string GetFilterValue(DocumentFilters filterName)
		{
			string result = null;

			switch (filterName)
			{
				case DocumentFilters.CTY:
					result = Env.CurrentCompany.Country.Code;
					break;
			}

			return result;
		}

		public virtual bool MatchFilterValue(string filterValue)
		{
			return false;
		}

		/// <summary>
		/// An additional filter can be defined by overriding this method. The filter should return true in order to exclude the OrgDocument from the
		/// list of available documents for the menu item.
		/// </summary>
		public virtual bool AdditionalExcludeFilter(IStmMenuItem commandAboutToBeRun, ZGuid orgDocumentPK)
		{
			return false;
		}

		/// <summary>
		/// Use to expose that a DocumentSupporter can handle DocBuilder Invoices added in other documents.
		/// Without this flag, DocBuilder Invoice will be ignored as its filter (e.g. CTY=HideThisMenuItem) will never be applicable
		/// </summary>
		public virtual bool SupportDocBuilderInvoiceAsChildCommand
		{
			get { return false; }
		}

		/// <summary>
		/// If you need to prevent a particular document menu from being run, then return a DocumentSupporterDataState
		/// object and set its IsValid flag to false and also set an appropriate ErrorMessage for it to be displayed.
		/// </summary>
		public virtual DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			ZString errorMessage = "";

			if (commandAboutToBeRun != null)
			{
				IDocumentDataStateManager dataStateManager = ServiceLocator.GetService<IDocumentDataStateManager>(BusinessObject);
				DocumentDataStateManagerResult result = DocumentDataStateManagerResult.NotApplicable;
				if (dataStateManager != null)
				{
					result = dataStateManager.Evaluate(commandAboutToBeRun);
					if (result == DocumentDataStateManagerResult.Fail)
					{
						return new DocumentSupporterDataState(dataStateManager.IsDataStateValid, dataStateManager.DataStateErrorMessage);
					}
				}
				if (result == DocumentDataStateManagerResult.NotApplicable)
				{
					if (commandAboutToBeRun.SU_DeliveryRestrictionType == nameof(DeliveryRestrictionType.CNH) && !IsDeliveryRestrictionCheckSuspended)
					{
						errorMessage = ObjectFactory.Get<IDocumentDeliveryCreditControlManager>().GetDocumentDeliveryStatusForCreditManagement(DeliveryObject, Res.GetString("6036910c-be4e-4d91-b010-5d1dd8b89b28", "document"),
							commandAboutToBeRun.PK, true, commandAboutToBeRun.SU_DocumentDirection);
					}
					else
					{
						var documentCommand = GetDocumentCommand(commandAboutToBeRun);
						errorMessage = documentCommand?.GetDeliveryRestrictionErrorMessage(BusinessObject, DeliveryObject);
					}
				}
			}

			return new DocumentSupporterDataState(errorMessage.IsEmpty, errorMessage);
		}

		public IDisposable SuspendDeliveryRestrictionCheck() => new DisposableAction(() => documentDeliveryRestrictionCheckSuspendCount++, () => documentDeliveryRestrictionCheckSuspendCount--);

		int documentDeliveryRestrictionCheckSuspendCount;
		protected bool IsDeliveryRestrictionCheckSuspended => documentDeliveryRestrictionCheckSuspendCount > 0;

		IDocumentCommand GetDocumentCommand(IStmMenuItem commandAboutToBeRun)
		{
			if (commandAboutToBeRun is IDocumentCommand documentCommand)
			{
				return documentCommand;
			}
			if (commandAboutToBeRun != null)
			{
				return Factory.Load<IDocumentCommand>(commandAboutToBeRun.PK);
			}
			return null;
		}

		protected virtual BusinessObject DeliveryObject => BusinessObject;

		/// <summary>
		/// For each pivot to be rendered, this method will be called to get any overriding document titles
		/// from the business object. If you return null, then a single copy of this template will be rendered
		/// using the title specified on the pivot. If you return an array of e.g. 2 titles, then 2 copies of
		/// the template will be rendered using the 2 titles provided.
		/// </summary>
		public virtual TitleCopyCountPair GetDocumentTitlesForPivot(ZString parentDocumentMenuName, IDocumentSupportable parentBusinessObject, IStmMenuTemplatePivot pivot)
		{
			return null;
		}

		/// <summary>
		/// Based on the FilterType sepcified on the Pivot and the DocWrapper given, return the value your business object
		/// has for this filter type. This string value will be used to compared to what's required
		/// by the Pivot. If the value match, then the Pivot record will be included in the DocumentPack,
		/// otherwise this template the Pivot points to will not be printed.
		/// E.g. if you are setting up a pivot on consol to print shipment template, you can return a filter value based on
		/// the shipment doc wrapper that's passed to you.
		/// </summary>
		public virtual string GetMenuTemplateFilterValue(MenuTemplateFilterType filterType, IBODocDataProvider dataProvider)
		{
			string result = null;

			switch (filterType)
			{
				case MenuTemplateFilterType.PrintStandard:
					result = ZBool.True.ToString();
					break;

				case MenuTemplateFilterType.PrintClientSpecific:
					result = ZBool.False.ToString();
					break;

				default:
					result = ZBool.True.ToString();
					break;
			}

			return result;
		}

		/// <summary>
		/// For auto delivery, the document engine may deliver to different contacts based on the transport mode.
		/// return "" if your business object doesn't support TransportMode.
		/// </summary>
		public virtual string TransportMode
		{
			get { return ""; }
		}

		/// <summary>
		/// For auto delivery, the document engine may deliver to different contacts based on the transport mode + Container Mode.
		/// return "" if your business object doesn't support ContainerMode.
		/// </summary>
		public virtual string ContainerMode
		{
			get { return ""; }
		}

		/// <summary>
		/// The local port that relates to this document. If not applicable, return ""
		/// eg: For an import, local port is the port of discharge
		/// </summary>
		public virtual string LocalPort(IContactType contactType, DocumentDirection direction)
		{
			return "";
		}

		/// <summary>
		/// The foreign port that relates to this document. If not applicable, return ""
		/// eg: For an import, foreign port is the port of loading
		/// </summary>
		public virtual string ForeignPort(IContactType contactType, DocumentDirection direction)
		{
			return "";
		}

		public virtual string RelatedBranch => "";

		public virtual string RelatedCompany => "";

		public virtual string RelatedDepartment => "";

		/// <summary>
		/// The import / export status that relates to this document
		/// </summary>
		public virtual bool IsImport
		{
			get { return false; }
		}

		/// <summary>
		/// For auto delivery, Document menus can have a contact type to be defined as the auto delivery target, e.g. the Consignee.
		/// When document engine finds a contact type defined for a document, it will request the IDocumentSupport object for either the
		/// organisation or contact for the given ContactType. So if you document has a contact type defined for auto delivery, make sure you implement
		/// this method.
		/// </summary>
		public virtual IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return null;
		}

		/// <summary>
		/// This method is used for handling the override contact introduced in the invoice, the override contact should be added to the existing list of contacts.
		/// </summary>
		/// <returns></returns>
		public virtual IOrgContact GetAdditionalDeliveryContact()
		{
			return null;
		}

		/// <summary>
		/// This method is used to determine the organisation that will be used for branding (when branding is enabled).
		public virtual IOrgHeader GetBrandedOrganisation(IContactType contactType, DocumentDirection direction)
		{
			return null;
		}

		/// <summary>
		/// On consumers where the organisation / contact / address details can be overridden (ie not a 'real' organisation),
		/// you must return the JobDocAddress object that contains these overridden details.
		/// These details will then be used for the document delivery as well as the recipient on the document itself.
		/// </summary>
		public virtual IDocAddress GetOverriddenDeliveryDetails(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			return null;
		}

		public virtual IOrgContact GetOverriddenDeliveryContact(IStmMenuItem menuItem)
		{
			return null;
		}

		/// <summary>
		/// The Document Field Menu exposes document wrapper properties to users so that they can customise their document.
		/// In GetWrapperTypesForDocumentFieldMenu return all the types that will have their properties exposed. The menu will
		/// not be displayed if this property is null.
		/// </summary>
		public virtual Type[] GetWrapperTypesForDocumentFieldMenu
		{
			get { return null; }
		}

		/// <summary>
		/// If there is no client or agent branding then use the branding returned by this method.
		/// Returns a client and agent document branding object to use or null if the system defaults should be used.
		/// </summary>
		public virtual ClientAndAgentBrandingBusinessObject GetAlternativeBranding()
		{
			return null;
		}

		/// <summary>
		/// Returns menu item for getting visualisation data from menu item's note.
		/// If copy document exists its menu item will be returned, otherwise original document menu item.
		/// </summary>
		public virtual IStmMenuItem GetMenuItemForVisualisationData(IStmMenuItem originalMenuItem)
		{
			return null;
		}

		public virtual bool StorageDocsAreEditableIfInRelated { get { return false; } }

		/// <summary>
		/// When the reason for not printing is handled, return false.
		/// </summary>
		public virtual ZBool ShowReasonForNotPrinting(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return true;
		}

		public virtual bool ShowDocumentsInDynamicMenu
		{
			get { return true; }
		}

		public virtual MultilingualString CustomWatermarkText => null;

		public virtual MultilingualString GetCustomWatermarkText(IDocumentCommand documentCommand, IBODocDataProvider docDataProvider) => null;

		public ZString GetEncryptedPDFPassword(DeliverableInfo deliverableInfo)
		{
			var password = GetPDFPasswordCore(deliverableInfo);
			if (!password.IsEmpty)
			{
				var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
				// http://www.pdfsharp.net/wiki/ProtectDocument-sample.ashx
				// We use PDF Sharp to encrypt, and it use Encrypted128Bit as recommended level.
				// So it allows at most 32 characters for password in plain text.
				return encoder.Encrypt(password.Substring(0, 32));
			}
			return ZString.Empty;
		}

		protected virtual ZString GetPDFPasswordCore(DeliverableInfo deliverableInfo) => null;

		#endregion

		#region Implementation

		public IBODocDataProvider[] GetBODocDataProviders(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if (BusinessObject == null)
			{
				return null;
			}

			if (dataContextValue.IsBusinessObjectType)
			{
				return GetBODocDataProvidersCore(dataContextValue, commandBeingRun);
			}

			DocumentWrapper[] wrappers = GetDocumentWrappers(dataContextValue.DataContext, commandBeingRun);
			return wrappers == null ? null : Array.ConvertAll(wrappers, (DocumentWrapper wrapper) => { return BODocDataProvider.Get(wrapper); });
		}

		public virtual ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun) => ZString.Empty;

		public DocumentWrapper[] GetDocumentWrappers(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return GetDocumentWrappersInternal(dataContext, commandBeingRun)
					 ?? (from dataSource in AdditionalBODataSourceDocumentSupporters
						 where dataSource.BusinessObject != null
						 let docWrappers = dataSource.GetDocumentWrappers(dataContext, commandBeingRun)
						 where docWrappers != null
						 select docWrappers).FirstOrDefault();
		}

		public List<DocumentSupporterQuestion> GenerateQuestionsToAskUsersBeforeRunningDocument(IStmMenuItem commandAboutToBeRun)
		{
			return GenerateQuestionsToAskUsersBeforeRunningDocumentCore(commandAboutToBeRun);
		}

		internal List<DataContextValue> SupportedDataContexts
		{
			get
			{
				List<DataContextValue> result = new List<DataContextValue>();
				result.AddRange(GetSupportedDocWrapperDataSources());
				result.AddRange(GetSupportedBODataSources());
				foreach (DocumentSupporter additionalBODataSource in AdditionalBODataSourceDocumentSupporters)
				{
					result.AddRange(additionalBODataSource.GetSupportedBODataSources());
				}
				return result;
			}
		}

		List<DataContextValue> GetSupportedDocWrapperDataSources()
		{
			List<DataContextValue> result = new List<DataContextValue>();
			foreach (DataContext dataContext in GetSupportedDataContexts())
			{
				result.Add(new DataContextValue(dataContext));
			}
			return result;
		}

		protected virtual List<DataContextValue> GetSupportedBODataSources()
		{
			return GetSupportedBODataSourcesFor(GetNonClientSpecificBOType());
		}

		Type GetNonClientSpecificBOType()
		{
			Type parentBOType = BusinessObject.GetType();
			while (parentBOType.Namespace.StartsWith("Enterprise.Client."))
			{
				parentBOType = parentBOType.BaseType;
			}
			return parentBOType;
		}

		protected virtual IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var specificBOType = BusinessObject is ITopLevelBusinessEntityForDocSup ? ((ITopLevelBusinessEntityForDocSup)BusinessObject).TopLevelBusinessEntity : GetNonClientSpecificBOType();

			return (dataContextValue.WantsBusinessObjectOfType(specificBOType) ? new IBODocDataProvider[] { BODocDataProvider.Get(BusinessObject) } : null);
		}

		protected List<DataContextValue> GetSupportedBODataSourcesFor(Type supportedBODataSource)
		{
			List<DataContextValue> result = new List<DataContextValue>();
			string[] typeNameSegments = supportedBODataSource.ToString().Split('.');
			string endOfType = "";
			for (int segmentIndex = typeNameSegments.Length - 1; segmentIndex > 0; segmentIndex--)
			{
				endOfType = "." + typeNameSegments[segmentIndex] + endOfType;
				if (endOfType.Length > StmTemplateSchema.SO_DataContext.MaxLength)
				{
					break;
				}

				result.Add(new DataContextValue(endOfType, supportedBODataSource));
			}
			return result;
		}

		IBODocDataProvider[] GetBODocDataProvidersCore(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			IBODocDataProvider[] result = GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
			if (result == null)
			{
				foreach (DocumentSupporter additionalBODataSource in AdditionalBODataSourceDocumentSupporters)
				{
					if (additionalBODataSource.BusinessObject != null)
					{
						result = additionalBODataSource.GetBODocDataProvidersInternal(dataContextValue, commandBeingRun);
						if (result != null)
						{
							return result;
						}
					}
				}
			}
			return result;
		}

		public DataContext DefaultDataContext
		{
			get { return SupportedDataContexts != null && SupportedDataContexts.Count >= 1 ? SupportedDataContexts[0].DataContext : DataContext.None; }
		}

		public bool IsDataContextSupported(DataContextValue dataContextValue)
		{
			return SupportedDataContexts.Contains(dataContextValue);
		}

		public string CommaSeparatedListOfSupportedDataContexts
		{
			get
			{
				ZStringBuilder result = new ZStringBuilder();
				foreach (DataContextValue dataContext in SupportedDataContexts)
				{
					result.Append(dataContext.FullDataContext);
				}
				return result.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		public CodeDescriptionPairList ListOfSupportedDataContexts
		{
			get
			{
				CodeDescriptionPairList result = new UntranslatableCodeDescriptionPairList((NoResString)"Data context names cannot be translated");
				foreach (DataContextValue dataContext in SupportedDataContexts)
				{
					result.AddPair(dataContext.FullDataContext);
				}
				return result;
			}
		}

		public ZQuery FilterForSupportedDataContexts
		{
			get
			{
				ZQuery result = new ZQuery(StmTemplateSchema.SO_DataContext, Array.ConvertAll(SupportedDataContexts.ToArray(), dataContext => dataContext.FullDataContext));
				return result;
			}
		}

		protected virtual List<DocumentSupporter> AdditionalBODataSourceDocumentSupporters
		{
			get { return new List<DocumentSupporter>(); }
		}

		/// <summary>
		/// Hack, will need to find a better way to implement custom print task handling in a consistent way
		/// for AutoDocumentDeliveryJobs and standard PrintTasks.
		/// </summary>
		/// <param name="printTask"></param>
		public virtual void SetupDeliveryForAutoDocumentDeliveryJob(DocumentEngineIntegration.IPrintTask printTask)
		{
		}

		public void CustomizeApplicableMenusFilter(ZQuery filter)
		{
			CustomizeApplicableMenusFilterCore(filter);
		}

		protected virtual void CustomizeApplicableMenusFilterCore(ZQuery filter)
		{
		}

		public bool IgnoreHasChanges
		{
			get { return IgnoreHasChangesCore(); }
		}

		protected virtual bool IgnoreHasChangesCore()
		{
			return false;
		}

		#endregion
	}
}
