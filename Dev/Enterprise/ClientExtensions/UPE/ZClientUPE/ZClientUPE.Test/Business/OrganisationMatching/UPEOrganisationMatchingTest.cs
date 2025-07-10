using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterData.Business.Tests;
using Enterprise.Registry.Business;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class UPEOrganisationMatchingTest : TestCaseWithFactory
	{
		public void TestMatchNoAccountNumber()
		{
			SetupDefaultMatchingOrganisationCandidate();
			Factory.Save();
			var matchedOrg = organisationMatching.TryMatchOrganisation(TestFullName, TestPortCode, TestCity, ZString.Empty, ZString.Empty, TestPostCode, TestState, TestAddress1, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertEquals(TestFullName, matchedOrg.OH_FullName);
			matchedOrg = organisationMatching.TryMatchOrganisation(TestFullName, TestPortCode, TestCity, ZString.Empty, ZString.Empty, TestPostCode, TestState, TestAddress1, ZString.Empty, ZString.Empty, "TEST");
			AssertEquals(TestFullName, matchedOrg.OH_FullName);
		}

		public void TestMatchWithNoAddress1IsNeverAMatch()
		{
			SetupDefaultMatchingOrganisationCandidate();
			Factory.Save();
			var matchedOrg = organisationMatching.TryMatchOrganisation(TestFullName, TestPortCode, TestCity, ZString.Empty, ZString.Empty, TestPostCode, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNull("No organisation should be matched due to a blank address1", matchedOrg);
		}

		public void TestMatchWithAccountNumber()
		{
			SetupDefaultMatchingOrganisationCandidate();
			SetupUANMatchingOrganisationCandidate();
			Factory.Save();
			var matchedOrg = organisationMatching.TryMatchOrganisation("UYEMURA INTERNATIONAL (SINGAPORE) PTE LTD", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, TestUAN);
			AssertEquals("UYEMURA INTERNATIONAL (SINGAPORE) PTE LTD", matchedOrg.OH_FullName);
			matchedOrg = organisationMatching.TryMatchOrganisation(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "2 JURONG EAST STREET 21", ZString.Empty, ZString.Empty, TestUAN);
			AssertEquals("UYEMURA INTERNATIONAL (SINGAPORE) PTE LTD", matchedOrg.OH_FullName);
			matchedOrg = organisationMatching.TryMatchOrganisation(ZString.Empty, ZString.Empty, "SINGAPORE", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, TestUAN);
			AssertEquals("UYEMURA INTERNATIONAL (SINGAPORE) PTE LTD", matchedOrg.OH_FullName);
			matchedOrg = organisationMatching.TryMatchOrganisation(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, "609601", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, TestUAN);
			AssertEquals("UYEMURA INTERNATIONAL (SINGAPORE) PTE LTD", matchedOrg.OH_FullName);
			matchedOrg = organisationMatching.TryMatchOrganisation(ZString.Empty, ZString.Empty, ZString.Empty, "+6562753398", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, TestUAN);
			AssertEquals("UYEMURA INTERNATIONAL (SINGAPORE) PTE LTD", matchedOrg.OH_FullName);
			matchedOrg = organisationMatching.TryMatchOrganisation(ZString.Empty, TestPortCode, ZString.Empty, ZString.Empty, "+6562753387", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, TestUAN);
			AssertEquals("UYEMURA INTERNATIONAL (SINGAPORE) PTE LTD", matchedOrg.OH_FullName);
		}

		public void TestEmptyUANNotUsedForMatching()
		{
			SetupDefaultMatchingOrganisationCandidate();
			SetupSimilarMatchingOrganisationCandidate();
			SetupUANMatchingOrganisationCandidate();
			Factory.Save();
			//ensure that account number matching does not happen
			var matchedOrg = organisationMatching.TryMatchOrganisation(TestFullName, TestPortCode, TestCity, ZString.Empty, ZString.Empty, TestPostCode, TestState, TestAddress1, ZString.Empty, TestCountry, ZString.Empty);
			AssertEquals(TestFullName, matchedOrg.OH_FullName);
		}

		public void TestOnlyUANNotUsedForMatching()
		{
			SetupDefaultMatchingOrganisationCandidate();
			SetupSimilarMatchingOrganisationCandidate();
			SetupUANMatchingOrganisationCandidate();
			Factory.Save();
			var matchedOrg = organisationMatching.TryMatchOrganisation(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, TestUAN);
			AssertNull(matchedOrg);
		}

		public void TestOrgIsReturnedWhenNoMatchesAndCreateFlagTrue()
		{
			var matchedOrg = organisationMatching.TryMatchOrganisation("TEMP ORG", TestPortCode, TestCity, ZString.Empty, ZString.Empty, TestPostCode, ZString.Empty, TestAddress1, ZString.Empty, Core.Constants.CountryCodes.Andorra, ZString.Empty);
			AssertEquals("TEMP ORG", matchedOrg.OH_FullName);
			AssertEquals(TestPortCode, matchedOrg.MainAddress.OA_RL_NKRelatedPortCode);
			AssertEquals(Core.Constants.CountryCodes.Andorra, matchedOrg.MainAddress.OA_RN_NKCountryCode);
		}

		public void TestNullIsReturnedWhenNoMatchesAndCreateFlagFalse()
		{
			organisationMatching = new UPEOrganisationMatching(Factory, false);
			var matchedOrg = organisationMatching.TryMatchOrganisation("TEMP ORG", TestPortCode, TestCity, ZString.Empty, ZString.Empty, TestPostCode, ZString.Empty, TestAddress1, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNull(matchedOrg);
		}

		public void TestOrgNotCreatedWhenFullNameIsEmpty()
		{
			var matchedOrg = organisationMatching.TryMatchOrganisation(ZString.Empty, TestPortCode, TestCity, ZString.Empty, ZString.Empty, TestPostCode, ZString.Empty, TestAddress1, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNull(matchedOrg);
		}

		public void TestOrgNotCreatedWhenClosestPortCodeIsEmpty()
		{
			var matchedOrg = organisationMatching.TryMatchOrganisation(TestFullName, ZString.Empty, TestCity, ZString.Empty, ZString.Empty, TestPostCode, ZString.Empty, TestAddress1, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNull(matchedOrg);
		}

		public void TestOrgNotCreatedWhenAddress1IsEmpty()
		{
			var matchedOrg = organisationMatching.TryMatchOrganisation(TestFullName, TestPortCode, TestCity, ZString.Empty, ZString.Empty, TestPostCode, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			AssertNull(matchedOrg);
		}

		public void TestOrgNotCreatedWhenLowOrgIsFound()
		{
			SetupDefaultMatchingOrganisationCandidate();
			SetupSimilarMatchingOrganisationCandidate();
			SetupUANMatchingOrganisationCandidate();
			Factory.Save();
			var matchedOrg = organisationMatching.TryMatchOrganisation("SIME DARBY", TestPortCode, "AUKLAND", ZString.Empty, ZString.Empty, "229965", "NSW", "55 ADIS RD", ZString.Empty, Core.Constants.CountryCodes.Singapore, ZString.Empty);
			AssertNull(matchedOrg);
		}

		public void TestDictionaryReturnsCorrectOrg()
		{
			var org = SetupDefaultMatchingOrganisationCandidate();
			org.MainAddress.OA_Phone = "0889322740";
			org.MainAddress.OA_Fax = "0889322741";
			org.MainAddress.OA_State = "NT";
			org.MainAddress.OA_Address2 = "ADDRESS2";
			org.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var key = string.Join("|", TestFullName, TestPortCode, TestCity, "0889322740", "0889322741", TestPostCode, "NT", TestAddress1, "ADDRESS2", Core.Constants.CountryCodes.Australia, ZString.Empty);
			using (organisationMatching.TemporarilySetOrganisationForTest(key, org))
			{
				var matchedOrg = organisationMatching.TryMatchOrganisation(TestFullName, TestPortCode, TestCity, "0889322740", "0889322741", TestPostCode, "NT", TestAddress1, "ADDRESS2", Core.Constants.CountryCodes.Australia, ZString.Empty);
				AssertEquals(org, matchedOrg);
			}
		}

		public void TestNewMatchEngineDoesNotMatchWhatTheLegacyMatchingOrgDid()
		{
			using (OrganisationsDataRegistry.Instance.OrgMatchThreshold.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, OrgMatchThresholds.Codes.High))
			{
				var orgHeader = Factory.New<UPEOrgHeader>();
				orgHeader.OH_Code = "ASHINSSIN";
				orgHeader.OH_FullName = "ASHCROFT INSTRUMENTS SINGAPORE PTE LTD";
				orgHeader.OH_RL_NKClosestPort = "SGSIN";
				var mainAddress = orgHeader.MainAddress;
				mainAddress.OA_Code = "BLK 1004 TOA PAYOH NORTH";
				mainAddress.OA_Address1 = "BLK 1004 TOA PAYOH NORTH #07-15/17";
				mainAddress.OA_Address2 = "#07-15/17";
				mainAddress.OA_City = "SINGAPORE";
				mainAddress.OA_PostCode = "318995";
				mainAddress.OA_Phone = "+6562526602";
				mainAddress.OA_Fax = "+6562526602";
				mainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
				Factory.Save();
				var matchedOrg = organisationMatching.TryMatchOrganisation("ACECRAFT ASIA PTE LTD", ZString.Empty, "SINGAPORE", ZString.Empty, ZString.Empty, "318995", ZString.Empty, "BLK 1004 TOA PAYOH NORTH", "#04-05/06/07", Core.Constants.CountryCodes.Singapore, ZString.Empty);
				AssertNull(matchedOrg);
			}
		}

		UPEOrgHeader SetupDefaultMatchingOrganisationCandidate()
		{
			var orgHeader = Factory.New<UPEOrgHeader>();
			orgHeader.OH_FullName = TestFullName;
			orgHeader.OH_RL_NKClosestPort = TestPortCode;
			orgHeader.MainAddress.OA_Address1 = TestAddress1;
			orgHeader.MainAddress.OA_City = TestCity;
			orgHeader.MainAddress.OA_State = TestState;
			orgHeader.MainAddress.OA_RN_NKCountryCode = TestCountry;
			orgHeader.MainAddress.OA_PostCode = TestPostCode;
			orgHeader.CreatePatternMatchingAddressFromMainAddress(Factory);
			orgHeader.CreatePatternMatchingName(Factory);
			return orgHeader;
		}

		UPEOrgHeader SetupSimilarMatchingOrganisationCandidate()
		{
			var orgHeader = Factory.New<UPEOrgHeader>();
			orgHeader.OH_FullName = "SIME DARBY SINGAPORE";
			orgHeader.OH_RL_NKClosestPort = TestPortCode;
			orgHeader.MainAddress.OA_Address1 = "32 MUJA AVENUE";
			orgHeader.MainAddress.OA_City = "JURONG EAST";
			orgHeader.MainAddress.OA_State = TestState;
			orgHeader.MainAddress.OA_RN_NKCountryCode = TestCountry;
			orgHeader.MainAddress.OA_PostCode = TestPostCode;
			orgHeader.CreatePatternMatchingAddressFromMainAddress(Factory);
			orgHeader.CreatePatternMatchingName(Factory);
			return orgHeader;
		}

		UPEOrgHeader SetupUANMatchingOrganisationCandidate()
		{
			var result = Factory.New<UPEOrgHeader>();
			result.AccountNumber = TestUAN;
			result.OH_FullName = "UYEMURA INTERNATIONAL (SINGAPORE) PTE LTD";
			result.OH_RL_NKClosestPort = "SGSIN";
			result.MainAddress.OA_Address1 = "2 JURONG EAST STREET 21";
			result.MainAddress.OA_Address2 = "IMM BUILDING";
			result.MainAddress.OA_City = "SINGAPORE";
			result.MainAddress.OA_PostCode = "609601";
			result.MainAddress.OA_Phone = "+6562753398";
			result.MainAddress.OA_Fax = "+6562753387";
			return result;
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			organisationMatching = new UPEOrganisationMatching(Factory, true);
		}

		UPEOrganisationMatching organisationMatching;
		const string TestFullName = "SIME DARBY SINGAPORE PTE LTD";
		const string TestPortCode = "SGSIN";
		const string TestAddress1 = "32 ADIS ROAD";
		const string TestCity = "ROCHOR";
		const string TestState = "SINGAPORE";
		const string TestCountry = Core.Constants.CountryCodes.Singapore;
		const string TestPostCode = "229978";
		const string TestUAN = "ACCT_NO";
	}
}
