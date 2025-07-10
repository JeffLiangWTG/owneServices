using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CargoHelperTest : TestCaseWithFactory
	{
		public void TestGetMatchingOrgAddressUsingCodeOrAddress1_MatchedByCode() => CombineAssertions(() =>
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "XYZ01";
			var address1 = orgHeader.Addresses.AddNew();
			address1.OA_Code = "ADD01";
			var address2 = orgHeader.Addresses.AddNew();
			address2.OA_Code = "ADD02";

			var orgAddressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddressData.OrganizationCode = "XYZ01";
			orgAddressData.AddressShortCode = "ADD01";
			AssertEquals("Match by code ADD01", address1, orgAddressData.GetMatchingOrgAddressUsingCodeOrAddress1(Factory));

			orgAddressData.AddressShortCode = "ADD03";
			AssertNull("Match by code ADD03", orgAddressData.GetMatchingOrgAddressUsingCodeOrAddress1(Factory));
		});

		public void TestGetMatchingOrgAddressUsingCodeOrAddress1_MatchedByAddress1() => CombineAssertions(() =>
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "XYZ01";
			var address1 = orgHeader.Addresses.AddNew();
			address1.Address1 = "46 Delivery Street";
			var address2 = orgHeader.Addresses.AddNew();
			address2.Address1 = "96 Testing Street";

			var orgAddressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddressData.OrganizationCode = "XYZ01";
			orgAddressData.Address1 = "96 TESTING STREET";
			AssertEquals("Match by address '96 TESTING STREET'", address2, orgAddressData.GetMatchingOrgAddressUsingCodeOrAddress1(Factory));

			orgAddressData.Address1 = "96 TESTING ST";
			AssertNull("Match by address '96 TESTING ST'", orgAddressData.GetMatchingOrgAddressUsingCodeOrAddress1(Factory));
		});

		public void TestGetMatchingOrgAddressUsingCodeOrAddress1_OrgMainAddress()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "XYZ01";

			var orgAddressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddressData.OrganizationCode = "XYZ01";
			AssertEquals(orgHeader.MainAddress, orgAddressData.GetMatchingOrgAddressUsingCodeOrAddress1(Factory));
		}

		public void TestGetMatchingOrgAddressUsingCodeOrAddress1_NoMatchedOrg()
		{
			var orgAddressData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			orgAddressData.OrganizationCode = "~~~";
			AssertNull(orgAddressData.GetMatchingOrgAddressUsingCodeOrAddress1(Factory));
		}

		public void TestIsUnmatchedOrganisation()
		{
			OrgHeader org = null;
			Assert(!org.IsUnmatchedOrganisation());

			org = Factory.New<OrgHeader>();
			org.OH_Code = "Test 1";
			Assert(!org.IsUnmatchedOrganisation());

			org = OrgHeader.UnmatchOrg(Factory);
			Assert(org.IsUnmatchedOrganisation());
		}

		public void TestGetConsigneeBusinessNumber()
		{
			AssertEquals("", CargoHelper.GetConsigneeBusinessNumber(null));

			var consignee1 = Factory.New<OrgHeader>();
			consignee1.OH_Code = "Test 1";
			consignee1.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901");
			AssertEquals("12345678901", CargoHelper.GetConsigneeBusinessNumber(consignee1));

			var consignee2 = Factory.New<OrgHeader>();
			consignee2.OH_Code = "Test 2";
			consignee2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CreditAgencyCode, "123");
			AssertEquals("", CargoHelper.GetConsigneeBusinessNumber(consignee2));

			var consignee3 = Factory.New<OrgHeader>();
			consignee3.OH_Code = "Test 3";
			consignee3.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901");
			consignee3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CreditAgencyCode, "123");
			AssertEquals("12345678901/123", CargoHelper.GetConsigneeBusinessNumber(consignee3));

			var consignee4 = Factory.New<OrgHeader>();
			consignee4.OH_Code = "Test 3";
			consignee4.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901123456");
			consignee4.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CreditAgencyCode, "1234567");
			AssertEquals("12345678901/123", CargoHelper.GetConsigneeBusinessNumber(consignee4));

			var consignee5 = Factory.New<OrgHeader>();
			consignee5.OH_Code = "Test 1";
			consignee5.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901234567890");
			AssertEquals("12345678901/234", CargoHelper.GetConsigneeBusinessNumber(consignee5));

			var unmatchedOrg = OrgHeader.UnmatchOrg(Factory);
			unmatchedOrg.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901234567890");
			AssertEquals(ZString.Empty, CargoHelper.GetConsigneeBusinessNumber(unmatchedOrg));
		}

		public void TestGetBusinessNoForABNsWithSpaces()
		{
			var consignee1 = Factory.New<OrgHeader>();
			consignee1.OH_Code = "Test 1";
			consignee1.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12 123 123 123 / CAC");
			AssertEquals("ABN stored with spaces should be handled correctly", "12123123123/CAC", CargoHelper.GetConsigneeBusinessNumber(consignee1));

			var consignee2 = Factory.New<OrgHeader>();
			consignee2.OH_Code = "Test 1";
			consignee2.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, @"12 123 123 123\75Y");
			AssertEquals("ABN stored with spaces should be handled correctly", "12123123123/75Y", CargoHelper.GetConsigneeBusinessNumber(consignee2));

			var consignee3 = Factory.New<OrgHeader>();
			consignee3.OH_Code = "Test 1";
			consignee3.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12 123 123 123");
			AssertEquals("ABN stored with spaces should be handled correctly", "12123123123", CargoHelper.GetConsigneeBusinessNumber(consignee3));

			var unmatchedOrg = OrgHeader.UnmatchOrg(Factory);
			unmatchedOrg.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "1234 5678901234567890");
			AssertEquals(ZString.Empty, CargoHelper.GetConsigneeBusinessNumber(unmatchedOrg));
		}

		public void TestGetIdentifier_OrgHeader()
		{
			AssertEquals("", CargoHelper.GetIdentifier(null, null));

			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "Test 1";
			var cusCode = orgHeader1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "1234567890112345");
			AssertEquals("12345678901", CargoHelper.GetIdentifier(orgHeader1, null));
			cusCode.OK_OA_PremisesAddress = Factory.New<OrgAddress>().PK;
			AssertEquals(ZString.Empty, CargoHelper.GetIdentifier(orgHeader1, null));

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "Test 2";
			orgHeader2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "1234");
			AssertEquals("1234", CargoHelper.GetIdentifier(orgHeader2, null));

			var orgHeader3 = Factory.New<OrgHeader>();
			orgHeader3.OH_Code = "Test 3";
			orgHeader3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "12345678901");
			AssertEquals("12345678901", CargoHelper.GetIdentifier(orgHeader3, null));

			var unmatchedOrg = OrgHeader.UnmatchOrg(Factory);
			unmatchedOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "12345678901234567890");
			AssertEquals(ZString.Empty, CargoHelper.GetIdentifier(unmatchedOrg, null));
		}

		public void TestGetIdentifier_OrgAddress()
		{
			AssertEquals("", CargoHelper.GetIdentifier(null, null));

			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "Test 1";
			var cusCode = orgHeader1.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "1234567890112345");
			cusCode.OK_OA_PremisesAddress = orgHeader1.MainAddress.PK;
			AssertEquals("12345678901", CargoHelper.GetIdentifier(orgHeader1, orgHeader1.MainAddress));

			cusCode.OK_OA_PremisesAddress = Factory.New<OrgAddress>().PK;
			AssertEquals(ZString.Empty, CargoHelper.GetIdentifier(orgHeader1, orgHeader1.MainAddress));

			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "Test 2";
			orgHeader2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "1234");
			AssertEquals("1234", CargoHelper.GetIdentifier(orgHeader2, orgHeader2.MainAddress));

			var otherAddress2 = orgHeader2.Addresses.AddNew();
			var cusCode2 = orgHeader2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "22223333");
			cusCode2.OK_OA_PremisesAddress = otherAddress2.PK;
			AssertEquals("22223333", CargoHelper.GetIdentifier(orgHeader2, otherAddress2));

			var orgHeader3 = Factory.New<OrgHeader>();
			orgHeader3.OH_Code = "Test 3";
			orgHeader3.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "12345678901");
			AssertEquals("12345678901", CargoHelper.GetIdentifier(orgHeader3, orgHeader3.MainAddress));

			var otherAddress3 = orgHeader3.Addresses.AddNew();
			AssertEquals("12345678901", CargoHelper.GetIdentifier(orgHeader3, otherAddress3));

			var unmatchedOrg = OrgHeader.UnmatchOrg(Factory);
			unmatchedOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "12345678901234567890");
			AssertEquals(ZString.Empty, CargoHelper.GetIdentifier(unmatchedOrg, unmatchedOrg.MainAddress));
		}

		public void TestGetConsignorVendorFromARN()
		{
			AssertEquals("", CargoHelper.GetConsignorVendor(null));

			var consignor1 = Factory.New<OrgHeader>();
			consignor1.OH_Code = "Test 1";
			consignor1.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.ARN, "12345678901");
			AssertEquals("12345678901", CargoHelper.GetConsignorVendor(consignor1));

			var consignor2 = Factory.New<OrgHeader>();
			consignor2.OH_Code = "Test 2";
			consignor2.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.ARN, "123456789012");
			AssertEquals("123456789012", CargoHelper.GetConsignorVendor(consignor2));

			var consignor3 = Factory.New<OrgHeader>();
			consignor3.OH_Code = "Test 3";
			consignor3.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.ARN, "123456789012345");
			AssertEquals("123456789012", CargoHelper.GetConsignorVendor(consignor3));

			var unmatchedOrg = OrgHeader.UnmatchOrg(Factory);
			unmatchedOrg.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.ARN, "12345678901234567890");
			AssertEquals(ZString.Empty, CargoHelper.GetConsignorVendor(unmatchedOrg));
		}

		public void TestGetConsignorVendorFromABN()
		{
			var consignor1 = Factory.New<OrgHeader>();
			consignor1.OH_Code = "Test 1";
			consignor1.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.ARN, "123456789012");
			consignor1.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901234");
			AssertEquals("It should not return ABN if ARN is defined.", "123456789012", CargoHelper.GetConsignorVendor(consignor1));

			var consignor2 = Factory.New<OrgHeader>();
			consignor2.OH_Code = "Test 2";
			consignor2.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901234");
			AssertEquals("It should return ABN if ARN is not defined.", "12345678901234", CargoHelper.GetConsignorVendor(consignor2));

			var consignor3 = Factory.New<OrgHeader>();
			consignor3.OH_Code = "Test 3";
			consignor3.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "1234 A 5678 90 * 1234");
			AssertEquals("It should return ABN including the valid chars only.", "12345678901234", CargoHelper.GetConsignorVendor(consignor3));

			var consignor4 = Factory.New<OrgHeader>();
			consignor4.OH_Code = "Test 4";
			consignor4.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "1234567890123456");
			AssertEquals("It should return ABN with the max length of 14.", "12345678901234", CargoHelper.GetConsignorVendor(consignor4));

			var unmatchedOrg = OrgHeader.UnmatchOrg(Factory);
			unmatchedOrg.CustomsCodes.AddNew(OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber, "12345678901234567890");
			AssertEquals(ZString.Empty, CargoHelper.GetConsignorVendor(unmatchedOrg));
		}

		public void TestGetConsignorVendorId()
		{
			AssertEquals("", CargoHelper.GetConsignorVendorId(null));

			var address1 = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			address1.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
				{
					new RegistrationNumber
					{
						Type = new RegistrationNumberType()
						{
							Code = "ABN"
						},
						CountryOfIssue = new Country()
						{
							Code = "AU"
						},
						Value = "1234567890"
					}
			});

			AssertEquals("1234567890", CargoHelper.GetConsignorVendorId(address1));

			var address2 = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			address2.SetRegistrationNumberCollection(() => new List<RegistrationNumber>
				{
					new RegistrationNumber
					{
						Type = new RegistrationNumberType()
						{
							Code = "ARN"
						},
						CountryOfIssue = new Country()
						{
							Code = "AU"
						},
						Value = "123456789012"
					}
			});

			AssertEquals("123456789012", CargoHelper.GetConsignorVendorId(address2));
		}

		[TestDate(2012, 8, 1, 06, 15, 00)] // Wednesday
		public void TestCalculateScheduledMessagesSendTime()
		{
			RefUNLOCO cbrLoco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUCBR");
			DateTime GetLocationDateTime() => cbrLoco.TimeZoneSet.GetCalculationTimeZone().ToLocalTime(ZDateTime.UtcNow.ToDateTime()); // portCBR.LocationDateTime is cached so it will return the same time over and over
			AssertEquals("Pre-condition - CBR time calculated from the test date attribute", 16, GetLocationDateTime().Hour);
			AssertEquals("Pre-condition - lateTimeframe", 4, AUCustomsDataRegistry.Instance.AirMandatoryLatestCargoReportingTimeframe.Value);

			var scheduledTime = CargoHelper.CalculateScheduledMessagesSendTime(Factory, Core.Constants.TransportCodes.Air, "AUCBR", new ZDateTime(2012, 8, 1, 22, 30, 0));
			AssertEquals("Immediate when Scheduled is within the 4 hour late window", ZDateTime.Empty, scheduledTime);

			scheduledTime = CargoHelper.CalculateScheduledMessagesSendTime(Factory, Core.Constants.TransportCodes.Air, "AUCBR", new ZDateTime(2012, 8, 1, 23, 30, 0));
			AssertEquals("Scheduled at 7PM when Scheduled Time is outside the 4 hour late window and CBR Time is in the White Window (6AM-7PM)", new ZDateTime(2012, 8, 1, 19, 0, 0), scheduledTime);

			scheduledTime = CargoHelper.CalculateScheduledMessagesSendTime(Factory, Core.Constants.TransportCodes.Air, "AUCBR", new ZDateTime(2012, 8, 5, 23, 30, 0));
			AssertEquals("Scheduled at 48 hours before arrival when Scheduled Time falls outside the White Window (6AM-7PM)", new ZDateTime(2012, 8, 3, 23, 30, 0), scheduledTime);

			scheduledTime = CargoHelper.CalculateScheduledMessagesSendTime(Factory, Core.Constants.TransportCodes.Air, "AUCBR", new ZDateTime(2012, 8, 5, 8, 30, 0));
			AssertEquals("Scheduled at 7PM within 48 hours of arrival when Scheduled Time falls inside the White Window (6AM-7PM)", new ZDateTime(2012, 8, 3, 19, 0, 0), scheduledTime);

			AssertEquals("Pre-condition - lateTimeframe", 48, AUCustomsDataRegistry.Instance.SeaMandatoryLatestCargoReportingTimeframe.Value);

			scheduledTime = CargoHelper.CalculateScheduledMessagesSendTime(Factory, Core.Constants.TransportCodes.Sea, "AUCBR", new ZDateTime(2012, 8, 3, 18, 30, 0));
			AssertEquals("Immediate when Scheduled time is within the 48 hour late window", ZDateTime.Empty, scheduledTime);

			scheduledTime = CargoHelper.CalculateScheduledMessagesSendTime(Factory, Core.Constants.TransportCodes.Sea, "AUCBR", new ZDateTime(2012, 8, 3, 19, 30, 0));
			AssertEquals("Scheduled at 7PM when Scheduled Time is outside the 48 hour late window and CBR Time is in the White Window (6AM-7PM)", new ZDateTime(2012, 8, 1, 19, 0, 0), scheduledTime);

			scheduledTime = CargoHelper.CalculateScheduledMessagesSendTime(Factory, Core.Constants.TransportCodes.Sea, "AUCBR", new ZDateTime(2012, 8, 6, 23, 30, 0));
			AssertEquals("Scheduled at 7PM on the day of submission regardless of the arrival time", new ZDateTime(2012, 8, 1, 19, 0, 0), scheduledTime);
		}
	}
}
