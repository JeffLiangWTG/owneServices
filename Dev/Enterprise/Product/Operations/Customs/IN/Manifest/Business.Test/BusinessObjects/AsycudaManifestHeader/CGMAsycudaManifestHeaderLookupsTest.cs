using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.IN.Business.Testing;
using Enterprise.Customs.ManifestBase.Extensions;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMAsycudaManifestHeaderLookups))]
sealed class CGMAsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCustomsOffices()
	{
		var header = Factory.New<CGMAsycudaManifestHeader>();

		ReferenceDataTestHelper.AssertCustomsOfficeCollection(
			() =>
			{
				header.AMA_TransportMode = RefTransportModeList.Codes.SEA;
				return header.Lookups.CustomsOffices as ZZRefCusCodeListCombinedCollection;
			},
			() =>
			{
				header.AMA_TransportMode = RefTransportModeList.Codes.AIR;
				return header.Lookups.CustomsOffices as ZZRefCusCodeListCombinedCollection;
			});
	}

	public void TestCustomsDischargePortList()
	{
		var today = ZDateTime.Now;
		var yesterday = today.AddDays(-1);
		var tomorrow = today.AddDays(1);
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusCodeType(RefCusCodeListTypes.Codes.CustomsOffice, RefCusCodeListTypes.Codes.CustomsOffice);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.India, RefCusCodeListTypes.Codes.CustomsOffice, "INABC", yesterday, tomorrow);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.India, RefCusCodeListTypes.Codes.CustomsOffice, "INDEF", yesterday, tomorrow);
		helper.CreateCusCodeList(Core.Constants.CountryCodes.India, RefCusCodeListTypes.Codes.CustomsOffice, "INFGI", yesterday, tomorrow);

		Factory.Save();
		var expectedValues = new[] { "INABC", "INDEF", "INFGI" };
		var header = Factory.New<CGMAsycudaManifestHeader>();
		header.AMA_TransportMode = TransportTypeList.Codes.Sea;
		var list = header.Lookups.CustomsDischargePortList as BusinessObjectCollection;
		list.Load();
		AssertContainsExactElementsInAnyOrder(expectedValues, list.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
	}

	public void TestGetOriginPortListCore()
	{
		(var india, var nonIndia) = GetRefUNLOCOs();
		var header = Factory.NewWithValidTestData<CGMAsycudaManifestHeader>();
		var containsIndia = header.Lookups.OriginPortList.Contains(india);
		var containsNonIndia = header.Lookups.OriginPortList.Contains(nonIndia);
		CombineAssertions(() =>
		{
			AssertEquals("OriginPortList: Contains India", expected: false, containsIndia);
			AssertEquals("OriginPortList: Contains other than India", expected: true, containsNonIndia);
		});
	}

	public void TestGetDestinationPortListCore()
	{
		(var india, var nonIndia) = GetRefUNLOCOs();
		var header = Factory.NewWithValidTestData<CGMAsycudaManifestHeader>();
		var containsIndia = header.Lookups.DestinationPortList.Contains(india);
		var containsNonIndia = header.Lookups.DestinationPortList.Contains(nonIndia);
		CombineAssertions(() =>
		{
			AssertEquals("DestinationPortList: Does not contain India", expected: true, containsIndia);
			AssertEquals("DestinationPortList: Contains other than India", expected: false, containsNonIndia);
		});
	}

	(RefUNLOCO india, RefUNLOCO nonIndia) GetRefUNLOCOs()
	{
		var india = Factory.New<RefUNLOCO>();
		india.RL_RN_NKCountryCode = Core.Constants.CountryCodes.India;
		india.RL_Code = "INZZ1";
		var notIndia = Factory.New<RefUNLOCO>();
		notIndia.RL_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;
		notIndia.RL_Code = "ESZZ2";
		Factory.Save();
		return (india, notIndia);
	}

	public void TestWeightUQList()
	{
		AssertSame(Factory.GetWeightUQList(), Factory.New<CGMAsycudaManifestHeader>().Lookups.WeightUQList);
	}

	public void TestManifestUQList()
	{
		AssertSame(Factory.GetPackageTypeList(), Factory.New<CGMAsycudaManifestHeader>().Lookups.ManifestUQList);
	}

	public void TestActionList()
	{
		var header = Factory.New<CGMAsycudaManifestHeader>();
		CombineAssertions(() =>
		{
			AssertEquals("Master Bill", "A, D, F", header.Lookups.ActionList.CodesAsString);

			header.RegistrationStatus = RegistrationStatusList.Codes.ManifestRegistered;
			AssertEquals("Master Bill", "A, D", header.Lookups.ActionList.CodesAsString);
		});
	}

	public void TestMessageStatusList()
	{
		AssertSame(Factory.GetCachedValue<MessageStatusList>(), Factory.New<CGMAsycudaManifestHeader>().Lookups.MessageStatusList);
	}

	public void TestTransportModeList()
	{
		var header = Factory.New<CGMAsycudaManifestHeader>();
		var list = header.Lookups.TransportModeList;
		AssertEquals("DefaultCode", Core.Constants.TransportModes.Air, list.DefaultCode);
		AssertEquals("CodesAsString", Core.Constants.TransportModes.Air, list.CodesAsString);
	}

	public void TestRegistrationStatusList()
	{
		var lookups = Factory.New<CGMAsycudaManifestHeader>().Lookups;

		CombineAssertions(() =>
		{
			AssertEquals("CAN, DEL, AMD, REG", lookups.RegistrationStatusList.CodesAsString);
			AssertSame(Factory.GetCachedValue<RegistrationStatusList>(), lookups.RegistrationStatusList);
		});
	}
}
