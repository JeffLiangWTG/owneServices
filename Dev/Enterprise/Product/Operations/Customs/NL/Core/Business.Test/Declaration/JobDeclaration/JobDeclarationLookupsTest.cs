using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

sealed class JobDeclarationLookupsTest : EU.Business.Declaration.Testing.JobDeclarationLookupsTest<JobDeclarationLookups, JobDeclaration>
{
	public void TestMethodOfPaymentLookupValues()
	{
		var declaration = Factory.New<JobDeclaration>();
		CodeDescriptionPairList methodsOfPayment = declaration.Lookups.PaymentPartyList;
		Assert(methodsOfPayment.ContainsOnly("A", "B"));
	}

	public void TestCustomsOfficesList()
	{
		var dec = Factory.New<JobDeclaration>();
		dec.JE_MessageType = JobMessageTypeList.Codes.Export;
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		AssertType<CustomsOfficesList>(dec.Lookups.CustomsOfficesList);
		dec.JE_MessageType = JobMessageTypeList.Codes.Import;
		AssertType<CustomsOfficesList>(dec.Lookups.CustomsOfficesList);
		dec.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Interfaced;
		AssertType<CustomsOfficesImportInterfaceList>(dec.Lookups.CustomsOfficesList);
	}

	public void TestLocationTypeList()
	{
		var locationTypeList = lookups.LocationQualifierList;

		AssertContains("C, A, D, B", locationTypeList.CodesAsString);
		AssertType<LocationQualifierList>(locationTypeList);
	}

	public void TestControllingCustomers()
	{
		AssertType<OrganisationsFindBoxCollection>(lookups.ControllingCustomers);
	}

	public void TestTransportMeansList()
	{
		var jobDeclaration = Factory.New<JobDeclaration>();
		AssertTransportMeansList(jobDeclaration, TransportTypeList.Codes.Road, new[] { "30" });
		AssertTransportMeansList(jobDeclaration, TransportTypeList.Codes.Sea, new[] { "10", "11" });
		AssertTransportMeansList(jobDeclaration, TransportTypeList.Codes.Rail, new[] { "20", "21" });
		AssertTransportMeansList(jobDeclaration, TransportTypeList.Codes.InlandWaterwayTransport, new[] { "80", "81" });
		AssertTransportMeansList(jobDeclaration, TransportTypeList.Codes.Air, new[] { "40", "41" });
		AssertTransportMeansList(jobDeclaration, TransportTypeList.Codes.Mail, new[] { "10", "11", "20", "21", "30", "31", "40", "41", "80", "81" });
		AssertTransportMeansList(jobDeclaration, TransportTypeList.Codes.OwnPropulsion, new[] { "10", "11", "20", "21", "30", "31", "40", "41", "80", "81" });
		AssertTransportMeansList(jobDeclaration, TransportTypeList.Codes.FixedTransportInstallations, new[] { "10", "11", "20", "21", "30", "31", "40", "41", "80", "81" });
	}

	void AssertTransportMeansList(JobDeclaration declaration, string transportModeInland, string[] exceptList)
	{
		declaration.JE_TransportModeInland = transportModeInland;
		var lookups = declaration.Lookups.TransportMeansList;
		CombineAssertions(() =>
		{
			AssertSame("Cached", declaration.Lookups.TransportMeansList, lookups);
			AssertContainsExactElementsInExactOrder(exceptList, lookups.GetAllCodes());
		});
	}

	public void TestPhaseStatusList() => AssertType<CustomsEntryPhaseStatusList>(lookups.EntryPhaseStatusList);
}
