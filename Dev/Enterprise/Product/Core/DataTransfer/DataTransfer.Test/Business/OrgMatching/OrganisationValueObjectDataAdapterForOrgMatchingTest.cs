using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using OrganisationTypes = Enterprise.MasterFiles.Integration.OrganisationTypes;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class OrganisationValueObjectDataAdapterForOrgMatchingTest : TestCaseWithFactory
	{
		public void TestImportAddresses()
		{
			var notify = new NotificationBuffer();

			var org = new Organisation();
			var addressValueCollection = new Xsd.OrgAddressCollection();

			var mainAddressValue = addressValueCollection.AddNew(AddressCapabilityAddressType.MAIN);
			mainAddressValue.AddressLine1 = "Main";
			var otherExistingAddressValue = addressValueCollection.AddNew(AddressCapabilityAddressType.DLV);
			otherExistingAddressValue.AddressLine1 = "ExistingOther";
			var otherNewAddressValue = addressValueCollection.AddNew(AddressCapabilityAddressType.PAD);
			otherNewAddressValue.AddressLine1 = "NewOther";
			otherNewAddressValue.Sequence = 17;

			org.OrganisationDetails.Addresses = addressValueCollection;
			org.EDICode = "muhaha";
			org.OwnerCode = "muhaha";

			var orgHeader = new OrgHeaderForMatching(Factory);

			var adapter = new TempOrganisationValueObjectDataAdapterForOrgMatching(OrganisationTypes.None, new OrgAddressSorter(GetDocAddressCollectionForTest()));
			var context = new ValueObjectImportContext(Factory, notify);
			adapter.ImportFromValueObjectForMatching(orgHeader, org, context);
			AssertEquals(2, orgHeader.Addresses.Count);
			AssertEquals("Main", orgHeader.Addresses[0].OA_Address1);
			AssertEquals("NewOther", orgHeader.Addresses[1].OA_Address1);
		}

		public void TestDoNotImportUSDeprecatedSAN()
		{
			var notify = new NotificationBuffer();
			var context = new ValueObjectImportContext(Factory, notify);
			var regNumbers = new RegistrationNumberCollection();
			var san = regNumbers.AddNew();
			san.NumberType = Xsd.RegistrationNumberTypes.SAN;
			san.Number = "12-12345678";
			san.IsSpecified = true;
			san.CountryOfRegistration = Core.Constants.CountryCodes.UnitedStates;

			var org = new Organisation();
			org.OrganisationDetails.Name = "Something";
			org.OrganisationDetails.RegistrationNumbers = regNumbers;

			var orgHeader = new OrgHeaderForMatching(Factory);

			var adapter = new TempOrganisationValueObjectDataAdapterForOrgMatching(OrganisationTypes.None);
			adapter.ImportFromValueObjectForMatching(orgHeader, org, context);
			Assert(!orgHeader.CustomsCodes.Any(x => x.OK_CodeType == OrgCusCode.USACodeTypes.DeprecatedSpecialAddressNotification
				&& x.OK_RN_NKCodeCountry == Core.Constants.CountryCodes.UnitedStates));
			Assert(notify.HasWarnings);
		}

		static DocAddressCollection GetDocAddressCollectionForTest()
		{
			DocAddressCollection collection = new DocAddressCollection { IsSpecified = true };
			collection.Add(OrgAddressSorterTest.GetDocAddressForOrgAddressSorter(1, "muhaha"));
			collection.Add(OrgAddressSorterTest.GetDocAddressForOrgAddressSorter(55, "yahooo"));
			collection.Add(OrgAddressSorterTest.GetDocAddressForOrgAddressSorter(17, "muhaha"));
			return collection;
		}
	}
}
