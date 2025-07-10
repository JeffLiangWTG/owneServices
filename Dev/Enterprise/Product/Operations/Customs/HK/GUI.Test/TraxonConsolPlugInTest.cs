using System;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.HK.Business;
using Enterprise.Customs.HK.PlugIn;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.HK.GUI.Testing
{
	class TraxonConsolPlugInTest : TestCaseWithFactory
	{
		public void TestZPluginVisibility_Disabled()
		{
			using (var testPlugIn = new TraxonConsolPlugIn(consol))
			{
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("Cannot see the Menu", false, testPlugIn.Enabled);
			}
		}

		public void TestSendMessageClick()
		{
			SetupMinimumConsolAndShipmentRequirementsForMessageSending();
			using (SetupHKTraxon(GlbCompany.CurrentCompany.PK.ToGuid()))
			using (var testPlugIn = new TraxonConsolPlugIn(consol))
			{
				var traxonMenu = testPlugIn.TopLevelMenu;
				traxonMenu.MenuItems[0].PerformClick();
				AssertEquals("ISAC message has been sent.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestLicenceManifestLogin()
		{
			SetupMinimumConsolAndShipmentRequirementsForMessageSending();
			using (SetupHKTraxon(GlbCompany.CurrentCompany.PK.ToGuid()))
			using (var testPlugIn = new TraxonConsolPlugIn(consol))
			{
				CombineAssertions(() =>
				{
					var traxonMenu = testPlugIn.TopLevelMenu;
					AssertEquals("Should not be Logged", false, Env.Licence.Manifest.IsLoggedIn);
					traxonMenu.MenuItems[0].PerformClick();
					AssertEquals("Should be Logged", true, Env.Licence.Manifest.IsLoggedIn);
				});
			}
		}

		public void TestSendWithMessageErrors_IsAllowed()
		{
			Env.Security.ISACHKSendWithMessageErrors.IsAllowed = true;

			SetupMinimumConsolAndShipmentRequirementsForMessageSending();

			consol.Shipments[0].JS_ActualWeight = 0;
			consol.Shipments[0].JS_ActualVolume = 0;

			Factory.Save();

			var company = GlbCompany.CurrentCompany;
			using (var plugIn = new TraxonConsolPlugIn(consol))
			using (HKDataRegistry.Instance.HKTraxonOutputDirectory.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Env.TempPath))
			using (HKDataRegistry.Instance.CosacAgentCode.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "12342"))
			using (HKDataRegistry.Instance.HKTraxonSenderID.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "RHKAGT021332880/HKG81"))
			using (HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "PRDAGENT027"))
			{
				var traxonMenu = plugIn.TopLevelMenu;

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);
				traxonMenu.MenuItems[0].PerformClick();
				AssertEquals("should no message generated", 0, consol.Messages.Count);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
				traxonMenu.MenuItems[0].PerformClick();
				AssertEquals("should have message generated", 1, consol.Messages.Count);
				AssertEquals("ISAC message has been sent(probably has error in it).", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendWithMessageErrors_IsNotAllowed()
		{
			Env.Security.ISACHKSendWithMessageErrors.IsAllowed = false;

			SetupMinimumConsolAndShipmentRequirementsForMessageSending();

			consol.Shipments[0].JS_ActualWeight = 0;
			consol.Shipments[0].JS_ActualVolume = 0;

			Factory.Save();

			var company = GlbCompany.CurrentCompany;
			using (var plugIn = new TraxonConsolPlugIn(consol))
			using (HKDataRegistry.Instance.HKTraxonOutputDirectory.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Env.TempPath))
			using (HKDataRegistry.Instance.CosacAgentCode.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "12342"))
			using (HKDataRegistry.Instance.HKTraxonSenderID.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "RHKAGT021332880/HKG81"))
			using (HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "PRDAGENT027"))
			{
				var traxonMenu = plugIn.TopLevelMenu;
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
				traxonMenu.MenuItems[0].PerformClick();
				AssertEquals("should no message generated", 0, consol.Messages.Count);
			}
		}

		public void TestCriticalErrorsPreventSending()
		{
			Env.Security.ISACHKSendWithMessageErrors.IsAllowed = true;

			SetupMinimumConsolAndShipmentRequirementsForMessageSending();

			consol.Shipments[0].JS_ActualWeight = 0;
			consol.Shipments[0].JS_ActualVolume = 0;

			Factory.Save();

			var company = GlbCompany.CurrentCompany;
			using (var plugIn = new TraxonConsolPlugIn(consol))
			using (HKDataRegistry.Instance.HKTraxonOutputDirectory.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Env.TempPath))
			using (HKDataRegistry.Instance.CosacAgentCode.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, string.Empty))
			using (HKDataRegistry.Instance.HKTraxonSenderID.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, string.Empty))
			using (HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, string.Empty))
			{
				var traxonMenu = plugIn.TopLevelMenu;

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
				traxonMenu.MenuItems[0].PerformClick();
				AssertEquals("No message should be generated", 0, consol.Messages.Count);
				AssertEquals("ISAC Message sending configuration should be completed in the registry before sending messages", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
		}
		ForwardingConsol consol;

		void SetupMinimumConsolAndShipmentRequirementsForMessageSending()
		{
			consol.JK_MasterBillNum = "435";

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "HKHKG";
			transport.JW_RL_NKDiscPort = "AUMEL";
			transport.JW_VoyageFlight = "23";
			transport.JW_ETA = ZDateTime.Today.AddDays(1);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = "123";
			shipment.JS_RL_NKOrigin = "HKHKG";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_OuterPacks = 100;
			shipment.JS_ActualWeight = 1000;
			shipment.JS_UnitOfWeight = "KG";

			var validOrgHeader = Factory.New<OrgHeader>();
			validOrgHeader.OH_Code = "TESTORG";
			validOrgHeader.OH_FullName = "TESTORG NAME";
			validOrgHeader.MainAddress.OA_Address1 = "TEST ORGANISATION ADDRESS";
			validOrgHeader.MainAddress.OA_City = "TEST ORGANISATION CITY";
			validOrgHeader.MainAddress.OA_RL_NKRelatedPortCode = "HKHKG";
			shipment.ConsigneePK = validOrgHeader.PK;

			shipment.ConsignorPK = validOrgHeader.PK;

			Factory.Save();
		}

		static IDisposable SetupHKTraxon(Guid currentCompany)
		{
			IDisposable hkTraxonOutputDirectoryDisposable = null;
			IDisposable cosacAgentCode = null;
			IDisposable hkTraxonSenderId = null;
			IDisposable hkTraxonRecipientReferencePassword = null;

			void CreateAction()
			{
				hkTraxonOutputDirectoryDisposable = HKDataRegistry.Instance.HKTraxonOutputDirectory.SetTemporaryValue(currentCompany, Guid.Empty, Guid.Empty, Env.TempPath);
				cosacAgentCode = HKDataRegistry.Instance.CosacAgentCode.SetTemporaryValue(currentCompany, Guid.Empty, Guid.Empty, "12342");
				hkTraxonSenderId = HKDataRegistry.Instance.HKTraxonSenderID.SetTemporaryValue(currentCompany, Guid.Empty, Guid.Empty, "RHKAGT021332880/HKG81");
				hkTraxonRecipientReferencePassword = HKDataRegistry.Instance.HKTraxonRecipientReferencePassword.SetTemporaryValue(currentCompany, Guid.Empty, Guid.Empty, "PRDAGENT027");
			}

			void DisposeAction()
			{
				hkTraxonRecipientReferencePassword.Dispose();
				hkTraxonSenderId.Dispose();
				cosacAgentCode.Dispose();
				hkTraxonOutputDirectoryDisposable.Dispose();
			}

			return new DisposableAction(CreateAction, DisposeAction);
		}
	}
}
