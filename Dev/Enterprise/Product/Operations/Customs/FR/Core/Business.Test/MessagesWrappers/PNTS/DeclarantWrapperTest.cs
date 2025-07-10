using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	class DeclarantWrapperTest : Customs.Business.Testing.DataProviderTestCase<DeclarantWrapper>
	{
		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber should equal from declarant address.", ZString.Empty, Provider.IdentificationNumber);

			header.Declarant.CustomsCodes.AddNew("EOR", "ABC", header.AMA_RN_NKCountry);
			var declarantWrapper = DeclarantWrapper.New(header);
			AssertEquals("IdentificationNumber should equal EORI from declarant address where country equal AMA_RN_NKCountry.", "FRABC", declarantWrapper.IdentificationNumber);

			var storageHeader2 = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			orgHeader.OH_FullName = "ABCD";
			orgHeader.Addresses.MainAddress.Address1 = "ABCDE";
			orgAddress.OA_OH = orgHeader.PK;
			storageHeader2.AMA_OA_Declarant = orgHeader.Addresses.MainAddress.PK;

			storageHeader2.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "DEF", Core.Constants.CountryCodes.UnitedKingdom);
			declarantWrapper = DeclarantWrapper.New(storageHeader2);
			AssertEquals("IdentificationNumber should equal EORI from declarant", "GBDEF", declarantWrapper.IdentificationNumber);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", storageHeader2.AMA_RN_NKCountry);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			declarantWrapper = DeclarantWrapper.New(storageHeader2);
			AssertEquals("IdentificationNumber should equal EORI from declarant where country equal AMA_RN_NKCountry.", "FR12345678900001", declarantWrapper.IdentificationNumber);
		}

		public void TestName()
		{
			AssertEquals("Name should equal orgHeader.OH_FullName", "ABCD", Provider.Name);
		}

		public void TestCommunication()
		{
			AssertEquals("Length of Communication should be 1.", 1, Provider.Communication.Count);
			var broker = Provider.Communication.ElementAt(0);
			AssertType<BrokerWrapper>("Element in Communication should be of type BrokerWrapper.", broker);
			BrokerWrapperTest.AssertBrokerWrapper(broker as BrokerWrapper, "EM", "PP@broker.job");
		}

		protected override DeclarantWrapper GetProvider()
		{
			header = SetUpTemporaryStorageHeader();
			return DeclarantWrapper.New(header);
		}

		TemporaryStorageHeader SetUpTemporaryStorageHeader()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "ABCD";
			orgHeader.Addresses.MainAddress.Address1 = "ABCDE";
			orgAddress.OA_OH = orgHeader.PK;
			storageHeader.AMA_OA_Declarant = orgHeader.Addresses.MainAddress.PK;

			orgAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "DEF", Core.Constants.CountryCodes.UnitedKingdom);

			var broker = Factory.NewWithValidTestData<GlbStaff>();
			broker.GS_Code = "BOK";
			broker.GS_WorkPhone = "123456";
			broker.GS_EmailAddress = "PP@broker.job";

			storageHeader.AMA_GS_NKCustomsAgent = "BOK";
			Factory.Save();
			return storageHeader;
		}
		TemporaryStorageHeader header;
	}
}
