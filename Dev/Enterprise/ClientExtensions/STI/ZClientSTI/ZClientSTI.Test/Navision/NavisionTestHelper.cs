using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.STI.Navision.Testing
{
	public static class NavisionTestHelper
	{
		public static OrgHeader OrgForTesting(BusinessObjectFactory factory)
		{
			OrgHeader org = factory.New<OrgHeader>();
			org.CompanyData.SetARTaxApplicable(ZBool.False);
			org.OH_IsDebtor = ZBool.True;
			org.OH_FullName = "Eagle Datamation International";
			org.OH_Code = "EAGLEDAT";
			org.MainAddress.OA_Address1 = "Level 3";
			org.MainAddress.OA_Address2 = "184 Bourke Road";
			org.MainAddress.OA_City = "Alexandria";
			org.MainAddress.OA_Phone = "61 2 9025 1100";
			org.MiscServ.OM_ARCreditLimit = 100000m;
			org.MiscServ.OM_OJ_ARDebtorGroup = factory.LoadFromUniqueKey(typeof(OrgDebtorGroup), OrgDebtorGroupSchema.OJ_Code, (ZString)"ASC").PK;
			org.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceTerm = Core.Constants.InvoiceTerms.FromInvoiceDate;
			org.CompanyData.LoadARTermForAllInvoiceTypes().PY_InvoiceDays = 30;
			OrgStaffAssignments staffAssignment = org.StaffAssignments.AddNew();
			staffAssignment.O8_GS_NKPersonResponsible = "E";
			org.MiscServ.OM_EXDefaultIncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			org.OH_RL_NKClosestPort = "AUSYD";
			org.ARSettlementGroupPK = factory.LoadFromUniqueKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, (ZString)"NYKLIN").PK;
			org.MainAddress.OA_Fax = "61 2 9025 1199";
			org.MainAddress.OA_PostCode = "2015";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_Email = "support@cargowise.com";
			org.MainWebURL.PU_URL = "http://www.cargowise.com";
			org.PrimaryRegistrationNumber.Number = "41 065 894 724";
			org.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			factory.Save();
			return org;
		}

		public static CommonConsol ConsolForTesting(BusinessObjectFactory factory)
		{
			CommonConsol consol = factory.NewWithValidTestData<CommonConsol>();
			Transport transport = consol.Transports[0];
			transport.JW_ETD = new ZDateTime(2006, 2, 22, 9, 50, 31);
			transport.JW_ATA = new ZDateTime(2006, 2, 23, 19, 34, 29);
			consol.JK_AgentsReference = "QIUOHIFUH98174H";
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "GBLON";
			var vessel = factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Qantas";
			JobVoyage voyage = factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_AirSeaRoad = "Air";
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "QF1234";
			factory.Save();
			consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = "USLAX";
			consol.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2005, 9, 24, 14, 23, 34);
			consol.Transports.MostInterestingTransport.JW_ATD = new ZDateTime(2005, 10, 2, 13, 12, 43);
			factory.Save();
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = "GBLON";
			consol.Transports.MostInterestingTransport.JW_ETA = new ZDateTime(2005, 9, 29, 15, 47, 3);
			consol.Transports.MostInterestingTransport.JW_ATA = new ZDateTime(2005, 9, 30, 13, 2, 4);
			factory.Save();
			voyage.GenerateSailings();
			transport.JW_ATA = new ZDateTime(2005, 10, 1, 4, 5, 2);
			transport.JW_ETD = new ZDateTime(2005, 9, 24, 14, 23, 34);
			transport.JW_ETA = new ZDateTime(2005, 9, 29, 15, 47, 3);
			factory.Save();
			transport.JW_VoyageFlight = "QF320";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "8JO09UP93U2R928P3DY";
			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "MOOOD09908";
			container = consol.Containers.AddNew();
			container.JC_ContainerNum = "98U09230954";
			factory.Save();
			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_BookingReference = "BOOKIH98723948IYHS";
			shipment.JS_GoodsDescription = "Wheat";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_IsConsignee, ZBool.True)).PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_IsConsignor, ZBool.True)).PK;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_HouseBill = "3JM93570943759O8FOUI";
			OrderItem orderItem = shipment.DocsAndCartage.OrderItems.AddNew();
			orderItem.JT_OrderReference = "Order ref 1";
			orderItem.JT_Sequence = 1;
			orderItem = shipment.DocsAndCartage.OrderItems.AddNew();
			orderItem.JT_OrderReference = "Order ref 2";
			orderItem.JT_Sequence = 2;
			factory.Save();
			return consol;
		}
	}
}
