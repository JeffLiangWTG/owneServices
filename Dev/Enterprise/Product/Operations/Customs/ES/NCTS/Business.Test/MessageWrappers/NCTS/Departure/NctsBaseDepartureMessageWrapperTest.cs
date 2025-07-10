using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NCTSBaseDepartureMessageWrapperTest : WrapperHelperTest<NctsBaseDepartureMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("NctsDepartureMovementHeader", () => new NctsBaseDepartureMessageWrapper(Factory.New<NctsHeader>(), null));
				AssertExceptionThrown<ArgumentOutOfRangeException>("No Goods Items", () =>
				{
					var header = Factory.New<NctsHeader>();
					header.SetMovementType(NctsMovementType.Codes.Departure);
					_ = new NctsBaseDepartureMessageWrapper(header, Certificate);
				});
			});
		}

		public void TestCustomsProcedureCategory5()
		{
			var office = nctsHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDeparture);
			office.CY_Data = HeaderDataNCTS.DepartureCustomsOffice;

			AssertEquals("Expected filled CustomsProcedureCategory5", HeaderDataNCTS.DepartureCustomsOfficeCode, wrapper.CustomsProcedureCategory5);
		}

		public void TestLocationOfGoodsExamCustomsOffice()
		{
			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.BM_LocationOfGoodsCode = "9999000002";
				wrapper = new NctsBaseDepartureMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected filled LocationOfGoodsExamCustomsOffice", "9999", wrapper.LocationOfGoodsExamCustomsOffice);

				nctsHeader.MovementHeader.BM_LocationOfGoodsCode = ZString.Empty;
				wrapper = new NctsBaseDepartureMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected empty LocationOfGoodsExamCustomsOffice", ZString.Empty, wrapper.LocationOfGoodsExamCustomsOffice);

				nctsHeader.MovementHeader.BM_LocationOfGoodsCode = "ES009999000002";
				wrapper = new NctsBaseDepartureMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected filled LocationOfGoodsExamCustomsOffice Long", "9999", wrapper.LocationOfGoodsExamCustomsOffice);
			});
		}

		public void TestLocationOfGoodsExam()
		{
			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.BM_LocationOfGoodsCode = "9999000002";
				wrapper = new NctsBaseDepartureMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected filled LocationOfGoodsExam", "000002", wrapper.LocationOfGoodsExam);

				nctsHeader.MovementHeader.BM_LocationOfGoodsCode = ZString.Empty;
				wrapper = new NctsBaseDepartureMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected empty LocationOfGoodsExam", ZString.Empty, wrapper.LocationOfGoodsExam);

				nctsHeader.MovementHeader.BM_LocationOfGoodsCode = "ES009999000002";
				wrapper = new NctsBaseDepartureMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected filled LocationOfGoodsExam Long", "000002", wrapper.LocationOfGoodsExam);
			});
		}

		public void TestCodeOfLoadingLocation()
		{
			nctsHeader.MovementHeader.BM_RL_NKForeignDestPort = HeaderDataNCTS.LoadingPlaceCode;
			AssertEquals("Expected filled CodeOfLoadingLocation", HeaderDataNCTS.LoadingPlaceCode, wrapper.CodeOfLoadingLocation);
		}

		public void TestCodeOfUnloadingLocation()
		{
			nctsHeader.PlaceOfUnloadingCode = HeaderDataNCTS.UnloadingPlaceCode;
			AssertEquals("Expected filled CodeOfUnloadingLocation", HeaderDataNCTS.UnloadingPlaceCode, wrapper.CodeOfUnloadingLocation);
		}

		public void TestGoodsInContainerIndicator()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected false GoodsInContainerIndicator", false, wrapper.GoodsInContainerIndicator);

				var nctsHeaderContainer1 = nctsHeader.DepartureHeaderContainers.AddNew();
				nctsHeaderContainer1.BC_ContainerNum = "Container";
				nctsHeaderContainer1.BC_Mode = "CNT";
				var nonPersistentContainerPivot = goodsItem.ContainersPivots.AddNew();
				nonPersistentContainerPivot.Container = nctsHeaderContainer1;
				nonPersistentContainerPivot.ContainerSelected = true;

				AssertEquals("Expected true GoodsInContainerIndicator", true, wrapper.GoodsInContainerIndicator);
			});
		}

		public void TestSecurityDeclaration()
		{
			CombineAssertions(() =>
			{
				nctsHeader.BH_FTZMove = false;
				AssertEquals("Expected false SecurityDeclaration", false, wrapper.SecurityDeclaration);

				nctsHeader.BH_FTZMove = true;
				nctsHeader.MovementHeader.BM_TypeOfSecurity = "BTH";
				AssertEquals("Expected true SecurityDeclaration", true, wrapper.SecurityDeclaration);
			});
		}

		public void TestCountryCodes()
		{
			foreach (var country in CountriesOfRouting)
			{
				AddItineraryCountryForTest(nctsHeader, country);
			}
			var countryCodes = wrapper.CountryCodes;

			CombineAssertions(() =>
			{
				AssertArrayEqualsByElements("No empty CountryCodes", CountriesOfRouting, countryCodes.ToArray());
				AssertSame("Cached CountryCodes", wrapper.CountryCodes, countryCodes);
			});

			void AddItineraryCountryForTest(NctsHeader nctsHeader, ZString country)
			{
				var itineraryCountry = nctsHeader.Itinerary.AddNew();
				itineraryCountry.CountryCode = country;
			}
		}

		public void TestSealCodes()
		{
			for (int i = 1; i < 6; i++)
			{
				var isEven = (i % 2) == 0;
				var seal1 = !isEven ? "SEAL" + i : string.Empty;
				var seal2 = isEven ? "SEAL" + i : string.Empty;
				AddSealCodesForTest(nctsHeader, seal1, seal2);
			}

			nctsHeader.Seals.AddNew().CY_Data = ZString.Empty;
			nctsHeader.Seals.AddNew().CY_Data = "SEAL6";
			nctsHeader.Seals.AddNew().CY_Data = "SEAL7";

			var sealCodes = wrapper.SealCodes;

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("No empty SealCodes", new ZString[] { "SEAL1", "SEAL2", "SEAL3", "SEAL4", "SEAL5", "SEAL6", "SEAL7" }, sealCodes.ToArray());
				AssertSame("Cached SealCodes", wrapper.SealCodes, sealCodes);
			});

			void AddSealCodesForTest(NctsHeader nctsHeader, ZString sealCode1, ZString sealCode2)
			{
				var nctsHeaderContainer = nctsHeader.DepartureHeaderContainers.AddNew();
				nctsHeaderContainer.Seal1 = sealCode1;
				nctsHeaderContainer.Seal2 = sealCode2;
			}
		}

		public void TestTransportMethodOfPayment()
		{
			nctsHeader.MovementHeader.BM_MethodOfPayment = HeaderDataNCTS.PaymentMethod;
			AssertEquals("Expected filled TransportMethodOfPayment", HeaderDataNCTS.PaymentMethod, wrapper.TransportMethodOfPayment);
		}

		public void TestConveyanceReferenceNumber()
		{
			nctsHeader.MovementHeader.BM_ConveyanceNumber = HeaderDataNCTS.ConveyanceReferenceNumber;
			AssertEquals("Expected filled ConveyanceReferenceNumber", HeaderDataNCTS.ConveyanceReferenceNumber, wrapper.ConveyanceReferenceNumber);
		}

		public void TestReferenceNumber()
		{
			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.BM_AdditionalText = ZString.Empty;
				nctsHeader.LocalReferenceNumber = HeaderDataNCTS.LocalReferenceNumber;
				AssertEquals("Expected filled ReferenceNumber with LocalReferenceNumber", HeaderDataNCTS.LocalReferenceNumber, wrapper.ReferenceNumber);

				nctsHeader.MovementHeader.BM_AdditionalText = HeaderDataNCTS.ReferenceNumber;
				AssertEquals("Expected filled ReferenceNumber with CommercialReferenceNumber", HeaderDataNCTS.ReferenceNumber, wrapper.ReferenceNumber);
			});
		}

		public void TestSpecificCircumstancesIndicator()
		{
			nctsHeader.MovementHeader.BM_BTAIndicator = HeaderDataNCTS.SpecificCircumstancesIndicator;
			AssertEquals("Expected filled SpecificCircumstancesIndicator", HeaderDataNCTS.SpecificCircumstancesIndicator, wrapper.SpecificCircumstancesIndicator);
		}

		public void TestBorderTransportMode()
		{
			nctsHeader.MovementHeader.BM_ExportTransportMode = NctsTransportData1.Mode;
			nctsHeader.MovementHeader.BM_TOLCarrierID = NctsTransportData1.Id;
			nctsHeader.MovementHeader.BM_TOLCarrierCode = NctsTransportData1.Nationality;
			var borderTransportMode = wrapper.BorderTransportMode;

			CombineAssertions(() =>
			{
				AssertEquals("BorderTransportMode wrapped", NctsTransportData1.Mode, borderTransportMode.TransportMode);
				AssertSame("Cached BorderTransportMode", wrapper.BorderTransportMode, borderTransportMode);
			});
		}

		public void TestNullDeclarant()
		{
			nctsHeader.DeclarantOrgPK = ZGuid.Empty;
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Declarant.ToString());
		}

		public void TestDeclarant()
		{
			var newBranch = Factory.New<GlbBranch>();
			newBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			newBranch.GB_Code = "XAX";
			var orgProxyHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgProxyHeader.OH_FullName = "Org Proxy, S.A.";
			var orgRepresentative = Factory.NewWithValidTestData<OrgHeader>();
			orgRepresentative.OH_FullName = "Representative, S.A.";
			var orgPrincipal = Factory.NewWithValidTestData<OrgHeader>();
			orgPrincipal.OH_FullName = "Principal, S.A.";
			newBranch.GB_OH_OrgProxy = orgProxyHeader.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser, newBranch.PK.ToGuid(), Env.CurrentDepartmentPK)))
			{
				var newNctsHeader = CreateNctsHeader();
				Factory.Save();
				wrapper = new NctsBaseDepartureMessageWrapper(newNctsHeader, Certificate);
				var declarant = wrapper.Declarant;

				CombineAssertions(() =>
				{
					AssertNotNull("if DeclarantOrg is not empty Declarant is filled by it", declarant);
					AssertSame("if DeclarantOrg is not empty Cached Declarant is filled by it", wrapper.Declarant, declarant);
					AssertEquals("if DeclarantOrg is not empty Declarant will be DeclarantOrg", newNctsHeader.DeclarantAddress.Header.OH_FullName, wrapper.Declarant.Name);

					newBranch.GB_OH_OrgProxy = ZGuid.Empty;
					newNctsHeader = CreateNctsHeader();
					newNctsHeader.MovementHeader.Representative.OrganisationPK = orgRepresentative.PK;
					newNctsHeader.Principal.OrganisationPK = orgPrincipal.PK;
					Factory.Save();
					wrapper = new NctsBaseDepartureMessageWrapper(newNctsHeader, Certificate);
					declarant = wrapper.Declarant;
					AssertNotNull("if DeclarantOrg is empty and Representative is not empty Declarant is filled by Representative", declarant);
					AssertSame("if DeclarantOrg is empty and Representative is not empty Cached Declarant is filled by Representative", wrapper.Declarant, declarant);
					AssertEquals("if DeclarantOrg is empty and Representative is not empty Declarant will be Representative", newNctsHeader.MovementHeader.Representative.Address.Header.OH_FullName, wrapper.Declarant.Name);

					newNctsHeader.MovementHeader.Representative.OrganisationPK = ZGuid.Empty;
					wrapper = new NctsBaseDepartureMessageWrapper(newNctsHeader, Certificate);
					declarant = wrapper.Declarant;
					AssertNotNull("if DeclarantOrg is empty and Representative is empty also Declarant is filled by Principal", declarant);
					AssertSame("if DeclarantOrg is empty and Representative is empty also Cached Declarant is filled by Principal", wrapper.Declarant, declarant);
					AssertEquals("if DeclarantOrg is empty and Representative is empty also Declarant will be Principal", newNctsHeader.Principal.Address.Header.OH_FullName, wrapper.Declarant.Name);
				});
			}
		}

		public void TestNullSecurityCarrier()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.SecurityCarrier.ToString());
		}

		public void TestSecurityCarrier()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				nctsHeader.BH_OH_Carrier = orgHeader.PK;
				wrapper = new NctsBaseDepartureMessageWrapper(nctsHeader, Certificate);
				var securityCarrier = wrapper.SecurityCarrier;

				AssertNotNull("Expected filled SecurityCarrier", securityCarrier);
				AssertSame("Cached SecurityCarrier", wrapper.SecurityCarrier, securityCarrier);
			});
		}

		public void TestNullSecurityConsignor()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.SecurityConsignor.ToString());
		}

		public void TestSecurityConsignor()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				nctsHeader.SecurityConsignor.OrganisationPK = orgHeader.PK;
				wrapper = new NctsBaseDepartureMessageWrapper(nctsHeader, Certificate);
				var securityConsignor = wrapper.SecurityConsignor;

				AssertNotNull("Expected filled SecurityConsignor", securityConsignor);
				AssertSame("Cached SecurityConsignor", wrapper.SecurityConsignor, securityConsignor);
			});
		}

		public void TestNullSecurityConsignee()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.SecurityConsignee.ToString());
		}

		public void TestSecurityConsignee()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				nctsHeader.SecurityConsignee.OrganisationPK = orgHeader.PK;
				wrapper = new NctsBaseDepartureMessageWrapper(nctsHeader, Certificate);
				var securityConsignee = wrapper.SecurityConsignee;

				AssertNotNull("Expected filled SecurityConsignee", securityConsignee);
				AssertSame("Cached SecurityConsignee", wrapper.SecurityConsignee, securityConsignee);
			});
		}

		public void TestTotalNumberOfGoods()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected 1 TotalNumberOfGoods (mandatory at least one)", 1, wrapper.TotalNumberOfGoods);

				nctsHeader.MovementHeader.GoodsItems.AddNew();
				nctsHeader.MovementHeader.GoodsItems.AddNew();
				AssertEquals("Expected 3 TotalNumberOfGoods", 3, wrapper.TotalNumberOfGoods);
			});
		}

		public void TestTotalNumberOfPackageElements_IsVehicles()
		{
			goodsItem.IsVehicles = true;
			goodsItem.Packages.AddNew();
			goodsItem.Packages.AddNew();
			AssertEquals("Expected filled TotalNumberOfPackageElements with vehicles", 2, wrapper.TotalNumberOfPackageElements);
		}

		public void TestTotalNumberOfPackageElements_PackagesWithoutUnitCount()
		{
			goodsItem.IsVehicles = false;
			goodsItem.Packages.AddNew();
			goodsItem.Packages.AddNew();
			AssertEquals("Expected filled TotalNumberOfPackageElements with empty packages", 2, wrapper.TotalNumberOfPackageElements);
		}

		public void TestTotalNumberOfPackageElements_PackagesWithUnitCount()
		{
			goodsItem.IsVehicles = false;
			var pack1 = goodsItem.Packages.AddNew();
			pack1.B5_UnitCount = 2;
			var pack2 = goodsItem.Packages.AddNew();
			pack2.B5_UnitCount = 1;
			AssertEquals("Expected filled TotalNumberOfPackageElements with full packages", 3, wrapper.TotalNumberOfPackageElements);
		}

		public void TestTotalNumberOfPackageElements_MixtureOfIsVehiclesAndPackages()
		{
			goodsItem.IsVehicles = false;
			var pack1 = goodsItem.Packages.AddNew();
			pack1.B5_UnitCount = 2;
			var pack2 = goodsItem.Packages.AddNew();
			pack2.B5_UnitCount = 1;

			var goodsItem2 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem2.IsVehicles = true;
			goodsItem2.Packages.AddNew();
			goodsItem2.Packages.AddNew();
			AssertEquals("Expected filled TotalNumberOfPackageElements with 2 goodsItems, one with vehicles and one with full packages", 5, wrapper.TotalNumberOfPackageElements);
		}

		public void TestTotalNumberOfPackageElements_BulkPackages()
		{
			CreateBulkPackage();
			goodsItem.IsVehicles = false;
			var pack1 = goodsItem.Packages.AddNew();
			pack1.B5_UnitCount = 4;
			pack1.B5_UnitType = "VO";
			var pack2 = goodsItem.Packages.AddNew();
			pack2.B5_UnitType = "VO";
			AssertEquals("Expected filled TotalNumberOfPackageElements with bulk packages", 2, wrapper.TotalNumberOfPackageElements);
		}

		public void TestTotalNumberOfPackageElements_MixtureOfAllTypes()
		{
			CreateBulkPackage();
			goodsItem.IsVehicles = false;
			var pack1 = goodsItem.Packages.AddNew();
			pack1.B5_UnitCount = 2;
			var pack2 = goodsItem.Packages.AddNew();
			pack2.B5_UnitCount = 1;
			var pack3 = goodsItem.Packages.AddNew();
			pack3.B5_UnitCount = 0;

			var goodsItem2 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem2.IsVehicles = true;
			goodsItem2.Packages.AddNew();
			goodsItem2.Packages.AddNew();

			var goodsItem3 = nctsHeader.MovementHeader.GoodsItems.AddNew();
			goodsItem3.IsVehicles = false;
			var pack4 = goodsItem3.Packages.AddNew();
			pack4.B5_UnitCount = 4;
			pack4.B5_UnitType = "VO";
			var pack5 = goodsItem3.Packages.AddNew();
			pack5.B5_UnitType = "VO";

			AssertEquals("Expected filled TotalNumberOfPackageElements with 3 goodsItems, one with vehicles, one with full packages and one with Bulks", 7, wrapper.TotalNumberOfPackageElements);
		}

		NctsHeader CreateNctsHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.GoodsItems.AddNew();

			return nctsHeader;
		}

		void CreateBulkPackage()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "VO", "Bulk", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = CreateNctsHeader();
			goodsItem = nctsHeader.MovementHeader.GoodsItems.Single();
			wrapper = new NctsBaseDepartureMessageWrapper(nctsHeader, Certificate);
		}

		NctsHeader nctsHeader;
		NctsDepartureCargoDesc goodsItem;
		NctsBaseDepartureMessageWrapper wrapper;

		protected override NctsBaseDepartureMessageWrapper GetProvider() => wrapper;
	}
}
