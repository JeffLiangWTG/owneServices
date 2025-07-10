using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using OrgAddress = Enterprise.MasterFiles.Business.OrgAddress;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	public class PartyProviderTest : DataProviderTestCase<PartyProvider>
	{
		public void TestNewOrNull()
		{
			AssertNull(GenerateProvider(null));
			AssertNotNull(GenerateProvider(orgAddress));
		}

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => _ = new PartyProviderForTest(null));
		}

		public void TestName()
		{
			AssertEquals("Name", "Name", Provider.Name);

			orgAddress.Header.OH_FullName = string.Empty;

			AssertNull("Name", Provider.Name);
		}

		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber", "IEIdentificationNumber", Provider.IdentificationNumber);

			eoriNumber.OK_CustomsRegNo = string.Empty;

			AssertNull("IdentificationNumber", Provider.IdentificationNumber);
		}

		public void TestAddress()
		{
			AssertNotNull("Address", Provider.Address);
		}

		public void TestCommunications()
		{
			Assert("Communication", Provider.Communications.Count > 1);
		}

		public void TestStatus()
		{
			AssertStatus();
		}

		protected virtual void AssertStatus()
		{
			AssertNull("Status", Provider.Status);
		}

		public void TestTypeOfPerson()
		{
			AssertNull("TypeOfPerson", Provider.TypeOfPerson);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Name";

			orgAddress = orgHeader.Addresses.AddNew();
			orgAddress.OA_Email = "1234@test.org";
			orgAddress.OA_RN_NKCountryCode = "IE";

			eoriNumber = orgAddress.CustomsCodes.AddNew();
			eoriNumber.OK_RN_NKCodeCountry = "IE";
			eoriNumber.OK_CodeType = "EOR";
			eoriNumber.OK_CustomsRegNo = "IdentificationNumber";

			var eoriNumber2 = orgAddress.CustomsCodes.AddNew();
			eoriNumber2.OK_RN_NKCodeCountry = "GB";
			eoriNumber2.OK_CodeType = "EOR";
			eoriNumber2.OK_CustomsRegNo = "IdentificationNumber2";
		}
		protected OrgAddress orgAddress;
		protected OrgCusCode eoriNumber;

		PartyProvider GenerateProvider(OrgAddress orgAddress) => PartyProvider.NewOrNull(orgAddress);

		protected override PartyProvider GetProvider()
		{
			return GenerateProvider(orgAddress);
		}

		class PartyProviderForTest(OrgAddress address) : PartyProvider(address);
	}
}
