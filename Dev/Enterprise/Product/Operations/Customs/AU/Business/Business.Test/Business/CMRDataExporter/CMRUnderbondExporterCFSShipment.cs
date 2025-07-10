using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRUnderbondExporter))]
	public class CMRUnderbondExporterCFSShipment : CMRDataExporterCSVTest
	{
		protected override string ExpectedFileName
		{
			get { return "UBond"; }
		}

		protected override string ExpectedMailSubject
		{
			get { return "Contingency Underbond Movement"; }
		}

		protected override bool IsDocManagerSupported
		{
			get { return true; }
		}

		protected override CMRDataExporterCSV GetNewExporter()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			consol.JK_MasterBillNum = "MASTER";
			consol.JK_RL_NKDischargePort = "AUBNE";
			Transport transport = consol.Transports.AddNew();
			transport.JW_Vessel = "ADMIRALENGRACHT";
			transport.JW_VoyageFlight = "4321S";
			CFSShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HOUSE";
			shipment.JS_GoodsDescription = "GOODS DESC.";
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			CFSContainer container = consol.Containers.AddNew();
			container.JC_ContainerMode = "FCL";
			container.JC_ContainerNum = "OCLU333344";
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE";
			consignee.MainAddress.OA_Address1 = "STREET,";
			consignee.MainAddress.OA_Address2 = "STREET2,";
			consignee.MainAddress.OA_City = "CITY";
			consignee.MainAddress.OA_State = "NSW";
			consignee.MainAddress.OA_PostCode = "2222";
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "CONSIGNOR";
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_ModeOfMovement = "MOV";
			underbond.C4_OriginPremiseID = "1234X";
			underbond.C4_DestinationPremiseID = "5678A";
			underbond.C4_PiecesManifested = 7;
			underbond.C4_SendersMessageReference = "H999999";
			underbond.LinkedObject = CFSShipmentWrapper.Load(shipment);
			underbond.C4_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			return new CMRUnderbondExporter(underbond);
		}

		protected override string ExpectedCSVResult
		{
			get
			{
				return
"123 441,test@example.com,8811924,4321S,MASTER,HOUSE,FCL,OCLU333344,GOODS DESC.,CONSIGNOR,CONSIGNEE,STREET STREET2 CITY NSW 2222,AUBNE,,,MOV,,1234X,5678A,7,H999999\r\n";
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			currentUserEmailAddress = GlbStaff.CurrentUser.GS_EmailAddress;
			currentyCompanyABN = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number;
			GlbStaff.CurrentUser.GS_EmailAddress = "test@example.com";
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "123 441";
		}

		protected override void TearDown()
		{
			GlbStaff.CurrentUser.GS_EmailAddress = currentUserEmailAddress;
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = currentyCompanyABN;
			base.TearDown();
		}

		string currentUserEmailAddress;
		string currentyCompanyABN;

		#endregion
	}
}
