using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(AsycudaTransportMeansLookups))]
	sealed class AsycudaTransportMeansLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypesTransportOfIdentification()
		{
			var list = lookups.TypesTransportOfIdentification;
			CombineAssertions(() =>
			{
				AssertEquals("Values", "10, 20, 21, 30, 31, 41, 80", list.CodesAsString);
				AssertSame("Cached", list, lookups.TypesTransportOfIdentification);
			});
		}

		public void TestTypeOfIdentificationList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var europeanUnionCode = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

			var eunId = helper.CreateNewOrGetExistingDataGrouping(europeanUnionCode);

			var codeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2MT;
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, codeType, "150", "General cargo vessel Vessel designed to carry general cargo", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, codeType, "151", "Unit carrier Vessel designed to carry unit loads", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, codeType, "152", "Bulk carrier Vessel designed to carry bulk cargo", startDate, endDate);

			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var list = header.Lookups.MeansOfTransportTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("150, 151, 152", list.CodesAsString);
				AssertSame("Cached", list, header.Lookups.MeansOfTransportTypeList);
			});
		}

		public void TestCountryList()
		{
			var today = ZDateTime.Now;
			var yesterday = today.AddDays(-1);
			var tomorrow = today.AddDays(1);
			var dayAfterTomorrow = today.AddDays(2);
			var dayBeforeYesterday = today.AddDays(-2);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("NC008", "NC008");

			helper.CreateCusCodeList("EUN", "NC008", "IT", "Italy", yesterday, tomorrow);
			helper.CreateCusCodeList("DE", "NC008", "NL", "Germany.", yesterday, tomorrow);
			helper.CreateCusCodeList("EUN", "NC008", "AT", "France", tomorrow, dayAfterTomorrow);
			helper.CreateCusCodeList("EUN", "NC008", "GR", "Italy", dayBeforeYesterday, yesterday);
			Factory.Save();

			var countryList = Factory.NewWithValidTestData<AsycudaTransportMeans>();
			var list = countryList.Lookups.CountryList;
			list.Load();

			CombineAssertions(() =>
			{
				AssertEquals(1, list.Count);
				Assert(list.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "IT"));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var cusTransportMeans = bill.AsycudaTransportMeans.AddNew();

			lookups = cusTransportMeans.Lookups;
		}
		AsycudaTransportMeansLookups lookups;
	}
}
