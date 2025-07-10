using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing;

sealed class TemporaryStorageHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestMessageTypeList()
	{
		var lookups = new TemporaryStorageHeaderLookups(Factory.NewWithValidTestData<TemporaryStorageHeader>());
		var messageTypeList = lookups.MessageTypeList;
		CombineAssertions(() =>
		{
			AssertType<G5MessageTypeCodeList>(messageTypeList);
			AssertSequencesEqual(new[] { "G5X", "G5P", "LAM", "TSM" }, messageTypeList.GetAllCodes());
			AssertEquals("Description from G5X", "G5V1 Expedition", messageTypeList.GetDescriptionFromCode("G5X"));
			AssertEquals("Description from G5P", "G5V1 Reception", messageTypeList.GetDescriptionFromCode("G5P"));
			AssertEquals("Description from LAM", "LAME Manual Entry", messageTypeList.GetDescriptionFromCode("LAM"));
			AssertEquals("Description from TSM", "Temporary Storage Manual Entry", messageTypeList.GetDescriptionFromCode("TSM"));
		});
	}

	public void TestCertificateNames()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "AH";
		staff.GS_LoginName = "ahtest";
		var wrapper = GlbStaffWrapper.Get(staff);
		var cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert1";
		cert.GP_MailBoxID = "Test";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert2";
		cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
		cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;

		cert = wrapper.ESBPasswordCollection.AddNew();
		cert.GP_Name = "TestCert3";
		cert.GP_PasswordStatus = PasswordStatusList.Codes.Deactivated;

		var staff2 = Factory.New<GlbStaff>();
		staff2.GS_Code = "AZ";
		staff2.GS_LoginName = "aztest";

		Factory.Save();

		var header = Factory.New<TemporaryStorageHeader>();

		CombineAssertions(() =>
		{
			header.AMA_GS_NKCustomsAgent = ZString.Empty;
			var list = header.Lookups.CertificateNames;
			AssertEquals("List has no values when broker is not declared", 0, list.Count);

			header.AMA_GS_NKCustomsAgent = staff.GS_Code;
			list = header.Lookups.CertificateNames;
			AssertEquals("CertificateNames has the correct list for the declared broker", "TESTCERT1, TESTCERT2", list.CodesAsString);
			AssertSame("Cached value", list, header.Lookups.CertificateNames);

			header.AMA_GS_NKCustomsAgent = staff2.GS_Code;
			list = header.Lookups.CertificateNames;
			AssertEquals("List has no values when broker declared has no certificates associated", 0, list.Count);
		});
	}

	public void TestCustomsOfficeList()
	{
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		header.AMA_RN_NKCountry = "ES";
		var lookups = new TemporaryStorageHeaderLookups(header);
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var enuZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: enuZZZ);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		var cusCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ESOEE001", "Spain", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode1.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDeparture);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: enuZZZ);
		var cusCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FROEE002", "France", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode2.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDeparture);
		var cusCode3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FROEE003", "France", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode3.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDestination);
		var cusCode4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FROEE004", "France", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode4.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDelivery);
		Factory.Save();

		var list = lookups.CustomsOfficeCodeList;
		list.Load();
		CombineAssertions(() =>
		{
			AssertType<EUCustomsOfficeCodeCollection>(list);
			AssertContainsExactElementsInAnyOrder("CustomsOfficeList should only return offices of role DEP", new[] { "ESOEE001", "FROEE002" }, list.Select(x => x.ZZD_Code));
		});
	}

	public void TestDestinationCustomsOfficeList()
	{
		var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
		header.AMA_RN_NKCountry = "ES";
		var lookups = new TemporaryStorageHeaderLookups(header);
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var enuZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: enuZZZ);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		var cusCode1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ESOEE001", "Spain", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode1.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDestination);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: enuZZZ);
		var cusCode2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FROEE002", "France", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode2.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDestination);
		var cusCode3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FROEE003", "France", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode3.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDeparture);
		var cusCode4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FROEE004", "France", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(cusCode4.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EU.Business.EuOfficeCodesTypes.Codes.OfficeOfDelivery);
		Factory.Save();

		var list = lookups.DestinationCustomsOfficeCodeList;
		list.Load();
		CombineAssertions(() =>
		{
			AssertType<EUCustomsOfficeCodeCollection>(list);
			AssertContainsExactElementsInAnyOrder("CustomsOfficeList should only return offices of role DES", new[] { "ESOEE001", "FROEE002" }, list.Select(x => x.ZZD_Code));
		});
	}

	public void TestTransportTypeList()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var lookups = new TemporaryStorageHeaderLookups(header);
		var transportTypeList = lookups.TransportTypeList;

		CombineAssertions(() =>
		{
			AssertEquals("9 transport types for ES", 9, transportTypeList.Count);
			AssertSequencesEqual(new[] { "10", "11", "20", "30", "31", "40", "41", "80", "81" }, transportTypeList.GetAllCodes());

			header.AMA_TransportMode = Customs.Business.TransportTypeList.Codes.Air;
			var airTypes = lookups.TransportTypeList;
			AssertEquals("2 transport types for Air", 2, airTypes.Count);
			AssertSequencesEqual(new[] { "40", "41" }, airTypes.GetAllCodes());
			AssertEquals("Description from 40", "IATA flight number", airTypes.GetDescriptionFromCode("40"));
			AssertEquals("Description from 41", "Registration number of the aircraft", airTypes.GetDescriptionFromCode("41"));

			header.AMA_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
			var seaTypes = lookups.TransportTypeList;
			AssertEquals("2 transport types for Sea", 2, seaTypes.Count);
			AssertSequencesEqual(new[] { "10", "11" }, seaTypes.GetAllCodes());
			AssertEquals("Description from 10", "IMO ship identification number", seaTypes.GetDescriptionFromCode("10"));
			AssertEquals("Description from 11", "Name of the sea-going vessel", seaTypes.GetDescriptionFromCode("11"));

			header.AMA_TransportMode = Customs.Business.TransportTypeList.Codes.Rail;
			var railTypes = lookups.TransportTypeList;
			AssertEquals("1 transport type for Rail", 1, railTypes.Count);
			AssertSequencesEqual(new[] { "20" }, railTypes.GetAllCodes());
			AssertEquals("Description from 20", "Wagon number", railTypes.GetDescriptionFromCode("20"));

			header.AMA_TransportMode = Customs.Business.TransportTypeList.Codes.Road;
			var roadTypes = lookups.TransportTypeList;
			AssertEquals("2 transport types for Road", 2, roadTypes.Count);
			AssertSequencesEqual(new[] { "30", "31" }, roadTypes.GetAllCodes());
			AssertEquals("Description from 30", "Registration number of the road vehicle", roadTypes.GetDescriptionFromCode("30"));
			AssertEquals("Description from 31", "Registration Number of the Road Trailer", roadTypes.GetDescriptionFromCode("31"));

			header.AMA_TransportMode = Customs.Business.TransportTypeList.Codes.InlandWaterwayTransport;
			var iwtTypes = lookups.TransportTypeList;
			AssertEquals("2 transport types for IWT", 2, iwtTypes.Count);
			AssertSequencesEqual(new[] { "80", "81" }, iwtTypes.GetAllCodes());
			AssertEquals("Description from 80", "European Vessel Identification Number (ENI code)", iwtTypes.GetDescriptionFromCode("80"));
			AssertEquals("Description from 81", "Name of the inland waterways vessel", iwtTypes.GetDescriptionFromCode("81"));
		});
	}

	public void TestCusTempStorageRegPremisesList()
	{
		var header = Factory.New<TemporaryStorageHeader>();
		var lookups = new TemporaryStorageHeaderLookups(header);
		var premises1 = Factory.New<CusTempStorageRegPremises>();
		premises1.SRP_Code = "premises1";
		premises1.SRP_Type = EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility;

		var premises2 = Factory.New<CusTempStorageRegPremises>();
		premises2.SRP_Code = "premises2";
		premises2.SRP_Type = EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility;

		var premises3 = Factory.New<CusTempStorageRegPremises>();
		premises3.SRP_Code = "premises3";
		premises3.SRP_Type = EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;

		var premises4 = Factory.New<CusTempStorageRegPremises>();
		premises4.SRP_Code = "premises4";
		premises4.SRP_Type = EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse;

		CombineAssertions(() =>
		{
			header.AMA_MessageType = G5MessageTypeCodeList.Codes.LameManualEntry;
			var list = lookups.CusTempStorageRegPremisesList;
			AssertEquals("Module filter is set to ExportStorageFacility", EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility, list.FilterBusinessObjectDefaults["Type:Property"].Value);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.TemporaryStorageManualEntry;
			list = lookups.CusTempStorageRegPremisesList;
			AssertEquals("Module filter is set to TemporaryStorageWarehouse", EFTA.TemporaryStorageRegister.Business.CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse, list.FilterBusinessObjectDefaults["Type:Property"].Value);

			header.AMA_MessageType = G5MessageTypeCodeList.Codes.G5v1Reception;
			list = lookups.CusTempStorageRegPremisesList;
			AssertEquals("all premises are returned", 4, list.Count);
		});
	}
}
