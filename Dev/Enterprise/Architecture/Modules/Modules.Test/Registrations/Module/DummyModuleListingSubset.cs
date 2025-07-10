using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Core.Environment;
using Enterprise.Core.Modules;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public static class DummyModuleIDs
	{
		public static readonly ModuleIdentifier Dummy = new ModuleIdentifier(ModuleId.Dummy, (NoResString)"Test Module");
		public static readonly ModuleIdentifier Dummy2 = new ModuleIdentifier(ModuleId.Dummy2, (NoResString)"Test Module2");
		public static readonly ModuleIdentifier Dummy3 = new ModuleIdentifier(ModuleId.Dummy3, (NoResString)"Test Module3");
		public static readonly ModuleIdentifier DummyWithExtendedDescription = new ModuleIdentifier(ModuleId.DummyWithExtendedDescription, (NoResString)"Test Module3", (NoResString)"Test Module3 (Extended)");
		public static readonly ModuleIdentifier DummyDependent = new ModuleIdentifier(ModuleId.DummyDependent, (NoResString)"Test Dependent Module");
		public static readonly ModuleIdentifier DummyDependentWithCode = new ModuleIdentifier(ModuleId.DummyDependentWithCode, (NoResString)"Test Dependent With Code Module");
		public static readonly ModuleIdentifier DummyWithNoGLOWSupport = new ModuleIdentifier(ModuleId.DummyWithNoGLOWSupport, (NoResString)"Test Module With No Glow Support");
		public static readonly ModuleIdentifier DummyNoPopup = new ModuleIdentifier(ModuleId.DummyNoPopup, (NoResString)"Test Module");
		public static readonly ModuleIdentifier DummyWithTemplates = new ModuleIdentifier(ModuleId.DummyWithTemplates, (NoResString)"Test Module With Templates");
		public static readonly ModuleIdentifier DummyThatHitsFilterBizoOnDispose = new ModuleIdentifier(ModuleId.DummyThatHitsFilterBizoOnDispose, (NoResString)"Test Module That Hits FilterBusinessObject on Dispose");
	}

	public static class DummyControllerIDs
	{
		public static readonly ControllerID Dummy = new ControllerID("Dummy");
		public static readonly ControllerID Dummy1 = new ControllerID("Dummy1");
		public static readonly ControllerID Dummy2 = new ControllerID("Dummy2");
		public static readonly ControllerID Dummy3 = new ControllerID("Dummy3");
		public static readonly ControllerID Dummy4 = new ControllerID("Dummy4");
		public static readonly ControllerID DummyControllerWithGetNewTopLevelMenuException = new ControllerID("DummyControllerWithGetNewTopLevelMenuException");
		public static readonly ControllerID DummyControllerWithCancellableBizO = new ControllerID("DummyControllerWithCancellableBizO");
		public static readonly ControllerID DummyControllerwithCancellableWhichCanNotBeDeleted = new ControllerID("ControllerIDs.DummyControllerwithCancellableWhichCanNotBeDeleted");
		public static readonly ControllerID DummyControllerCancellableHandlingDeleteError = new ControllerID("ControllerIDs.DummyControllerCancellableHandlingDeleteError");
		public static readonly ControllerID DummyControllerWhichAllowsDelete = new ControllerID("DummyControllerWhichAllowsDelete");
		public static readonly ControllerID DummyControllerWhichDoesNotAllowDelete = new ControllerID("DummyControllerWhichDoesNotAllowDelete");
		public static readonly ControllerID DummyControllerWithTemplateModule = new ControllerID("DummyControllerWithTemplateModule");
		public static readonly ControllerID DummyControllerNoTabControl = new ControllerID("DummyControllerNoTabControl");
		public static readonly ControllerID DummyControllerWithModuleGuiNotSupportedException = new ControllerID("DummyControllerWithModuleGuiNotSupportedException");
		public static readonly ControllerID DummyDependent = new ControllerID("DummyDependent");
		public static readonly ControllerID DummyDependentWithCode = new ControllerID("DummyDependentWithCode");
		public static readonly ControllerID DummyForOA = new ControllerID("DummyForOA");
		public static readonly ControllerID DummyForPlugIn = new ControllerID("DummyForPlugIn");
		public static readonly ControllerID DummyControllerNotSupportsHyperlinking = new ControllerID("DummyControllerNotSupportsHyperlinking");
		public static readonly ControllerID eDocsPlugInForTesting = new ControllerID("eDocsPlugInForTesting");
		public static readonly ControllerID DummyControllerWithGetNewTopMenuInPlugInIsExistingMenu = new ControllerID("DummyControllerWithGetNewTopMenuInPlugInIsExistingMenu");
		public static readonly ControllerID DummyControllerWithPlugInAndNavigationProvider = new ControllerID("DummyControllerWithPlugInAndNavigationProvider");
	}

	public class DummyModuleListingSubset : IModuleListingSubset
	{
		public IEnumerable<ModuleIdentifier> ModuleIdentifiers => [
			DummyModuleIDs.Dummy,
			DummyModuleIDs.Dummy2,
			DummyModuleIDs.Dummy3,
			DummyModuleIDs.DummyWithExtendedDescription,
			DummyModuleIDs.DummyDependent,
			DummyModuleIDs.DummyDependentWithCode,
			DummyModuleIDs.DummyWithNoGLOWSupport,
			DummyModuleIDs.DummyNoPopup,
			DummyModuleIDs.DummyWithTemplates,
			DummyModuleIDs.DummyThatHitsFilterBizoOnDispose
			];

		public IEnumerable<ModuleInfo> ModuleInfos => [
			new ModuleInfo(DummyModuleIDs.Dummy, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyFilterGridModule", new TableRegistrationInfo(DummyBizoSchema.Constants.TableName)),
			new ModuleInfo(DummyModuleIDs.Dummy2, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyFilterGridModule"),
			new ModuleInfo(DummyModuleIDs.Dummy, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyFilterGridModule", Constants.CountryCodes._TemplateCountryName_, (NoResString)"Test Template Module"),
			new ModuleInfo(DummyModuleIDs.DummyDependent, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyDependentModule"),
			new ModuleInfo(DummyModuleIDs.DummyDependentWithCode, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyDependentWithCodeModule"),
			new ModuleInfo(DummyModuleIDs.DummyWithNoGLOWSupport, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyFilterGridModuleWithCollectionTypeNotSupportedByGLOW"),
			new ModuleInfo(DummyModuleIDs.Dummy3, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyFilterGridModule", Constants.CountryCodes._TemplateCountryName_, new TableRegistrationInfo(DummyBizoSchema.Constants.TableName)),
			new ModuleInfo(DummyModuleIDs.DummyNoPopup, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyModule", new TableRegistrationInfo(DummyBizoSchema.Constants.TableName)),
			new ModuleInfo(DummyModuleIDs.DummyWithTemplates, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyFilterGridModuleWithTemplates", new TableRegistrationInfo(DummyBizoSchema.Constants.TableName)),
			new ModuleInfo(DummyModuleIDs.DummyThatHitsFilterBizoOnDispose, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyFilterGridModuleThatHitsFilterBusinessObjectOnDispose", new TableRegistrationInfo(DummyBizoSchema.Constants.TableName)),
			];

		public IEnumerable<ControllerInfo> ControllerInfos => [
			new ControllerInfo(DummyControllerIDs.Dummy, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyController"),
			new ControllerInfo(DummyControllerIDs.Dummy1, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyController1"),
			new ControllerInfo(DummyControllerIDs.Dummy2, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyController2"),
			new ControllerInfo(DummyControllerIDs.Dummy3, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyController3"),
			new ControllerInfo(DummyControllerIDs.Dummy4, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyControllerWithMakeUrlsOnlyOpenableForCurrentCompanySetToTrue"),
			new ControllerInfo(DummyControllerIDs.DummyControllerWithGetNewTopLevelMenuException, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyControllerWithGetNewTopLevelMenuException"),
			new ControllerInfo(DummyControllerIDs.DummyControllerWhichAllowsDelete, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyControllerWhichAllowsDelete"),
			new ControllerInfo(DummyControllerIDs.DummyControllerWhichDoesNotAllowDelete, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyControllerWhichDoesNotAllowDelete"),
			new ControllerInfo(DummyControllerIDs.DummyControllerWithTemplateModule, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyControllerWithTemplateModule"),
			new ControllerInfo(DummyControllerIDs.DummyControllerNoTabControl, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyControllerNoTabControl"),
			new ControllerInfo(DummyControllerIDs.DummyControllerWithModuleGuiNotSupportedException, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyControllerWithModuleGuiNotSupportedException"),
			new ControllerInfo(DummyControllerIDs.DummyControllerWithCancellableBizO, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyControllerWithCancellableBizO"),
			new ControllerInfo(DummyControllerIDs.DummyControllerwithCancellableWhichCanNotBeDeleted, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyControllerwithCancellableWhichCanNotBeDeleted"),
			new ControllerInfo(DummyControllerIDs.DummyControllerCancellableHandlingDeleteError, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyControllerCancellableHandlingDeleteError"),
			new ControllerInfo(DummyControllerIDs.DummyDependent, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyDependentController"),
			new ControllerInfo(DummyControllerIDs.DummyDependentWithCode, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyDependentWithCodeController"),
			new ControllerInfo(DummyControllerIDs.DummyForOA, "Enterprise.Services.OperationalActions.Business.Test", "Enterprise.Services.OperationalActions.Business.Testing.DummyControllerForOATest"),
			new ControllerInfo(DummyControllerIDs.DummyForPlugIn, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyControllerForPlugInTest"),
			new ControllerInfo(DummyControllerIDs.DummyControllerNotSupportsHyperlinking, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyControllerNotSupportsHyperlinking"),
			new ControllerInfo(DummyControllerIDs.eDocsPlugInForTesting, "Enterprise.DocumentScanning.GUI.Test", "Enterprise.DocumentScanning.PlugIn.eDocsPlugInControllerForTesting"),
			new ControllerInfo(DummyControllerIDs.DummyControllerWithGetNewTopMenuInPlugInIsExistingMenu, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyControllerWithGetNewTopMenuInPlugInIsExistingMenu"),
			new ControllerInfo(DummyControllerIDs.DummyControllerWithPlugInAndNavigationProvider, "Enterprise.ZArchitecture.GUI.Test", "Enterprise.ZArchitecture.Modules.Testing.DummyControllerWithPlugInAndNavigationProvider"),
		];

		public void InitializeSecurityCheckpoints(IZSecurity security)
		{
			if (!InitializeDummyModule.Value)
			{
				return;
			}

			security.AddCheckPoint(new CheckpointLookupKey("Dummy"), Mock.Of<ISecurityCheckpoint>(c => c.Code == "Dummy"));
		}

		public void InitializeModuleTree(ModuleTreeCategories moduleTreeCategories, IZSecurity security)
		{
			if (!InitializeDummyModule.Value)
			{
				return;
			}

			var section = new ModuleSection("Dummy", (NoResString)"Added in DummyModuleListingSubset", "DUM", null, IconTypes.TreeView, IconTypes.TreeView);
			var moduleMock = new Mock<IMainFormModule>();
			moduleMock.Setup(m => m.ID).Returns("Dummy");
			moduleMock.Setup(m => m.ParentSection).Returns(section);
			section.Modules.Add(moduleMock.Object);
			moduleTreeCategories.Jump.Sections.Add(section);
		}

		public static readonly Overridable<bool> InitializeDummyModule = new(false);
	}
}
