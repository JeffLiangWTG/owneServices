using System;
using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging.Testing
{
	[TestedType(typeof(CC044ADeclarationWrapperExpected))]
	class CC044ADeclarationWrapperExpectedTests : EU.NCTS.Business.Testing.DeclarationWrapperAbstractTest<CC044ADeclarationWrapperExpected>
	{
		public void TestMRN()
		{
			var mrn = CusEntryNumber.LoadOrCreate(header, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.UnitedKingdom);
			mrn.CE_EntryNum = "21GB98989";
			mrn.CE_IssueDate = CargoWise.Types.ZDateTime.BrettsBirthday;
			AssertEquals("21GB98989", wrapper.MovementReferenceNumber);
		}

		public void TestLocalReferenceNumber()
		{
			header.LocalReferenceNumber = "LRN123";
			AssertEquals("LRN123", wrapper.LocalReferenceNumber);
		}

		public void TestDestinationOffice()
		{
			header.DestinationCustomsOfficeCodeForArrival = "GB00009";
			AssertEquals("GB00009", wrapper.DestinationCustomsOfficeReferenceNumber);
		}

		public void TestUnloadedGoodsItems()
		{
			AssertEquals("No *unloaded* items are anticipated via the Expected wrapper, of course", false, wrapper.UnloadedGoodsItems.Any());
		}

		public void TestNumberOfSeals()
		{
			AssertExceptionThrown<NotSupportedException>(() => _ = wrapper.NumberOfSeals);
		}

		public void TestSeals()
		{
			AssertEquals("No seals are anticipated via the Expected wrapper, they are seen via the Actual wrapper", false, wrapper.Seals.Any());
		}

		public void TestExpectedGoodsItems()
		{
			arrivalMovementHeader.GoodsItems.AddNew();
			arrivalMovementHeader.GoodsItems.AddNew();
			arrivalMovementHeader.GoodsItems.AddNew();
			AssertEquals(3, wrapper.ExpectedGoodsItems.Count);
		}

		public void TestIdentityOfMeansOfTransportAtDeparture()
		{
			arrivalMovementHeader.BM_TransportAtDeparture = "MT10VHC";
			AssertEquals("MT10VHC", wrapper.IdentityOfMeansOfTransportAtDeparture);
		}

		public void TestIdentityOfMeansOfTransportAtDepartureLanguage()
		{
			AssertEquals("", wrapper.IdentityOfMeansOfTransportAtDepartureLanguage);
		}

		public void TestNationalityOfMeansOfTransportAtDeparture()
		{
			arrivalMovementHeader.BM_RN_NKTransportAtDepartureCountry = "DC";
			AssertEquals("DC", wrapper.NationalityOfMeansOfTransportAtDeparture);
		}

		public void TestTotalNumberOfItems()
		{
			arrivalMovementHeader.GoodsItems.AddNew();
			arrivalMovementHeader.GoodsItems.AddNew();
			AssertEquals(2, wrapper.TotalNumberOfItems);
		}

		public void TestTotalNumberOfPackages()
		{
			var gi1 = arrivalMovementHeader.GoodsItems.AddNew();
			var p11 = gi1.Packages.AddNew();
			p11.B5_UnitCount = 29;
			var p12 = gi1.Packages.AddNew();
			p12.B5_UnitCount = 31;
			var gi2 = arrivalMovementHeader.GoodsItems.AddNew();
			var p21 = gi2.Packages.AddNew();
			p21.B5_UnitCount = 5;
			var p22 = gi2.Packages.AddNew();
			p22.B5_UnitCount = 4;
			AssertEquals(69, wrapper.TotalNumberOfPackages);
		}

		public void TestTotalGrossMass()
		{
			arrivalMovementHeader.BM_GrossWeight = 123.45m;
			var gi1 = arrivalMovementHeader.GoodsItems.AddNew();
			gi1.BY_GrossWeight = 68m;
			gi1.BY_GrossWeightUnit = Core.Constants.Weight.Kilograms;
			var gi2 = arrivalMovementHeader.GoodsItems.AddNew();
			gi2.BY_GrossWeight = 2.205m;
			gi2.BY_GrossWeightUnit = Core.Constants.Weight.Pounds;
			AssertEquals(123.45m, wrapper.TotalGrossMass);
		}

		public void TestDestinationTrader()
		{
			var destinationTrader = Factory.NewWithValidTestData<OrgHeader>();
			destinationTrader.OH_FullName = "destinationTrader NAME";
			destinationTrader.MainAddress.Address1 = "10234-119 BOULEVARD FELIX FAURE STREET";
			destinationTrader.MainAddress.City = "SAINT-REMY-EN-BOUZEMONT-SAINT-GENEST-ET-ISSON";
			destinationTrader.MainAddress.Postcode = "00937-0616";
			destinationTrader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			var eori = destinationTrader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			eori.OK_CustomsRegNo = "32582075100080999";
			header.DestinationTrader.E2_OA_Address = destinationTrader.MainAddress.PK;

			CombineAssertions(() =>
			{
				AssertEquals("destinationTrader NAME", wrapper.DestinationTrader.Name);
				AssertEquals("SAINT-REMY-EN-BOUZEMONT-SAINT-GENES", wrapper.DestinationTrader.City);
				AssertEquals(Core.Constants.CountryCodes.France, wrapper.DestinationTrader.CountryCode);
				AssertEquals("00937-061", wrapper.DestinationTrader.PostalCode);
				AssertEquals("EN", wrapper.DestinationTrader.NameAndAddressLanguage);
				AssertEquals("10234-119 BOULEVARD FELIX FAURE STR", wrapper.DestinationTrader.StreetAndNumber);
				AssertEquals("FR325820751000809", wrapper.DestinationTrader.TIN);
			});
		}

		protected override CC044ADeclarationWrapperExpected GetProvider() => wrapperCore;

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			wrapperCore = new CC044ADeclarationWrapperExpected(header);
			wrapper = wrapperCore;
			arrivalMovementHeader = header.ArrivalMovementHeader;
		}
		ICC044ADeclaration wrapper;
		CC044ADeclarationWrapperExpected wrapperCore;
		NctsHeader header;
		EU.NCTS.Business.NctsArrivalMovementHeader arrivalMovementHeader;
	}
}
