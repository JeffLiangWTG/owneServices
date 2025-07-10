using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	class AsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSpecificCircumstanceList_NVC_SEA()
		{
			CombineAssertions("SpecificCircumstanceList under TransportMode SEA", () =>
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

				var list = header.Lookups.SpecificCircumstanceList;
				AssertContainsExactElementsInAnyOrder(
					"Should contain the following codes: ",
					new string[] {
						EUICS2SpecificCircumstanceList.Codes.F14,
						EUICS2SpecificCircumstanceList.Codes.F15,
						EUICS2SpecificCircumstanceList.Codes.F16,
						EUICS2SpecificCircumstanceList.Codes.F17,
						EUICS2SpecificCircumstanceList.Codes.F43,
						EUICS2SpecificCircumstanceList.Codes.F44,
					},
					list.GetAllCodes()
				);
				var anotherHeader = Factory.New<AsycudaManifestHeader>();
				anotherHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				anotherHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				AssertSame("List is cached", list, anotherHeader.Lookups.SpecificCircumstanceList);
			});
		}

		public void TestSpecificCircumstanceList_VOC_SEA()
		{
			CombineAssertions("SpecificCircumstanceList under TransportMode SEA", () =>
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

				var list = header.Lookups.SpecificCircumstanceList;
				AssertContainsExactElementsInAnyOrder(
					"Should contain the following codes: ",
					new string[] {
						EUICS2SpecificCircumstanceList.Codes.F10,
						EUICS2SpecificCircumstanceList.Codes.F11,
						EUICS2SpecificCircumstanceList.Codes.F12,
						EUICS2SpecificCircumstanceList.Codes.F13,
					},
					list.GetAllCodes()
				);
				var anotherHeader = Factory.New<AsycudaManifestHeader>();
				anotherHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				anotherHeader.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				AssertSame("List is cached", list, anotherHeader.Lookups.SpecificCircumstanceList);
			});
		}

		public void TestSpecificCircumstanceList_TransportModeAir()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				header.AMA_TransportMode = Core.Constants.TransportModes.Air;

				var list = header.Lookups.SpecificCircumstanceList;
				AssertContainsExactElementsInExactOrder(new []
				{
					EUICS2SpecificCircumstanceList.Codes.F22,
					EUICS2SpecificCircumstanceList.Codes.F23,
					EUICS2SpecificCircumstanceList.Codes.F24,
					EUICS2SpecificCircumstanceList.Codes.F25,
					EUICS2SpecificCircumstanceList.Codes.F26,
					EUICS2SpecificCircumstanceList.Codes.F43,
					EUICS2SpecificCircumstanceList.Codes.F44,
				}, list.GetAllCodes());
				AssertSame("List is cached", list, header.Lookups.SpecificCircumstanceList);
			});
		}

		public void TestSpecificCircumstanceList_NVC_IWT()
		{
			CombineAssertions("SpecificCircumstanceList under TransportMode IWT", () =>
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				header.AMA_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;

				var list = header.Lookups.SpecificCircumstanceList;
				AssertContainsExactElementsInAnyOrder(
					"Should contain the following codes: ",
					new string[] {
						EUICS2SpecificCircumstanceList.Codes.F14,
						EUICS2SpecificCircumstanceList.Codes.F15,
						EUICS2SpecificCircumstanceList.Codes.F16,
						EUICS2SpecificCircumstanceList.Codes.F17,
						EUICS2SpecificCircumstanceList.Codes.F43,
						EUICS2SpecificCircumstanceList.Codes.F44,
					},
					list.GetAllCodes()
				);

				var anotherHeader = Factory.New<AsycudaManifestHeader>();
				anotherHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				anotherHeader.AMA_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
				AssertSame("List is cached", list, anotherHeader.Lookups.SpecificCircumstanceList);
			});
		}

		public void TestSpecificCircumstanceList_VOC_IWT()
		{
			CombineAssertions("SpecificCircumstanceList under TransportMode IWT", () =>
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				header.AMA_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;

				var list = header.Lookups.SpecificCircumstanceList;
				AssertContainsExactElementsInAnyOrder(
					"Should contain the following codes: ",
					new string[] {
						EUICS2SpecificCircumstanceList.Codes.F10,
						EUICS2SpecificCircumstanceList.Codes.F11,
						EUICS2SpecificCircumstanceList.Codes.F12,
						EUICS2SpecificCircumstanceList.Codes.F13,
					},
					list.GetAllCodes()
				);

				var anotherHeader = Factory.New<AsycudaManifestHeader>();
				anotherHeader.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				anotherHeader.AMA_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
				AssertSame("List is cached", list, anotherHeader.Lookups.SpecificCircumstanceList);
			});
		}

		public void TestSpecificCircumstanceList_TransportModeROA()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				header.AMA_TransportMode = Core.Constants.TransportModes.Road;

				var list = header.Lookups.SpecificCircumstanceList;
				AssertContainsExactElementsInAnyOrder(new[] { EUICS2SpecificCircumstanceList.Codes.F50, EUICS2SpecificCircumstanceList.Codes.F40 }, list.GetAllCodes());
				AssertSame("List is cached", list, header.Lookups.SpecificCircumstanceList);

				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				list = header.Lookups.SpecificCircumstanceList;
				AssertContainsExactElementsInAnyOrder(new string[] { EUICS2SpecificCircumstanceList.Codes.F43, EUICS2SpecificCircumstanceList.Codes.F44 }, list.GetAllCodes());
				AssertSame("List is cached", list, header.Lookups.SpecificCircumstanceList);
			});
		}

		public void TestSpecificCircumstanceList_TransportModeRAIL()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				header.AMA_TransportMode = Core.Constants.TransportModes.Rail;

				var list = header.Lookups.SpecificCircumstanceList;
				AssertContainsExactElementsInAnyOrder(new string[] { EUICS2SpecificCircumstanceList.Codes.F41, EUICS2SpecificCircumstanceList.Codes.F51 }, list.GetAllCodes());
				AssertSame("List is cached", list, header.Lookups.SpecificCircumstanceList);

				header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				list = header.Lookups.SpecificCircumstanceList;
				AssertContainsExactElementsInAnyOrder(new string[] { EUICS2SpecificCircumstanceList.Codes.F43, EUICS2SpecificCircumstanceList.Codes.F44 }, list.GetAllCodes());
				AssertSame("List is cached", list, header.Lookups.SpecificCircumstanceList);
			});
		}

		public void TestMessageStatusList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var list = header.Lookups.MessageStatusList;

			AssertSame("Accessing the list twice should get the exact same object as the list is cached", list, header.Lookups.MessageStatusList);
		}

		public void TestMOTIdentifierTypeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var list = header.Lookups.MOTIdentifierTypeList;
			AssertEquals(true, list.ContainsCode("10"));
			AssertEquals(true, list.ContainsCode("20"));
			AssertEquals(true, list.ContainsCode("21"));
			AssertEquals(true, list.ContainsCode("30"));
			AssertEquals(true, list.ContainsCode("31"));
			AssertEquals(true, list.ContainsCode("41"));
			AssertEquals(true, list.ContainsCode("80"));

			AssertSame("Accessing the list twice should get the exact same object as the list is cached", list, header.Lookups.MOTIdentifierTypeList);
		}

		public void TestCountryCodeICS2MS()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var europeanUnionCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

			helper.CreateNewOrGetExistingDataGrouping(europeanUnionCode);

			var codeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2MS;
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, codeType, "QQ", "Q Continuum", startDate, endDate);

			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var list = header.Lookups.CountryCodeICS2MS;

			CombineAssertions(() =>
			{
				AssertEquals(true, list.ContainsCode("QQ"));
				AssertSame("Cached", list, header.Lookups.CountryCodeICS2MS);
			});
		}

		public void TestEmptyCustomsProfileList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var list = header.Lookups.CustomsProfileList;
			AssertEquals("Empty list expected", 0, list.Count);
		}

		public void TestCustomsProfileList()
		{
			CreateTestCompany("TC1", "Test Company 1", true, false);
			CreateTestCompany("TC2", "Test Company 2", false, false);
			CreateTestCompany("TC3", "Test Company 3", false, false);
			CreateTestCompany("TC4", "Test Company 4", true, true);
			CreateTestCompany("TC5", "Test Company 5", true, false);
			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header.AMA_ManifestType = "ENS";
			var list = header.Lookups.CustomsProfileList;
			AssertEquals("Valid ICS2 Companies list expected", 2, list.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "TC1", "TC5" }, list.OfType<ICodeDescription>().Select(x => x.Code));
			AssertContainsExactElementsInAnyOrder(new[] { "Test Company 1", "Test Company 5" }, list.OfType<ICodeDescription>().Select(x => x.Description));
		}

		public void TestTransportModeList()
		{
			CombineAssertions(() =>
			{
				using (SetICS2TransportModeROA(isActive: true))
				using (SetICS2TransportModeRAI(isActive: true))
				{
					var header = Factory.New<AsycudaManifestHeader>();
					header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
					var list = header.Lookups.TransportModeList;
					AssertContainsExactElementsInAnyOrder(new string[] { Core.Constants.TransportModes.Air, Core.Constants.TransportModes.Road, Core.Constants.TransportModes.Sea, Core.Constants.TransportModes.InlandWaterwayTransport, Core.Constants.TransportModes.Rail }, list.GetAllCodes());

					header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
					list = header.Lookups.TransportModeList;
					AssertContainsExactElementsInAnyOrder(
						"TransportModeList should contain ROA, SEA, IWT, RAI",
						new string[] {
							Core.Constants.TransportModes.Road,
							Core.Constants.TransportModes.Rail,
							Core.Constants.TransportModes.InlandWaterwayTransport,
							Core.Constants.TransportModes.Sea
						},
						list.GetAllCodes()
					);
				}
			});
		}

		public void TestTransportModeList_ROAAndRAIDisabled()
		{
			using var roaOff = SetICS2TransportModeROA(isActive: false);
			using var raiOff = SetICS2TransportModeRAI(isActive: false);

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			var list = header.Lookups.TransportModeList;
			AssertContainsExactElementsInAnyOrder(
				"TransportModeList should contain ",
				new string[] {
					Core.Constants.TransportModes.Air,
					Core.Constants.TransportModes.Sea,
					Core.Constants.TransportModes.InlandWaterwayTransport,
				},
				list.GetAllCodes()
			);
		}

		public void TestMeansOfTransportTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var startDate = ZDateTime.Today.AddYears(-2);
			var endDate = ZDateTime.Today.AddYears(2);
			var europeanUnionCode = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;

			var eunId = helper.CreateNewOrGetExistingDataGrouping(europeanUnionCode);

			var codeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_IC2MT;
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, codeType, "150", "General cargo vessel Vessel designed to carry general cargo", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, codeType, "151", "Unit carrier Vessel designed to carry unit loads", startDate, endDate);
			helper.CreateNewOrGetExistingCusCodeList(europeanUnionCode, codeType, "152", "Bulk carrier Vessel designed to carry bulk cargo", startDate, endDate);

			Factory.Save();

			var header = Factory.New<AsycudaManifestHeader>();
			var list = header.Lookups.MeansOfTransportTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("150, 151, 152", list.CodesAsString);
				AssertSame("Cached", list, header.Lookups.MeansOfTransportTypeList);
			});
		}

		public void TestRegistrationStatusList()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<AsycudaManifestHeader>();
				header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
				var list = header.Lookups.RegistrationStatusList;

				AssertEquals("ACP, ADD, AEO, ARV, ASC, CAN, CNR, DNL, HRC, INS, NCN, PND, RAI, RAR, REG, RHR, RIR, VAL", list.CodesAsString);
				AssertSame("Accessing the list twice should get the exact same object as the list is cached", list, header.Lookups.RegistrationStatusList);
			});
		}

		public void TestCustomsOffices()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Rail;
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, "Italy", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004323", "GERMAN OFFICE1", ZDateTime.BrettsBirthday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004324", "GERMAN OFFICE2", ZDateTime.BrettsBirthday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE004325", "GERMAN OFFICE3", ZDateTime.BrettsBirthday, tomorrow);

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT004323", "Italy OFFICE1", ZDateTime.BrettsBirthday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT004324", "Italy OFFICE2", ZDateTime.BrettsBirthday, tomorrow);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT004325", "Italy OFFICE3", ZDateTime.BrettsBirthday, tomorrow);
			Factory.Save();

			var coll = header.Lookups.CustomsOffices as ZZRefCusCodeListCombinedCollection;
			coll.Load();

			AssertContainsExactElementsInAnyOrder(new[] { "DE004323","DE004324", "DE004325", "IT004323", "IT004324", "IT004325" }, coll.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		public void TestDeclarantAddresses()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			var declarantAddresses = header.Lookups.DeclarantAddresses;

			CombineAssertions(() =>
			{
				AssertType<OrganisationsFindBoxCollection>(declarantAddresses);
				AssertSame("Cached", declarantAddresses, header.Lookups.DeclarantAddresses);
			});
		}

		public void TestPaymentMethodList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var list = header.Lookups.PaymentMethodList;

			AssertSame("Accessing the list twice should get the exact same object as the list is cached", list, header.Lookups.PaymentMethodList);
			AssertType<EUICS2PaymentMethodList>(list);
			AssertEquals("A, B, C, D, H, Y, Z", list.CodesAsString);
		}

		GlbCompany CreateTestCompany(string companyCode, string companyName, bool createICSCertificate, bool certificateExpired)
		{
			var testCompany = Factory.NewWithValidTestData<GlbCompany>();
			testCompany.GC_Code = companyCode;
			testCompany.GC_Name = companyName;

			if (createICSCertificate)
			{
				var ics2Certificate = Factory.NewWithValidTestData<GlbCompanyCredentialICS2>();
				ics2Certificate.GP_GC = testCompany.PK;
				ics2Certificate.GP_PasswordType = PasswordTypesList.Codes.IC2;
				ics2Certificate.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
				ics2Certificate.GP_MailBoxID = "DE00528829548";
				if (certificateExpired)
				{
					ics2Certificate.GP_ExpiryDate = ZDateTime.Today.AddDays(-15);
				}
				else
				{
					ics2Certificate.GP_ExpiryDate = ZDateTime.Today.AddYears(1);
				}
			}

			return testCompany;
		}

		protected IDisposable SetICS2TransportModeROA(bool isActive) =>
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.ICS2TransportModeROA, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isActive);

		protected IDisposable SetICS2TransportModeRAI(bool isActive) =>
			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.ICS2TransportModeRAI, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isActive);
	}
}
