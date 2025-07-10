using System;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ConsolCRNExporter))]
	public class ConsolCRNExporterAIRTest : CMRDataExporterCSVTest
	{
		protected override string ExpectedFileName
		{
			get { return "CRN"; }
		}

		protected override string ExpectedMailSubject
		{
			get { return "Contingency Export Sub Manifest"; }
		}

		protected override bool IsDocManagerSupported
		{
			get { return true; }
		}

		protected override CMRDataExporterCSV GetNewExporter()
		{
			FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			OrgHeader consignor1 = OrgHeader.New(Factory);
			consignor1.OH_FullName = "Consignor 1";
			OrgHeader consignor2 = OrgHeader.New(Factory);
			consignor2.OH_FullName = "Consignor 2";
			OrgHeader consignor3 = OrgHeader.New(Factory);
			consignor3.OH_FullName = "Consignor 3";

			OrgHeader importer = OrgHeader.New(Factory);
			importer.OH_FullName = "I am an importer";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C0000100";
			consol.JK_TransportMode = "AIR";
			consol.JK_MasterBillNum = "08111112222";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_RL_NKLoadPort = "AUSYD";

			Transport transportLeg1 = consol.Transports[0];
			transportLeg1.JW_ETD = new ZDateTime(2005, 10, 14);
			transportLeg1.JW_RL_NKLoadPort = "AUPER";
			transportLeg1.JW_RL_NKDiscPort = "AUMEL";
			transportLeg1.JW_VoyageFlight = "QF123";

			Transport transportLeg2 = consol.Transports.AddNew();
			transportLeg2.JW_ETD = new ZDateTime(2005, 10, 14);
			transportLeg2.JW_RL_NKLoadPort = "AUMEL";
			transportLeg2.JW_RL_NKDiscPort = "AUSYD";
			transportLeg2.JW_VoyageFlight = "QF315";

			Transport transportLeg3 = consol.Transports.AddNew();
			transportLeg3.JW_ETD = new ZDateTime(2005, 10, 15);
			transportLeg3.JW_RL_NKLoadPort = "AUSYD";
			transportLeg3.JW_RL_NKDiscPort = "USLAX";
			transportLeg3.JW_VoyageFlight = "AA422";

			CommonShipment ship1 = consol.Shipments.AddNew();
			ship1.JS_HouseBill = "H111";
			ship1.CustomsEntryNumberType = "CAN";
			ship1.CustomsEntryNumber = "1234";
			ship1.ConsignorPK = consignor1.PK;
			ship1.JS_GoodsDescription = "Ship 1 Goods";

			CommonShipment ship2 = consol.Shipments.AddNew();
			ship2.JS_HouseBill = "07304222";
			ship2.CustomsEntryNumberType = "";
			ship2.ConsignorPK = consignor2.PK;
			ship2.JS_GoodsDescription = "Ship 2 Goods";

			CommonShipment ship3 = consol.Shipments.AddNew();
			ship3.JS_HouseBill = "H333";
			ship3.CustomsEntryNumberType = "EXDC";
			ship3.ConsignorPK = consignor3.PK;
			ship3.JS_GoodsDescription = "Ship 3 Goods";

			return new ConsolCRNExporter(consol);
		}

		protected override string ExpectedCSVResult
		{
			get
			{
				return
"Eagle Datamation International,23112936991,test@example.com,C0000100,08111112222,H111,AA422,AIR,20051015,1234,Consignor 1,Ship 1 Goods,US,AUSYD,,\r\n" +
"Eagle Datamation International,23112936991,test@example.com,C0000100,08111112222,07304222,AA422,AIR,20051015,,Consignor 2,Ship 2 Goods,US,AUSYD,,\r\n" +
"Eagle Datamation International,23112936991,test@example.com,C0000100,08111112222,H333,AA422,AIR,20051015,EXDC,Consignor 3,Ship 3 Goods,US,AUSYD,,\r\n";
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			currentUserEmailAddress = GlbStaff.CurrentUser.GS_EmailAddress;
			currentyCompanyABN = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number;
			GlbStaff.CurrentUser.GS_EmailAddress = "test@example.com";
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "23 112 936 991";
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
