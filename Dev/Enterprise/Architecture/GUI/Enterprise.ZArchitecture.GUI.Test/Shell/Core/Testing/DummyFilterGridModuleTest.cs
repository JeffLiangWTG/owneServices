using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using CargoWise.DataTransfer;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Modules;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public interface IDefaultActionCounter
	{
		int DefaultActionsCount { get; }
	}

	interface IDummyFilterModule : IDisposable
	{
		DummyFilterBusinessObject FilterBusinessObject { get; }
		DummyController LastController { get; }
		Control EmbeddedControl { get; }
		IModuleDecisionProvider ModuleDecisionProvider { get; set; }
		IBusinessObjectCollection GridList { get; }
		IBusinessObjectCollection Collection { get; }
		Type ExpectedModuleDecisionProviderType { get; }
		IModuleDecisionProvider GetModuleDecisionProviderForFindBox();
		IModuleDecisionProvider GetModuleDecisionProviderForFindBoxPopup();

		BusinessObjectFactory Factory { get; }

		void ExportToExcel();
		void SetInitialCodeForSearch(string code);
		IZForm ShowNewForm();
		DummyFilterBusinessObject GetNewFilterBusinessObject();

		void SetAllowEdit(bool value);
		void SetAllowDelete(bool value);
		void SetAllowView(bool value);
		object GetContextMenuItemByText(string text);
		void ModifyMaxRowsToLoad(int maxRowsToLoad);
		void SetAllowToggleFilterVisibilityMenuItem(bool value);

		event CollectionLoadingEventHandler LoadingCollection;
		event CollectionLoadedEventHandler LoadedCollection;
	}

	public delegate void CollectionLoadingEventHandler(BusinessObjectFactory factory);
	public delegate void CollectionLoadedEventHandler(BusinessObjectFactory factory);

	public class DummyFilterGridModule : ZFilterGridModule, IDummyFilterModule
	{
		public void StartAutoRefreshExposed(byte timeout) => StartAutoRefresh(timeout);

		public void StopAutoRefreshExposed(AutoRefreshWarningType warningType) => StopAutoRefresh(warningType);

		public Timer AutoRefreshTimerExposed => AutoRefreshTimer;

		public void fAutoRefreshTimer_TickExposed(object sender, EventArgs e) => fAutoRefreshTimer_Tick(sender, e);

		public ZFilterGridMenuItem AutoRefreshMenuItemExposed => AutoRefreshMenuItem;

		public void ModifyMaxRowsToLoad(int maxRowsToLoad)
		{
			maxRowsOverride = maxRowsToLoad;
			SearchManager.MaxRowsToLoad = maxRowsToLoad;
		}

		protected internal override int MaxRowsToLoad => maxRowsOverride ?? base.MaxRowsToLoad;
		int? maxRowsOverride;

		protected override PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			OnCollectionLoading(factory);
			var result = base.LoadCollection(factory, type, query);
			OnCollectionLoaded(result.Factory);
			return result;
		}

		public event CollectionLoadingEventHandler LoadingCollection;
		public event CollectionLoadedEventHandler LoadedCollection;

		void OnCollectionLoading(BusinessObjectFactory factory)
		{
			if (LoadingCollection != null)
			{
				LoadingCollection(factory);
			}
		}

		public PerformSearchResult SearchResultBoundToGrid { get; private set; }

		protected override void PushItemsIntoCollectionCore(IBusinessObjectCollection collection, PerformSearchResult searchResult, SortInfo sort)
		{
			SearchResultBoundToGrid = searchResult;
			base.PushItemsIntoCollectionCore(collection, searchResult, sort);
		}

		public override ZString DefaultMessageOverridingSecurityRightMessage
		{
			get
			{
				return fDefaultMessageOverridingSecurityRightMessage;
			}
			set
			{
				fDefaultMessageOverridingSecurityRightMessage = value;
			}
		}
		ZString fDefaultMessageOverridingSecurityRightMessage;

		void OnCollectionLoaded(BusinessObjectFactory factory)
		{
			if (LoadedCollection != null)
			{
				LoadedCollection(factory);
			}
		}

		public PerformSearchResult LoadCollectionExposed(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			return LoadCollection(factory, type, query);
		}

		internal interface IDummyFilterGridModuleListener
		{
			void SetReference(DummyFilterGridModule reference);
		}

		public DummyFilterGridModule()
		{
			if (DummyFilterGridModuleListener != null)
			{
				DummyFilterGridModuleListener.SetReference(this);
			}
		}

		internal static IDummyFilterGridModuleListener DummyFilterGridModuleListener;

		public static ZQuery Filter = new ZQuery(); // for testing only

		public void ExportToExcel()
		{
			base.HandleExportClick(null, EventArgs.Empty);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new DummyFilterControl(GridCollection, FilterBusinessObject);
		}

		protected internal override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			LastController = (DummyController)ZControllerFactory.Create(DummyControllerIDs.Dummy);
			return LastController;
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new DummyBusinessObjectCollection(Factory, Filter);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			DummyFilterBusinessObject = new DummyFilterBusinessObject();
			return DummyFilterBusinessObject;
		}

		public DummyFilterBusinessObject DummyFilterBusinessObject;

		public override ModuleIdentifier ID
		{
			get { return IDOverride ?? DummyModuleIDs.Dummy; }
		}

		public ModuleIdentifier IDOverride { get; set; }

		public override string WorkflowType
		{
			get { return WorkflowTypeOverride ?? base.WorkflowType; }
		}

		public string WorkflowTypeOverride { get; set; }

		#region AllowNew

		public override bool AllowNew
		{
			get { return fAllowNew; }
		}

		public static void SetAllowNew(bool value)
		{
			fAllowNew = value;
		}

		public static void ResetAllowNewToDefault()
		{
			fAllowNew = true;
		}

		static bool fAllowNew = true;

		#endregion

		#region AllowAdvancedDataAutomationWizard

		protected override bool AllowAdvancedDataAutomationWizard
		{
			get { return fAllowAdvancedDataAutomationWizard; }
		}

		public void SetAllowAdvancedDataAutomationWizard(bool value)
		{
			fAllowAdvancedDataAutomationWizard = value;
		}

		bool fAllowAdvancedDataAutomationWizard = fAllowNew;

		#endregion

		public void SetAllowDefaultActivateDeactivate(bool value)
		{
			allowDefaultActivateDeactivate = value;
		}

		bool allowDefaultActivateDeactivate;
		public override bool AllowDefaultActivateDeactivate
		{
			get
			{
				return allowDefaultActivateDeactivate;
			}
		}

		bool fAllowEdit = true;
		public override bool AllowEdit
		{
			get { return fAllowEdit; }
		}
		public void SetAllowEdit(bool value)
		{
			fAllowEdit = value;
		}

		bool fAllowView = true;
		public override bool AllowView
		{
			get { return fAllowView; }
		}
		public void SetAllowView(bool value)
		{
			fAllowView = value;
		}

		bool fAllowDelete = true;
		public override bool AllowDelete
		{
			get { return fAllowDelete; }
		}
		public void SetAllowDelete(bool value)
		{
			fAllowDelete = value;
		}

		public void HandleDeleteClick_Exposed(object sender, EventArgs e)
		{
			HandleDeleteClick(sender, e);
		}

		public void SetCanBeCopied(bool value)
		{
			canBeCopied = value;
		}
		protected override bool CanBeCopied()
		{
			return canBeCopied ?? base.CanBeCopied();
		}
		bool? canBeCopied;

		public void SetCanBeReversed(bool value)
		{
			canBeReversed = value;
		}
		protected override bool CanBeReversed()
		{
			return canBeReversed ?? base.CanBeReversed();
		}
		bool? canBeReversed;

		public void AddExportDataItemForTest(string caption, EventHandler handler)
		{
			base.AddExportDataMenuItem(caption, handler);
		}

		public void AddImportDataItemForTest(string caption, EventHandler handler)
		{
			base.AddImportDataMenuItem(caption, handler);
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return DummyCheckPointWithSecuritySet; }
		}

		bool fAllowToggleFilterVisibilityMenuItem = true;
		public override bool AllowToggleFilterVisibilityMenuItem
		{
			get { return fAllowToggleFilterVisibilityMenuItem; }
		}
		public void SetAllowToggleFilterVisibilityMenuItem(bool value)
		{
			fAllowToggleFilterVisibilityMenuItem = value;
		}

		DummyCheckPointWithSecuritySet DummyCheckPointWithSecuritySet
		{
			get
			{
				if (fDummyCheckPointWithSecuritySet == null)
				{
					fDummyCheckPointWithSecuritySet = new DummyCheckPointWithSecuritySet(LaunchFromMainFormIsAllowed);
				}
				return fDummyCheckPointWithSecuritySet;
			}
		}

		DummyCheckPointWithSecuritySet fDummyCheckPointWithSecuritySet;

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static ZBool LaunchFromMainFormIsAllowed = false;
		public static void ResetLaunchFromMainFormIsAllowedToDefault()
		{
			LaunchFromMainFormIsAllowed = false;
		}

		public override SecurityCheckpoint[] GetSecurityCheckpointForPopups()
		{
			return new[] { DummyCheckPointWithSecuritySetForPopups };
		}

		DummyCheckPointWithSecuritySet DummyCheckPointWithSecuritySetForPopups
		{
			get
			{
				if (fDummyCheckPointWithSecuritySetForPopups == null)
				{
					fDummyCheckPointWithSecuritySetForPopups = new DummyCheckPointWithSecuritySet(PopupsAreAllowed);
				}
				return fDummyCheckPointWithSecuritySetForPopups;
			}
		}

		DummyCheckPointWithSecuritySet fDummyCheckPointWithSecuritySetForPopups;

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		public static ZBool PopupsAreAllowed = true;
		public static void ResetPopupsAreAllowedToDefault()
		{
			PopupsAreAllowed = true;
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return (LicenceCheckpoint)EnvProxy.Instance.Licence.Core; }
		}

		public static ZBool HasActionsExposed = true;

		public override ZBool HasActions
		{
			get { return HasActionsExposed; }
		}

		public FilterModuleMenuItemDescriptorCollection ImportMenuItemsForTest
		{
			get { return base.ImportMenuItems; }
		}

		public FilterModuleMenuItemDescriptorCollection ExportMenuItemsForTest
		{
			get { return base.ExportMenuItems; }
		}

		public MenuItem[] UniversalDataTransferMenuItemsForTest { get; set; }

		protected override MenuItem[] GetUniversalDataTransferMenuItems()
		{
			return UniversalDataTransferMenuItemsForTest ?? base.GetUniversalDataTransferMenuItems();
		}

		public MenuItem[] ContextMenuExposed { get { return ContextMenu; } }

		public new DummyController LastController;

		public override BusinessObject[] GetSelectedBusinessObjects()
		{
			return SelectedBusinessObjectsOverride ?? base.GetSelectedBusinessObjects();
		}

		protected override BusinessObject[] SelectedBusinessObjects
		{
			get { return SelectedBusinessObjectsOverride ?? base.SelectedBusinessObjects; }
		}

		public BusinessObject[] SelectedBusinessObjectsOverride { get; set; }

		public void SetAllowExcelExport(bool allowExcelExport)
		{
			(this.ModuleDecisionProvider as DummeyModuleDecisionProvider).SetAllowExcelExport(allowExcelExport);
		}

		protected class DummeyModuleDecisionProvider : DefaultModuleDecisionProvider, IDefaultActionCounter
		{
			public DummeyModuleDecisionProvider(ZFilterGridModule module)
				: base(module)
			{
				DefaultActionsCount = 0;
			}

			public override void HandleDefaultAction(BusinessObject[] selectedBusinessObjects)
			{
				DefaultActionsCount = selectedBusinessObjects.Length;
			}

			public int DefaultActionsCount { get; set; }

			public override bool AllowExcelExport
			{
				get { return fAllowExcelExport; }
			}
			bool fAllowExcelExport = true;

			public void SetAllowExcelExport(bool allowExcelExport)
			{
				fAllowExcelExport = allowExcelExport;
			}
		}

		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider()
		{
			return new DummeyModuleDecisionProvider(this);
		}

		#region Menu Items

		public MenuItem[] GetNewStandardMenuItemsExposed()
		{
			return GetNewStandardMenuItems();
		}
		public MenuItem[] AddUniversalCopyMenuItemsExposed()
		{
			var result = base.GetNewStandardMenuItems();
			var newMenuItem = result.FindByText("&New");

			if (newMenuItem != null)
			{
				newMenuItem.MenuItems.Add("Universal Copy 1").DefaultItem = true;
				newMenuItem.MenuItems.Add("Universal Copy 2");
			}
			return result;
		}

		public MenuItem[] GetNewAdditionalMenuItemsExposed()
		{
			return GetNewAdditionalMenuItems();
		}

		public MenuItem[] GetNewActionMenuItemsExposed()
		{
			return GetNewActionMenuItems();
		}

		public readonly List<MenuItem> ActionMenuItems = new List<MenuItem>();

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			result.AddRange(ActionMenuItems);
			return result.ToArray();
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var result = base.GetNewStandardMenuItems();
			var newMenuItem = result.FindByText("&New");

			if (newMenuItem != null)
			{
				newMenuItem.MenuItems.Add("Default Item").DefaultItem = true;
			}

			return result;
		}

		#endregion

		#region IDummyFilterModule Members

		DummyFilterBusinessObject IDummyFilterModule.FilterBusinessObject
		{
			get { return (DummyFilterBusinessObject)FilterBusinessObject; }
		}

		DummyController IDummyFilterModule.LastController
		{
			get { return LastController; }
		}

		IModuleDecisionProvider IDummyFilterModule.ModuleDecisionProvider
		{
			get { return ModuleDecisionProvider; }
			set { OverrideModuleDecisionProvider(value); }
		}

		IBusinessObjectCollection IDummyFilterModule.GridList
		{
			get { return (BusinessObjectCollection)Grid.List; }
		}

		IBusinessObjectCollection IDummyFilterModule.Collection
		{
			get { return GridCollection; }
		}

		Type IDummyFilterModule.ExpectedModuleDecisionProviderType
		{
			get { return typeof(DummeyModuleDecisionProvider); }
		}

		IModuleDecisionProvider IDummyFilterModule.GetModuleDecisionProviderForFindBox()
		{
			return GetModuleDecisionProviderForFindBox(new DummyFindBox());
		}

		IModuleDecisionProvider IDummyFilterModule.GetModuleDecisionProviderForFindBoxPopup()
		{
			return GetModuleDecisionProviderForFindBoxPopup(new DummyFindBox());
		}

		BusinessObjectFactory IDummyFilterModule.Factory
		{
			get { return Factory; }
		}

		void IDummyFilterModule.SetInitialCodeForSearch(string code)
		{
			SetInitialCodeForSearch(code);
		}

		IZForm IDummyFilterModule.ShowNewForm()
		{
			return ShowNewForm();
		}

		DummyFilterBusinessObject IDummyFilterModule.GetNewFilterBusinessObject()
		{
			return (DummyFilterBusinessObject)GetNewFilterBusinessObject();
		}

		object IDummyFilterModule.GetContextMenuItemByText(string text)
		{
			return ContextMenu.FindByText(text);
		}

		#endregion

		#region ModulePluginProvider

		public sealed class ModulePluginProvider : IDummyFilterGridModuleListener, IDisposable
		{
			public ModulePluginProvider(params ControllerID[] ids)
			{
				this.ids = ids;

				DummyFilterGridModule.DummyFilterGridModuleListener = this;
			}

			#region IDummyFilterGridModuleListener Members

			public void SetReference(DummyFilterGridModule reference)
			{
				foreach (var id in ids)
				{
					reference.Plugins.Add(id);
				}
			}

			#endregion

			#region IDisposable Members

			public void Dispose()
			{
				System.Threading.Interlocked.CompareExchange(
					ref DummyFilterGridModule.DummyFilterGridModuleListener, null, this);
			}

			#endregion

			readonly ControllerID[] ids;
		}

		#endregion

		public bool CheckCopySelectedRowsAllowed_Exposed()
		{
			return CheckCopySelectedRowsAllowed();
		}

		public SecurityCheckpoint ExportSecurityCheckpoint_Exposed
		{
			get
			{
				return ExportSecurityCheckpoint;
			}
		}

		internal void SetNativeXmlImportServiceForTesting(IImportService importService)
		{
			this.importService = importService;
		}

		IImportService importService;

		internal override IImportService GetNativeXmlImportService()
		{
			return importService ?? base.GetNativeXmlImportService();
		}

		internal void SetNativeXmlExportServiceForTesting(IExportService exportService)
		{
			this.exportService = exportService;
		}

		IExportService exportService;

		internal override IExportService GetNativeXmlExportService()
		{
			return exportService ?? base.GetNativeXmlExportService();
		}

		public void Activate_Exposed()
		{
			base.ActivateBO(this, EventArgs.Empty);
		}

		public void DeActivate_Exposed()
		{
			base.DeActivateBO(this, EventArgs.Empty);
		}

		protected override void ExportVisibleIntoAndOpenExcel()
		{
			throw new NotImplementedException();
		}
	}

	public class DummyFilterGridModuleWithDummyImportWizard : DummyFilterGridModule
	{
		public DummyFilterGridModuleWithDummyImportWizard(string errorMessage) : base()
		{
			this.errorMessage = errorMessage;
			mockNewMapping = new Mock<IDataTransferMapping>();
			mockNewMapping.SetupGet(m => m.Name).Returns("New Mapping");
			mockNewMapping.SetupGet(m => m.PK).Returns(Guid.Empty);
		}

		readonly string errorMessage;

		internal Type TypeOfElements { get; private set; }
		internal IDataTransferMapping Mapping { get; private set; }

		internal override string ShowImportMappingWizardCore(Type typeOfElements, IDataTransferMapping mapping)
		{
			TypeOfElements = typeOfElements;
			Mapping = mapping ?? mockNewMapping.Object;
			return errorMessage;
		}

		readonly Mock<IDataTransferMapping> mockNewMapping;
	}

	public class DummyFilterGridModuleWithCancellableBizO : DummyFilterGridModule
	{
		public DummyFilterGridModuleWithCancellableBizO(IMainForm mainForm)
		{
			mainFormForTest = mainForm;
		}

		public DummyFilterGridModuleWithCancellableBizO()
		{
			mainFormForTest = null;
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new DummyBusinessObjectCancellableCollection(Factory, Filter);
		}

		readonly IMainForm mainFormForTest;
		protected override IMainForm MainForm
		{
			get
			{
				return mainFormForTest ?? base.MainForm;
			}
		}
	}

	public class DummyFilterGridModuleThatHitsFilterBusinessObjectOnDispose : DummyFilterGridModule
	{
		protected override void Dispose(bool isDisposing)
		{
			var filterBizo = FilterBusinessObject;
			base.Dispose(isDisposing);
		}

		public override ModuleIdentifier ID => DummyModuleIDs.DummyThatHitsFilterBizoOnDispose;
	}

	public class DummyFilterGridModuleWithNoRecentItems : DummyFilterGridModule
	{
		protected override bool ShowRecentItemsCore()
		{
			return false;
		}
	}

	public class DummyFilterGridModuleWithRecentModuleID : DummyFilterGridModule
	{
		public DummyFilterGridModuleWithRecentModuleID(ModuleIdentifier overrideModuleIdentifier = null)
		{
			this.overrideModuleIdentifier = overrideModuleIdentifier;
		}

		readonly ModuleIdentifier overrideModuleIdentifier;

		protected override ModuleIdentifier GetRecentItemsModuleIDCore()
		{
			return overrideModuleIdentifier ?? DummyModuleIDs.Dummy2;
		}
	}

	public class DummyFilterGridModuleWithIBusinessObjectLoaderCollection : DummyFilterGridModuleWithRecentModuleID
	{
		public DummyFilterGridModuleWithIBusinessObjectLoaderCollection(ModuleIdentifier overrideModuleIdentifier = null) : base(overrideModuleIdentifier)
		{
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new DummyBusinessObjectCollectionWithBusinessObjectLoader(Factory);
		}
	}

	public class DummyBusinessObjectCollectionWithBusinessObjectLoader : DummyBusinessObjectCollection, IBusinessObjectLoader
	{
		public DummyBusinessObjectCollectionWithBusinessObjectLoader(BusinessObjectFactory factory) : base(factory)
		{
		}
		public bool HasBeenInvoked;

		BusinessObject IBusinessObjectLoader.Load(BusinessObjectFactory factory, ZGuid pk)
		{
			HasBeenInvoked = true;
			return null;
		}
	}

	public class DummyFilterGridModuleWithCollectionTypeNotSupportedByGLOW : DummyFilterGridModule
	{
		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new DummyLoggedBusinessObjectCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return DummyModuleIDs.DummyWithNoGLOWSupport; }
		}
	}

	public class DummyFilterGridModuleWithActiveBOC : DummyFilterGridModule
	{
		public ZQuery RelationshipFilter = new ZQuery();

		protected override IBusinessObjectCollection GetNewGridCollection() => new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, RelationshipFilter);
	}

	public class DummyLoggedBusinessObjectCollection : BusinessObjectCollection<DummyLogged>
	{
		public DummyLoggedBusinessObjectCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}

	#region DummyBusinessObjectCancellableCollection

	class DummyBusinessObjectCancellableCollection : ActiveBusinessObjectCollection<Enterprise.Core.GUI.Testing.ZFormTest.DummyCancellableWhichCanBeCancelled>
	{
		public DummyBusinessObjectCancellableCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DummyBusinessObjectCancellableCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}

	#endregion

	public class DummyFilterGridModuleWithTemplates : DummyFilterGridModule
	{
		public override ModuleIdentifier ID => DummyModuleIDs.DummyWithTemplates;

		protected override bool SupportTemplateRecords => SupportTemplateRecordsForTest;

		public bool SupportTemplateRecordsForTest { get; set; }

		protected override BusinessObject LoadFromTemplateRecordPkCore(BusinessObjectFactory localFactory, ZGuid templateRecordPk)
		{
			var templateRecordProvider = localFactory.New<DummyTemplateRecordProvider>();
			templateRecordProvider.IsTemplateRecord = true;
			templateRecordProvider.TemplateRecord = new SimpleDummyTemplateRecord { Identifier = templateRecordPk };
			return templateRecordProvider;
		}
	}

	public class SimpleDummyTemplateRecord : ITemplateRecord, IIdentified
	{
		public ZGuid Identifier { get; set; }
	}

	public class DummyFilterGridModuleWithTypeOfTopLevelBusinessTest : DummyFilterGridModule
	{
		public DummyFilterGridModuleWithTypeOfTopLevelBusinessTest(Type typeForTest)
		{
			typeOfTopLevelBusinessObjectForTest = typeForTest;
		}

		readonly Type typeOfTopLevelBusinessObjectForTest;

		protected override Type TypeOfTopLevelBusinessObjectCore => typeOfTopLevelBusinessObjectForTest;
	}
}
