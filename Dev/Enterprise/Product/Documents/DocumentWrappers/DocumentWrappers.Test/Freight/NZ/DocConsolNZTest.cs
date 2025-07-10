using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.NZ.Testing
{
	[TestedType(typeof(DocumentWrappers.DocForwardingConsol))]
	sealed class DocConsolNZTest : DocumentWrapperTestCase
	{
		public void TestRefNoWithMAWBForSea()
		{
			Consol.JK_TransportMode = "SEA";
			Consol.JK_MasterBillNum = "111";
			Consol.JK_UniqueConsignRef = "CA1111111";
			AssertEquals("RefNo used for ORN", Consol.JK_UniqueConsignRef, docConsol.RefNoWithMAWB);
		}

		public void TestRefNoWithMAWBForAir()
		{
			Consol.JK_TransportMode = "AIR";
			Consol.JK_MasterBillNum = "08155555555";
			Consol.JK_UniqueConsignRef = "CA1111111";
			AssertEquals("RefNo used for ORN", Consol.JK_UniqueConsignRef + "/081-55555555", docConsol.RefNoWithMAWB);
		}

		public void TestContainerNumbers()
		{
			CommonContainer container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "CRUX123456";
			AssertEquals("Container numbers", "CRUX123456", docConsol.ContainerNumbers);
		}

		public void TestContainerNumbersForTwoContainers()
		{
			CommonContainer container1 = Consol.Containers.AddNew();
			container1.JC_ContainerNum = "CRUX123456";

			CommonContainer container2 = Consol.Containers.AddNew();
			container2.JC_ContainerNum = "CRUX123457";

			AssertEquals("Container numbers", "CRUX123456, CRUX123457", docConsol.ContainerNumbers);
		}

		public void TestORN()
		{
			var oRN = Factory.New<CusEntryNumber>();
			oRN.CE_ParentID = Consol.PK;
			oRN.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			oRN.CE_EntryType = CusEntryNumberTypeList.Codes.OutwardReportNumber;
			oRN.CE_EntryNum = "123456";

			AssertEquals("ORN", oRN.CE_EntryNum, docConsol.ORN);
		}

		public void TestConsolID()
		{
			Consol.JK_UniqueConsignRef = "C00009000";
			AssertEquals("ConsolID", Consol.JK_UniqueConsignRef, docConsol.ConsolNumber);
		}

		public void TestCurrentCompanyName()
		{
			AssertEquals("Current Company Name", GlbCompany.CurrentCompany.GC_Name, docConsol.CurrentCompany.Name);
		}

		public void TestVesselAircraftOperator()
		{
			var shippingLine = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Consol.SetDefaultShippingLineAddress(shippingLine);
			AssertEquals("Shipping line Name", shippingLine.OH_FullName, docConsol.VesselAircraftOperator);
		}

		public void TestCraftFlightNoForSea()
		{
			Consol.JK_TransportMode = "SEA";
			Transport.JW_Vessel = "Victoria";
			AssertEquals("Vessel Name", "Victoria", docConsol.CraftFlightNo);
		}

		public void TestCraftFlightNoForAir()
		{
			Consol.JK_TransportMode = "AIR";
			Transport.JW_VoyageFlight = "QF23";
			AssertEquals("Flight No", "QF23", docConsol.CraftFlightNo);
		}

		public void TestVoyageNoForSea()
		{
			Consol.JK_TransportMode = "SEA";
			Transport.JW_VoyageFlight = "123456789";
			AssertEquals("Voyage No", "123456789", docConsol.VoyageNo);
		}

		public void TestDepartureDate()
		{
			Transport.JW_ETD = new ZDateTime(2005, 2, 10);
			AssertEquals("Departure Date", "10-Feb-05", docConsol.DepartureDate);
		}

		public void TestSelectInternationalLeg()
		{
			Consol.JK_TransportMode = "AIR";

			var domesticLeg = Transport;
			domesticLeg.JW_TransportMode = "AIR";
			domesticLeg.JW_RL_NKLoadPort = "NZCHC";
			domesticLeg.JW_RL_NKDiscPort = "NZAKL";
			domesticLeg.JW_VoyageFlight = "NZ520";
			domesticLeg.JW_ETD = new ZDateTime(2011, 10, 24, 15, 10, 0);
			domesticLeg.JW_ETA = new ZDateTime(2011, 10, 24, 16, 30, 0);

			var internationalLeg = Consol.Transports.AddNew();
			internationalLeg.JW_TransportMode = "AIR";
			internationalLeg.JW_RL_NKLoadPort = "NZAKL";
			internationalLeg.JW_RL_NKDiscPort = "SGSIN";
			internationalLeg.JW_VoyageFlight = "SQ286";
			internationalLeg.JW_ETD = new ZDateTime(2011, 10, 25, 13, 10, 0);
			internationalLeg.JW_ETA = new ZDateTime(2011, 10, 25, 19, 30, 0);

			AssertEquals("International Leg", "SQ286", docConsol.CraftFlightNo);
			AssertEquals("DeparturePort", "NZAKL:Auckland", docConsol.DeparturePort);
			AssertEquals("ArrivalPort", "SGSIN:Singapore", docConsol.DischargePort);
			AssertEquals("DepartureDate", "25-Oct-11", docConsol.DepartureDate);
		}

		public void TestSelectInternationalLegForSEA()
		{
			Consol.JK_TransportMode = "SEA";

			var query = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.GB_RL_NKHomePort);
			query.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			var localPort2 = Factory.LoadTop1<RefUNLOCO>(query);

			var domesticLeg = Transport;
			domesticLeg.JW_TransportMode = "SEA";
			domesticLeg.JW_RL_NKLoadPort = "NZCHC";
			domesticLeg.JW_RL_NKDiscPort = "NZAKL";
			domesticLeg.JW_VoyageFlight = "V0520";
			domesticLeg.JW_ETD = new ZDateTime(2011, 10, 24, 15, 10, 0);
			domesticLeg.JW_ETA = new ZDateTime(2011, 10, 24, 16, 30, 0);

			var internationalLeg = Consol.Transports.AddNew();
			internationalLeg.JW_TransportMode = "SEA";
			internationalLeg.JW_RL_NKLoadPort = "NZAKL";
			internationalLeg.JW_RL_NKDiscPort = "SGSIN";
			internationalLeg.JW_VoyageFlight = "V1381";
			internationalLeg.JW_ETD = new ZDateTime(2011, 10, 25, 13, 10, 0);
			internationalLeg.JW_ETA = new ZDateTime(2011, 10, 25, 19, 30, 0);

			AssertEquals("International Leg", "V1381", docConsol.VoyageNo);
			AssertEquals("DeparturePort", "NZAKL:Auckland", docConsol.DeparturePort);
			AssertEquals("ArrivalPort", "SGSIN:Singapore", docConsol.DischargePort);
			AssertEquals("DepartureDate", "25-Oct-11", docConsol.DepartureDate);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var consol = Factory.New<ForwardingConsol>();
			return new DocumentWrapper[] { DocForwardingConsol.New(Factory, consol.PK) };
		}

		ForwardingConsol Consol;
		Transport Transport;
		DocForwardingConsol docConsol;

		protected override void SetUp()
		{
			base.SetUp();
			Consol = Factory.New<ForwardingConsol>();
			Transport = Consol.Transports[0];
			Transport.JW_RL_NKLoadPort = "NZAKL";
			Transport.JW_RL_NKDiscPort = "KRPUS";
			docConsol = DocForwardingConsol.New(Consol, Factory);

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.NewZealand);
		}
	}
}
