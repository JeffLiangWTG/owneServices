using System.Reflection;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class AutoDocumentDeliveryJobForEDocsWithMultipleContacts : TestCaseWithFactory
	{
		public void TestEDocsWithMultipleContacts()
		{
			var documentSupportable = shipment as IDocumentSupportable;
			AutoDocumentDeliveryJob deliveryJob;

			ClearStmPrintJob();
			AssertEquals("AddDocumentToEDocs with no contact(previous)", 0, GetStmPrintJobCount());
			deliveryJob = new AutoDocumentDeliveryJob(businessObject: documentSupportable, documentCommandPK: stmMenuItem.PK, onlySendToDocManager: true, isFactoryPopulateButDoNotSave: true);
			deliveryJob.Deliver(new NotificationBuffer());
			Factory.Save();
			AssertEquals("AddDocumentToEDocs with no contact", 1, GetStmPrintJobCount());

			ClearStmPrintJob();
			stmMenuTemplatePivot.SI_PrintCopyType = Core.Constants.ContactNotifyModes.Fax;
			AssertEquals("AddDocumentToEDocs with 1 contact, delivery method not match(previous)", 0, GetStmPrintJobCount());
			AddContact("Accounts", "accounts.usord@unitedenterprises.com");
			Factory.Save();
			deliveryJob = new AutoDocumentDeliveryJob(businessObject: documentSupportable, documentCommandPK: stmMenuItem.PK, onlySendToDocManager: true, isFactoryPopulateButDoNotSave: true);
			deliveryJob.Deliver(new NotificationBuffer());
			Factory.Save();
			AssertEquals("AddDocumentToEDocs with 1 contact, delivery method not match", 1, GetStmPrintJobCount());

			ClearStmPrintJob();
			stmMenuTemplatePivot.SI_PrintCopyType = Core.Constants.ContactNotifyModes.Email;
			AssertEquals("AddDocumentToEDocs with 1 contact(previous)", 0, GetStmPrintJobCount());
			deliveryJob = new AutoDocumentDeliveryJob(businessObject: documentSupportable, documentCommandPK: stmMenuItem.PK, onlySendToDocManager: true, isFactoryPopulateButDoNotSave: true);
			deliveryJob.Deliver(new NotificationBuffer());
			Factory.Save();
			AssertEquals("AddDocumentToEDocs with 1 contact", 1, GetStmPrintJobCount());

			ClearStmPrintJob();
			AssertEquals("AddDocumentToEDocs with multiple contacts(previous)", 0, GetStmPrintJobCount());
			AddContact("Operations", "operations.usord@unitedenterprises.com");
			AddContact("Sales", "sales.cobog@accugreen.com");
			AddContact("u4", "u4.usord@unitedenterprises.com");
			AddContact("u5", "u5.usord@unitedenterprises.com");
			AddContact("u6", "u6.usord@unitedenterprises.com");
			Factory.Save();
			deliveryJob = new AutoDocumentDeliveryJob(businessObject: documentSupportable, documentCommandPK: stmMenuItem.PK, onlySendToDocManager: true, isFactoryPopulateButDoNotSave: true);
			deliveryJob.Deliver(new NotificationBuffer());
			Factory.Save();
			AssertEquals("AddDocumentToEDocs with multiple contacts", 1, GetStmPrintJobCount());
		}

		#region Test - Organization Details Can Be Found

		public void TestEDocsDeliver_WhenNoContact_ThenOrganizationDetailsCanBeFound()
		{
			// Arrange
			var documentSupportable = shipment as IDocumentSupportable;
			var deliveryJob = new AutoDocumentDeliveryJob(businessObject: documentSupportable, documentCommandPK: stmMenuItem.PK, onlySendToDocManager: true, isFactoryPopulateButDoNotSave: true);

			var type = typeof(AutoDocumentDeliveryJob);
			var methodGetDeliveryInstructions = type.GetMethod("GetDeliveryInstructions", BindingFlags.NonPublic | BindingFlags.Instance);
			var methodGetNewDocumentPrintSetForDelivery = type.GetMethod("GetNewDocumentPrintSetForDelivery", BindingFlags.NonPublic | BindingFlags.Instance);
			using (var documentPrintSet = (DocumentPrintSet)methodGetNewDocumentPrintSetForDelivery.Invoke(deliveryJob, System.Array.Empty<object>()))
			{
				AssertEquals("pre-condition", true, documentPrintSet.Count > 0);
				var docPack = documentPrintSet[0];

				// Act
				var instructions = (DeliveryInstructions)methodGetDeliveryInstructions.Invoke(deliveryJob, new object[] { docPack });

				// Assert
				CombineAssertions("Organization Info ", () =>
				{
					AssertEquals(1, instructions.Recipients.Count);
					AssertEquals("The Import Manager", instructions.Recipients[0].Name);
					AssertEquals("24 Kingfishers Avenue", instructions.Recipients[0].Address1);
					AssertEquals("Grove", instructions.Recipients[0].Address2);
					AssertEquals("Wantage", instructions.Recipients[0].City);
					AssertEquals("OXF", instructions.Recipients[0].State);
					AssertEquals("OX1234J", instructions.Recipients[0].PostCode);
					AssertEquals("GBOXF", instructions.Recipients[0].ClosestPort.Code);
					AssertEquals("919384", instructions.Recipients[0].Phone);
					AssertEquals("1234556", instructions.Recipients[0].Fax);
					AssertEquals("oxford@example.com", instructions.Recipients[0].Email);
				});
			}
		}

		public void TestEDocsDeliver_WhenOneContact_ThenOrganizationDetailsCanBeFound()
		{
			// Arrange
			AddContact("Operations", "operations.usord@unitedenterprises.com");
			var documentSupportable = shipment as IDocumentSupportable;
			var deliveryJob = new AutoDocumentDeliveryJob(businessObject: documentSupportable, documentCommandPK: stmMenuItem.PK, onlySendToDocManager: true, isFactoryPopulateButDoNotSave: true);

			var type = typeof(AutoDocumentDeliveryJob);
			var methodGetDeliveryInstructions = type.GetMethod("GetDeliveryInstructions", BindingFlags.NonPublic | BindingFlags.Instance);
			var methodGetNewDocumentPrintSetForDelivery = type.GetMethod("GetNewDocumentPrintSetForDelivery", BindingFlags.NonPublic | BindingFlags.Instance);
			using (var documentPrintSet = (DocumentPrintSet)methodGetNewDocumentPrintSetForDelivery.Invoke(deliveryJob, System.Array.Empty<object>()))
			{
				AssertEquals("pre-condition", true, documentPrintSet.Count > 0);
				var docPack = documentPrintSet[0];

				// Act
				var instructions = (DeliveryInstructions)methodGetDeliveryInstructions.Invoke(deliveryJob, new object[] { docPack });

				// Assert
				CombineAssertions("Organization Info ", () =>
				{
					AssertEquals(1, instructions.Recipients.Count);
					AssertEquals("Operations", instructions.Recipients[0].Name);
					AssertEquals("CustomsAddress2", instructions.Recipients[0].Address1);
					AssertEquals("", instructions.Recipients[0].Address2);
					AssertEquals("CustomsCity", instructions.Recipients[0].City);
					AssertEquals("HAM", instructions.Recipients[0].State);
					AssertEquals("SH1234X", instructions.Recipients[0].PostCode);
					AssertEquals("GBOXF", instructions.Recipients[0].ClosestPort.Code);
					AssertEquals("111111", instructions.Recipients[0].Phone);
					AssertEquals("2222222", instructions.Recipients[0].Fax);
					AssertEquals("operations.usord@unitedenterprises.com", instructions.Recipients[0].Email);
				});
			}
		}

		public void TestEDocsDeliver_WhenMultiContacts_ThenOrganizationDetailsCanBeFound()
		{
			// Arrange
			var documentSupportable = shipment as IDocumentSupportable;
			var deliveryJob = new AutoDocumentDeliveryJob(businessObject: documentSupportable, documentCommandPK: stmMenuItem.PK, onlySendToDocManager: true, isFactoryPopulateButDoNotSave: true);
			AddContact("Operations", "operations.usord@unitedenterprises.com");
			AddContact("Sales", "sales.cobog@accugreen.com");
			AddContact("u4", "u4.usord@unitedenterprises.com");
			AddContact("u5", "u5.usord@unitedenterprises.com");
			AddContact("u6", "u6.usord@unitedenterprises.com");

			var type = typeof(AutoDocumentDeliveryJob);
			var methodGetDeliveryInstructions = type.GetMethod("GetDeliveryInstructions", BindingFlags.NonPublic | BindingFlags.Instance);
			var methodGetNewDocumentPrintSetForDelivery = type.GetMethod("GetNewDocumentPrintSetForDelivery", BindingFlags.NonPublic | BindingFlags.Instance);
			using (var documentPrintSet = (DocumentPrintSet)methodGetNewDocumentPrintSetForDelivery.Invoke(deliveryJob, System.Array.Empty<object>()))
			{
				AssertEquals("pre-condition", true, documentPrintSet.Count > 0);
				var docPack = documentPrintSet[0];

				// Act
				var instructions = (DeliveryInstructions)methodGetDeliveryInstructions.Invoke(deliveryJob, new object[] { docPack });

				// Assert
				CombineAssertions("Organization Info ", () =>
				{
					AssertEquals(1, instructions.Recipients.Count);
					AssertEquals("The Import Manager", instructions.Recipients[0].Name);
					AssertEquals("24 Kingfishers Avenue", instructions.Recipients[0].Address1);
					AssertEquals("Grove", instructions.Recipients[0].Address2);
					AssertEquals("Wantage", instructions.Recipients[0].City);
					AssertEquals("OXF", instructions.Recipients[0].State);
					AssertEquals("OX1234J", instructions.Recipients[0].PostCode);
					AssertEquals("GBOXF", instructions.Recipients[0].ClosestPort.Code);
					AssertEquals("919384", instructions.Recipients[0].Phone);
					AssertEquals("1234556", instructions.Recipients[0].Fax);
					AssertEquals("oxford@example.com", instructions.Recipients[0].Email);
				});
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			refDocType = Factory.New<RefDocType>();
			refDocType.RT_ReferenceType = "SCL";
			refDocType.RT_DocType = "VEP";
			refDocType.RT_Desc = "Versioned Entry Print";
			refDocType.RT_SaveVersions = ZBool.True; // not ticked: Keep Lastest Version Only

			stmMenuItem = Factory.New<StmMenuItem>();
			stmMenuItem.SU_MenuName = "Versioned 7501 Entry Summary";
			stmMenuItem.SU_BusinessContext = "Shipment";
			stmMenuItem.SU_ContactType = ContactType.Consignee.Code;
			stmMenuItem.SU_MenuType = "DOC";
			stmMenuItem.SU_PreventAutoDelivery = ZBool.False;
			stmMenuItem.SU_FilterList = ""; // make sure that DocumentCommand.IsApplicable returns true

			var template1 = DocumentEngineTestHelper.CreateTemplateFromString(
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#EndOfReport]");

			stmTemplate = Factory.New<StmTemplate>();
			stmTemplate.SO_Name = "UnitTest";
			stmTemplate.SO_DataContext = "Notes";
			stmTemplate.SO_Template = template1;
			stmTemplate.SO_TemplateType = "DOC";

			stmMenuTemplatePivot = Factory.New<StmMenuTemplatePivot>();
			stmMenuTemplatePivot.SI_SU = stmMenuItem.PK;
			stmMenuTemplatePivot.SI_SO = stmTemplate.PK;
			stmMenuTemplatePivot.SI_DocumentTitle = "Versioned 7501 Entry Summary";
			stmMenuTemplatePivot.SI_PrintByDefault = ZBool.True;
			stmMenuTemplatePivot.SI_PrintCopyType = Core.Constants.ContactNotifyModes.Email;
			stmMenuTemplatePivot.SI_RT_DocType = refDocType.PK;

			orgHeaderConsignee = Factory.New<OrgHeader>();
			orgHeaderConsignee.OH_Code = "UNIENTORD";
			orgHeaderConsignee.OH_FullName = "UNITED ENTERPRISES (US) CORPORATION";
			orgHeaderConsignee.OH_IsConsignee = ZBool.True;
			orgHeaderConsignee.MainAddress.OA_Address1 = "24 Kingfishers Avenue";
			orgHeaderConsignee.MainAddress.OA_Address2 = "Grove";
			orgHeaderConsignee.MainAddress.OA_City = "Wantage";
			orgHeaderConsignee.MainAddress.OA_State = "Oxford";
			orgHeaderConsignee.MainAddress.OA_PostCode = "OX1234J";
			orgHeaderConsignee.OH_RL_NKClosestPort = "GBOXF";
			orgHeaderConsignee.MainAddress.OA_Phone = "919384";
			orgHeaderConsignee.MainAddress.OA_Fax = "1234556";
			orgHeaderConsignee.MainAddress.OA_Email = "oxford@example.com";

			testHeader = Factory.New<OrgHeader>();
			testHeader.OH_Code = "TEST";
			testHeader.OH_Code = "TEST HEADER";
			testHeader.OH_IsConsignee = ZBool.True;
			testHeader.MainAddress.OA_Address1 = "CustomsAddress1";
			testHeader.MainAddress.OA_Address1 = "CustomsAddress2";
			testHeader.MainAddress.OA_City = "CustomsCity";
			testHeader.MainAddress.OA_State = "Shanghai";
			testHeader.MainAddress.OA_PostCode = "SH1234X";
			testHeader.OH_RL_NKClosestPort = "GBSHG";
			testHeader.MainAddress.OA_Phone = "111111";
			testHeader.MainAddress.OA_Fax = "2222222";
			testHeader.MainAddress.OA_Email = "shanghai@example.com";
			testHeader.Contacts.AddNew();

			shipment = Factory.New<Forwarding.IForwardingShipment>();
			((BusinessObject)shipment).FillWithValidTestData();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = orgHeaderConsignee.PK; // for getting contact from shipment[documentSupporter]

			Factory.Save();
		}

		void ClearStmPrintJob()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipientSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmDeliveryGroupSchema.Constants.TableName);
		}

		int GetStmPrintJobCount()
		{
			return ((IDbConnected)Factory).Connection.ExecuteScalar<int>("SELECT count(1) FROM dbo.StmPrintJob");
		}

		void AddContact(ZString contactName, ZString eMail)
		{
			var contact1 = orgHeaderConsignee.Contacts.AddNew();
			contact1.OC_OA_OrgAddress = testHeader.MainAddress.PK;
			contact1.OC_OH = orgHeaderConsignee.PK;
			contact1.OC_ContactName = contactName;
			contact1.OC_Email = eMail;
			contact1.OC_JobCategory = "EMU";
			contact1.OC_NotifyMode = Core.Constants.ContactNotifyModes.Email;
			var doc1 = contact1.Documents.AddNew();
			doc1.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
			doc1.OD_DocumentGroup = "ALL";
			doc1.OD_SU_MenuItem = stmMenuItem.PK;
		}

		RefDocType refDocType;
		StmMenuItem stmMenuItem;
		StmTemplate stmTemplate;
		StmMenuTemplatePivot stmMenuTemplatePivot;
		OrgHeader orgHeaderConsignee;
		OrgHeader testHeader;
		Forwarding.IForwardingShipment shipment;

		#endregion
	}
}
