using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Module.CcsukAirInventoryCombinedMasterAndHouse.Testing;
using Enterprise.Customs.GB.Module.StandAloneFsrEnquiry;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.CcsukStandAloneFsrEnquiry.Testing
{
	[TestedType(typeof(StandAloneFsrEnquiryController))]
	class StandAloneFsrEnquiryControllerBasherTest : ZControllerBasherTest
	{
		public override Type ControllerToBashType
		{
			get { return typeof(StandAloneFsrEnquiryController); }
		}

		protected override Type GetBusinessObjectType()
		{
			return typeof(Ccsuk.AirCargoInventory.Messaging.StandAloneFsrEnquiry);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var message = GB.Ccsuk.AirCargoInventory.Messaging.StandAloneFsrEnquiry.MakeNewOutboundFromPayload(new Ccsuk.AirCargoInventory.Messaging.NonPersistentStandAloneFsrEnquiryForNew(Factory));
			Factory.Save();
			return message;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.GB.CcsukStandAloneFsrEnquiry;
		}

		public void TestSecurityCheckpoints()
		{
			var controller = new StandAloneFsrEnquiryController();
			var npbo = new Ccsuk.AirCargoInventory.Messaging.NonPersistentStandAloneFsrEnquiryForNew(Factory);
			var editableEdiEMessage = GetBusinessObjectThatIsInTheDatabase();
			AssertEquals(Env.Security.AirCcsukEnquiry, controller.GetCheckPointForDelete(editableEdiEMessage));
			AssertEquals(Env.Security.AirCcsukEnquiryNew, controller.GetCheckPointForNew(npbo));
			AssertEquals(Env.Security.AirCcsukEnquiryView, controller.GetCheckPointForView(editableEdiEMessage));
			AssertEquals(Env.Security.AirCcsukEnquiry, controller.GetCheckPointForEdit(editableEdiEMessage));
		}
	}

	[TestedType(typeof(StandAloneFsrEnquiryFilterStripBusinessObject))]
	class StandAloneFsrEnquiryFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestBranchAndCompanyLimit()
		{
			CcsukAirInventoryCombinedMasterAndHouseFilterStripBusinessObjectTest.RunTestBranchAndCompanyLimit<StandAloneFsrEnquiryFilterStripBusinessObject, Ccsuk.AirCargoInventory.Messaging.StandAloneFsrEnquiry>(
				delegate
				{
					var mock = Factory.NewMoq<Ccsuk.AirCargoInventory.Messaging.StandAloneFsrEnquiry>();
					mock.Protected().Setup<string>("GetMessageReferenceNumber").Returns("123465");
					return mock.Object;
				}, Factory, false);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new StandAloneFsrEnquiryFilterStripBusinessObject();
		}
	}

	[TestedType(typeof(StandAloneFsrEnquiryModule))]
	class StandAloneFsrEnquiryModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.EU.GB.CcsukStandAloneFsrEnquiry;
		}

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedKingdom;

		public void TestDatasImportButton()
		{
			GlowRegistry.Instance.EnableAdvancedDataAutomationWizard.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			using (var module = new StandAloneFsrEnquiryModuleForTest())
			{
				var throwAway = module.ContextMenuExposed;
				AssertEquals(3, module.DataTransferMenuItem.MenuItems.Count);
				AssertEquals("Import AWBs from CSV", module.DataTransferMenuItem.MenuItems[2].Text);
			}
		}

		class StandAloneFsrEnquiryModuleForTest : StandAloneFsrEnquiryModule
		{
			public MenuItem[] ContextMenuExposed
			{
				get { return base.ContextMenu; }
			}
		}
	}
}
