using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromVoyageAccount))]
	sealed class FreightWrapperFromVoyageAccountTest : FreightWrapperTest
	{
		public void TestWrapperMappingsFull()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();

			VoyageOrigin origin1 = voyage.Origins.AddNew();
			origin1.JA_E_DEP = new ZDateTime(2011, 1, 1);
			origin1.JA_RL_NKPortOfLoading = "AUMEL";
			VoyageOrigin origin2 = voyage.Origins.AddNew();
			origin2.JA_E_DEP = new ZDateTime(2011, 2, 3);
			origin2.JA_RL_NKPortOfLoading = "MYBAG";
			VoyageOrigin origin3 = voyage.Origins.AddNew();
			origin3.JA_E_DEP = new ZDateTime(2011, 1, 16);
			origin3.JA_RL_NKPortOfLoading = "AUSYD";

			VoyageDestination destination1 = voyage.Destinations.AddNew();
			destination1.JB_E_ARV = new ZDateTime(2011, 2, 18);
			destination1.JB_RL_NKPortOfDischarge = "HKHKG";
			VoyageDestination destination2 = voyage.Destinations.AddNew();
			destination2.JB_E_ARV = new ZDateTime(2011, 3, 12);
			destination2.JB_RL_NKPortOfDischarge = "NLAMS";
			VoyageDestination destination3 = voyage.Destinations.AddNew();
			destination3.JB_E_ARV = new ZDateTime(2011, 1, 20);
			destination3.JB_RL_NKPortOfDischarge = "AUBNE";
			VoyageDestination destination4 = voyage.Destinations.AddNew();
			destination4.JB_E_ARV = new ZDateTime(2011, 1, 15);
			destination4.JB_RL_NKPortOfDischarge = "AUSYD";

			VoyageAccount voyageAccount = Factory.New<VoyageAccount>();
			voyageAccount.NA_JV = voyage.PK;
			FreightWrapperFromVoyageAccount wrapper = new FreightWrapperFromVoyageAccount(voyageAccount, Factory);

			AssertEquals("Origin", "AUMEL", wrapper.Origin.Location.UNLOCO);
			AssertEquals("Destination", "NLAMS", wrapper.Destination.Location.UNLOCO);
			AssertEquals("ShipmentRoutes.Count", 5, wrapper.ShipmentRoutes.Count);
			AssertEquals("ShipmentRoutes[0]", "AUMEL", wrapper.ShipmentRoutes[0].Origin.UNLOCO);
			AssertEquals("ShipmentRoutes[0]", "AUSYD", wrapper.ShipmentRoutes[0].Destination.UNLOCO);
			AssertEquals("ShipmentRoutes[1]", "AUSYD", wrapper.ShipmentRoutes[1].Origin.UNLOCO);
			AssertEquals("ShipmentRoutes[1]", "AUBNE", wrapper.ShipmentRoutes[1].Destination.UNLOCO);
			AssertEquals("ShipmentRoutes[2]", "AUBNE", wrapper.ShipmentRoutes[2].Origin.UNLOCO);
			AssertEquals("ShipmentRoutes[2]", "MYBAG", wrapper.ShipmentRoutes[2].Destination.UNLOCO);
			AssertEquals("ShipmentRoutes[3]", "MYBAG", wrapper.ShipmentRoutes[3].Origin.UNLOCO);
			AssertEquals("ShipmentRoutes[3]", "HKHKG", wrapper.ShipmentRoutes[3].Destination.UNLOCO);
			AssertEquals("ShipmentRoutes[4]", "HKHKG", wrapper.ShipmentRoutes[4].Origin.UNLOCO);
			AssertEquals("ShipmentRoutes[4]", "NLAMS", wrapper.ShipmentRoutes[4].Destination.UNLOCO);
		}

		public void TestPrincipal()
		{
			VoyageAccount voyageAccount = Factory.New<VoyageAccount>();
			FreightWrapperFromVoyageAccount wrapper = new FreightWrapperFromVoyageAccount(voyageAccount, Factory);

			AssertEquals("wrapper.Principal.CompanyNameAndAddress", ZString.Empty, wrapper.Principal.CompanyNameAndAddress);

			OrgHeader header = GetOrgHeader("AD GGG");
			OrgAddress principalAddress = header.MainAddress;
			principalAddress.OA_Address1 = "ADDRESS1";
			principalAddress.OA_City = "CITY";
			principalAddress.OA_State = "STATE";
			principalAddress.OA_PostCode = "PCODE";
			principalAddress.OA_RN_NKCountryCode = "AU";
			voyageAccount.NA_OH = header.PK;

			wrapper = new FreightWrapperFromVoyageAccount(voyageAccount, Factory);
			AssertEquals("wrapper.Principal.CompanyNameAndAddress", "AD GGG\nADDRESS1\nCITY STATE PCODE\nAUSTRALIA", wrapper.Principal.CompanyNameAndAddress);
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
Destination :  is null
Origin :  is null";
			}
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<VoyageAccount>();
		}

		protected override Base.GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			voyageAccount = (VoyageAccount)GetNewBusinessObjectToWrap();
			return new FreightWrapperFromVoyageAccount(voyageAccount, Factory);
		}

		VoyageAccount voyageAccount;

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}
	}
}
