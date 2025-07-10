using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DataTransfer.GUI;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(XmlTransactionsImportController))]
	public class XmlTransactionsImportControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.XmlTransactionsImport;
		}

		public override void TestNewForm()
		{
			AssertNull("NewForm should be null", Controller.ShowNewForm());
			AssertEquals("Error Interface Connector is not enabled on this system.", ((UnitTestUserNotification)Globals.Message).LastMessage.ToString());

			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			using (eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow))
			{
				Controller.ShowNewForm();
				AssertType<XmlDataImporterForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestControllerID()
		{
			AssertEquals(ControllerIDs.XmlTransactionsImport, TestController.ID);
		}

		public void TestShowTemplateCopyForm()
		{
			var invoice = Factory.New<ARInvoice>();
			AssertExceptionThrown(typeof(ModuleTemplateCopyNotSupportedException), "You cannot Show a Template CopyForm for a Transaction Import", () => TestController.ShowTemplateCopyForm(invoice));
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			Type type = null;
			AssertExceptionThrown(typeof(ModuleGuiNotSupportedException), "Not supported", () => { type = TestController.TypeOfTopLevelBusinessObject; });
		}

		public void TestModuleID()
		{
			ModuleIdentifier moduleIdentifier = null;
			AssertExceptionThrown(typeof(ModuleGuiNotSupportedException), "Not supported", () => { moduleIdentifier = TestController.ModuleID; });
		}

		public void TestGetForm()
		{
			var testObject = new TestXmlTransactionsImportControllerObject();
			var invoice = Factory.New<ARInvoice>();
			AssertNull(testObject.ExposeGetForm(invoice));
		}

		public void TestGetNewBusinessEntityInLocalFactory()
		{
			var testObject = new TestXmlTransactionsImportControllerObject();
			AssertNull(testObject.ExposeGetNewBusinessEntityInLocalFactory());
		}

		public void TestCheckPointForNew()
		{
			var testObject = new TestXmlTransactionsImportControllerObject();
			var securityCheckpoint = testObject.ExposeCheckPointForNew();

			AssertEquals(securityCheckpoint.Code, "ImportXmlAccountingTransactions");
		}

		XmlTransactionsImportController testController;
		XmlTransactionsImportController TestController => testController ?? (testController = new XmlTransactionsImportController());
	}

	public class TestXmlTransactionsImportControllerObject : XmlTransactionsImportController
	{
		public IZForm ExposeGetForm(IBusiness businessEntity)
		{
			return base.GetForm(businessEntity);
		}
		public IBusiness ExposeGetNewBusinessEntityInLocalFactory()
		{
			return base.GetNewBusinessEntityInLocalFactory();
		}

		public SecurityCheckpoint ExposeCheckPointForNew()
		{
			return base.CheckPointForNew;
		}
	}
}
