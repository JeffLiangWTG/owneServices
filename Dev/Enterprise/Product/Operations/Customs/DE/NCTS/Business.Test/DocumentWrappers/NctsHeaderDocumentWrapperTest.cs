using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.DocumentWrappers.Testing
{
	[TestedType(typeof(NctsHeaderDocumentWrapper))]
	sealed class NctsHeaderDocumentWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNew()
		{
			var header = Factory.New<NctsHeader>();

			AssertExceptionThrown<ArgumentNullException>("Expected exception when NctsHeader parameter is null", () => NctsHeaderDocumentWrapper.New(null, Factory));
			AssertExceptionThrown<ArgumentNullException>("Expected exception when Factory parameter is null", () => NctsHeaderDocumentWrapper.New(header, null));

			AssertNotNull("Instance of NctsHeaderDocumentWrapper expected", NctsHeaderDocumentWrapper.New(header, Factory));
		}

		public void TestCUSTOMERREFERENCE()
		{
			header.MovementHeader.BM_PaperlessInbondNum = "1234V01";
			CombineAssertions(() =>
			{
				var wrapper = GetWrapper(header);
				AssertEquals(nameof(wrapper.CUSTOMERREFERENCE), "1234V01", wrapper.CUSTOMERREFERENCE);
			});
		}

		public void TestCONSIGNOREORI()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = "CONSIGNOR NAME";
			address.OA_Address1 = "CONSIGNOR STREET";
			address.OA_PostCode = "000000000";
			address.OA_City = "CONSIGNOR CITY";
			address.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.France;
			header.Consignor.E2_OA_Address = address.PK;

			var tcu = orgHeader.CustomsCodes.AddNew();
			tcu.OK_CodeType = OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU;
			tcu.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.China;
			tcu.OK_CustomsRegNo = "11111111111";

			var wrapper = GetWrapper(header);
			AssertEquals(nameof(wrapper.CONSIGNOREORI), "CN11111111111", wrapper.CONSIGNOREORI);

			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.France;
			eori.OK_CustomsRegNo = "000000000000000";

			wrapper = GetWrapper(header);
			AssertEquals(nameof(wrapper.CONSIGNOREORI), "FR000000000000000", wrapper.CONSIGNOREORI);
		}

		public void TestCONSIGNORBRANCH()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = "CONSIGNOR NAME";
			address.OA_Address1 = "CONSIGNOR STREET";
			address.OA_PostCode = "000000000";
			address.OA_City = "CONSIGNOR CITY";
			address.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.France;
			header.Consignor.E2_OA_Address = address.PK;

			var ebs = orgHeader.CustomsCodes.AddNew();
			ebs.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
			ebs.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.France;
			ebs.OK_CustomsRegNo = "EBS1";

			var wrapper = GetWrapper(header);
			AssertEquals(ZString.Empty, wrapper.CONSIGNORBRANCH);

			ebs.OK_OA_PremisesAddress = address.PK;

			AssertEquals("EBS1", wrapper.CONSIGNORBRANCH);
		}

		public void TestCOUNTRYOFDISPATCH()
		{
			header.MovementHeader.BM_RN_NKCountryOfDispatch = Enterprise.Core.Constants.CountryCodes.Germany;

			var wrapper = GetWrapper(header);
			AssertEquals(nameof(wrapper.COUNTRYOFDISPATCH), Enterprise.Core.Constants.CountryCodes.Germany, wrapper.COUNTRYOFDISPATCH);
		}

		public void TestCONSIGNEEEORI()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = "CONSIGNEE NAME";
			address.OA_Address1 = "CONSIGNEE STREET";
			address.OA_PostCode = "000000000";
			address.OA_City = "CONSIGNEE CITY";
			address.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.France;
			header.Consignee.E2_OA_Address = address.PK;

			var tcu = orgHeader.CustomsCodes.AddNew();
			tcu.OK_CodeType = OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU;
			tcu.OK_CustomsRegNo = "11111111111";
			tcu.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.Chile;

			var wrapper = GetWrapper(header);
			AssertEquals(nameof(wrapper.CONSIGNEEEORI), "CL11111111111", wrapper.CONSIGNEEEORI);

			var eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Enterprise.Core.Constants.CountryCodes.France;
			eori.OK_CustomsRegNo = "000000000000000";

			wrapper = GetWrapper(header);
			AssertEquals(nameof(wrapper.CONSIGNEEEORI), "FR000000000000000", wrapper.CONSIGNEEEORI);
		}

		public void TestCONSIGNEEBRANCH()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = "CONSIGEE NAME";
			address.OA_Address1 = "CONSIGNEE STREET";
			address.OA_PostCode = "000000000";
			address.OA_City = "CONSIGNEE CITY";
			address.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.France;
			header.Consignee.E2_OA_Address = address.PK;

			var ebs = orgHeader.CustomsCodes.AddNew();
			ebs.OK_CodeType = GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix;
			ebs.OK_RN_NKCodeCountry = address.OA_RN_NKCountryCode;
			ebs.OK_CustomsRegNo = "EBS1";

			ebs.OK_OA_PremisesAddress = address.PK;

			var wrapper = GetWrapper(header);
			AssertEquals("EBS1", wrapper.CONSIGNEEBRANCH);
		}

		public void TestTRANSPORTATDEPARTURE()
		{
			header.MovementHeader.BM_TransportAtDeparture = "XYZ";

			var wrapper = GetWrapper(header);
			AssertEquals(nameof(wrapper.TRANSPORTATDEPARTURE), "XYZ", wrapper.TRANSPORTATDEPARTURE);
		}

		public void TestTOTALSEALCOUNT()
		{
			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "CONTAINER1";
			container1.BC_Seal1 = "Container1Seal1";
			container1.BC_Seal2 = "Container1Seal2";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "Container1Seal3";

			var container2 = header.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "CONTAINER2";
			container2.BC_Seal1 = "Container2Seal1";
			container2.BC_Seal2 = "Container2Seal2";
			container2.AdditionalSeals.AddNew().BK_SealNumber = "Container2Seal3";
			container2.AdditionalSeals.AddNew().BK_SealNumber = "Container2Seal4";

			var wrapper = GetWrapper(header);
			AssertEquals("7", wrapper.TOTALSEALCOUNT);
		}

		public void TestPRINCIPAL()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			orgHeader.OH_FullName = "PRINCIPAL NAME";
			address.OA_Address1 = "PRINCIPAL STREET";
			address.OA_PostCode = "000000000";
			address.OA_City = "PRINCIPAL CITY";
			address.OA_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Germany;
			header.Principal.E2_OA_Address = address.PK;

			var wrapper = GetWrapper(header);
			AssertEquals("PRINCIPAL NAME", wrapper.PRINCIPAL);
		}

		public void TestOFFICEOFDESTINATIONCODE()
		{
			var customsOffices = header.IsPhase5 ? header.MovementHeader.CustomsOffices : header.CustomsOffices;
			var destinationoffice = customsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDestination);
			destinationoffice.CY_Data = "DE0058";

			var wrapper = GetWrapper(header);
			AssertEquals("DE0058", wrapper.OFFICEOFDESTINATIONCODE);
		}

		public void TestOFFICEOFDESTINATION()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");

			var cusCodeList = Factory.New<ZZRefCusCodeListCombined>();
			cusCodeList.ZZD_Code = "DE0058";
			cusCodeList.ZZD_Description = "Abfertigungsstelle Bingen";
			cusCodeList.ZZD_CodeType = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			cusCodeList.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Germany;
			cusCodeList.ZZD_EndDate = ZDateTime.Today.AddDays(2);
			cusCodeList.ZZD_StartDate = ZDateTime.Today.AddDays(-2);

			var destinationoffice = header.CommonMovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDestination);
			destinationoffice.CY_Data = "DE0058";

			var wrapper = GetWrapper(header);
			AssertEquals("Abfertigungsstelle Bingen", wrapper.OFFICEOFDESTINATION);
		}

		public void TestGOODSITEMSCOUNT()
		{
			var bill = header.Bills.AddNew();
			bill.GoodsItems.AddNew();
			bill.GoodsItems.AddNew();
			bill.GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				var wrapper = GetWrapper(header);
				AssertEquals("3", wrapper.GOODSITEMSCOUNT);

				var bill2 = header.Bills.AddNew();
				bill2.GoodsItems.AddNew();
				bill2.GoodsItems.AddNew();

				wrapper = GetWrapper(header);
				AssertEquals("5", wrapper.GOODSITEMSCOUNT);
			});
		}

		public void TestPACKAGESCOUNT()
		{
			var bill = header.Bills.AddNew();
			var goodsItem1 = bill.GoodsItems.AddNew();
			goodsItem1.Packages.AddNew();
			goodsItem1.Packages.AddNew();

			var goodsItem2 = bill.GoodsItems.AddNew();
			goodsItem2.Packages.AddNew();

			var goodsItem3 = bill.GoodsItems.AddNew();
			goodsItem3.Packages.AddNew();
			goodsItem3.Packages.AddNew();
			goodsItem3.Packages.AddNew();

			CombineAssertions(() =>
			{
				var wrapper = GetWrapper(header);
				AssertEquals("6", wrapper.PACKAGESCOUNT);

				var bill2 = header.Bills.AddNew();
				var goodsItem4 = bill2.GoodsItems.AddNew();
				goodsItem4.Packages.AddNew();
				goodsItem4.Packages.AddNew();

				var goodsItem5 = bill2.GoodsItems.AddNew();
				goodsItem5.Packages.AddNew();
				goodsItem5.Packages.AddNew();

				wrapper = GetWrapper(header);
				AssertEquals("10", wrapper.PACKAGESCOUNT);
			});
		}

		public void TestNOTRELEASEDWATERMARK()
		{
			var nctsHeader = Factory.New<NctsHeaderForDocumentWrapperTest>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var wrapper = GetWrapper(nctsHeader);
			AssertEquals("[PRE-CONDITION] BM_CustomsStatus", NctsTransitStatusList.Codes.Unknown, nctsHeader.MovementHeader.BM_CustomsStatus);
			AssertEquals("When BM_CustomsStatus is Unknown", "NOT RELEASED", wrapper.NOTRELEASEDWATERMARK);

			nctsHeader.IsFallBackActiveForTest = true;
			AssertEquals("When FallBack is Active, NOTRELEASEDWATERMARK will be empty", ZString.Empty, wrapper.NOTRELEASEDWATERMARK);
		}

		public void TestContainers()
		{
			var container1 = header.DepartureHeaderContainers.AddNew();
			container1.BC_ContainerNum = "CONTAINER1";
			container1.BC_Seal1 = "Container1Seal1";
			container1.BC_Seal2 = "Container1Seal2";
			container1.AdditionalSeals.AddNew().BK_SealNumber = "Container1Seal3";
			var container2 = header.DepartureHeaderContainers.AddNew();
			container2.BC_ContainerNum = "CONTAINER2";
			container2.BC_Seal1 = "Container2Seal1";
			container2.BC_Seal2 = "Container2Seal2";
			container2.AdditionalSeals.AddNew().BK_SealNumber = "Container2Seal3";
			container2.AdditionalSeals.AddNew().BK_SealNumber = "Container2Seal4";

			var wrapper = GetWrapper(header);
			AssertEquals(2, wrapper.Containers.Count);
		}

		protected override BusinessObject GetNewBusinessObject() => NctsHeaderDocumentWrapper.New(Factory.New<NctsHeader>(), Factory);

		NctsHeaderDocumentWrapper GetWrapper(NctsHeader header) => (NctsHeaderDocumentWrapper)NctsHeaderDocumentWrapper.New(header, Factory);

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
		}
		NctsHeader header;
	}
}
