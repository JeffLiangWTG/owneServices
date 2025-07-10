using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Module.Testing
{
	[TestedType(typeof(ESH7Module))]
	sealed class ESH7ModuleTest : ZModuleBasherTest
	{
		public void TestG3DeclarationMenuItem()
		{
			using (var module = new ESH7Module())
			{
				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
				AssertNotNull("actionMenuItem", actionMenuItem);

				var separator = actionMenuItem.MenuItems.FindByText("-");
				AssertNotNull("separator", separator);

				var g3DeclarationMenuItem = actionMenuItem.MenuItems.FindByText("View G3 Declarations");
				AssertNotNull("g3DeclarationMenuItem", g3DeclarationMenuItem);
			}
		}

		public void TestG3DeclarationMenuItemOnClick()
		{
			using (var module = new ESH7Module())
			{
				var actionMenuItem = module.FormActionMenu.FindByText("&Actions");
				var g3DeclarationMenuItem = actionMenuItem.MenuItems.FindByText("View G3 Declarations");

				g3DeclarationMenuItem.PerformClick();
				var lastFormShown = ZFormModaliser.LastFormShownDialogForTest;

				AssertType<EmbeddedModulePopup>(lastFormShown);
			}
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var result = (ASYCUDA.Business.AsycudaManifestHeader)base.GetNewBusinessObjectForHelperFilterTests(factory, businessObjectType);
			result.AMA_JobReference = ZString.Empty;
			return result;
		}

		protected override BusinessObject CreateValidBOForTestSpecifyWorkflowType(Type elementType)
		{
			return Factory.New<EU.H7.Business.AsycudaManifestHeader>();
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.ES.EUH7;

		protected override string CountryCode => Core.Constants.CountryCodes.Spain;
	}
}
