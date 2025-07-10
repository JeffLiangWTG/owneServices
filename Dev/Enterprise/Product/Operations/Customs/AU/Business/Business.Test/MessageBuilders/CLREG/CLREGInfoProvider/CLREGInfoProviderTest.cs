using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CLREGInfoProvider))]
	sealed class CLREGInfoProviderTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<CLREGInfoProvider>
	{
		public void TestICLREGInfoProviderIsCorrectlySetup()
		{
			AssertEquals(typeof(CLREGInfoProvider), ObjectFactory.GetType<Integration.Customs.AU.ICLREGInfoProvider>());
		}

		public void TestSetDefaultValues()
		{
			var header = Factory.New<OrgHeader>();
			header.LocalBusinessRegNo = "1234567";

			var wrapper = new OrgHeaderWrapper(header);
			var dataProvider = wrapper.CLREGInfoProvider;
			AssertEquals("ABN", "1234567", dataProvider.ZA_ABN);
			AssertEquals("CAC", "", dataProvider.ZA_CAC);
			AssertEquals("CAC type", "", dataProvider.ZA_CACType);

			header = Factory.New<OrgHeader>();
			header.LocalBusinessRegNo = "12345675236";
			wrapper = new OrgHeaderWrapper(header);
			dataProvider = wrapper.CLREGInfoProvider;
			AssertEquals("ABN", "12345675236", dataProvider.ZA_ABN);
			AssertEquals("CAC", "", dataProvider.ZA_CAC);
			AssertEquals("CAC type", "", dataProvider.ZA_CACType);

			header = Factory.New<OrgHeader>();
			header.LocalBusinessRegNo = "12345675236111";
			wrapper = new OrgHeaderWrapper(header);
			dataProvider = wrapper.CLREGInfoProvider;
			AssertEquals("ABN", "12345675236", dataProvider.ZA_ABN);
			AssertEquals("CAC", "111", dataProvider.ZA_CAC);
			AssertEquals("CAC type", "", dataProvider.ZA_CACType);

			header = Factory.New<OrgHeader>();
			header.OH_FullName = "Test Business Name";
			header.LocalBusinessRegNo = "1234567523611122";
			wrapper = new OrgHeaderWrapper(header);
			dataProvider = wrapper.CLREGInfoProvider;
			AssertEquals("ABN", "12345675236", dataProvider.ZA_ABN);
			AssertEquals("CAC", "111", dataProvider.ZA_CAC);
			AssertEquals("CAC type", "22", dataProvider.ZA_CACType);
			AssertEquals("CAC type", "22", dataProvider.ZA_CACType);
			AssertEquals("Business Name", "Test Business Name", dataProvider.ZA_BusinessName);
			AssertEquals("Addresses", header.Addresses, dataProvider.OrganizationAddresses);
			AssertEquals("Main Addresse", header.MainAddress, dataProvider.OrganizationMainAddress);
			AssertEquals("Contacts", header.Contacts, dataProvider.Contacts);
		}

		public void TestABN()
		{
			var dataProvider = (CLREGInfoProvider)GetNewBusinessObject();
			dataProvider.ZA_ABN = "123456";
			AssertEquals("ABN Type", ZString.Empty, dataProvider.ZA_ABNInd);
			dataProvider.ZA_ABNInd = dataProvider.AddInfoLookups.ABNNominatedClientTypeList[0].Code;
			AssertEquals("ABN Type", "I", dataProvider.ZA_ABNInd);

			dataProvider.ZA_ABN = ZString.Empty;
			AssertEquals("ABN Type", ZString.Empty, dataProvider.ZA_ABNInd);
			AssertEquals("ABN", ZString.Empty, dataProvider.ZA_ABN);
		}

		public void TestBusinessAddressDetails()
		{
			var wrapper = new OrgHeaderWrapper(OrganisationWithAddressDetails);
			var dataProvider = wrapper.CLREGInfoProvider;
			dataProvider.ZA_OA_BusinessAddress = OrganisationWithAddressDetails.MainAddress.PK;

			AssertEquals("address 1", "Test Address 1", dataProvider.ZA_Bsn1);
			AssertEquals("address 2", "Test address 2", dataProvider.ZA_Bsn2);
			AssertEquals("City", "London", dataProvider.ZA_BsnCity);
			AssertEquals("Post Code", "2000", dataProvider.ZA_BsnPostCode);
			AssertEquals("State", "NSW", dataProvider.ZA_BsnState);
			AssertEquals("Port", "AUSYD", dataProvider.ZA_BsnPort);
		}

		public void TestPostalAddressDetails()
		{
			var wrapper = new OrgHeaderWrapper(OrganisationWithAddressDetails);
			var dataProvider = wrapper.CLREGInfoProvider;
			dataProvider.ZA_CAC = "123";
			dataProvider.ZA_OA_PostalAddress = OrganisationWithAddressDetails.MainAddress.PK;

			AssertEquals("address 1", "Test Address 1", dataProvider.ZA_Post1);
			AssertEquals("address 2", "Test address 2", dataProvider.ZA_Post2);
			AssertEquals("City", "London", dataProvider.ZA_PostCity);
			AssertEquals("Post Code", "2000", dataProvider.ZA_PostPostCode);
			AssertEquals("State", "NSW", dataProvider.ZA_PostState);
			AssertEquals("Port", "AUSYD", dataProvider.ZA_PostPort);

			dataProvider.ZA_CAC = ZString.Empty;
			AssertEquals("Postal address details should be empty, when CACType becomes empty", ZString.Empty, dataProvider.ZA_Post1);
			AssertEquals("Postal address details should be empty, when CACType becomes empty", ZString.Empty, dataProvider.ZA_Post2);
			AssertEquals("Postal address details should be empty, when CACType becomes empty", ZString.Empty, dataProvider.ZA_PostCity);
			AssertEquals("Postal address details should be empty, when CACType becomes empty", ZString.Empty, dataProvider.ZA_PostPostCode);
			AssertEquals("Postal address details should be empty, when CACType becomes empty", ZString.Empty, dataProvider.ZA_PostState);
			AssertEquals("Postal address details should be empty, when CACType becomes empty", ZString.Empty, dataProvider.ZA_PostPort);
		}

		public void TestISupplyCLREGInfoMembers()
		{
			var header = Factory.New<OrgHeader>();
			var wrapper = new OrgHeaderWrapper(header);

			wrapper.Messages.AddNew();
			var dataProvider = wrapper.CLREGInfoProvider;
			dataProvider.ZA_IsIndiv = true;
			dataProvider.ZA_CAC = "FR4";
			dataProvider.ZA_CACType = "AB";
			dataProvider.ZA_Title = "Ms";
			dataProvider.ZA_FirstName = "First Name";
			dataProvider.ZA_SecondName = "Second Name";
			dataProvider.ZA_FamilyName = "Family Name";
			dataProvider.ZA_Suffix = "Suffix";
			dataProvider.CLREGContactInfoProvider.ZA_ContName = "Primary Contact";
			dataProvider.CLREGContactInfoProvider.ZA_ContPurpose = "Contact Purpose";
			dataProvider.ZA_BusinessName = "B Name";
			dataProvider.ZA_IsEvidenceOfID = true;
			dataProvider.ZA_IsExDocsUser = true;
			dataProvider.TravelDocuments.AddNew();
			dataProvider.Rolls.AddNew();
			dataProvider.ZA_Bsn1 = "BO address 1";
			dataProvider.ZA_Bsn2 = "BO address 2";
			dataProvider.ZA_BsnCity = "BO address city";
			dataProvider.ZA_BsnPostCode = "2018";
			dataProvider.ZA_BsnPort = "AUBNE";
			dataProvider.ZA_BsnState = "NSW";

			dataProvider.ZA_Post1 = "Post address 1";
			dataProvider.ZA_Post2 = "Post address 2";
			dataProvider.ZA_PostCity = "Post address city";
			dataProvider.ZA_PostPostCode = "2060";
			dataProvider.ZA_PostPort = "AUSYD";
			dataProvider.ZA_PostState = "NSW";

			dataProvider.ZA_DateofBirth = ZDate.Today;
			dataProvider.ZA_Gender = CMRGenderCodes.Codes.Male;

			var contactDataProvider = dataProvider.CLREGContactInfoProvider;
			contactDataProvider.ZA_Cont1 = "Contact address 1";
			contactDataProvider.ZA_Cont2 = "Contact address 2";
			contactDataProvider.ZA_ContCity = "Contact address city";

			contactDataProvider.ZA_ContPostCode = "2065";
			contactDataProvider.ZA_ContPort = "BBBBB";
			contactDataProvider.ZA_ContState = "RRR";

			contactDataProvider.ZA_ContPost1 = "Contact post address 1";
			contactDataProvider.ZA_ContPost2 = "Contact post address 2";
			contactDataProvider.ZA_ContPostCity = "Contact post address city";
			contactDataProvider.ZA_ContPostPostCode = "2077";
			contactDataProvider.ZA_ContPostPort = "AUADL";
			contactDataProvider.ZA_ContPostState = "NT";

			contactDataProvider.ZA_ContPhPref = "02";
			contactDataProvider.ZA_ContPh = "123456789";
			contactDataProvider.ZA_ContPhComment = "phone comment";
			contactDataProvider.ZA_ContFaxPref = "03";
			contactDataProvider.ZA_ContFax = "987654";
			contactDataProvider.ZA_ContFaxComment = "fax comment";
			contactDataProvider.ZA_ContAHPref = "04";
			contactDataProvider.ZA_ContAH = "456456";
			contactDataProvider.ZA_ContAHComment = "AH comment";
			contactDataProvider.ZA_ContMob = "04789456";
			contactDataProvider.ZA_ContMobComment = "mobile comment";
			contactDataProvider.ZA_ContEmail = "test@test.com";

			var iSupplyCLREG = (ISupplyCLREGInfo)dataProvider;

			AssertEquals("Messages", 1, iSupplyCLREG.Messages.Count);
			AssertEquals("IsIndividual", true, iSupplyCLREG.IsIndividual);

			dataProvider.ZA_ABN = "12456";
			AssertEquals("ABN", "12456", iSupplyCLREG.ABN);
			AssertEquals("Org PK", header.PK, iSupplyCLREG.OrganizationPK);
			AssertEquals("CAC", "FR4", iSupplyCLREG.CAC);
			AssertEquals("CAC Type", "AB", iSupplyCLREG.CACType);
			AssertEquals("Title", "Ms", iSupplyCLREG.Title);
			AssertEquals("First name", "First Name", iSupplyCLREG.FirstName);
			AssertEquals("Second name", "Second Name", iSupplyCLREG.SecondName);
			AssertEquals("Family name", "Family Name", iSupplyCLREG.FamilyName);
			AssertEquals("Suffix", "Suffix", iSupplyCLREG.Suffix);
			AssertEquals("ContactName", "Primary Contact", iSupplyCLREG.ContactName);
			AssertEquals("ContactPurpose", "Contact Purpose", iSupplyCLREG.ContactPurpose);
			AssertEquals("BusinessName", "B Name", iSupplyCLREG.BusinessName);
			AssertEquals("IsEvidenceOfID", true, iSupplyCLREG.IsEvidenceOfID);
			AssertEquals("IsExDocsUser", true, iSupplyCLREG.IsExDocsUser);
			AssertEquals("TravelDocuments", 1, iSupplyCLREG.TravelDocuments.Count);
			AssertEquals("Rolls", 1, iSupplyCLREG.Rolls.Count);
			AssertEquals("BusinessAddress1", "BO address 1", iSupplyCLREG.BusinessAddress1);
			AssertEquals("BusinessAddress2", "BO address 2", iSupplyCLREG.BusinessAddress2);
			AssertEquals("BusinessAddressCity", "BO address city", iSupplyCLREG.BusinessAddressCity);
			AssertEquals("BusinessAddressPostCode", "2018", iSupplyCLREG.BusinessAddressPostCode);
			AssertEquals("BusinessAddressCountry", "AU", iSupplyCLREG.BusinessAddressCountry);
			AssertEquals("BusinessAddressState", "NSW", iSupplyCLREG.BusinessAddressState);

			AssertEquals("PostalAddress1", "Post address 1", iSupplyCLREG.PostalAddress1);
			AssertEquals("PostalAddress2", "Post address 2", iSupplyCLREG.PostalAddress2);
			AssertEquals("PostalAddressCity", "Post address city", iSupplyCLREG.PostalAddressCity);
			AssertEquals("PostalAddressPostCode", "2060", iSupplyCLREG.PostalAddressPostCode);
			AssertEquals("PostalAddressCountry", "AU", iSupplyCLREG.PostalAddressCountry);
			AssertEquals("PostalAddressState", "NSW", iSupplyCLREG.PostalAddressState);

			AssertEquals("ContactAddress1", "Contact address 1", iSupplyCLREG.ContactAddress1);
			AssertEquals("ContactAddress2", "Contact address 2", iSupplyCLREG.ContactAddress2);
			AssertEquals("ContactAddressCity", "Contact address city", iSupplyCLREG.ContactAddressCity);
			AssertEquals("ContactAddressPostCode", "2065", iSupplyCLREG.ContactAddressPostCode);
			AssertEquals("ContactAddressCountry", "BB", iSupplyCLREG.ContactAddressCountry);
			AssertEquals("ContactAddressState", "RRR", iSupplyCLREG.ContactAddressState);

			AssertEquals("ContactPostalAddress1", "Contact post address 1", iSupplyCLREG.ContactPostalAddress1);
			AssertEquals("ContactPostalAddress2", "Contact post address 2", iSupplyCLREG.ContactPostalAddress2);
			AssertEquals("ContactPostalAddressCity", "Contact post address city", iSupplyCLREG.ContactPostalAddressCity);
			AssertEquals("ContactPostalAddressPostCode", "2077", iSupplyCLREG.ContactPostalAddressPostCode);
			AssertEquals("ContactPostalAddressCountry", "AU", iSupplyCLREG.ContactPostalAddressCountry);
			AssertEquals("ContactPostalAddressState", "NT", iSupplyCLREG.ContactPostalAddressState);

			AssertEquals("ContactPhPrefix", "02", iSupplyCLREG.ContactPhPrefix);
			AssertEquals("ContactPh", "123456789", iSupplyCLREG.ContactPh);
			AssertEquals("ContactPhComment", "phone comment", iSupplyCLREG.ContactPhComment);
			AssertEquals("ContactPhPrefix", "03", iSupplyCLREG.ContactFaxPrefix);
			AssertEquals("ContactPh", "987654", iSupplyCLREG.ContactFax);
			AssertEquals("ContactPhComment", "fax comment", iSupplyCLREG.ContactFaxComment);
			AssertEquals("ContactAHPrefix", "04", iSupplyCLREG.ContactAHPrefix);
			AssertEquals("ContactAH", "456456", iSupplyCLREG.ContactAH);
			AssertEquals("ContactAHComment", "AH comment", iSupplyCLREG.ContactAHComment);
			AssertEquals("ContactMobile", "04789456", iSupplyCLREG.ContactMobile);
			AssertEquals("ContactMobileComment", "mobile comment", iSupplyCLREG.ContactMobileComment);
			AssertEquals("ContactEmail", "test@test.com", iSupplyCLREG.ContactEmail);
			AssertEquals("DateofBirth", ZDate.Today, iSupplyCLREG.DateofBirth);
			AssertEquals("Gender", CMRGenderCodes.Codes.Male, iSupplyCLREG.Gender);
		}

		OrgHeader OrganisationWithAddressDetails => CreateHeader(Factory);

		OrgHeader CreateHeader(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<OrgHeader>();
			header.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			header.MainAddress.OA_Address1 = "Test Address 1";
			header.MainAddress.OA_Address2 = "Test address 2";
			header.MainAddress.OA_City = "London";
			header.MainAddress.OA_PostCode = "2000";
			header.MainAddress.OA_State = "NSW";
			return header;
		}

		protected override BusinessObject GetNewBusinessObject() => new OrgHeaderWrapper(OrganisationWithAddressDetails).CLREGInfoProvider;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = factory.New<CLREGInfoProvider>();
			var parent = CreateHeader(factory);
			result.B7_ParentID = parent.PK;
			result.B7_ParentTableCode = parent.TablePrefix;
			return result;
		}
	}
}
