using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;
using UniversalReferenceConstants = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(DEPDATHeaderProvider))]
	sealed class DEPDATHeaderProviderTest : NCTSHeaderProviderAbstractTest<DEPDATHeaderProvider>
	{
		public void TestLRN()
		{
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "19DE587500026775M6";
			AssertEquals("Contents", "19DE587500026775M6", HeaderProvider.LRN);
		}

		public void TestDeclarationType() => CombineAssertions(() =>
		{
			var item1 = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			item1.BY_Type = NctsDeclarationTypeList.Codes.T2F;

			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
			AssertEquals("Type is  T1", NctsDeclarationTypeList.Codes.T1, GetHeaderProvider().DeclarationType);

			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T;
			AssertEquals("Type is T", NctsDeclarationTypeList.Codes.T, GetHeaderProvider().DeclarationType);
		});

		public void TestTransitDeclarationType() => CombineAssertions(() =>
		{
			nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = false;
			AssertEquals("checkbox false", "00", GetHeaderProvider().TransitDeclarationType);

			nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = true;
			AssertEquals("checkbox true", "10", GetHeaderProvider().TransitDeclarationType);

			nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = true;
			nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew().AGC_Code = "SSE";
			AssertEquals("checkbox true and SSE", "11", GetHeaderProvider().TransitDeclarationType);
		});

		public void TestTIRCarnetNumber() => CombineAssertions(() =>
		{
			AssertNull("None", HeaderProvider.TIRCarnetNumber);

			nctsHeader.MovementHeader.TirCarnetNumber = "19DE265655002905M6";
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
			AssertEquals("TIR", "19DE265655002905M6", HeaderProvider.TIRCarnetNumber);
		});

		public void TestLimitDate() => CombineAssertions(() =>
		{
			nctsHeader.MovementHeader.BM_ExportDate = new ZDateTime(2023, 01, 01);
			nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = false;
			AssertEquals("00", null, GetHeaderProvider().LimitDate);

			nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = true;
			AssertEquals("10", new DateTime(2023, 01, 01), GetHeaderProvider().LimitDate);

			nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = true;
			nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew().AGC_Code = "SSE";
			AssertEquals("11", new DateTime(2023, 01, 01), GetHeaderProvider().LimitDate);
		});

		public void TestSecurity() => CombineAssertions(() =>
		{
			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertEquals(NctsTypeOfSecurityList.Codes.NON, "0", HeaderProvider.Security);

			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertEquals(NctsTypeOfSecurityList.Codes.ENT, "1", HeaderProvider.Security);

			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			AssertEquals(NctsTypeOfSecurityList.Codes.EXI, "2", HeaderProvider.Security);

			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
			AssertEquals(NctsTypeOfSecurityList.Codes.BTH, "3", HeaderProvider.Security);

			nctsHeader.MovementHeader.BM_TypeOfSecurity = "INV"; // Invalid
			AssertEquals("Invalid", null, HeaderProvider.Security);
		});

		public void TestReducesDataSet() => CombineAssertions(() =>
		{
			nctsHeader.MovementHeader.BM_ReducedDatasetIndicator = true;
			AssertEquals("true", "1", HeaderProvider.ReducesDataSet);

			nctsHeader.MovementHeader.BM_ReducedDatasetIndicator = false;
			AssertEquals("false", "0", HeaderProvider.ReducesDataSet);
		});

		public void TestSpecificCircumstance() => CombineAssertions(() =>
		{
			nctsHeader.MovementHeader.BM_SpecificCircumstance = NctsSpecificCircumstanceIndicatorList.Codes.A20;
			AssertEquals($"{nameof(CusInBondMoveHeader.BM_SpecificCircumstance)} == 'A20'", NctsSpecificCircumstanceIndicatorList.Codes.A20,
				HeaderProvider.SpecificCircumstance);
			nctsHeader.MovementHeader.BM_SpecificCircumstance = NctsSpecificCircumstanceIndicatorList.Codes.XXX;
			AssertEquals($"{nameof(CusInBondMoveHeader.BM_SpecificCircumstance)} == 'XXX'", NctsSpecificCircumstanceIndicatorList.Codes.XXX,
				HeaderProvider.SpecificCircumstance);
		});

		public void TestBindingItinerary() => CombineAssertions(() =>
		{
			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertEquals($"{nameof(CusInBondMoveHeader.BM_TypeOfSecurity)} == 'NON' no record", "0", GetHeaderProvider().BindingItinerary);

			var cor = nctsHeader.CountriesOfRouting.AddNew();
			cor.CY_Data = "FR";

			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertEquals($"{nameof(CusInBondMoveHeader.BM_TypeOfSecurity)} != 'NON' no record", "0", GetHeaderProvider().BindingItinerary);

			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertEquals($"{nameof(CusInBondMoveHeader.BM_TypeOfSecurity)} == 'NON' && record", "1",
				GetHeaderProvider().BindingItinerary);
		});

		public void TestAuthorisations() => CombineAssertions(() =>
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, MapDirectionList.Codes.OUT, "EUNAU", true);
			helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit, "C521", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.SpecialSeals, "C523", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateCusMap(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_EUNAU, CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset, "C524", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			Factory.Save();

			var provider = HeaderProvider;
			nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew().AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsignorTransit;
			AssertEquals("Count 1", 1, provider.Authorisations.Count);
			AssertCollectionContains("ACR -> C521", "C521", provider.Authorisations.Select(e => e.Type));

			provider = GetHeaderProvider();
			nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew().AGC_Code = CusAuthorizationHeaderTypeList.Codes.SpecialSeals;
			AssertEquals("Count 2", 2, provider.Authorisations.Count);
			AssertCollectionContains("SSE -> C523", "C523", provider.Authorisations.Select(e => e.Type));

			provider = GetHeaderProvider();
			nctsHeader.MovementHeader.CusAuthorizationUsages.AddNew().AGC_Code = CusAuthorizationHeaderTypeList.Codes.TransitReducedDataset;
			AssertEquals("Count 3", 3, provider.Authorisations.Count);
			AssertCollectionContains("TRD -> C524", "C524", provider.Authorisations.Select(e => e.Type));
		});

		public void TestDepartureOffice()
		{
			movementHeader.CustomsOffices.Single<NctsEuOfficeCode>(e => e.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDeparture).CY_Data = "DE000001";
			movementHeader.CustomsOffices.Single<NctsEuOfficeCode>(e => e.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDestination).CY_Data = "DE000002";

			AssertEquals(EuOfficeCodesTypes.Codes.OfficeOfDeparture, "DE000001", HeaderProvider.DepartureOffice);
		}

		public void TestDepartureOffice_WhenNoDepartureOffice()
		{
			movementHeader.CustomsOffices.RemoveRange(movementHeader.CustomsOffices.Where(e => e.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDeparture).ToArray());

			AssertNull(HeaderProvider.DepartureOffice);
		}

		public void TestDestinationOffice()
		{
			movementHeader.CustomsOffices.Single<NctsEuOfficeCode>(e => e.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDeparture).CY_Data = "DE000001";
			movementHeader.CustomsOffices.Single<NctsEuOfficeCode>(e => e.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDestination).CY_Data = "DE000002";

			AssertEquals(EuOfficeCodesTypes.Codes.OfficeOfDestination, "DE000002", HeaderProvider.DestinationOffice);
		}

		public void TestDestinationOffice_WhenNoDestinationOffice()
		{
			movementHeader.CustomsOffices.RemoveRange(movementHeader.CustomsOffices.Where(e => e.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDestination).ToArray());

			AssertNull(HeaderProvider.DestinationOffice);
		}

		public void TestTransitOffices() => CombineAssertions(() =>
		{
			movementHeader.CustomsOffices.Single<NctsEuOfficeCode>(e => e.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDeparture).CY_Data = "DE000001";
			movementHeader.CustomsOffices.Single<NctsEuOfficeCode>(e => e.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDestination).CY_Data = "DE000002";
			movementHeader.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfTransit, "DE000003");
			movementHeader.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfTransit, "DE000004");

			movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
			AssertEquals("TIR", 0, HeaderProvider.TransitOffices.Count);

			movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T;
			AssertContainsExactElementsInAnyOrder("Non TIR", new[] { "DE000003", "DE000004" },
				GetHeaderProvider().TransitOffices.Select(e => e.ReferenceNumber));
		});

		public void TestExitOffices() => CombineAssertions(() =>
		{
			movementHeader.CustomsOffices.Single<NctsEuOfficeCode>(e => e.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDeparture).CY_Data = "DE000001";
			movementHeader.CustomsOffices.Single<NctsEuOfficeCode>(e => e.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDestination).CY_Data = "DE000002";
			movementHeader.CustomsOffices.AddNew("TXT", "DE000003");
			movementHeader.CustomsOffices.AddNew("TXT", "DE000004");

			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			AssertEquals(NctsTypeOfSecurityList.Codes.NON, 0, HeaderProvider.ExitOffices.Count);

			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
			AssertEquals(NctsTypeOfSecurityList.Codes.ENT, 0, GetHeaderProvider().ExitOffices.Count);

			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
			AssertContainsExactElementsInAnyOrder(NctsTypeOfSecurityList.Codes.BTH, new[] { "DE000003", "DE000004" },
				GetHeaderProvider().ExitOffices);

			movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			AssertContainsExactElementsInAnyOrder(NctsTypeOfSecurityList.Codes.EXI, new[] { "DE000003", "DE000004" },
				GetHeaderProvider().ExitOffices);
		});

		public void TestHolderOfTransitProcedure()
		{
			AssertType<NCTSPartyIDAddressContactProvider>(HeaderProvider.HolderOfTransitProcedure);
		}

		public void TestHolderOfTransitProcedureTIRIdentification() => CombineAssertions(() =>
		{
			nctsHeader.Principal.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			nctsHeader.Principal.Organisation.CustomsCodes.AddNew(NctsDeclarationTypeList.Codes.TIR, "DE101");
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T;
			AssertNull($"{nameof(DEPDATHeaderProvider.HolderOfTransitProcedureTIRIdentification)} When not TIR",
				GetHeaderProvider().HolderOfTransitProcedureTIRIdentification);

			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
			AssertEquals($"{nameof(DEPDATHeaderProvider.HolderOfTransitProcedureTIRIdentification)} When TIR", "DE101",
				GetHeaderProvider().HolderOfTransitProcedureTIRIdentification);
		});

		public void TestHolderOfTransitProcedure_NoContactPersonWhenRepresentativeExists() => CombineAssertions(() =>
		{
			nctsHeader.Principal.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			Assert("Prerequisite", !nctsHeader.DocAddresses.Cast<JobDocAddress>().Any(x => x.E2_AddressType == "REP"));

			var provider = GetHeaderProvider();
			AssertEquals("Name", "CargoWise Support", provider.HolderOfTransitProcedure.Name);

			AddDocAddress("REP");
			provider = GetHeaderProvider();
			AssertNull("Name null", provider.HolderOfTransitProcedure.Name);
		});

		public void TestHolderOfTheTransitProcedure_SelectedContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			nctsHeader.Principal.OrganisationPK = org.PK;

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "TEST";
			contact.OC_Phone = "+123 234 345";
			contact.OC_Email = "test@test.com";

			nctsHeader.Principal.E2_Contact = "TEST";

			var principal = Provider.HolderOfTransitProcedure;
			AssertEquals("TEST", principal.Name);
			AssertEquals("+123 234 345", principal.PhoneNumber);
			AssertEquals("test@test.com", principal.MailAddress);
		}

		public void TestHolderOfTheTransitProcedure_OverriddenAddressUsed()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			nctsHeader.Principal.OrganisationPK = org.PK;

			var docAddress = nctsHeader.Principal;

			docAddress.E2_AddressOverride = true;

			docAddress.E2_Contact = "TEST";
			docAddress.E2_Phone = "+123 234 345";
			docAddress.E2_Email = "test@test.com";

			var principal = Provider.HolderOfTransitProcedure;
			AssertEquals("TEST", principal.Name);
			AssertEquals("+123 234 345", principal.PhoneNumber);
			AssertEquals("test@test.com", principal.MailAddress);
		}

		public void TestRepresentative() => CombineAssertions(() =>
		{
			AddDocAddress("REP");
			AssertType<NCTSPartyIDContactProvider>(HeaderProvider.Representative);
			AssertEquals("Name", "CargoWise Support", HeaderProvider.Representative.Name);
		});

		public void TestRepresentative_HasTcu()
		{
			AddDocAddress("REP", addEori: false, addTcu: true);
			AssertType<NCTSPartyIDContactProvider>(HeaderProvider.Representative);
		}

		public void TestRepresentative_NullIfNoEoriOrTcu()
		{
			AddDocAddress("REP", addEori: false, addTcu: false);
			AssertNull(HeaderProvider.Representative);
		}

		public void TestGuarantees_8BR() => CombineAssertions(() =>
		{
			foreach (var type in new[] { "8", "B", "R" })
			{
				nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
				nctsHeader.BH_ApplicationCode = Enterprise.Customs.Common.CusInBondApplicationCodeList.Codes.NCTS5;

				var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
				guarantee1.PW_BondType = type;
				var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
				guarantee2.PW_BondType = type;
				var provider = GetHeaderProvider();
				AssertEquals($"Type {type} Count", 1, provider.Guarantees.Count);
				AssertEquals($"Type {type} references Count", 0, HeaderProvider.Guarantees.First().References.Count);
			}
		});

		public void TestGuarantees_B() => CombineAssertions(() =>
		{
			var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondType = "B";
			var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee2.PW_BondType = "B";
			AssertEquals("Count", 1, HeaderProvider.Guarantees.Count);
			AssertEquals("References Count", 0, HeaderProvider.Guarantees.First().References.Count);
		});

		public void TestGuarantees_0124() => CombineAssertions(() =>
		{
			foreach (var type in new[] { "0", "1", "2", "4" })
			{
				var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
				guarantee1.PW_BondType = type;
				var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
				guarantee2.PW_BondType = type;
			}
			AssertEquals("Count", 4, HeaderProvider.Guarantees.Count);
			AssertEquals($"Type 0 references count", 2, HeaderProvider.Guarantees.SingleOrDefault(x => x.Type == "0").References.Count);
			AssertEquals($"Type 1 references count", 2, HeaderProvider.Guarantees.SingleOrDefault(x => x.Type == "1").References.Count);
			AssertEquals($"Type 2 references count", 2, HeaderProvider.Guarantees.SingleOrDefault(x => x.Type == "2").References.Count);
			AssertEquals($"Type 4 references count", 2, HeaderProvider.Guarantees.SingleOrDefault(x => x.Type == "4").References.Count);
		});

		public void TestGuarantees_3() => CombineAssertions(() =>
		{
			var guarantee1 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondType = "3";
			var guarantee2 = nctsHeader.MovementHeader.Guarantees.AddNew();
			guarantee2.PW_BondType = "3";
			var provider = GetHeaderProvider();
			AssertEquals("Count", 2, HeaderProvider.Guarantees.Count);
			AssertEquals("First references count", 1, HeaderProvider.Guarantees.First().References.Count);
			AssertEquals("Last references count", 1, HeaderProvider.Guarantees.Last().References.Count);
		});

		public void TestCountryOfDispatch() => CombineAssertions(() =>
		{
			nctsHeader.MovementHeader.BM_RN_NKCountryOfDispatch = "DE";
			var bill1 = nctsHeader.Bills.AddNew();
			var bill2 = nctsHeader.Bills.AddNew();
			var item1 = bill1.GoodsItems.AddNew();
			var item2 = bill2.GoodsItems.AddNew();
			var provider = HeaderProvider;
			AssertEquals("CountryOfDispatch all empty", "DE", provider.CountryOfDispatch);

			bill1.B0_RN_NKCountryOfExport = "DE";
			item2.BY_RN_NKCountryOfDispatch = "DE";
			provider = GetHeaderProvider();
			AssertEquals("CountryOfDispatch all equal", "DE", provider.CountryOfDispatch);
			AssertSame("Cached", provider.CountryOfDispatch, provider.CountryOfDispatch);

			item1.BY_RN_NKCountryOfDispatch = "FR";
			provider = GetHeaderProvider();
			AssertNull("CountryOfDispatch item different", provider.CountryOfDispatch);

			item1.BY_RN_NKCountryOfDispatch = "DE";
			item2.BY_RN_NKCountryOfDispatch = ZString.Empty;
			bill2.B0_RN_NKCountryOfExport = "FR";
			provider = GetHeaderProvider();
			AssertNull("CountryOfDispatch bill different", provider.CountryOfDispatch);
		});

		public void TestCountryOfDestination() => CombineAssertions(() =>
		{
			nctsHeader.MovementHeader.BM_RL_NKDestinationPort = "DE";
			var bill1 = nctsHeader.Bills.AddNew();
			var bill2 = nctsHeader.Bills.AddNew();
			var item1 = bill1.GoodsItems.AddNew();
			var item2 = bill2.GoodsItems.AddNew();
			var provider = HeaderProvider;
			AssertEquals("CountryOfDestination all empty", "DE", provider.CountryOfDestination);

			bill1.B0_RN_NKCountryOfDestination = "DE";
			item2.BY_RN_NKCountryOfDestination = "DE";
			provider = GetHeaderProvider();
			AssertEquals("CountryOfDestination all equal", "DE", provider.CountryOfDestination);
			AssertSame("Cached", provider.CountryOfDestination, provider.CountryOfDestination);

			item1.BY_RN_NKCountryOfDestination = "FR";
			provider = GetHeaderProvider();
			AssertNull("CountryOfDestination item empty", provider.CountryOfDestination);

			item1.BY_RN_NKCountryOfDestination = "DE";
			item2.BY_RN_NKCountryOfDestination = ZString.Empty;
			bill2.B0_RN_NKCountryOfDestination = "FR";
			provider = GetHeaderProvider();
			AssertNull("CountryOfDestination bill different", provider.CountryOfDestination);
		});

		public void TestContainerIndicator() => CombineAssertions(() =>
		{
			AssertEquals("ContainerIndicator none", false, HeaderProvider.ContainerIndicator);

			var container1 = nctsHeader.DepartureHeaderContainers.AddNew();
			container1.BC_Mode = "NCT";
			AssertEquals("ContainerIndicator NCT", false, HeaderProvider.ContainerIndicator);

			var container2 = nctsHeader.DepartureHeaderContainers.AddNew();
			container2.BC_Mode = "CNT";
			AssertEquals("ContainerIndicator CNT", true, HeaderProvider.ContainerIndicator);
		});

		public void TestInlandModeOfTransport() => CombineAssertions(() =>
		{
			nctsHeader.MovementHeader.BM_InlandTransportMode = string.Empty;
			AssertEquals("InlandModeOfTransport empty", null, HeaderProvider.InlandModeOfTransport);

			nctsHeader.MovementHeader.BM_InlandTransportMode = "1";
			AssertEquals("InlandModeOfTransport 1", "1", HeaderProvider.InlandModeOfTransport);
		});

		public void TestModeOfTransportAtBorder_Security() => CombineAssertions(() =>
		{
			nctsHeader.MovementHeader.BM_ExportTransportMode = "1";
			nctsHeader.MovementHeader.BM_TypeOfSecurity = "NON";
			AssertEquals("ModeOfTransportAtBorder no security", null, HeaderProvider.ModeOfTransportAtBorder);

			nctsHeader.MovementHeader.BM_TypeOfSecurity = "ENT";
			AssertEquals("ModeOfTransportAtBorder security ENT", "1", HeaderProvider.ModeOfTransportAtBorder);

			nctsHeader.MovementHeader.BM_TypeOfSecurity = "EXI";
			AssertEquals("ModeOfTransportAtBorder security EXI", "1", HeaderProvider.ModeOfTransportAtBorder);
		});

		public void TestModeOfTransportAtBorder_Office() => CombineAssertions(() =>
		{
			movementHeader.BM_ExportTransportMode = "1";
			movementHeader.BM_TypeOfSecurity = "NON";
			AssertEquals("ModeOfTransportAtBorder no office", null, HeaderProvider.ModeOfTransportAtBorder);

			var office1 = movementHeader.CustomsOffices.AddNew();
			office1.CY_Code = "DEP";
			AssertEquals("ModeOfTransportAtBorder office DEP", null, HeaderProvider.ModeOfTransportAtBorder);

			var office2 = movementHeader.CustomsOffices.AddNew();
			office2.CY_Code = "TRA";
			AssertEquals("ModeOfTransportAtBorder office TRA", "1", HeaderProvider.ModeOfTransportAtBorder);
		});

		public void TestGrossMass()
		{
			nctsHeader.MovementHeader.BM_GrossWeight = 1234.5678;
			AssertEquals(1234.568m, HeaderProvider.GrossMass);
		}

		public void TestGrossMass_Normalize()
		{
			nctsHeader.MovementHeader.BM_GrossWeight = 1.400m;
			AssertEquals("1.4", HeaderProvider.GrossMass.ToString());
		}

		public void TestReferenceNumberUCR() => CombineAssertions(() =>
		{
			var provider = HeaderProvider;
			nctsHeader.MovementHeader.BM_UniqueConsignmentReference = "UQREF123";
			AssertNull("ReferenceNumberUCR no bills", provider.ReferenceNumberUCR);

			var bill1 = nctsHeader.Bills.AddNew();
			var bill2 = nctsHeader.Bills.AddNew();
			var item1 = bill1.GoodsItems.AddNew();
			var item2 = bill2.GoodsItems.AddNew();
			provider = GetHeaderProvider();
			AssertEquals("ReferenceNumberUCR all empty", "UQREF123", provider.ReferenceNumberUCR);

			bill1.B0_ReferenceID = "UQREF123";
			item2.BY_CommercialReferenceNumber = "UQREF123";
			provider = GetHeaderProvider();
			AssertEquals("ReferenceNumberUCR all empty or equal", "UQREF123", provider.ReferenceNumberUCR);

			item1.BY_CommercialReferenceNumber = "ABC123";
			provider = GetHeaderProvider();
			AssertNull("ReferenceNumberUCR item different", provider.ReferenceNumberUCR);

			item1.BY_CommercialReferenceNumber = "UQREF123";
			bill2.B0_ReferenceID = "ABC123";
			item2.BY_CommercialReferenceNumber = ZString.Empty;
			provider = GetHeaderProvider();
			AssertNull("ReferenceNumberUCR bill different", provider.ReferenceNumberUCR);

			item1.BY_CommercialReferenceNumber = "ABC123";
			item2.BY_CommercialReferenceNumber = ZString.Empty;
			bill1.B0_ReferenceID = ZString.Empty;
			bill2.B0_ReferenceID = "ABC123";
			provider = GetHeaderProvider();
			AssertEquals("ReferenceNumberUCR all equal header different", "ABC123", provider.ReferenceNumberUCR);
			AssertSame("Cached", provider.ReferenceNumberUCR, provider.ReferenceNumberUCR);
		});

		public void TestCarrier_Null()
		{
			AssertNull("Carrier null", HeaderProvider.Carrier);
		}

		public void TestCarrier() => CombineAssertions(() =>
		{
			DE.Business.Testing.TestHelper.CreateCL010CoutryList(Factory);
			AddDocAddress("CAR", addEori: true);

			AssertNotNull("Carrier Populated", HeaderProvider.Carrier);
			AssertEquals("Carrier Eori", "FR111111", HeaderProvider.Carrier.EoriNumber);
			AssertSame("Cached", HeaderProvider.Carrier, HeaderProvider.Carrier);
		});

		public void TestCarrier_NullIfNoEoriOrTcu() => CombineAssertions(() =>
		{
			AddDocAddress("CAR", addEori: false);
			AssertNull("No Eori or Tcu", HeaderProvider.Carrier);

			AddDocAddress("CAR", addEori: false, addTcu: true);
			AssertNotNull("Has Tcu", GetHeaderProvider().Carrier);
		});

		public void TestConsignor_Null()
		{
			AssertNull("Consignor null", HeaderProvider.Consignor);
		}

		public void TestConsignor_ContactFromAllocation()
		{
			nctsHeader.Bills.AddNew().GoodsItems.AddNew();

			var jobAddress = nctsHeader.DocAddresses.AddNew();
			jobAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgAddress>().PK;
			jobAddress.E2_AddressType = "CRD";

			var contact = jobAddress.Organisation.Contacts.AddNew();
			contact.OC_ContactName = "TEST";
			contact.OC_Phone = "+49 1234 1234";
			contact.OC_Email = "bill@test.com";
			contact.Allocations.AddNew().PC_Type = "CUS";

			AssertEquals("TEST", HeaderProvider.Consignor.Name);
			AssertEquals("+49 1234 1234", HeaderProvider.Consignor.PhoneNumber);
			AssertEquals("bill@test.com", HeaderProvider.Consignor.MailAddress);
		}

		public void TestConsignor_BillsWithoutConsignor()
		{
			AddConsignorAndBills();
			AssertNotNull("Consignor", HeaderProvider.Consignor);
		}

		public void TestConsignor_SomeBillsWithConsignor()
		{
			var (address1, address2PK, bill1, bill2) = AddConsignorAndBills();

			bill1.Consignor.E2_OA_Address = address1.PK;
			AssertNotNull("Consignor", HeaderProvider.Consignor);
		}

		public void TestConsignor_DifferentConsignors()
		{
			var (address1, address2, bill1, bill2) = AddConsignorAndBills();

			bill1.Consignor.E2_OA_Address = address1.PK;
			bill2.Consignor.E2_OA_Address = address2.PK;
			address2.City = "Bigcity";
			AssertNull("Consignor", HeaderProvider.Consignor);
		}

		public void TestConsignor_SameConsignorsOnBills()
		{
			var (address1, _, bill1, bill2) = AddConsignorAndBills();

			bill1.Consignor.E2_OA_Address = address1.PK;
			bill2.Consignor.E2_OA_Address = address1.PK;
			AssertNotNull("Consignor", HeaderProvider.Consignor);
			AssertSame("Cached", HeaderProvider.Consignor, HeaderProvider.Consignor);
		}

		(OrgAddress address1, OrgAddress address2, NctsBill bill1, NctsBill bill2) AddConsignorAndBills()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = org1.Addresses.AddNewMainAddress();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var address2 = org2.Addresses.AddNewMainAddress();

			var bill1 = nctsHeader.Bills.AddNew();
			var bill2 = nctsHeader.Bills.AddNew();

			var jobAddress = nctsHeader.DocAddresses.AddNew();
			jobAddress.E2_OA_Address = address1.PK;
			jobAddress.E2_AddressType = "CRD";

			return (address1, address2, bill1, bill2);
		}

		public void TestConsignee_Null()
		{
			AssertNull("Consignee null", HeaderProvider.Consignee);
		}

		public void TestConsignee_ContactFromAllocation()
		{
			nctsHeader.Bills.AddNew().GoodsItems.AddNew();

			var jobAddress = nctsHeader.DocAddresses.AddNew();
			jobAddress.E2_OA_Address = Factory.NewWithValidTestData<OrgAddress>().PK;
			jobAddress.E2_AddressType = "CEA";
			nctsHeader.Consignee.E2_OA_Address = jobAddress.E2_OA_Address;

			var contact = jobAddress.Organisation.Contacts.AddNew();
			contact.OC_ContactName = "TEST";
			contact.OC_Phone = "+49 1234 1234";
			contact.OC_Email = "bill@test.com";
			contact.Allocations.AddNew().PC_Type = "CUS";

			AssertEquals("TEST", HeaderProvider.Consignee.Name);
			AssertEquals("+49 1234 1234", HeaderProvider.Consignee.PhoneNumber);
			AssertEquals("bill@test.com", HeaderProvider.Consignee.MailAddress);
		}

		public void TestConsignee_BillsWithoutConsignees()
		{
			AddConsigneeAndBills();
			AssertNotNull("Consignee", HeaderProvider.Consignee);
		}

		public void TestConsignee_DifferentConsigneeInBills()
		{
			var (address1PK, address2PK, _, bill2, item1, _) = AddConsigneeAndBills();

			bill2.Consignee.E2_OA_Address = address2PK;
			item1.Consignee.E2_OA_Address = address1PK;

			AssertNull("Consignee", HeaderProvider.Consignee);
		}

		public void TestConsignee_DifferentConsigneeInItems()
		{
			var (address1PK, address2PK, bill1, bill2, item1, _) = AddConsigneeAndBills();

			bill1.Consignee.E2_OA_Address = address1PK;
			bill2.Consignee.E2_OA_Address = address1PK;
			item1.Consignee.E2_OA_Address = address2PK;

			AssertNull("Consignee", HeaderProvider.Consignee);
		}

		public void TestConsignee_SameConsignee()
		{
			var (address1PK, _, bill1, _, _, item2) = AddConsigneeAndBills();

			bill1.Consignee.E2_OA_Address = address1PK;
			item2.Consignee.E2_OA_Address = address1PK;
			AssertNotNull("Consignee", HeaderProvider.Consignee);
			AssertSame("Cached", HeaderProvider.Consignee, HeaderProvider.Consignee);
		}

		public void TestConsignee_BM_TypeOfSecurityAndAdditionalInfo30600()
		{
			AddConsigneeAndBills();
			nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
			var additionalDocument = nctsHeader.AdditionalDocuments.AddNew();
			additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			additionalDocument.CSI_Code = UniversalReferenceConstants.AdditionalDocumentTypes._30600;
			AssertNull(Provider.Consignee);
		}

		(ZGuid address1PK, ZGuid address2PK, NctsBill bill1, NctsBill bill2, EU.NCTS.Business.NctsDepartureCargoDesc item1, EU.NCTS.Business.NctsDepartureCargoDesc item2) AddConsigneeAndBills()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = org1.Addresses.AddNewMainAddress();
			address1.OA_Address1 = "street_of_address_1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var address2 = org2.Addresses.AddNewMainAddress();
			address2.OA_Address1 = "street_of_address_2";

			var bill1 = nctsHeader.Bills.AddNew();
			var item1 = bill1.GoodsItems.AddNew();
			var bill2 = nctsHeader.Bills.AddNew();
			var item2 = bill2.GoodsItems.AddNew();

			var jobAddress = nctsHeader.DocAddresses.AddNew();
			jobAddress.E2_OA_Address = address1.PK;
			jobAddress.E2_AddressType = "CEA";
			nctsHeader.Consignee.E2_OA_Address = jobAddress.E2_OA_Address;

			return (address1.PK, address2.PK, bill1, bill2, item1, item2);
		}

		public void TestShouldPopulateConsignee_Header()
		{
			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
				var additionalDocument = nctsHeader.AdditionalDocuments.AddNew();
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalDocument.CSI_Code = UniversalReferenceConstants.AdditionalDocumentTypes._30600;
				AssertEquals("EXI, 30600", expected: false, Provider.ShouldPopulateConsignee);

				nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
				AssertEquals("BTH, 30600", expected: false, GetHeaderProvider().ShouldPopulateConsignee);

				nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				AssertEquals("Different to EXI and BTH, 30600", expected: true, GetHeaderProvider().ShouldPopulateConsignee);

				nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
				additionalDocument.CSI_Code = UniversalReferenceConstants.AdditionalDocumentTypes.T0000;
				AssertEquals("EXI, not 30600", expected: true, GetHeaderProvider().ShouldPopulateConsignee);
			});
		}

		public void TestShouldPopulateConsignee_HouseConsignment()
		{
			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
				var additionalDocument = nctsHeader.Bills.AddNew().AdditionalDocuments.AddNew();
				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalDocument.CSI_Code = UniversalReferenceConstants.AdditionalDocumentTypes._30600;
				AssertEquals("EXI, 30600", expected: false, Provider.ShouldPopulateConsignee);

				nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
				AssertEquals("BTH, 30600", expected: false, GetHeaderProvider().ShouldPopulateConsignee);

				nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				AssertEquals("Different to EXI and BTH, 30600", expected: true, GetHeaderProvider().ShouldPopulateConsignee);

				nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
				additionalDocument.CSI_Code = UniversalReferenceConstants.AdditionalDocumentTypes.T0000;
				AssertEquals("EXI, not 30600", expected: true, GetHeaderProvider().ShouldPopulateConsignee);
			});
		}

		public void TestShouldPopulateConsignee_ConsignmentItem()
		{
			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
				var additionalInfo = nctsHeader.Bills.AddNew().GoodsItems.AddNew().AdditionalInfos.AddNew();
				additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				additionalInfo.CSI_Code = UniversalReferenceConstants.AdditionalDocumentTypes._30600;
				AssertEquals("EXI, 30600", expected: false, Provider.ShouldPopulateConsignee);

				nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
				AssertEquals("BTH, 30600", expected: false, GetHeaderProvider().ShouldPopulateConsignee);

				nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				AssertEquals("Different to EXI and BTH, 30600", expected: true, GetHeaderProvider().ShouldPopulateConsignee);

				nctsHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.EXI;
				additionalInfo.CSI_Code = UniversalReferenceConstants.AdditionalDocumentTypes.T0000;
				AssertEquals("EXI, not 30600", expected: true, GetHeaderProvider().ShouldPopulateConsignee);
			});
		}

		public void TestAdditionalSupplyChainActors()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = org1.Addresses.AddNew();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var address2 = org2.Addresses.AddNew();

			var actor1 = nctsHeader.MovementHeader.CusSupplyChainActors.AddNew();
			actor1.CFR_Code = "RF1";
			actor1.CFR_Code = "SCA";
			var actor2 = nctsHeader.MovementHeader.CusSupplyChainActors.AddNew();
			actor2.CFR_Code = "RF2";
			actor2.CFR_Code = "SCA";

			AssertEquals("AdditionalSupplyChainActors count", 2, HeaderProvider.AdditionalSupplyChainActors.Count);
			AssertSame("AdditionalSupplyChainActors cached", HeaderProvider.AdditionalSupplyChainActors, HeaderProvider.AdditionalSupplyChainActors);
		}

		public void TestTransportEquipments() => CombineAssertions(() =>
		{
			nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = false;
			var container1 = nctsHeader.DepartureHeaderContainers.AddNew();
			container1.BC_Mode = "NCT";
			var container2 = nctsHeader.DepartureHeaderContainers.AddNew();
			container2.BC_Mode = "CNT";
			AssertEquals("TransportEquipments count", 1, HeaderProvider.TransportEquipments.Count);

			nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = true;
			AssertEquals("TransportEquipments count", 2, GetHeaderProvider().TransportEquipments.Count);
		});

		public void TestLocationOfGoodsType() => CombineAssertions(() =>
		{
			nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = false;
			AssertEquals("LocationOfGoodsType A", "A", HeaderProvider.LocationOfGoodsType);

			nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = true;
			AssertEquals("LocationOfGoodsType B", "B", HeaderProvider.LocationOfGoodsType);
		});

		public void TestLocationOfGoodsQualifierIfIdentification() => CombineAssertions(() =>
		{
			nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = false;
			AssertEquals("LocationOfGoodsQualifierIfIdentification V", "V", HeaderProvider.LocationOfGoodsQualifierIfIdentification);

			nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = true;
			AssertEquals("LocationOfGoodsQualifierIfIdentification Y", "Y", HeaderProvider.LocationOfGoodsQualifierIfIdentification);
		});

		public void TestLocationOfGoodsAdditionalIdentifier() => CombineAssertions(() =>
		{
			nctsHeader.MovementHeader.GoodsLocation.CGL_Qualifier = string.Empty;
			nctsHeader.MovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ABC1";
			AssertEquals("LocationOfGoodsAdditionalIdentifier empty", null, HeaderProvider.LocationOfGoodsAdditionalIdentifier);

			nctsHeader.MovementHeader.GoodsLocation.CGL_Qualifier = "Y";
			nctsHeader.MovementHeader.GoodsLocation.CGL_AdditionalIdentifier = "ABC1";
			AssertEquals("LocationOfGoodsAdditionalIdentifier populated", "ABC1", HeaderProvider.LocationOfGoodsAdditionalIdentifier);
		});

		public void TestLocationOfGoodsContact() => CombineAssertions(() =>
		{
			nctsHeader.MovementHeader.GoodsLocation.CGL_Qualifier = "";
			AssertNull("LocationOfGoodsContact null", HeaderProvider.LocationOfGoodsContact);

			nctsHeader.MovementHeader.GoodsLocation.CGL_Qualifier = "Y";
			nctsHeader.MovementHeader.GoodsLocation.Address.E2_Contact = "Contact1";
			nctsHeader.MovementHeader.GoodsLocation.Address.E2_Phone = "12345";
			nctsHeader.MovementHeader.GoodsLocation.Address.E2_Email = "aa@aa.com";

			AssertNotNull("LocationOfGoodsContact Provided", HeaderProvider.LocationOfGoodsContact);
			AssertEquals("LocationOfGoodsContact Name", "Contact1", HeaderProvider.LocationOfGoodsContact.Name);
			AssertEquals("LocationOfGoodsContact Phone", "12345", HeaderProvider.LocationOfGoodsContact.PhoneNumber);
			AssertEquals("LocationOfGoodsContact Email", "aa@aa.com", HeaderProvider.LocationOfGoodsContact.MailAddress);
		});

		public void TestCountriesOfRoutingOfConsignment() => CombineAssertions(() =>
		{
			var country1 = nctsHeader.CountriesOfRouting.AddNew("FR", "FR");
			var country2 = nctsHeader.CountriesOfRouting.AddNew("BE", "BE");

			AssertEquals("CountriesOfRoutingOfConsignment Count", 2, nctsHeader.CountriesOfRouting.Count);
			AssertContainsExactElementsInExactOrder("CountriesOfRoutingOfConsignment", new[] { "FR", "BE" }, HeaderProvider.CountriesOfRoutingOfConsignment);
		});

		public void TestDepartureTransportMeans()
		{
			nctsHeader.MovementHeader.BM_InlandTransportMode = "1";
			nctsHeader.MovementHeader.BM_TransportAtDeparture = "ABC123";
			nctsHeader.MovementHeader.BM_RN_NKTransportAtDepartureCountry = "BE";

			AssertEquals("DepartureTransportMeans Count", 1, HeaderProvider.DepartureTransportMeans.Count);
		}

		public void TestDepartureTransportMeansNotMapped_EmptyBM_TransportAtDepartureType()
		{
			nctsHeader.MovementHeader.BM_InlandTransportMode = "1";
			nctsHeader.MovementHeader.BM_TransportAtDeparture = "ABC123";
			nctsHeader.MovementHeader.BM_RN_NKTransportAtDepartureCountry = "BE";
			nctsHeader.MovementHeader.BM_TransportAtDepartureType = ZString.Empty;

			var provider = GetHeaderProvider();

			AssertEquals("DepartureTransportMeans not mapped as BM_TransportAtDepartureType is empty", 0, provider.DepartureTransportMeans.Count);

			CreateMovementHeader("1", "10", "ABC123", "BE");
			provider = GetHeaderProvider();
			AssertEquals("DepartureTransportMeans mapped as BM_TransportAtDepartureType is not empty", 1, provider.DepartureTransportMeans.Count);
		}

		public void TestGetDepartureTransportMeans_Sea()
		{
			CombineAssertions(() =>
			{
				CreateMovementHeader("1", "10", ZString.Empty, "DE");
				var provider = GetHeaderProvider();
				AssertEquals("BM_TransportAtDeparture empty", 0, provider.DepartureTransportMeans.Count);

				CreateMovementHeader("1", "10", "LloydNum", "DE");
				provider = GetHeaderProvider();
				AssertEquals("Vessel does not exist count", 1, provider.DepartureTransportMeans.Count);
				AssertNCTSTransportMeans("Identification Number populated with Lloyd Number", provider.DepartureTransportMeans.First(), "10", "LloydNum", "DE");

				CreateMovementHeader("1", "11", "ShipName", "DE");
				provider = GetHeaderProvider();
				AssertEquals(1, provider.DepartureTransportMeans.Count);
				AssertNCTSTransportMeans("Identification Number populated with Vessel Name", provider.DepartureTransportMeans.First(), "11", "ShipName", "DE");
			});
		}

		public void TestGetDepartureTransportMeans_Rail()
		{
			CombineAssertions(() =>
			{
				CreateMovementHeader("2", "20", ZString.Empty, "DE", "TRAI123", "FR");
				var provider = GetHeaderProvider();
				AssertEquals("BM_TransportAtDeparture empty", 1, provider.DepartureTransportMeans.Count);
				AssertNCTSTransportMeans("BM_TransportAtDeparture empty", provider.DepartureTransportMeans.First(), "20", "TRAI123", "FR");

				CreateMovementHeader("2", "20", "ABC123", "DE", "TRAI123", "FR");
				provider = GetHeaderProvider();
				AssertEquals("BM_TransportAtDeparture present count", 2, provider.DepartureTransportMeans.Count);
				AssertNCTSTransportMeans("BM_TransportAtDeparture present 0", provider.DepartureTransportMeans.First(), "20", "ABC123", "DE");
				AssertNCTSTransportMeans("BM_TransportAtDeparture present 1", provider.DepartureTransportMeans.Skip(1).First(), "20", "TRAI123", "FR");

				CreateMovementHeader("2", "20", "ABC123", "DE", "TRAI123", "FR");
				var wagon1 = nctsHeader.MovementHeader.InlandTransportList.AddNew();
				wagon1.CY_Data = "WAG123";
				wagon1.CY_Code = "BE";
				provider = GetHeaderProvider();
				AssertEquals("Wagon Present count", 3, provider.DepartureTransportMeans.Count);
				AssertNCTSTransportMeans("Wagon present 0", provider.DepartureTransportMeans.First(), "20", "ABC123", "DE");
				AssertNCTSTransportMeans("Wagon present 1", provider.DepartureTransportMeans.Skip(1).First(), "20", "TRAI123", "FR");
				AssertNCTSTransportMeans("Wagon present 2", provider.DepartureTransportMeans.Skip(2).First(), "20", "WAG123", "BE");
			});
		}

		public void TestGetDepartureTransportMeans_Road()
		{
			CombineAssertions(() =>
			{
				CreateMovementHeader("3", "30", ZString.Empty, "DE");
				var provider = GetHeaderProvider();
				AssertEquals("BM_TransportAtDeparture & trailers empty count", 0, provider.DepartureTransportMeans.Count);

				CreateMovementHeader("3", "30", "ABC123", "DE");
				provider = GetHeaderProvider();
				AssertEquals("BM_TransportAtDeparture present count", 1, provider.DepartureTransportMeans.Count);
				AssertNCTSTransportMeans("BM_TransportAtDeparture present", provider.DepartureTransportMeans.First(), "30", "ABC123", "DE");

				CreateMovementHeader("3", "30", "ABC123", "DE", "TRAI111", "FR", "TRAI222", "BE");
				provider = GetHeaderProvider();
				AssertEquals("All Present count", 3, provider.DepartureTransportMeans.Count);
				AssertNCTSTransportMeans("All Present 0", provider.DepartureTransportMeans.First(), "30", "ABC123", "DE");
				AssertNCTSTransportMeans("All Present 1", provider.DepartureTransportMeans.Skip(1).First(), "31", "TRAI111", "FR");
				AssertNCTSTransportMeans("All Present 2", provider.DepartureTransportMeans.Skip(2).First(), "31", "TRAI222", "BE");
			});
		}

		public void TestGetDepartureTransportMeans_Air()
		{
			CombineAssertions(() =>
			{
				CreateMovementHeader("4", "40", "ABC123", "DE");
				var provider = GetHeaderProvider();
				AssertEquals("BM_TransportAtDeparture present count", 1, provider.DepartureTransportMeans.Count);
				AssertNCTSTransportMeans("BM_TransportAtDeparture present", provider.DepartureTransportMeans.First(), "40", "ABC123", "DE");

				CreateMovementHeader("4", "41", "AIR123", "DE");
				provider = GetHeaderProvider();
				AssertEquals("Aircraft present count", 1, provider.DepartureTransportMeans.Count);
				AssertNCTSTransportMeans("Aircraft present", provider.DepartureTransportMeans.First(), "41", "AIR123", "DE");
			});
		}

		public void TestGetDepartureTransportMeans_InlandWaterway()
		{
			CombineAssertions(() =>
			{
				CreateMovementHeader("8", ZString.Empty, ZString.Empty, "DE");
				var provider = GetHeaderProvider();
				AssertEquals("BM_TransportAtDeparture empty count", 0, provider.DepartureTransportMeans.Count);

				CreateMovementHeader("8", "80", "LloydNumber", "DE");
				provider = GetHeaderProvider();
				AssertEquals("Vessel does not exist count", 1, provider.DepartureTransportMeans.Count);
				AssertNCTSTransportMeans("Lloyd Number is mapped", provider.DepartureTransportMeans.First(), "80", "LloydNumber", "DE");

				CreateMovementHeader("8", "81", "ShipName", "DE");
				provider = GetHeaderProvider();
				AssertEquals(1, provider.DepartureTransportMeans.Count);
				AssertNCTSTransportMeans("Ship Name is mapped", provider.DepartureTransportMeans.First(), "81", "ShipName", "DE");
			});
		}

		public void TestGetDepartureTransportMeans_Own()
		{
			CombineAssertions(() =>
			{
				CreateMovementHeader("9", "99", "ABC123", "DE");
				var provider = GetHeaderProvider();
				AssertEquals("BM_TransportAtDeparture present count", 1, provider.DepartureTransportMeans.Count);
				AssertNCTSTransportMeans("BM_TransportAtDeparture present", provider.DepartureTransportMeans.First(), "99", "ABC123", "DE");
			});
		}

		public void TestActiveBorderTransportMeans()
		{
			nctsHeader.MovementHeader.BM_ExportTransportMode = "2";
			nctsHeader.MovementHeader.BM_ActiveBorderIdentificationType = "21";
			nctsHeader.MovementHeader.BM_TOLCarrierID = "CARR1";
			nctsHeader.MovementHeader.BM_RN_NKTOLCarrierNationality = "BE";

			var item1 = nctsHeader.MovementHeader.AdditionalTransportAtBorderList.AddNew();
			item1.TPM_TypeOfIdentification = "21";
			item1.TPM_IdentificationNumber = "CARR2";
			item1.TPM_RN_NKTransportNationality = "FR";

			AssertEquals("ActiveBorderTransportMeans Count", 2, HeaderProvider.ActiveBorderTransportMeans.Count);
		}

		public void TestIsSimplifiedProcedure()
		{
			nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = true;
			AssertEquals(true, Provider.IsSimplifiedProcedure);
			nctsHeader.MovementHeader.IsSimplifiedNctsProcedure = false;
			AssertEquals(false, Provider.IsSimplifiedProcedure);
		}

		public void TestPlaceOfLoadingCountry_NullByDefault()
		{
			AssertNull("Null by default", Provider.PlaceOfLoadingCountry);
		}

		public void TestPlaceOfLoadingCountry()
		{
			nctsHeader.MovementHeader.BM_PortOfPresentationCode = "DE123";
			AssertEquals("First two characters of BM_PortOfPresentationCode", Constants.CountryCodes.Germany, Provider.PlaceOfLoadingCountry);
		}

		public void TestPlaceOfLoadingLocation_NullByDefault()
		{
			AssertNull("Null by default", Provider.PlaceOfLoadingLocation);
		}

		public void TestPlaceOfLoadingLocation()
		{
			nctsHeader.MovementHeader.BM_PlaceOfLoading = "Test Location";
			AssertEquals("Place of Loading Location", "Test Location", Provider.PlaceOfLoadingLocation);
		}

		public void TestPlaceOfUnloadingCountry_NullByDefault()
		{
			AssertNull("Null by default", Provider.PlaceOfUnloadingCountry);
		}

		public void TestPlaceOfUnloadingCountry()
		{
			nctsHeader.MovementHeader.BM_ForeignDestPortKCode = "DE123";
			AssertEquals("First two characters of BM_ForeignDestPortKCode", Constants.CountryCodes.Germany, Provider.PlaceOfUnloadingCountry);
		}

		public void TestPlaceOfUnloadingLocation_NullByDefault()
		{
			AssertNull("Null by default", Provider.PlaceOfLoadingLocation);
		}

		public void TestPlaceOfUnloadingLocation()
		{
			nctsHeader.MovementHeader.BM_PlaceOfUnloading = "Test Location";

			AssertEquals("Place of Unloading Location", "Test Location", Provider.PlaceOfUnloadingLocation);
		}

		public void TestPreviousDocuments()
		{
			nctsHeader.PreviousDocuments.AddNew();
			nctsHeader.PreviousDocuments.AddNew();

			AssertEquals(2, Provider.PreviousDocuments.Count);
		}

		public void TestSupportingDocuments()
		{
			nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			nctsHeader.MovementHeader.SupportingDocuments.AddNew();

			AssertEquals(2, Provider.SupportingDocuments.Count);
		}

		public void TestTransportDocuments()
		{
			CombineAssertions(() =>
			{
				var transportDocument1 = nctsHeader.AdditionalDocuments.AddNew();
				transportDocument1.CSI_SubType = AdditionalDocTypeList.Codes.TransportDocuments;
				transportDocument1.CSI_Code = "1";

				var additionalReference = nctsHeader.AdditionalDocuments.AddNew();
				additionalReference.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
				additionalReference.CSI_Code = "2";

				var transportDocument2 = nctsHeader.AdditionalDocuments.AddNew();
				transportDocument2.CSI_SubType = AdditionalDocTypeList.Codes.TransportDocuments;
				transportDocument2.CSI_Code = "3";

				var additionalInformation = nctsHeader.AdditionalDocuments.AddNew();
				additionalInformation.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
				additionalInformation.CSI_Code = "4";

				var result = Provider.TransportDocuments;
				AssertContainsExactElementsInAnyOrder("SubType TransportDocuments", new[] { "1", "3" }, result.Select(x => x.Type));
				AssertSame("Cached", result, Provider.TransportDocuments);
			});
		}

		public void TestAdditionalReferences()
		{
			CombineAssertions(() =>
			{
				var transportDocument = nctsHeader.AdditionalDocuments.AddNew();
				transportDocument.CSI_SubType = AdditionalDocTypeList.Codes.TransportDocuments;
				transportDocument.CSI_Code = "1";

				var additionalReference1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalReference1.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
				additionalReference1.CSI_Code = "2";

				var additionalInformation = nctsHeader.AdditionalDocuments.AddNew();
				additionalInformation.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
				additionalInformation.CSI_Code = "3";

				var additionalReference2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalReference2.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
				additionalReference2.CSI_Code = "4";

				var result = Provider.AdditionalReferences;
				AssertContainsExactElementsInAnyOrder("SubType AdditionalReference", new[] { "2", "4" }, result.Select(x => x.Type));
				AssertSame("Cached", result, Provider.AdditionalReferences);
			});
		}

		public void TestAdditionalInformation()
		{
			CombineAssertions(() =>
			{
				var transportDocument = nctsHeader.AdditionalDocuments.AddNew();
				transportDocument.CSI_SubType = AdditionalDocTypeList.Codes.TransportDocuments;
				transportDocument.CSI_Code = "1";

				var additionalReference = nctsHeader.AdditionalDocuments.AddNew();
				additionalReference.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalReference;
				additionalReference.CSI_Code = "2";

				var additionalInformation1 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInformation1.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
				additionalInformation1.CSI_Code = "3";

				var additionalInformation2 = nctsHeader.AdditionalDocuments.AddNew();
				additionalInformation2.CSI_SubType = AdditionalDocTypeList.Codes.AdditionalInformation;
				additionalInformation2.CSI_Code = "4";

				var result = Provider.AdditionalInformation;
				AssertContainsExactElementsInAnyOrder("SubType AdditionalInformation", new[] { "3", "4" }, result.Select(x => x.Code));
				AssertSame("Cached", result, Provider.AdditionalInformation);
			});
		}

		public void TestHouseConsignments()
		{
			CombineAssertions(() =>
			{
				nctsHeader.Bills.AddNew();
				nctsHeader.Bills.AddNew();
				var result = Provider.HouseConsignments;
				AssertEquals("Count", 2, result.Count);
				AssertSame("Cached", result, Provider.HouseConsignments);
			});
		}

		public void TestMethodOfPayment_ReturnsNull()
		{
			AssertNull(Provider.MethodOfPayment);
		}

		public void TestMethodOfPayment_BillAndItemMethodOfPaymentEmpty()
		{
			nctsHeader.MovementHeader.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.ElectronicCreditTransfer;

			var bill1 = nctsHeader.Bills.AddNew();
			bill1.GoodsItems.AddNew();
			bill1.GoodsItems.AddNew();

			var bill2 = nctsHeader.Bills.AddNew();

			bill2.GoodsItems.AddNew();
			bill2.GoodsItems.AddNew();

			AssertEquals("Use header Method of Payment", TransportChargesModeOfPayment.Codes.ElectronicCreditTransfer, Provider.MethodOfPayment);
		}

		public void TestMethodOfPayment_BillMethodOfPaymentSet()
		{
			nctsHeader.MovementHeader.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.ElectronicCreditTransfer;

			var bill1 = nctsHeader.Bills.AddNew();
			bill1.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.CreditCard;
			bill1.GoodsItems.AddNew();
			bill1.GoodsItems.AddNew();

			var bill2 = nctsHeader.Bills.AddNew();
			bill2.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.CreditCard;
			bill2.GoodsItems.AddNew();
			bill2.GoodsItems.AddNew();

			AssertEquals("Use bill Method of Payment", TransportChargesModeOfPayment.Codes.CreditCard, Provider.MethodOfPayment);
		}

		public void TestMethodOfPayment_BillMethodOfPaymentNotEqual()
		{
			nctsHeader.MovementHeader.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.ElectronicCreditTransfer;

			var bill1 = nctsHeader.Bills.AddNew();
			bill1.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.CreditCard;
			bill1.GoodsItems.AddNew();
			bill1.GoodsItems.AddNew();

			var bill2 = nctsHeader.Bills.AddNew();
			bill2.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cheque;
			bill2.GoodsItems.AddNew();
			bill2.GoodsItems.AddNew();

			AssertNull("Bills Method of Payment not equal", Provider.MethodOfPayment);
		}

		public void TestMethodOfPayment_ItemMethodOfPaymentNotEqual()
		{
			nctsHeader.MovementHeader.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.ElectronicCreditTransfer;

			var bill1 = nctsHeader.Bills.AddNew();
			bill1.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cash;

			var item1 = bill1.GoodsItems.AddNew();
			item1.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;

			var item2 = bill1.GoodsItems.AddNew();
			item2.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;

			var bill2 = nctsHeader.Bills.AddNew();
			bill2.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cash;

			var item3 = bill2.GoodsItems.AddNew();
			item3.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;

			var item4 = bill2.GoodsItems.AddNew();
			item4.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.CreditCard;

			AssertNull("All Goods Item Methods of Payment are not equal", Provider.MethodOfPayment);
		}

		public void TestMethodOfPayment_ItemMethodOfPaymentEqual()
		{
			nctsHeader.MovementHeader.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.ElectronicCreditTransfer;

			var bill1 = nctsHeader.Bills.AddNew();
			bill1.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cheque;

			var item1 = bill1.GoodsItems.AddNew();
			item1.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;

			var item2 = bill1.GoodsItems.AddNew();
			item2.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;

			var bill2 = nctsHeader.Bills.AddNew();
			bill2.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cheque;

			var item3 = bill2.GoodsItems.AddNew();
			item3.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;

			var item4 = bill2.GoodsItems.AddNew();
			item4.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;

			AssertEquals("All Goods Item methods of payment are equal", TransportChargesModeOfPayment.Codes.Cash, Provider.MethodOfPayment);
		}

		public void TestMethodOfPayment_ItemsMethodOfPaymentNotEqual()
		{
			nctsHeader.MovementHeader.BM_MethodOfPayment = TransportChargesModeOfPayment.Codes.ElectronicCreditTransfer;

			var bill1 = nctsHeader.Bills.AddNew();
			bill1.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.CreditCard;

			var goodsItem = bill1.GoodsItems.AddNew();
			goodsItem.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cheque;

			bill1.GoodsItems.AddNew();

			var bill2 = nctsHeader.Bills.AddNew();
			bill2.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.CreditCard;
			bill2.GoodsItems.AddNew();
			bill2.GoodsItems.AddNew();

			AssertNull("Goods Items Method of payment are not equal", Provider.MethodOfPayment);
		}

		protected override DEPDATHeaderProvider GetHeaderProvider()
		{
			return new DEPDATHeaderProvider(nctsHeader);
		}

		protected override IEnumerable<Expression<Func<DEPDATHeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return p => p.TransitDeclarationType;
			yield return p => p.BindingItinerary;
			yield return p => p.DepartureOffice;
			yield return p => p.DestinationOffice;
			yield return p => p.HolderOfTransitProcedureTIRIdentification;
			yield return p => p.HolderOfTransitProcedure;
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader = nctsHeader.MovementHeader;
		}
		NctsDepartureMovementHeader movementHeader;
		NctsHeader nctsHeader;

		void CreateMovementHeader(ZString inlandTransportMode, ZString transportAtDepartureType, ZString transportAtDeparture, string transportAtDepartureCountry, string trailer1RegNo = null, string trailer1Nationality = null, string trailer2RegNo = null, string trailer2Nationality = null)
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = nctsHeader.MovementHeader;

			movementHeader.BM_InlandTransportMode = inlandTransportMode;
			movementHeader.BM_TransportAtDepartureType = transportAtDepartureType;
			movementHeader.BM_TransportAtDeparture = transportAtDeparture;
			movementHeader.BM_RN_NKTransportAtDepartureCountry = transportAtDepartureCountry;
			movementHeader.BM_TransportAtDepartureTrailer1RegNo = trailer1RegNo;
			movementHeader.BM_RN_NKTransportAtDepartureTrailer1Nationality = trailer1Nationality;
			movementHeader.BM_TransportAtDepartureTrailer2RegNo = trailer2RegNo;
			movementHeader.BM_RN_NKTransportAtDepartureTrailer2Nationality = trailer2Nationality;
		}

		void AddDocAddress(string type, bool addEori = true, bool addTcu = false)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "TEST ORG";
			if (addEori)
			{
				org.CustomsCodes.AddNew("EOR", "111111", "FR");
			}
			if (addTcu)
			{
				org.CustomsCodes.AddNew("TCU", "222222", "AR");
			}
			var address = org.Addresses.AddNew();

			var jobAddress = nctsHeader.MovementHeader.DocAddresses.AddNew();
			jobAddress.E2_OA_Address = address.PK;
			jobAddress.E2_AddressType = type;
		}

		void AssertNCTSTransportMeans(string message, INCTSTransportMeans nctsTransportMeans, string typeOfIdentification, string identificationNumber, string nationality)
		{
			AssertEquals($"{message} - TypeOfIdentification", typeOfIdentification, nctsTransportMeans.TypeOfIdentification);
			AssertEquals($"{message} - IdentificationNumber", identificationNumber, nctsTransportMeans.IdentificationNumber);
			AssertEquals($"{message} - Nationality", nationality, nctsTransportMeans.Nationality);
		}
	}
}
