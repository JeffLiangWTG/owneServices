using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.AIS.UCC5;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class Authorisation8FProviderTest : DataProviderTestCase<Authorisation8FProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("EntryInstruction missing", () => new Authorisation8FProvider(null));
		}

		public void TestParties()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Ireland;
			cusCode.OK_CustomsRegNo = "EU1234567890";

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode2 = orgHeader2.CustomsCodes.AddNew();
			cusCode2.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.DefermentApprovalNumber;
			cusCode2.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Ireland;
			cusCode2.OK_CustomsRegNo = "EU1234567891";
			Factory.Save();

			SetUpTestData();
			var orgAddress = orgHeader.Addresses.Cast<OrgAddress>().First();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address1-1";
			orgAddress.OA_Address2 = "Address2-1";
			orgAddress.OA_PostCode = "PostCode1";
			orgAddress.OA_City = "City1";
			orgAddress.OA_RN_NKCountryCode = "IE";
			orgAddress.OA_CompanyNameOverride = "TestCompany1";
			entryInstruction.CEI_OH_Owner = orgHeader.PK;

			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.OA_Address1 = "Address1-2";
			orgAddress2.OA_Address2 = "Address2-2";
			orgAddress2.OA_PostCode = "PostCode2";
			orgAddress2.OA_City = "City2";
			orgAddress2.OA_RN_NKCountryCode = "IE";
			orgAddress2.OA_CompanyNameOverride = "TestCompany2";
			orgAddress2.OA_OH = orgHeader2.PK;
			var owner = entryInstruction.OwnerOfGoodsCollection.AddNew();
			owner.E2_OA_Address = orgAddress2.PK;

			CombineAssertions(() =>
			{
				var parties = Provider.Parties;
				AssertType<AddressWithNameProvider>("Type", parties.FirstOrDefault());
				AssertEquals("Count", 2, parties.Count);
				var party1 = parties.First();
				AssertEquals("StreetAndNumber", "Address1-1, Address2-1", party1.StreetAndNumber);
				AssertEquals("Postcode", "PostCode1", party1.Postcode);
				AssertEquals("City", "City1", party1.City);
				AssertEquals("Country", "IE", party1.Country);
				AssertEquals("Name", "TestCompany1", party1.Name);
				var party2 = parties.Last();
				AssertEquals("StreetAndNumber", "Address1-2, Address2-2", party2.StreetAndNumber);
				AssertEquals("Postcode", "PostCode2", party2.Postcode);
				AssertEquals("City", "City2", party2.City);
				AssertEquals("Country", "IE", party2.Country);
				AssertEquals("Name", "TestCompany2", party2.Name);
			});
		}

		public void TestDateTimesPeriodsAndPlaces()
		{
			SetUpTestData();

			entryInstruction.ZG_PeriodForDischarge = 1;
			entryInstruction.ZG_PeriodForDischargeAutoExtension = true;
			entryInstruction.PeriodForDischargeDetails = "12";
			entryInstruction.ZG_BillOfDischargeIsNecessary = true;
			entryInstruction.ZG_BillOfDischargeDeadline = 1;
			entryInstruction.BillOfDischargeDetails = "23";

			CombineAssertions(() =>
			{
				var dateTimesPeriodsAndPlaces = Provider.DateTimesPeriodsAndPlaces;
				AssertType<DateTimesPeriodsAndPlacesProvider>("Type", dateTimesPeriodsAndPlaces);
				AssertEquals("Period", "1", dateTimesPeriodsAndPlaces.PeriodForDischarge.Period);
				AssertEquals("AutomaticExtension", true, dateTimesPeriodsAndPlaces.PeriodForDischarge.AutomaticExtension);
				AssertEquals("Details", "12", dateTimesPeriodsAndPlaces.PeriodForDischarge.Details);
				AssertEquals("Period", true, dateTimesPeriodsAndPlaces.BillOfDischarge.UseOfTheBillOfDischarge);
				AssertEquals("Period", "1", dateTimesPeriodsAndPlaces.BillOfDischarge.Deadline);
				AssertEquals("Period", "23", dateTimesPeriodsAndPlaces.BillOfDischarge.Details);
			});
		}

		public void TestIdentificationOfGoods()
		{
			AssertType<IdentificationOfGoodsProvider>(Provider.IdentificationOfGoods);
		}

		public void TestEconomicConditions()
		{
			AssertType<EconomicConditionsProvider>(Provider.EconomicConditions);
		}

		public void TestDetailsOfPlannedActivities()
		{
			AssertEquals("DetailsOfPlannedActivities", "IE123 Sample", Provider.DetailsOfPlannedActivities);
		}

		public void TestOthers()
		{
			AssertType<OthersProvider>(Provider.Others);
		}

		protected override Authorisation8FProvider GetProvider()
		{
			SetUpTestData();
			return new Authorisation8FProvider(entryHeader);
		}

		void SetUpTestData()
		{
			if (entryHeader == null)
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

				entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = "H1";
				entryInstruction.DetailsOfPlannedActivities = "IE123 Sample";

				entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			}
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
	}
}
