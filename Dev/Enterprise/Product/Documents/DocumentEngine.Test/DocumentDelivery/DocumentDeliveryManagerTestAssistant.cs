using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Testing
{
	public static class DocumentDeliveryManagerTestAssistant
	{
		public static (StmDocumentDelivery, Forwarding.IForwardingShipment) CreateNewDocumentDeliveryWithDocumentSupportable_Shipment(BusinessObjectFactory factory, GlbStaff staff, GlbBranch branch, GlbDepartment department)
		{
			var shipment = factory.New<Forwarding.IForwardingShipment>();
			shipment.JS_TransportMode = "AIR";
			shipment.JS_RL_NKOrigin = "SGSIN";
			shipment.JS_RL_NKDestination = "DEFRA";

			var consol = factory.New<Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.AddShipment(shipment);

			var forwardingDocumentSupporter = consol as IDocumentSupportable;
			var customization = DocumentMenuCustomisation.New(forwardingDocumentSupporter, null, factory);
			var documentCommand = customization.Menus.AddNew();
			documentCommand.SU_MenuName = "Test Primary Doc" + ZGuid.NewZGuid();
			documentCommand.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			documentCommand.SU_IsPublished = true;
			documentCommand.SU_BusinessContext = "Shipment";
			documentCommand.SU_MenuIndex = 0;
			documentCommand.SU_FilterList = "\"<JS_TransportMode>\" == \"AIR\"";

			var shipmentDocumentMenu = customization.AvailableChildMenus.Cast<StmMenuItemBase>().FirstOrDefault(a => a.SU_BusinessContext == "Shipment" && a.SU_MenuName == "Combined Cartage Advice");
			var shipmentChildPivot = documentCommand.ChildMenus.AddNew();
			shipmentChildPivot.SF_SU_Inward = documentCommand.PK;
			shipmentChildPivot.SF_SU_Outward = shipmentDocumentMenu.PK;

			var consolDocumentMenu = customization.AvailableChildMenus.Cast<StmMenuItemBase>().FirstOrDefault(a => a.SU_BusinessContext == "Consol" && a.SU_MenuName == "Combined Cartage Advice");
			var consolChildPivot = documentCommand.ChildMenus.AddNew();
			consolChildPivot.SF_SU_Inward = documentCommand.PK;
			consolChildPivot.SF_SU_Outward = consolDocumentMenu.PK;

			documentCommand.SU_IsDocPack = true;
			documentCommand.SU_PrimaryDocPackItemId = consolChildPivot.PK;

			var documentDelivery = factory.New<StmDocumentDelivery>();
			var identifier = getIdentifier(shipmentDocumentMenu.Documents[0].PK, shipment.PK, shipmentDocumentMenu.Documents[0].DocumentTitle);
			documentDelivery.SDL_IsProcessed = false;
			documentDelivery.SDL_SU = documentCommand.PK;
			documentDelivery.SDL_ParentId = shipment.PK;
			documentDelivery.SDL_ParentControllerIdOrTableCode = "JobShipment";
			documentDelivery.SDL_RetryAttempts = 0;
			documentDelivery.SDL_Instructions = "{\"Language\":\"EN-GB\",\"NumberOfCopies\":\"1\",\"Recipients\":[{\"DeliveryMethod\":\"EML\",\"AttachmentType\":\"PDF\",\"DeliveryAddress\":\"b@b.com\"}],\"DocumentsToBeDelivered\":[{\"Copies\":\"1\",\"Identifier\":\"" + identifier + "\"}],\"EDocsToBeDelivered\":[]}";
			documentDelivery.SDL_GS = staff.PK;
			documentDelivery.SDL_GB = branch.PK;
			documentDelivery.SDL_GE = department.PK;

			factory.Save();
			return (documentDelivery, shipment);
		}

		public static (StmDocumentDelivery, Forwarding.IForwardingConsol) CreateNewDocumentDeliveryWithDocumentSupportable_Consol(BusinessObjectFactory factory, GlbStaff staff, GlbBranch branch, GlbDepartment department)
		{
			var consol = factory.New<Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = "AIR";

			var forwardingDocumentSupporter = consol as IDocumentSupportable;
			var customization = DocumentMenuCustomisation.New(forwardingDocumentSupporter, null, factory);
			var documentCommand = customization.Menus.AddNew();
			documentCommand.SU_MenuName = "Test Primary Doc" + ZGuid.NewZGuid();
			documentCommand.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			documentCommand.SU_IsPublished = true;
			documentCommand.SU_BusinessContext = "Consol";
			documentCommand.SU_MenuIndex = 0;
			documentCommand.SU_FilterList = "\"<JK_TransportMode>\" == \"AIR\"";

			var consolDocumentMenu = customization.AvailableChildMenus.Cast<StmMenuItemBase>().FirstOrDefault(a => a.SU_BusinessContext == "Consol" && a.SU_MenuName == "Combined Cartage Advice");
			var consolChildPivot = documentCommand.ChildMenus.AddNew();
			consolChildPivot.SF_SU_Inward = documentCommand.PK;
			consolChildPivot.SF_SU_Outward = consolDocumentMenu.PK;

			documentCommand.SU_IsDocPack = true;
			documentCommand.SU_PrimaryDocPackItemId = consolChildPivot.PK;

			var documentDelivery = factory.New<StmDocumentDelivery>();
			var identifier = getIdentifier(consolDocumentMenu.Documents[0].PK, consol.PK, consolDocumentMenu.Documents[0].DocumentTitle);
			documentDelivery.SDL_IsProcessed = false;
			documentDelivery.SDL_SU = documentCommand.PK;
			documentDelivery.SDL_ParentId = consol.PK;
			documentDelivery.SDL_ParentControllerIdOrTableCode = "JK";
			documentDelivery.SDL_RetryAttempts = 0;
			documentDelivery.SDL_Instructions = "{\"Language\":\"EN-GB\",\"NumberOfCopies\":\"1\",\"Recipients\":[{\"DeliveryMethod\":\"EML\",\"AttachmentType\":\"PDF\",\"DeliveryAddress\":\"b@b.com\"}],\"DocumentsToBeDelivered\":[{\"Copies\":\"1\",\"Identifier\":\"" + identifier + "\"}],\"EDocsToBeDelivered\":[]}";
			documentDelivery.SDL_GS = staff.PK;
			documentDelivery.SDL_GB = branch.PK;
			documentDelivery.SDL_GE = department.PK;
			factory.Save();
			return (documentDelivery, consol);
		}

		public static (StmDocumentDelivery, IDtbBooking) CreateNewDocumentDeliveryWithDocumentSupportable_DtbBooking(BusinessObjectFactory factory, GlbStaff staff, GlbBranch branch, GlbDepartment department)
		{
			var shipment = factory.New<Forwarding.IForwardingShipment>();
			var dtbBookingConsolidation = factory.New<IDtbBookingConsolidation>();
			dtbBookingConsolidation.KB_ParentID = shipment.PK;
			dtbBookingConsolidation.KB_ParentTableCode = "JS";
			var dtbBooking = factory.New<IDtbBooking>();
			dtbBooking.KM_KB_Booking = dtbBookingConsolidation.PK;
			var parent = dtbBookingConsolidation as IDocumentSupportable;
			var documentMenuCustomisation = DocumentMenuCustomisation.New(parent, null, factory);
			var documentCommand = documentMenuCustomisation.Menus.AddNew();
			documentCommand.SU_MenuName = "Cartage Advice";
			documentCommand.SU_MenuType = "WEB";
			documentCommand.SU_IsPublished = true;
			documentCommand.SU_BusinessContext = "DtbBooking";
			documentCommand.SU_MenuIndex = 0;
			var stmMenuItemBase = (from StmMenuItemBase a in documentMenuCustomisation.AvailableChildMenus
								   where a.SU_BusinessContext == "DtbBooking" && a.SU_MenuName == "Cartage Advice"
								   select a).FirstOrDefault();
			var stmMenuMenuPivotBase = documentCommand.ChildMenus.AddNew();
			stmMenuMenuPivotBase.SF_SU_Inward = documentCommand.PK;
			stmMenuMenuPivotBase.SF_SU_Outward = stmMenuItemBase.PK;
			documentCommand.SU_IsDocPack = true;
			documentCommand.SU_PrimaryDocPackItemId = stmMenuMenuPivotBase.PK;
			var stmDocumentDelivery = factory.New<StmDocumentDelivery>();
			var identifier = getIdentifier(stmMenuItemBase.Documents[0].PK, dtbBooking.PK, stmMenuItemBase.Documents[0].DocumentTitle);
			stmDocumentDelivery.SDL_IsProcessed = false;
			stmDocumentDelivery.SDL_SU = documentCommand.PK;
			stmDocumentDelivery.SDL_ParentId = dtbBooking.PK;
			stmDocumentDelivery.SDL_ParentControllerIdOrTableCode = "DtbBooking";
			stmDocumentDelivery.SDL_RetryAttempts = 0;
			stmDocumentDelivery.SDL_Instructions = "{\"Language\":\"EN-GB\",\"NumberOfCopies\":\"1\",\"Recipients\":[{\"DeliveryMethod\":\"EML\",\"AttachmentType\":\"PDF\",\"DeliveryAddress\":\"b@b.com\"}],\"DocumentsToBeDelivered\":[{\"Copies\":\"1\",\"Identifier\":\"" + identifier + "\"}],\"EDocsToBeDelivered\":[]}";
			stmDocumentDelivery.SDL_GS = staff.PK;
			stmDocumentDelivery.SDL_GB = branch.PK;
			stmDocumentDelivery.SDL_GE = department.PK;
			factory.Save();
			return (stmDocumentDelivery, dtbBooking);
		}

		static string getIdentifier(ZGuid menuTemplatePivotPK, ZGuid sourceBOPK, string documentTitle)
		{
			var key = string.Join("+",
				menuTemplatePivotPK,
				sourceBOPK,
				documentTitle,
				""
			);
			using var md5 = MD5.Create();
			return Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(key)));
		}

		public static (GlbStaff staff, GlbBranch branch, GlbDepartment department) InitialEnv(BusinessObjectFactory factory)
		{
			var staff = factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "a@b.com";
			staff.GS_Code = "ABC";
			staff.GS_LoginName = "ABC";
			staff.GS_FullName = "Damien Li";
			var company = factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "GB1";
			var department = factory.NewWithValidTestData<GlbDepartment>();
			department.GE_Code = "GE1";
			factory.Save();
			return (staff, branch, department);
		}
	}
}
