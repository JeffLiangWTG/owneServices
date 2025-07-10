using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(JobDeclarationLookups))]
	sealed class JobDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestInspectionWitnessAddressCodeList()
		{
			CombineAssertions(() =>
			{
				var lookups = Factory.New<JobDeclaration>().Lookups;

				var filterDefaultsNUC = lookups.InspectionWitnessOrganisations.FilterBusinessObjectDefaults.ToList<FilterBusinessObjectDefault>();
				AssertContainsExactElementsInExactOrder(new[] { "Registration Country/Type", "Registration Country/Type", "Organisation Types" }, filterDefaultsNUC.Select(d => d.FilterName));
				AssertContainsExactElementsInExactOrder(new[] { "Property1", "Property2", "Property8" }, filterDefaultsNUC.Select(d => d.PropertyName));
				AssertContainsExactElementsInExactOrder(new IZType[] { (ZString)Core.Constants.CountryCodes.Japan, (ZString)OrgCusCode.JapanCodeTypes.NUC, (ZBool)true }, filterDefaultsNUC.Select(d => d.Value));
				AssertContainsExactElementsInExactOrder(new[] { true, true, true }, filterDefaultsNUC.Select(d => d.IsRemovable));
				AssertSame("Should have been cached", lookups.InspectionWitnessOrganisations, lookups.InspectionWitnessOrganisations);
			});
		}

		public void TestTransportTypeList()
		{
			var transportTypeList = declarationLookups.TransportTypeList;
			AssertContainsExactElementsInAnyOrder(new[] { "AIR", "SEA" }, transportTypeList.GetAllCodes());
		}

		public void TestNACCSCredentialList()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "st1";
			var seaPassword = Factory.NewWithValidTestData<GlbExternalPasswordCUS>();
			seaPassword.GP_GS = staff.PK;
			seaPassword.GP_PasswordType = JPPasswordType.Codes.CUS;
			seaPassword.GP_Transport = UserCodeSpecificTransportModeList.Codes.SEA;
			seaPassword.GP_GC = GlbCompany.CurrentCompany.PK;
			var airPassword = Factory.NewWithValidTestData<GlbExternalPasswordCUS>();
			airPassword.GP_GS = staff.PK;
			airPassword.GP_PasswordType = JPPasswordType.Codes.CUS;
			airPassword.GP_Transport = UserCodeSpecificTransportModeList.Codes.AIR;
			airPassword.GP_GC = GlbCompany.CurrentCompany.PK;
			var bthPassword = Factory.NewWithValidTestData<GlbExternalPasswordCUS>();
			bthPassword.GP_GS = staff.PK;
			bthPassword.GP_PasswordType = JPPasswordType.Codes.CUS;
			bthPassword.GP_Transport = UserCodeSpecificTransportModeList.Codes.BTH;
			bthPassword.GP_GC = GlbCompany.CurrentCompany.PK;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "st2";
			var bthPassword2 = Factory.NewWithValidTestData<GlbExternalPasswordCUS>();
			bthPassword2.GP_GS = staff2.PK;
			bthPassword2.GP_PasswordType = JPPasswordType.Codes.CUS;
			bthPassword2.GP_Transport = UserCodeSpecificTransportModeList.Codes.BTH;
			bthPassword2.GP_GC = GlbCompany.CurrentCompany.PK;

			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals("There should be no password without an agent", 0, declaration.Lookups.NACCSCredentialsList.Count);
				declaration.JE_GS_NKCusAgent = staff.GS_Code;
				AssertContainsExactElementsInAnyOrder("No transport mode", new[] { bthPassword }, declaration.Lookups.NACCSCredentialsList);
				declaration.JE_TransportMode = UserCodeSpecificTransportModeList.Codes.SEA;
				AssertContainsExactElementsInAnyOrder("Sea", new[] { seaPassword, bthPassword }, declaration.Lookups.NACCSCredentialsList);
				declaration.JE_TransportMode = UserCodeSpecificTransportModeList.Codes.AIR;
				AssertContainsExactElementsInAnyOrder("Air", new[] { airPassword, bthPassword }, declaration.Lookups.NACCSCredentialsList);
			});
		}

		public void TestPaymentDeadlineExtensionCodeList()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<PaymentDeadlineExtensionCodeList>(declaration.Lookups.PaymentDeadlineExtensionCodeList);
		}

		public void TestEntryStatusList()
		{
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedValue<CustomsStatusList>(), declarationLookups.EntryStatusList);
		}

		public void TestMessageStatusList()
		{
			AssertType<JPMessageStatusList>(declarationLookups.MessageStatusList);
		}

		public void TestPaymentMethodList()
		{
			AssertType<PaymentMethodCodeList>(declarationLookups.PaymentPartyList);
		}

		public void TestVolumeUnitList()
		{
			AssertEquals("BF, CC, CF, CI, CY, D3, GA, GI, L, M3, ML, TE", declarationLookups.VolumeUnitList.CodesAsString);
		}

		public void TestPackingUnitTypesList()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var universalReferenceTestHelper = new UniversalReferenceTestDataHelper(Factory);
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanPackageTypes, "Package types");
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanPackageTypes, "BK", "Basket", startDate, endDate);
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanPackageTypes, "CG", "Cage", startDate, endDate);
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanPackageTypes, "DP", "DemiJohn, protected", startDate, endDate);
			universalReferenceTestHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanPackageTypes, "PC", "Parcel", ZDateTime.Today.AddMonths(1), ZDateTime.Today.AddMonths(2));

			Factory.Save();
			AssertEquals("BK, CG, DP", declarationLookups.PackingUnitTypesList.CodesAsString);
		}

		public void TestCarrierCodeCollection()
		{
			SetUpCarrierCodeData();

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertType<RefAirlineCollection>(declaration.Lookups.CarrierCodeCollection);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertType<ZZRefCarrierCombinedCollection>(declaration.Lookups.CarrierCodeCollection);
			AssertEquals(1, declaration.Lookups.CarrierCodeCollection.Count);
			AssertEquals("2B23", (declaration.Lookups.CarrierCodeCollection as ZZRefCarrierCombinedCollection)[0].ZZ4_Code);
		}

		public void TestReceiptModeList()
		{
			AssertType<ReceiptModeList>(declarationLookups.ReceiptModeList);
		}

		public void TestDeliveryModeList()
		{
			AssertType<DeliveryModeList>(declarationLookups.DeliveryModeList);
		}

		public void TestRadioCallSignVessels()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateVesselZZ("LA BOUDEUSE", "LXBH", "CV", "JP");
			helper.CreateVesselZZ("A P MOLLER", "OVYQ2", "CV", "JP");
			Factory.Save();

			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertContainsExactElementsInAnyOrder(["LA BOUDEUSE", "A P MOLLER"], declaration.Lookups.RadioCallSignVessels.Cast<RefVesselZZForRadioCallSign>().Select(c => c.ZZO_Code));
		}

		void SetUpCarrierCodeData()
		{
			var carrier1 = Factory.NewWithValidTestData<ZZRefCarrierCombined>();
			carrier1.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Japan;
			carrier1.ZZ4_Code = "1A23";
			carrier1.ZZ4_IsAir = true;
			var carrier2 = Factory.NewWithValidTestData<ZZRefCarrierCombined>();
			carrier2.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Japan;
			carrier2.ZZ4_Code = "2B23";
			carrier2.ZZ4_IsSea = true;
			var carrier3 = Factory.NewWithValidTestData<ZZRefCarrierCombined>();
			carrier3.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Australia;
			carrier3.ZZ4_Code = "3C23";
			carrier3.ZZ4_IsSea = true;

			Factory.Save();
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declarationLookups = declaration.Lookups;
		}

		JobDeclaration declaration;
		JobDeclarationLookups declarationLookups;
	}
}
