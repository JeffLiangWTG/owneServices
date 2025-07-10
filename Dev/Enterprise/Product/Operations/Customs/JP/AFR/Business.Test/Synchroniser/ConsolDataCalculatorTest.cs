using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class ConsolDataCalculatorTest : Customs.Business.Testing.ConsolDataCalculatorTest
	{
		protected override Customs.Business.ConsolDataCalculator CreateNewCalculator(ForwardingConsol consol)
		{
			var header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			header.Synchroniser.SetEnabled(false, false);

			return new ConsolDataCalculator(consol, header);
		}

		protected override RefUNLOCO CreatePortInTheCountry1()
		{
			return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "JPTKY");
		}

		protected override RefUNLOCO CreatePortInTheCountry2()
		{
			return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "JPHAO");
		}

		protected override RefUNLOCO CreatePortInTheCountry3()
		{
			return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "JPHTR");
		}

		protected override RefUNLOCO CreatePortNotInTheCountry1()
		{
			return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
		}

		protected override RefUNLOCO CreatePortNotInTheCountry2()
		{
			return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
		}

		protected override RefUNLOCO CreatePortNotInTheCountry3()
		{
			return Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN");
		}
	}

	class JPConsolDataCalculatorTest : AFRSynchroniserTestCase
	{
		public void TestGetSCAC()
		{
			#region GetSCAC from dbo.OrgHeader
			AssertEquals(ZString.Empty, ConsolDataCalculator.GetSCAC(null as OrgHeader));
			AssertEquals(orgProxyCarrierCode.OK_CustomsRegNo, ConsolDataCalculator.GetSCAC(orgProxy));
			AssertEquals(org1CarrierCode.OK_CustomsRegNo, ConsolDataCalculator.GetSCAC(org1));
			AssertEquals(org2CarrierCode.OK_CustomsRegNo, ConsolDataCalculator.GetSCAC(org2));
			AssertEquals(ZString.Empty, ConsolDataCalculator.GetSCAC(Factory.New<OrgHeader>()));
			#endregion

			#region GetSCAC from dbo.JobDocAddress
			var testAddress1 = Factory.New<JobDocAddress>();
			testAddress1.E2_OA_Address = org1.MainAddress.PK;
			var testAddress2 = Factory.New<JobDocAddress>();
			testAddress2.E2_OA_Address = org2.MainAddress.PK;
			AssertEquals(ZString.Empty, ConsolDataCalculator.GetSCAC(null as JobDocAddress));
			AssertEquals(org1CarrierCode.OK_CustomsRegNo, ConsolDataCalculator.GetSCAC(testAddress1));
			AssertEquals(org2CarrierCode.OK_CustomsRegNo, ConsolDataCalculator.GetSCAC(testAddress2));
			AssertEquals(ZString.Empty, ConsolDataCalculator.GetSCAC(Factory.New<JobDocAddress>()));
			#endregion
		}

		public void TestOrgProxySCAC()
		{
			AssertEquals(orgProxyCarrierCode.OK_CustomsRegNo, calculator.OrgProxySCAC);
			header.JPH_GB_Branch = otherCompanyBranch.PK;
			AssertEquals(otherCompanyOrgProxyCarrierCode.OK_CustomsRegNo, calculator.OrgProxySCAC);
			header.JPH_GB_Branch = GlbBranch.CurrentBranch.PK;
			AssertEquals(orgProxyCarrierCode.OK_CustomsRegNo, calculator.OrgProxySCAC);
		}

		public void TestCarrierCode()
		{
			consol.JK_OA_ShippingLineAddress = org1.MainAddress.PK;
			AssertEquals(org1CarrierCode.OK_CustomsRegNo, calculator.CarrierCode);
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;
			AssertEquals(ZString.Empty, calculator.CarrierCode);
		}

		public void TestLastForeignPortOfLoading()
		{
			AssertNull(calculator.LastForeignPortOfLoading);

			consol.JK_RL_NKLastForeignPort = AUSYD.RL_Code;
			consol.JK_DateLastForeignPort = new ZDateTime(2011, 4, 10);
			AssertEquals(AUSYD, calculator.LastForeignPortOfLoading);
			var transport1 = consol.Transports[0];
			transport1.JW_LegOrder = 1;
			transport1.JW_RL_NKLoadPort = AUMEL.RL_Code;
			transport1.JW_RL_NKDiscPort = JPTKY.RL_Code;
			transport1.JW_ETD = new ZDateTime(2011, 4, 9);
			transport1.JW_ATD = new ZDateTime(2011, 4, 11);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_LegOrder = 2;
			transport2.JW_RL_NKLoadPort = JPTKY.RL_Code;
			transport2.JW_RL_NKDiscPort = SGSIN.RL_Code;
			transport2.JW_ATD = new ZDateTime(2011, 4, 12);
			AssertEquals(AUMEL, calculator.LastForeignPortOfLoading);

			transport1.JW_ATD = ZDateTime.Empty;
			AssertEquals(AUSYD, calculator.LastForeignPortOfLoading);

			consol.JK_RL_NKLastForeignPort = ZString.Empty;
			AssertEquals(AUMEL, calculator.LastForeignPortOfLoading);

			consol.JK_RL_NKLastForeignPort = JPTKY.RL_Code;
			AssertEquals(AUMEL, calculator.LastForeignPortOfLoading);
		}

		public void TestGetInfosAffectingLastForeignPortOfLoading()
		{
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();

			var list = new List<ZPropertyInfo>(calculator.GetInfosAffectingLastForeignPortOfLoading());
			AssertEquals(14, list.Count);
			AssertCollectionContains(consol.JK_RL_NKLastForeignPortInfo, list);
			AssertCollectionContains(consol.JK_DateLastForeignPortInfo, list);

			AssertCollectionContains(transport1.JW_LegOrderInfo, list);
			AssertCollectionContains(transport1.JW_TransportModeInfo, list);
			AssertCollectionContains(transport1.JW_RL_NKDiscPortInfo, list);
			AssertCollectionContains(transport1.JW_RL_NKLoadPortInfo, list);
			AssertCollectionContains(transport1.JW_ETDInfo, list);
			AssertCollectionContains(transport1.JW_ATDInfo, list);

			AssertCollectionContains(transport2.JW_LegOrderInfo, list);
			AssertCollectionContains(transport2.JW_TransportModeInfo, list);
			AssertCollectionContains(transport2.JW_RL_NKDiscPortInfo, list);
			AssertCollectionContains(transport2.JW_RL_NKLoadPortInfo, list);
			AssertCollectionContains(transport2.JW_ETDInfo, list);
			AssertCollectionContains(transport2.JW_ATDInfo, list);
		}

		public void TestLoadTransportForJPBoundVessel()
		{
			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport1.JW_LegOrder = 1;
			transport1.JW_Vessel = "VESSEL";
			transport1.JW_RL_NKLoadPort = AUSYD.RL_Code;
			transport1.JW_RL_NKDiscPort = SGSIN.RL_Code;
			AssertEquals(transport1, calculator.LoadTransportForJPBoundVessel);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport2.JW_LegOrder = 2;
			transport2.JW_Vessel = "VESSEL";
			transport2.JW_RL_NKLoadPort = SGSIN.RL_Code;
			transport2.JW_RL_NKDiscPort = JPTKY.RL_Code;
			AssertEquals(transport1, calculator.LoadTransportForJPBoundVessel);

			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport3.JW_LegOrder = 3;
			transport3.JW_Vessel = "VESSEL";
			transport3.JW_RL_NKLoadPort = JPTKY.RL_Code;
			transport3.JW_RL_NKDiscPort = JPHTR.RL_Code;
			AssertEquals(transport1, calculator.LoadTransportForJPBoundVessel);

			var transport4 = consol.Transports.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport4.JW_LegOrder = 4;
			transport4.JW_RL_NKLoadPort = JPHTR.RL_Code;
			transport4.JW_RL_NKDiscPort = AUMEL.RL_Code;
			AssertEquals(transport1, calculator.LoadTransportForJPBoundVessel);

			transport1.JW_Vessel = "VESSEL1";
			AssertEquals(transport2, calculator.LoadTransportForJPBoundVessel);
		}

		public void TestGetInfosAffectingLoadTransportForJPBoundVessel()
		{
			var transport1 = consol.Transports[0];
			var transport2 = consol.Transports.AddNew();

			var list = new List<ZPropertyInfo>(calculator.GetInfosAffectingLoadTransportForJPBoundVessel());
			AssertEquals(10, list.Count);

			AssertCollectionContains(transport1.JW_LegOrderInfo, list);
			AssertCollectionContains(transport1.JW_TransportModeInfo, list);
			AssertCollectionContains(transport1.JW_RL_NKLoadPortInfo, list);
			AssertCollectionContains(transport1.JW_RL_NKDiscPortInfo, list);
			AssertCollectionContains(transport1.JW_VesselInfo, list);

			AssertCollectionContains(transport2.JW_LegOrderInfo, list);
			AssertCollectionContains(transport2.JW_TransportModeInfo, list);
			AssertCollectionContains(transport2.JW_RL_NKLoadPortInfo, list);
			AssertCollectionContains(transport2.JW_RL_NKDiscPortInfo, list);
			AssertCollectionContains(transport2.JW_VesselInfo, list);
		}

		ConsolDataCalculator calculator;

		protected override void SetUp()
		{
			base.SetUp();
			calculator = new ConsolDataCalculator(consol, header);
		}

		protected override void TearDown()
		{
			calculator.Dispose();
			calculator = null;
			base.TearDown();
		}
	}

	abstract class AFRSynchroniserTestCase : SynchroniserTestCase
	{
		protected void AssertBill(JPAFRBills bill, ZGuid pk, ZString billNumber)
		{
			AssertEquals(pk, bill.PK);
			AssertBill(bill, billNumber);
		}

		protected void AssertBill(JPAFRBills bill, ZString billNumber)
		{
			AssertEquals(billNumber, bill.JPB_BillNumber);
		}

		protected OrgHeader orgProxy;
		protected OrgCusCode orgProxyCarrierCode;
		protected GlbCompany otherCompany;
		protected GlbBranch otherCompanyBranch;
		protected OrgHeader otherCompanyOrgProxy;
		protected OrgCusCode otherCompanyOrgProxyCarrierCode;
		protected OrgHeader org1;
		protected OrgCusCode org1CarrierCode;
		protected OrgHeader org2;
		protected OrgCusCode org2CarrierCode;
		protected ForwardingConsol consol;
		protected JPAFRHeader header;

		protected RefUNLOCO AUSYD
		{
			get { return ausyd ?? (ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD")); }
		}
		RefUNLOCO ausyd;

		protected RefUNLOCO AUMEL
		{
			get { return aumel ?? (aumel = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL")); }
		}
		RefUNLOCO aumel;

		protected RefUNLOCO SGSIN
		{
			get { return sgsin ?? (sgsin = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN")); }
		}
		RefUNLOCO sgsin;

		protected RefUNLOCO JPTKY
		{
			get { return jptky ?? (jptky = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "JPTKY")); }
		}
		RefUNLOCO jptky;

		protected RefUNLOCO JPHAO
		{
			get { return jphao ?? (jphao = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "JPHAO")); }
		}
		RefUNLOCO jphao;

		protected RefUNLOCO JPHTR
		{
			get { return jphtr ?? (jphtr = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "JPHTR")); }
		}
		RefUNLOCO jphtr;

		protected override void SetUp()
		{
			base.SetUp();
			otherCompany = Factory.NewWithValidTestData<GlbCompany>();
			otherCompany.GC_Code = "Z!Z";
			otherCompanyOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
			otherCompanyOrgProxy.OH_Code = "ZZZ123WWW";
			otherCompanyOrgProxyCarrierCode = otherCompanyOrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT2", Core.Constants.CountryCodes.Japan);
			otherCompany.GC_OH_OrgProxy = otherCompanyOrgProxy.PK;
			otherCompanyBranch = Factory.NewWithValidTestData<GlbBranch>();
			otherCompanyBranch.GB_Code = "Z!Z";
			otherCompany.Branches.Add(otherCompanyBranch);

			orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			orgProxyCarrierCode = orgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT1", Core.Constants.CountryCodes.Japan);

			org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ORG1Z";
			org1.MainAddress.OA_Address1 = "ORG1Z ADDRESS 1";
			org1CarrierCode = org1.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "ORG1", Core.Constants.CountryCodes.Japan);

			org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ORG2Z";
			org2.MainAddress.OA_Address1 = "ORG2Z ADDRESS 1";
			org2CarrierCode = org2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "ORG2", Core.Constants.CountryCodes.Japan);

			consol = CreateFCLConsol();

			header = Factory.New<JPAFRHeader>();
			header.JPH_ParentId = consol.PK;
			header.JPH_ParentTableCode = consol.TablePrefix;
			header.Synchroniser.SetEnabled(false, false);
		}
	}
}
