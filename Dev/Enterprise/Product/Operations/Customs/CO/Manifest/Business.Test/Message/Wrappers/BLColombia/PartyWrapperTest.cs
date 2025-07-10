using System.Linq;
using CargoWise.Customs.CO.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	class PartyWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestWrapperParty()
		{
			GlbCompany.CurrentCompany.GC_BusinessRegNo = "210413450013";

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			PopulateManifestHeader();

			Factory.Save();

			IManifest wrapper = new ManifestWrapper(header, MessageSubTypeCodes.Codes.Original, header.GetValidDocumentIDs(header.Bills.Count + 1));

			var party = wrapper.Master.Carrier;

			CombineAssertions(() =>
			{
				AssertEquals(31, party.DocumentType);
				AssertEquals("21336937002", party.ID);
				AssertEquals(3, party.VerificationDigit);
				AssertEquals("Party2", party.CompanyName);
			});

			party = wrapper.Master.Shipper;

			CombineAssertions(() =>
			{
				AssertEquals(31, party.DocumentType);
				AssertEquals("213369370011", party.ID);
				AssertEquals(1, party.VerificationDigit);
				AssertEquals("Party1", party.CompanyName);
			});

			party = wrapper.Master.Consignee;

			CombineAssertions(() =>
			{
				AssertEquals(43, party.DocumentType);
				AssertEquals("21041345001", party.ID);
				AssertEquals(3, party.VerificationDigit);
				AssertEquals(ZString.Empty, party.CompanyName);
			});

			party = wrapper.Master.Houses.ElementAt(0).Shipper;

			CombineAssertions(() =>
			{
				AssertEquals(31, party.DocumentType);
				AssertEquals("213369370011", party.ID);
				AssertEquals(1, party.VerificationDigit);
				AssertEquals(ZString.Empty, party.CompanyName);
			});

			party = wrapper.Master.Houses.ElementAt(0).Consignee;

			CombineAssertions(() =>
			{
				AssertEquals(43, party.DocumentType);
				AssertEquals(ZString.Empty, party.ID);
				AssertEquals(0, party.VerificationDigit);
				AssertEquals("Party4", party.CompanyName);
			});
		}

		void PopulateManifestHeader()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "123457";
			orgHeader.OH_FullName = "Party1";
			orgHeader.CustomsCodes.AddNew(ColombiaOrgCusCodeInfo.OrgCusCodes.NIT, "21.336.937.001-1");

			var orgAddress = orgHeader.MainAddress;
			orgAddress.Address1 = "1234";
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Colombia;

			header.AMA_OA_ShippingAgent = orgAddress.PK;

			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "123456";
			orgHeader.OH_FullName = "Party2";
			orgHeader.CustomsCodes.AddNew(ColombiaOrgCusCodeInfo.OrgCusCodes.NIT, "21.336.937.002-3");

			orgAddress = orgHeader.MainAddress;
			orgAddress.Address1 = "1345";
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Colombia;

			header.AMA_OA_Carrier = orgAddress.PK;

			CreateAndPopulateHouseBill(header);
		}

		void CreateAndPopulateHouseBill(AsycudaManifestHeader header)
		{
			var bill = header.Bills.AddNew();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "123458";
			orgHeader.OH_FullName = "Party3";
			orgHeader.CustomsCodes.AddNew(ColombiaOrgCusCodeInfo.OrgCusCodes.NIT, "21.336.937.001-1");

			var orgAddress = orgHeader.MainAddress;
			orgAddress.Address1 = "1235";
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Colombia;

			bill.ABL_OA_Shipper = orgAddress.PK;

			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "123459";
			orgHeader.OH_FullName = "Party4";
			orgHeader.CustomsCodes.AddNew(UruguayOrgCusCodeInfo.OrgCusCodes.RUT, "21.336.937.002-3");

			orgAddress = orgHeader.MainAddress;
			orgAddress.Address1 = "1346";
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Uruguay;

			bill.ABL_OA_Consignee = orgAddress.PK;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var cusTransactionNumber = Factory.NewWithValidTestData<CusTransactionNumber>();
			cusTransactionNumber.TN_TransactionReference = "11667803932049";
			cusTransactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.ColombiaManifest;
			cusTransactionNumber.TN_GC_Company = GlbCompany.CurrentCompany.PK;
		}
	}
}
