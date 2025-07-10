using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.H7.Business.Testing
{
	sealed class AsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRepresentativeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var list = header.Lookups.RepresentativeList;
			AssertType<BrokerCollection>(list);
		}

		public void TestDeclarantList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var list = header.Lookups.DeclarantList;
			AssertType<OrganisationsFindBoxCollection>(list);
		}

		public void TestPresenterList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var list = header.Lookups.PresenterList;
			AssertType<OrganisationsFindBoxCollection>(list);
		}

		public void TestAgentTypeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var agentTypeCodes = header.Lookups.AgentTypeList.GetAllCodes();

			AssertContainsExactElementsInAnyOrder("Should contain expected agent types", new[] { "DIR", "IND" }, agentTypeCodes);
		}

		public void TestCustomsOfficeList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "customs office");
			var mockTypeCodes = new List<string> { "IEARK100", "IEATH200" };
			mockTypeCodes.ForEach(li => helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice,
				li,
				"desc",
				ZDateTime.MinSmallDateTimeValue,
				ZDateTime.MaxSmallDateTimeValue));
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var manifestHeader = Factory.New<AsycudaManifestHeader>();
				var collection = (ZZRefCusCodeListCombinedCollection)manifestHeader.Lookups.CustomsOffices;
				collection.Load();

				AssertEquals("customs office code list contains 2 items", 2, collection.Count);
				AssertContainsExactElementsInAnyOrder("items exist in custom office code list", new[] { "IEARK100", "IEATH200" }, collection.Select(c => c.ZZD_Code));
			}
		}
	}
}
