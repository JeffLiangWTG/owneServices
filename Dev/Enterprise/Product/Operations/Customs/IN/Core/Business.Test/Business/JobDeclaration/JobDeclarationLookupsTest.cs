using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(JobDeclarationLookups))]
sealed class JobDeclarationLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestOriginStateList()
	{
		var indiaStates = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.India).States.Select(x => x.RW_Code);
		CombineAssertions(() =>
		{
			Assert("Precondition: India States", indiaStates.Any());
			AssertContainsExactElementsInAnyOrder(indiaStates, Factory.New<JobDeclaration>().Lookups.OriginStateList.GetAllCodesZString());
		});
	}

	public void TestCargoIdTypeList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var cargoIdTypeList = declaration.Lookups.CargoIdTypeList;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInExactOrder("CargoIdTypeList codes", new[] { "CNT", "BBK", "BLK", "LQD", "CPC" }, cargoIdTypeList.GetAllCodes());
			AssertSame("CargoIdTypeList cached", cargoIdTypeList, declaration.Lookups.CargoIdTypeList);
		});
	}

	public void TestEPZCodeList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var epzCodeList = declaration.Lookups.EPZCodeList;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("EPZCodeList codes", new[] { "E", "Z" }, epzCodeList.GetAllCodes());
			AssertSame("EPZCodeList cached", epzCodeList, declaration.Lookups.EPZCodeList);
		});
	}

	public void TestSealByCodeList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var sealByCodeList = declaration.Lookups.SealByCodeList;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("SealByCodeList codes", new[] { "A", "S", "W" }, sealByCodeList.GetAllCodes());
			AssertSame("SealByCodeList cached", sealByCodeList, declaration.Lookups.SealByCodeList);
		});
	}

	public void TestVerifiedList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var verifiedList = declaration.Lookups.VerifiedList;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("SampleAccompanied codes", new[] { "Y", "N" }, verifiedList.GetAllCodes());
			AssertSame("SampleAccompanied cached", verifiedList, declaration.Lookups.VerifiedList);
		});
	}

	public void TestSampleForwardedList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var sampleForwardedList = declaration.Lookups.SampleForwardedList;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("SampleAccompanied codes", new[] { "Y", "N" }, sampleForwardedList.GetAllCodes());
			AssertSame("SampleAccompanied cached", sampleForwardedList, declaration.Lookups.SampleForwardedList);
		});
	}

	public void TestCustomsOfficeList()
	{
		var header = Factory.New<JobDeclaration>();

		ReferenceDataTestHelper.AssertCustomsOfficeCollection(
			() =>
			{
				header.JE_TransportMode = RefTransportModeList.Codes.SEA;
				return header.Lookups.CustomsOfficeList as ZZRefCusCodeListCombinedCollection;
			},
			() =>
			{
				header.JE_TransportMode = RefTransportModeList.Codes.AIR;
				return header.Lookups.CustomsOfficeList as ZZRefCusCodeListCombinedCollection;
			});
	}

	public void TestExporterTypeList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var exporterTypeList = declaration.Lookups.ExporterTypeList;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("DeclarantTypeList codes", new[] { "R", "F" }, exporterTypeList.GetAllCodes());
			AssertSame("DeclarantTypeList cached", exporterTypeList, declaration.Lookups.ExporterTypeList);
		});
	}

	public void TestStuffingAtCodeList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var stuffingAtList = declaration.Lookups.StuffingAtList;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("StuffingAt codes", new[] { "FAC", "CFS" }, stuffingAtList.GetAllCodes());
			AssertSame("StuffingAt cached", stuffingAtList, declaration.Lookups.StuffingAtList);
		});
	}

	public void TestSampleAccompaniedCodeList()
	{
		var declaration = Factory.New<JobDeclaration>();
		var sampleAccompaniedList = declaration.Lookups.SampleAccompaniedList;
		CombineAssertions(() =>
		{
			AssertContainsExactElementsInAnyOrder("SampleAccompanied codes", new[] { "Y", "N" }, sampleAccompaniedList.GetAllCodes());
			AssertSame("SampleAccompanied cached", sampleAccompaniedList, declaration.Lookups.SampleAccompaniedList);
		});
	}

	public void TestExportOrientedUnitsCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		var exportOrientedUnitsCollection = declaration.Lookups.ExportOrientedUnitsCollection;
		AssertType<OrganisationsFindBoxCollection>(exportOrientedUnitsCollection);
		var filters = exportOrientedUnitsCollection.FilterBusinessObjectDefaults;
		CombineAssertions(() =>
		{
			AssertEquals(2, filters.Count);
			AssertEquals(ZBool.True, filters["Organisation Types:Property3"].Value);
			AssertEquals(Core.Constants.CountryCodes.India, filters["Country/Region:Property"].Value);
		});
	}

	public void TestTranshipperCollection()
	{
		var declaration = Factory.New<JobDeclaration>();
		var transhipper = declaration.Lookups.TranshipperCollection;
		AssertType<OrganisationsFindBoxCollection>(transhipper);
		var filters = transhipper.FilterBusinessObjectDefaults;
		CombineAssertions(() =>
		{
			AssertEquals(3, filters.Count);
			AssertEquals(OrganisationSecondaryTypes.LocalTransport, filters["Secondary Type:Property"].Value);
			AssertEquals(ZBool.True, filters["Organisation Types:Property4"].Value);
			AssertEquals(Core.Constants.CountryCodes.India, filters["Country/Region:Property"].Value);
		});
	}
}
