using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class DepartureLineWrapperTest : WrapperHelperTest<DepartureLineWrapper>
	{
		public void TestCountryOfDeparture()
		{
			goodsItem.BY_RN_NKCountryOfDispatch = GoodsItemDataNCTS.DepartureCountry;
			AssertEquals("Expected filled CountryOfDeparture", GoodsItemDataNCTS.DepartureCountry, wrapper.CountryOfDeparture);
		}

		public void TestCountryOfDestination()
		{
			goodsItem.BY_RN_NKCountryOfDestination = GoodsItemDataNCTS.DestinationCountry;
			AssertEquals("Expected filled CountryOfDestination", GoodsItemDataNCTS.DestinationCountry, wrapper.CountryOfDestination);
		}

		public void TestGoodsCustomsProcedureCategory5()
		{
			goodsItem.BY_Type = GoodsItemDataNCTS.DeclarationType;
			AssertEquals("Expected filled GoodsCustomsProcedureCategory5", GoodsItemDataNCTS.DeclarationType, wrapper.GoodsCustomsProcedureCategory5);
		}

		public void TestGoodsCountryOfOrigin()
		{
			goodsItem.BY_RN_NKCountryOfDispatch = GoodsItemDataNCTS.DepartureCountry;
			AssertEquals("Expected filled GoodsCountryOfOrigin", GoodsItemDataNCTS.DepartureCountry, wrapper.GoodsCountryOfOrigin);
		}

		public void TestGoodsCountryOfDestination()
		{
			goodsItem.BY_RN_NKCountryOfDestination = GoodsItemDataNCTS.DestinationCountry;
			AssertEquals("Expected filled GoodsCountryOfDestination", GoodsItemDataNCTS.DestinationCountry, wrapper.GoodsCountryOfDestination);
		}

		public void TestNetWeightInKG()
		{
			goodsItem.BY_NetWeightUnit = Core.Constants.Weight.Kilograms;
			goodsItem.BY_NetWeight = GoodsItemDataNCTS.NetMassKg;
			AssertEquals("Expected filled NetWeightInKG", GoodsItemDataNCTS.NetMassKg, wrapper.NetWeightInKG);
		}

		public void TestFiscalUnitsNumber()
		{
			goodsItem.BY_CustomsSecondQuantity = GoodsItemDataNCTS.FiscalUnits;
			AssertEquals("Expected filled FiscalUnitsNumber", GoodsItemDataNCTS.FiscalUnits, wrapper.FiscalUnitsNumber);
		}

		public void TestFiscalUnitsQualifier()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			helper.CreateCusMapType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, MapDirectionList.Codes.BTH, "Local Customs Quantity Units", true);
			helper.CreateCusMap(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, EntryLineFeeData.MethodOfCalculationCW1, EntryLineFeeData.MethodOfCalculationCustoms, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1), Core.Constants.CountryCodes.Spain);
			Factory.Save();

			CombineAssertions(() =>
			{
				goodsItem.BY_CustomsSecondUnitQty = GoodsItemDataNCTS.SecondUnitQtyCW1;
				AssertEquals("Expected filled FiscalUnitsQualifier with mapped value", GoodsItemDataNCTS.SecondUnitQtyCustoms, wrapper.FiscalUnitsQualifier);

				goodsItem.BY_CustomsSecondUnitQty = GoodsItemDataNCTS.SecondUnitQtyNotMapped;
				AssertEquals("Expected filled FiscalUnitsQualifier with original value because the value is not mapped", GoodsItemDataNCTS.SecondUnitQtyNotMapped, wrapper.FiscalUnitsQualifier);
			});
		}

		public void TestNullGoodsConsignor()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.GoodsConsignor.ToString());
		}

		public void TestGoodsConsignor()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				goodsItem.Consignor.OrganisationPK = orgHeader.PK;
				wrapper = new DepartureLineWrapper(goodsItem);
				var goodsConsignor = wrapper.GoodsConsignor;

				AssertNotNull("Expected filled GoodsConsignor", goodsConsignor);
				AssertSame("Cached GoodsConsignor", wrapper.GoodsConsignor, goodsConsignor);
			});
		}

		public void TestNullGoodsConsignee()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.GoodsConsignee.ToString());
		}

		public void TestGoodsConsignee()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				goodsItem.Consignee.OrganisationPK = orgHeader.PK;
				wrapper = new DepartureLineWrapper(goodsItem);
				var goodsConsignee = wrapper.GoodsConsignee;

				AssertNotNull("Expected filled GoodsConsignee", goodsConsignee);
				AssertSame("Cached GoodsConsignee", wrapper.GoodsConsignee, goodsConsignee);
			});
		}

		public void TestNullSecurityGoodsConsignor()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.SecurityGoodsConsignor.ToString());
		}

		public void TestSecurityGoodsConsignor()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				goodsItem.SecurityConsignor.OrganisationPK = orgHeader.PK;
				wrapper = new DepartureLineWrapper(goodsItem);
				var securityGoodsConsignor = wrapper.SecurityGoodsConsignor;

				AssertNotNull("Expected filled SecurityGoodsConsignor", securityGoodsConsignor);
				AssertSame("Cached SecurityGoodsConsignor", wrapper.SecurityGoodsConsignor, securityGoodsConsignor);
			});
		}

		public void TestNullSecurityGoodsConsignee()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.SecurityGoodsConsignee.ToString());
		}

		public void TestSecurityGoodsConsignee()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				goodsItem.SecurityConsignee.OrganisationPK = orgHeader.PK;
				wrapper = new DepartureLineWrapper(goodsItem);
				var securityGoodsConsignee = wrapper.SecurityGoodsConsignee;

				AssertNotNull("Expected filled SecurityGoodsConsignee", securityGoodsConsignee);
				AssertSame("Cached SecurityGoodsConsignee", wrapper.SecurityGoodsConsignee, securityGoodsConsignee);
			});
		}

		public void TestInternalPackages()
		{
			goodsItem.IsVehicles = true;
			goodsItem.Packages.AddNew();
			goodsItem.Packages.AddNew();
			var internalPackages = wrapper.InternalPackages;

			CombineAssertions(() =>
			{
				AssertEquals("Expected filled InternalPackages.Packages with vehicles but only one", 1, internalPackages.Packages.Count);
				AssertSame("Cached InternalPackages", wrapper.InternalPackages, internalPackages);
			});
		}

		public void TestInternalPackagesIsNotVehicles()
		{
			goodsItem.IsVehicles = false;
			goodsItem.Packages.AddNew();
			goodsItem.Packages.AddNew();
			var internalPackages = wrapper.InternalPackages;
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled InternalPackages.Packages with packages", 2, internalPackages.Packages.Count);
				AssertSame("Cached InternalPackages", wrapper.InternalPackages, internalPackages);
			});
		}

		public void TestVehiclePackages()
		{
			CombineAssertions(() =>
			{
				goodsItem.IsVehicles = false;

				AssertNull("Expected null VehiclePackages.Packages when IsVehicles is false", wrapper.VehiclePackages);

				goodsItem.IsVehicles = true;
				goodsItem.Packages.AddNew().B5_Brand = "Brand";
				goodsItem.Packages.AddNew().B5_PackageID = "VIN";

				var vehiclePackages = wrapper.VehiclePackages;

				AssertEquals("Expected filled VehiclePackages.Packages", 2, vehiclePackages.Packages.Count);
				AssertSame("Cached VehiclePackages", wrapper.VehiclePackages, vehiclePackages);
			});
		}

		public void TestTotalGoodValueInEuros()
		{
			goodsItem.BY_MonetaryValue = GoodsItemDataNCTS.CustomsValue;
			AssertEquals("Expected filled TotalGoodValueInEuros", GoodsItemDataNCTS.CustomsValue, wrapper.TotalGoodValueInEuros);
		}

		public void TestDocuments()
		{
			goodsItem.SupportingDocuments.AddNew();
			goodsItem.SupportingDocuments.AddNew();
			var documents = wrapper.Documents;
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled Documents", 2, documents.Count);
				AssertSame("Cached Documents", wrapper.Documents, documents);
			});
		}

		public void TestGoodsTransportMethodOfPayment()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Document", ZString.Empty, wrapper.GoodsTransportMethodOfPayment);
				var addInfo = goodsItem.AdditionalInfos.AddNew();
				addInfo.CSI_Code = GoodsItemDataNCTS.MethodOfPayment;
				AssertEquals("Expected filled GoodsTransportMethodOfPayment", GoodsItemDataNCTS.MethodOfPayment, wrapper.GoodsTransportMethodOfPayment);
			});
		}

		public void TestGoodsCountryCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Document", ZString.Empty, wrapper.GoodsCountryCode);
				var addInfo = goodsItem.AdditionalInfos.AddNew();
				addInfo.CSI_RN_NKCountryCode = GoodsItemDataNCTS.GoodsCountryCode;
				AssertEquals("Expected filled GoodsCountryCode", GoodsItemDataNCTS.GoodsCountryCode, wrapper.GoodsCountryCode);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			wrapper = new DepartureLineWrapper(goodsItem);
		}
		NctsDepartureCargoDesc goodsItem;
		DepartureLineWrapper wrapper;

		protected override DepartureLineWrapper GetProvider() => wrapper;
	}
}
