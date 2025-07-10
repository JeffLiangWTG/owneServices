using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core.Modules;
using Enterprise.CustomerService.Business;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.eHubMessaging.Business;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Test;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.CustomerService.Module.Test
{
	[TestedType(typeof(IncidentApprovalController))]
	sealed class IncidentApprovalControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var bizO = Factory.New<IncidentApproval>();
			Factory.Save();
			return bizO;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ServiceRequest;
		}

		public void TestControllerID()
		{
			IncidentApprovalController controller = new IncidentApprovalController();
			AssertEquals(ControllerIDs.ServiceRequest, controller.ID);
		}

		public void TestSecurity()
		{
			var controller = new IncidentApprovalController();

			AssertEquals(Env.Security.IncidentApprovalView, controller.GetCheckPointForView(null));
			AssertEquals(Env.Security.IncidentApprovalModify, controller.GetCheckPointForEdit(null));
			AssertEquals(Env.Security.IncidentApprovalNew, controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.IncidentApprovalDelete, controller.GetCheckPointForDelete(null));
		}

		public void TestGetCustomerServiceMenuSectionCode()
		{
			var controller = new IncidentApprovalController();
			var organisationMainFormModule = ModuleTree.Tree.FindByID(ModuleIDs.Organisation.ToString());
			AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.MasterData, controller.GetCustomerServiceMenuSectionCode(organisationMainFormModule.ModuleTreeID));
			AssertEquals(MandatoryCustomerServiceMenuSectionList.Codes.Other, controller.GetCustomerServiceMenuSectionCode("XXX"));
		}

		public void TestGetCustomerServiceMenuSectionCode_VisualBoards()
		{
			var bmsRegistry = ObjectFactory.Get<IBMSRegistry>();
			bmsRegistry.BufferManagementEnabled = true;

			var moduleTreeLoader = ObjectFactory.Get<IModuleTreeLoader>();
			var moduleTreeForTest = new ModuleTree();

			using (var form = new DummyVisualBoardForm())
			using (ModuleTree.OverrideTreeForTest(moduleTreeForTest))
			{
				moduleTreeLoader.Initialise(moduleTreeForTest, Env.Security);
				moduleTreeLoader.LoadModules();

				var visualBoardFormModule = ModuleTree.Tree.FindByID(ModuleIDs.VisualBoard.ToString());
				AssertNull("The module for a displayed visual board should not be in the module tree", visualBoardFormModule);

				var visualBoardConfigurationModule = ModuleTree.Tree.FindByID(ModuleIDs.BMBoard.ToString());
				AssertNotNull("The module for visual board configuration should be in the module tree", visualBoardConfigurationModule);

				var controller = new IncidentApprovalController();
				(controller as IServiceRequestController).SetParentForm(form);
				AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.BufferManagement, controller.GetCustomerServiceMenuSectionCode(ModuleIDs.VisualBoard.ToString()));
				AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.BufferManagement, controller.GetCustomerServiceMenuSectionCode(visualBoardConfigurationModule.ModuleTreeID));
			}

			bmsRegistry.BufferManagementEnabled = false;
		}

		public void TestGetCustomerServiceMenuSectionCode_DoesNotIncludeSectionsUnderJumpCategory()
		{
			var controller = new IncidentApprovalController();
			var moduleTreeLoader = ObjectFactory.Get<IModuleTreeLoader>();
			var moduleTreeForTest = new ModuleTree();
			using (ModuleTree.OverrideTreeForTest(moduleTreeForTest))
			{
				var recentItemManagerType = Type.GetType("Enterprise.ZArchitecture.Favorites.RecentItemManager, Enterprise.ZArchitecture.Favorites");
				var recentItemManager = recentItemManagerType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public).GetValue(null);
				var addFavoriteForTestMethod = recentItemManagerType.GetMethod("AddToFavoriteForTest", new Type[] { typeof(string) });
				addFavoriteForTestMethod.Invoke(recentItemManager, new[] { ModuleIDs.RefUNLOCO.ToString() });

				moduleTreeLoader.Initialise(moduleTreeForTest, Env.Security);
				moduleTreeLoader.LoadModules();

				var incident = Factory.New<IncidentApproval>();
				var unlocoMainFormModule = ModuleTree.Tree.FindByID(ModuleIDs.RefUNLOCO.ToString());
				AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.Locations, controller.GetCustomerServiceMenuSectionCode(unlocoMainFormModule.ModuleTreeID));
			}
		}

		public void TestGetCustomerServiceMenuSectionCode_TarrifsAndRates()
		{
			var controller = new IncidentApprovalController();
			var incident = Factory.New<IncidentApproval>();
			var clientRatesMainFormModule = ModuleTree.Tree.FindByID(ModuleIDs.ClientRates.ToString());
			AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.TariffsRates, controller.GetCustomerServiceMenuSectionCode(clientRatesMainFormModule.ModuleTreeID));

			var globalRatesMainFormModule = ModuleTree.Tree.FindByID(ModuleIDs.GlobalRates.ToString());
			AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.TariffsRates, controller.GetCustomerServiceMenuSectionCode(globalRatesMainFormModule.ModuleTreeID));

			var costingMainFormModule = ModuleTree.Tree.FindByID(ModuleIDs.Costing.ToString());
			AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.TariffsRates, controller.GetCustomerServiceMenuSectionCode(costingMainFormModule.ModuleTreeID));

			var quotationsMainFormModule = ModuleTree.Tree.FindByID(ModuleIDs.Quotations.ToString());
			AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.TariffsRates, controller.GetCustomerServiceMenuSectionCode(quotationsMainFormModule.ModuleTreeID));

			var tariffReportsMainFormModule = ModuleTree.Tree.FindByID(ModuleIDs.TariffRateReports.ToString());
			AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.TariffsRates, controller.GetCustomerServiceMenuSectionCode(tariffReportsMainFormModule.ModuleTreeID));
		}

		public override void TestNewForm()
		{
			using (var form = new ZForm())
			{
				form.Show();
				var controller = Controller;
				(controller as IServiceRequestController).SetParentForm(form);
				UnitTestUserNotification.Instance.AddOKAnswer();
				AssertNull(Controller.ShowNewForm());
			}
		}

		public void TestGetForm()
		{
			var incident = Factory.New<IncidentApproval>();
			incident.IA_ActiveModuleId = "123";
			Factory.Save();

			var controller = new ControllerForTest();
			using (var form = controller.GetFormForTest(incident))
			{
				AssertEquals(false, incident.HasChanges);
				AssertEquals("123", incident.IA_ActiveModuleId);
			}
		}

		public void TestShowEditForm()
		{
			var incident = Factory.New<IncidentApproval>();
			incident.IA_Status = IncidentApprovalLookups.StatusCodes.New;
			Factory.Save();

			var controller = new ControllerForTest();
			controller.ShowEditForm(incident);
			AssertEquals("unsent request can be edited", FormAction.Edit, controller.LastAction);

			incident.IA_Status = IncidentApprovalLookups.StatusCodes.ApprovedAndSent;
			controller.ShowEditForm(incident);
			AssertEquals("sent request can not be edited", FormAction.View, controller.LastAction);
		}

		[TestDate(2020, 1, 14)]
		public void TestCancelPrompt()
		{
			var utcNowAsString = ZDateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
			using (var form = new ZForm())
			{
				form.Show();
				var controller = Controller;
				(controller as IServiceRequestController).SetParentForm(form);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				AssertNull(Controller.ShowNewForm());

				AssertEquals(0, Factory.Load<EDIInterchange>(new ZQuery()).Length);
				AssertEquals(@"Your eRequest will be handled more efficiently if you first navigate to the relevant module related to your query.

If your query is not related to a module, click 'OK' to continue. Otherwise click 'Cancel' and navigate there first.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2020, 1, 14)]
		public void TestSendERequestDocument_AttachScreenshot()
		{
			var utcNowAsString = ZDateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
			using (var form = new ZForm())
			{
				form.Show();
				var controller = Controller;
				(controller as IServiceRequestController).SetParentForm(form);
				UnitTestUserNotification.Instance.AddOKAnswer();
				UnitTestUserNotification.Instance.AddYesAnswer();
				AssertNull(Controller.ShowNewForm());

				var interchange = Factory.Load<EDIInterchange>(new ZQuery()).Single();
				AssertContains("<ERequestDocument ", interchange.EI_BodyText);

				var ediMsg = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
				var serializer = ZXmlSerializer.New(typeof(ERequestDocument));
				var requestDoc = (ERequestDocument)SystemMessage.Deserialize(ediMsg, serializer);
				AssertEquals(true, new ZGuid(requestDoc.ReferenceId).IsValid);
				AssertEquals("Should have attached screenshot", 2, requestDoc.Attachments.Count);

				var screenShot = requestDoc.Attachments.OfType<ERequestDocumentAttachment>().Single(x => x.FileName == $"ScreenShot_{utcNowAsString}.png");
				AssertEquals(true, screenShot.IsPublished);

				var image = Image.FromStream(new MemoryStream(screenShot.Data));
				AssertEquals(false, image.Size.IsEmpty);
				AssertEquals(ImageFormat.Png, image.RawFormat);

				var systemReportZipFile = requestDoc.Attachments.OfType<ERequestDocumentAttachment>().Single(x => x.FileName == $"SystemReport_{utcNowAsString}.zip");
				using (var zipStream = new MemoryStream(systemReportZipFile.Data))
				using (var unzipStream = new MemoryStream())
				{
					var extractor = new ZipExtractor();
					extractor.ExtractZipStream(zipStream, unzipStream, $"SystemReport_{utcNowAsString}.xml");

					var xdoc = new XmlDocument();
					xdoc.LoadXml(Encoding.UTF8.GetString(unzipStream.ToArray()));
					AssertEquals(true, xdoc.GetElementsByTagName("EDI_Exception_Report").Count > 0);
					AssertEquals(true, xdoc.GetElementsByTagName("SqlEvents").Count > 0);
					AssertEquals(true, xdoc.GetElementsByTagName("StackTrace").Count > 0);
					AssertEquals(true, xdoc.GetElementsByTagName("UserEvents").Count > 0);
				}
			}
		}

		[TestDate(2020, 1, 14)]
		public void TestSendERequestDocument_DoNotAttachScreenshot()
		{
			var utcNowAsString = ZDateTime.UtcNow.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
			using (var form = new ZForm())
			{
				form.Show();
				var controller = Controller;
				(controller as IServiceRequestController).SetParentForm(form);
				UnitTestUserNotification.Instance.AddOKAnswer();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				AssertNull(Controller.ShowNewForm());

				var interchange = Factory.Load<EDIInterchange>(new ZQuery()).Single();
				AssertContains("<ERequestDocument ", interchange.EI_BodyText);

				var ediMsg = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
				var serializer = ZXmlSerializer.New(typeof(ERequestDocument));
				var requestDoc = (ERequestDocument)SystemMessage.Deserialize(ediMsg, serializer);
				AssertEquals(true, new ZGuid(requestDoc.ReferenceId).IsValid);
				AssertEquals("Should not have attached screenshot", 1, requestDoc.Attachments.Count);

				AssertEquals(false, requestDoc.Attachments.OfType<ERequestDocumentAttachment>().Any(x => x.FileName == $"ScreenShot_{utcNowAsString}.png"));

				var systemReportZipFile = requestDoc.Attachments.OfType<ERequestDocumentAttachment>().Single(x => x.FileName == $"SystemReport_{utcNowAsString}.zip");
				using (var zipStream = new MemoryStream(systemReportZipFile.Data))
				using (var unzipStream = new MemoryStream())
				{
					var extractor = new ZipExtractor();
					extractor.ExtractZipStream(zipStream, unzipStream, $"SystemReport_{utcNowAsString}.xml");

					var xdoc = new XmlDocument();
					xdoc.LoadXml(Encoding.UTF8.GetString(unzipStream.ToArray()));
					AssertEquals(true, xdoc.GetElementsByTagName("EDI_Exception_Report").Count > 0);
					AssertEquals(true, xdoc.GetElementsByTagName("SqlEvents").Count > 0);
					AssertEquals(true, xdoc.GetElementsByTagName("StackTrace").Count > 0);
					AssertEquals(true, xdoc.GetElementsByTagName("UserEvents").Count > 0);
				}
			}
		}

		public void TestSendEREquestDocument_FormDisposed()
		{
			var form = new ZForm();
			form.Show();
			var controller = Controller;
			(controller as IServiceRequestController).SetParentForm(form);
			UnitTestUserNotification.Instance.AddOKAnswer();

			form.Dispose();
			Controller.ShowNewForm();

			var interchange = Factory.Load<EDIInterchange>(new ZQuery()).Single();
			AssertContains("<ERequestDocument ", interchange.EI_BodyText);

			var ediMsg = SystemMessage.DebugOnlyCreateDownloadedMessage(interchange);
			var serializer = ZXmlSerializer.New(typeof(ERequestDocument));
			var requestDoc = (ERequestDocument)SystemMessage.Deserialize(ediMsg, serializer);
			AssertEquals(false, requestDoc.Attachments.OfType<ERequestDocumentAttachment>().Any(x => x.FileName.StartsWith("ScreenShot_")));
		}

		public void TestShouldSendERequestDocument()
		{
			WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (var form = new ZForm())
			{
				form.Show();
				var controller = Controller;

				var serviceRequestController = controller as IServiceRequestController;
				AssertEquals(true, serviceRequestController.ShouldSendERequestDocument);
				serviceRequestController.ShouldSendERequestDocument = false;
				AssertEquals(false, serviceRequestController.ShouldSendERequestDocument);

				UnitTestUserNotification.Instance.AddOKAnswer();
				AssertNull(Controller.ShowNewForm());
				AssertEquals(false, Factory.Load<EDIInterchange>(new ZQuery()).Any());

				serviceRequestController.ShouldSendERequestDocument = true;
				AssertEquals(true, serviceRequestController.ShouldSendERequestDocument);
				serviceRequestController.SetParentForm(form);
				UnitTestUserNotification.Instance.AddOKAnswer();
				AssertNull(Controller.ShowNewForm());
				var interchange = Factory.Load<EDIInterchange>(new ZQuery()).Single();
				AssertContains("<ERequestDocument ", interchange.EI_BodyText);
			}
		}

		public void TestSendERequestDocument_MainForm()
		{
			WebDataRegistry.Instance.EnableTrustedMessaging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using var dummyModule = ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy);
#if !WINZOR
			// CWNext_Enabled will always be true in Winzor,
			// So this part of the code only applies to non-winzor environments.
			AssertSendERequestDocumentInMainForm(false, null, false);
			AssertSendERequestDocumentInMainForm(false, dummyModule, true);
#endif
			AssertSendERequestDocumentInMainForm(true, null, true);
			AssertSendERequestDocumentInMainForm(true, dummyModule, true);
		}

		void AssertSendERequestDocumentInMainForm(bool isCWNextEnabled, INamedModule module, bool isExpectedSentERequestDocument)
		{
			using (CWNextFeatureTestHelper.SetIsCWNextEnabled(isCWNextEnabled))
			{
				using var form = new MainFormForTest();
				form.CurrentModule = module;
				form.Show();
				var controller = Controller;
				(controller as IServiceRequestController).SetParentForm(form);
				UnitTestUserNotification.Instance.AddOKAnswer();
				AssertNull(Controller.ShowNewForm());
				AssertEquals(isExpectedSentERequestDocument, Factory.Load<EDIInterchange>(new ZQuery()).Any());
				AssertEquals(
					isExpectedSentERequestDocument,
					UnitTestUserNotification.Instance.PreviousMessages.Any(
						m => m.Text == "Would you like us to take a Screenshot and attach it to your incident?"));
			}
		}

		[ExpectNoExceptions]
		public void TestCustomerServiceMenuSectionCodeWithERequestPortalModuleCode()
		{
			var tree = ModuleTree.Tree;
			var categories = tree.Categories;
			var sections = new HashSet<ModuleSection>();
			foreach (var category in categories.ValuesIncludingHidden)
			{
				foreach (var section in category.Sections.ValuesIncludingHidden)
				{
					foreach (var module in section.Modules.ValuesIncludingHidden)
					{
						if (!sections.Contains(module.ParentSection))
						{
							sections.Add(module.ParentSection);
						}
					}
				}
			}

			var eRequestPortalCodes20210829 = new Dictionary<string, string>() {
				{ "ACC", "Account" },
				{ "ADV", "Address Cleansing" },
				{ "ADM", "Administrative" },
				{ "ALT", "Alerts" },
				{ "APP", "Application Deployment" },
				{ "ARM", "Archive Manager" },
				{ "BMG", "Behavior Management" },
				{ "BLU", "Blue Planet" },
				{ "BUG", "Budgets" },
				{ "BUF", "Buffer Management" },
				{ "BOT", "Business Intelligence - CDC / Other" },
				{ "BIE", "Business Intelligence - Enhancements" },
				{ "BNI", "Business Intelligence - New Implementation" },
				{ "NEO", "Cargowise Neo" },
				{ "EXM", "CargoWise One Certification Exams" },
				{ "CCM", "Carrier Contract Management (WiseRates)" },
				{ "CBK", "Cash Books" },
				{ "CFS", "CFS/CTO" },
				{ "CRM", "Client Relationship Management" },
				{ "ENE", "ComPay Integration" },
				{ "GLT", "Container & AWB Automation" },
				{ "CYM", "Container Yard" },
				{ "GDM", "Gate Management" },
				{ "CRD", "Credit Reports" },
				{ "CST", "Customer Service Tickets (Module)" },
				{ "CUS", "Customs" },
				{ "DBO", "Database Optimisation" },
				{ "DDS", "Deduplication" },
				{ "DPS", "Denied Party Screening" },
				{ "DIS", "Distance Calculations" },
				{ "DOC", "Docbuilder/Document Engine" },
				{ "DCM", "DocManager" },
				{ "EAH", "eAdaptor HTTP+XML" },
				{ "EAS", "eAdaptor SOAP" },
				{ "ECO", "Ecommerce" },
				{ "EDI", "EDI Interchange" },
				{ "EDM", "EDI Message" },
				{ "EDT", "ediTariff" },
				{ "HUB", "eHub Interfaces" },
				{ "EMS", "Email System Components" },
				{ "ENS", "Enrichment" },
				{ "INC", "eRequest Management Portal" },
				{ "ESV", "eServices" },
				{ "FOR", "Forwarding" },
				{ "GLG", "General Ledger" },
				{ "GLC", "GL Consolidations" },
				{ "GLM", "GL Period Management Change" },
				{ "GPS", "GPS (do not use)" },
				{ "INS", "Installation" },
				{ "INT", "Internal Development" },
				{ "I&L", "Invoicing & Licensing - General Queries" },
				{ "JCT", "Job Costing" },
				{ "JRB", "Job Related Billing" },
				{ "JTS", "Job Titles" },
				{ "LTN", "Land Transport" },
				{ "LAN", "Language" },
				{ "LAD", "Learning & Development" },
				{ "SHM", "Liner & Agency" },
				{ "LCT", "Locations" },
				{ "LDS", "Logistics Device" },
				{ "MDM", "Master Data" },
				{ "MYA", "MyAccount Authentication/Credentials" },
				{ "XNA", "Native XML" },
				{ "NET", "Netting" },
				{ "OVO", "Operations View Only Access" },
				{ "ORD", "Order Manager" },
				{ "OTH", "Other" },
				{ "PAY", "Payables" },
				{ "HRM", "People Operations" },
				{ "PNS", "Phone Number Standards" },
				{ "NCN", "Planning, NCN and SST's" },
				{ "LOC", "Port Transport" },
				{ "PTS", "Printing System Components" },
				{ "PRT", "Productivity Tools" },
				{ "WRS", "Rates Service" },
				{ "RTU", "Realtime Universal Shipment" },
				{ "RCB", "Receivables" },
				{ "RCR", "Recruitment" },
				{ "REC", "Recruitment - Candidate management" },
				{ "REF", "Reference Files/Data" },
				{ "REG", "Registry" },
				{ "RPS", "Reports" },
				{ "SAL", "Sales & Marketing" },
				{ "SCH", "Schedules" },
				{ "SVT", "Service Tasks / Process Controllers" },
				{ "SQL", "SQL Server" },
				{ "STM", "Staff and Resources" },
				{ "SYS", "System" },
				{ "TAR", "Tariffs & Rates" },
				{ "TRN", "Training Schedules / Manager" },
				{ "TWH", "Transit Warehouse" },
				{ "TBK", "Transport Booking" },
				{ "XUN", "Universal XML" },
				{ "UPN", "Update Notes" },
				{ "UPG", "Upgrades Architecture" },
				{ "USA", "User Admin" },
				{ "VAL", "Value Analysis" },
				{ "WAR", "Product Warehouse" },
				{ "WEB", "WebTracker / Web Portals" },
				{ "HOS", "Wise Cloud" },
				{ "WAM", "WiseCloud Automated Monitoring" },
				{ "WRK", "Workflow & Process" },
				{ "PRO", "Workflow Manager" },
				{ "BIL", "WTG Automated Billing/Usage reports" }
			};

			var cw1MenuSectionCodeExclusion = new HashSet<string> { "OCS", "AST", "RPB", "EQM" }; // OCS & AST & RPB & EQM are not released as of 2025-02-04

			bool FuzzyCompare(string portalText, string cw1Text)
			{
				return (portalText.Equals("Business Intelligence - New Implementation") && cw1Text.Equals("Business Intelligence & Analytics"))
					|| (portalText.Equals("Cash Books") && cw1Text.Equals("Cash Book"))
					|| (portalText.Equals("Buffer Management") && cw1Text.Equals("Planning"))
					|| (portalText.Equals("Reference Files/Data") && cw1Text.Equals("Reference Files"))
					|| (portalText.Equals("Recruitment - Candidate management") && cw1Text.Equals("Recruiter"))
					|| (portalText.Equals("EDI Interchange") && cw1Text.Equals("EDI Messaging"))
					|| (portalText.Equals("Printing System Components") && cw1Text.Equals("Printing"))
					|| (portalText.Equals("Email System Components") && cw1Text.Equals("Email"))
					|| (portalText.Equals("Customs") && cw1Text.Equals("Customs Global"))
					|| (portalText.Equals("Customs") && cw1Text.Equals("CCS-UK"))
					|| (portalText.Equals("Customs") && cw1Text.Equals("NCTS"))
					|| (portalText.Equals("Customs") && cw1Text.Equals("Customs Files"))
					|| (portalText.Equals("Customs") && cw1Text.Equals("Customs (US)"))
					|| (portalText.Equals("Customs") && cw1Text.Equals("Customs (CA)"))
					|| (portalText.Equals("Customs") && cw1Text.Equals("Customs (CO)"))
					|| (portalText.Equals("Customs") && cw1Text.Equals("Customs (GB)"))
					|| (portalText.Equals("Customs") && cw1Text.Equals("EXDOC"))
					|| (portalText.Equals("Customs") && cw1Text.Equals("Stamp Duty"))
					|| (portalText.Equals("Product Warehouse") && cw1Text.Equals("Warehouse")); //config shared by Operate -> Product Warehouse and Maintain -> Warehouse.
			}

			CombineAssertions(() =>
			{
				foreach (var section in sections)
				{
					if (!eRequestPortalCodes20210829.TryGetValue(section.CustomerServiceMenuSectionCode ?? "", out var moduleDescription))
					{
						if (!cw1MenuSectionCodeExclusion.Contains(section.CustomerServiceMenuSectionCode))
						{
							Fail($"The eRequest Portal Module Code list does not contain the code [{section.CustomerServiceMenuSectionCode}] [{section.DisplayText}]");
						}
					}
					else
					{
						if (!moduleDescription.Equals(section.DisplayTextWithoutAmpersand, StringComparison.OrdinalIgnoreCase) && !FuzzyCompare(moduleDescription, section.DisplayTextWithoutAmpersand))
						{
							Fail($"The CW1 menu description does not match the module description on eRequest Portal. Code:[{section.CustomerServiceMenuSectionCode}], (CW1 description)[{section.DisplayText}] <--> (eRequest description)[{moduleDescription}]");
						}
					}
				}
			});
		}

		public void TestMainForm_DefaultModuleId()
		{
			var controller = new IncidentApprovalController();

			AssertEquals(MandatoryCustomerServiceMenuSectionList.Codes.Other, controller.GetCustomerServiceMenuSectionCode(""));

			using (var form = new MainFormForTest())
			{
				AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.System, controller.GetCustomerServiceMenuSectionCode("", form));
			}
		}

		class ControllerForTest : IncidentApprovalController
		{
			internal IZForm GetFormForTest(IncidentApproval incident)
			{
				return base.GetForm(incident);
			}

			public FormAction LastAction;

			protected override IZForm ShowLoadedForm(IBusiness bizo, FormAction action)
			{
				LastAction = action;
				return null;
			}
		}

		class MainFormForTest : ZForm, IMainForm
		{
			public INamedModule CurrentModule { get; set; }
			public string CurrentModuleLicenceCheckPointName => "Dummy";
			public void UpdateToolBarDeleteButton(ZEmbeddedModule embeddedModule) { }
		}

		class DummyVisualBoardForm : MainFormForTest, ICustomerServiceMenuSectionCodeOverridable
		{
			public string SectionCode => ModuleTreeCustomerServiceMenuSectionList.Codes.BufferManagement;
		}
	}
}
