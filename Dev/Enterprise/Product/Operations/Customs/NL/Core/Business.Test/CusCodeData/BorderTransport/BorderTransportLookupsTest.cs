using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

class BorderTransportLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestIdTypeCodeList_SEA()
	{
		declaration.JE_TransportModeInland = TransportTypeList.Codes.Sea;
		CombineAssertions(() =>
		{
			var idTypeCodeList = lookups.CY_CodeList;
			AssertEquals("Valid Codes", "10, 11", idTypeCodeList.CodesAsString);
			AssertSame("Cached", idTypeCodeList, lookups.CY_CodeList);
		});
	}

	public void TestIdTypeCodeList_FIX()
	{
		declaration.JE_TransportModeInland = TransportTypeList.Codes.FixedTransportInstallations;
		CombineAssertions(() =>
		{
			var idTypeCodeList = lookups.CY_CodeList;
			AssertEquals("Valid Codes", "10, 11, 20, 21, 30, 31, 40, 41, 80, 81", idTypeCodeList.CodesAsString);
			AssertSame("Cached", idTypeCodeList, lookups.CY_CodeList);
		});
	}

	public void TestIdTypeCodeList_OWN()
	{
		declaration.JE_TransportModeInland = TransportTypeList.Codes.OwnPropulsion;
		CombineAssertions(() =>
		{
			var idTypeCodeList = lookups.CY_CodeList;
			AssertEquals("Valid Codes", "10, 11, 20, 21, 30, 31, 40, 41, 80, 81", idTypeCodeList.CodesAsString);
			AssertSame("Cached", idTypeCodeList, lookups.CY_CodeList);
		});
	}
	public void TestIdTypeCodeList_MAI()
	{
		declaration.JE_TransportModeInland = TransportTypeList.Codes.Mail;
		CombineAssertions(() =>
		{
			var idTypeCodeList = lookups.CY_CodeList;
			AssertEquals("Valid Codes", "10, 11, 20, 21, 30, 31, 40, 41, 80, 81", idTypeCodeList.CodesAsString);
			AssertSame("Cached", idTypeCodeList, lookups.CY_CodeList);
		});
	}

	public void TestIdTypeCodeList_RAI()
	{
		declaration.JE_TransportModeInland = TransportTypeList.Codes.Rail;
		CombineAssertions(() =>
		{
			var idTypeCodeList = lookups.CY_CodeList;
			AssertEquals("Valid Codes", "20, 21", idTypeCodeList.CodesAsString);
			AssertSame("Cached", idTypeCodeList, lookups.CY_CodeList);
		});
	}

	public void TestIdTypeCodeList_ROA()
	{
		declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
		CombineAssertions(() =>
		{
			var idTypeCodeList = lookups.CY_CodeList;
			AssertEquals("Valid Codes", "30, 31", idTypeCodeList.CodesAsString);
			AssertSame("Cached", idTypeCodeList, lookups.CY_CodeList);
		});
	}
	public void TestIdTypeCodeList_AIR()
	{
		declaration.JE_TransportModeInland = TransportTypeList.Codes.Air;
		CombineAssertions(() =>
		{
			var idTypeCodeList = lookups.CY_CodeList;
			AssertEquals("Valid Codes", "40, 41", idTypeCodeList.CodesAsString);
			AssertSame("Cached", idTypeCodeList, lookups.CY_CodeList);
		});
	}

	public void TestIdTypeCodeList_IWT()
	{
		declaration.JE_TransportModeInland = TransportTypeList.Codes.InlandWaterwayTransport;
		CombineAssertions(() =>
		{
			var idTypeCodeList = lookups.CY_CodeList;
			AssertEquals("Valid Codes", "80, 81", idTypeCodeList.CodesAsString);
			AssertSame("Cached", idTypeCodeList, lookups.CY_CodeList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		var inlandTransport = declaration.BorderTransports.AddNew();

		lookups = inlandTransport.Lookups;
	}
	BorderTransportLookups lookups;
	JobDeclaration declaration;
}
