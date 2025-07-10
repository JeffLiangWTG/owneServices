using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	public class ExtensionsTest : TestCaseWithFactory
	{
		public void TestIsEFTA()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

			var euctp = helper.CreateTradeGroup("EUN", UniversalReferenceConstants.RefCusTradeGroups.Groups.EFTA, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(euctp, "CH", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(euctp, "IS", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(euctp, "LI", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(euctp, "NO", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			Factory.Save();

			RefCountry country = null;
			Assert(!country.IsEFTA());

			country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Switzerland);
			Assert(country.IsEFTA());

			country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Norway);
			Assert(country.IsEFTA());

			country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Iceland);
			Assert(country.IsEFTA());

			country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Liechtenstein);
			Assert(country.IsEFTA());
		}

		public void TestGetAggregatedData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine1Reference1 = invoiceLine1.CusSupplyChainActorReferences.AddNew();
			invoiceLine1Reference1.CFR_Code = "FR1";
			invoiceLine1Reference1.CFR_Reference = "REF1";
			var invoiceLine1Reference2 = invoiceLine1.CusSupplyChainActorReferences.AddNew();
			invoiceLine1Reference2.CFR_Code = "FR1";
			invoiceLine1Reference2.CFR_Reference = "REF2";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2Reference1 = invoiceLine2.CusSupplyChainActorReferences.AddNew();
			invoiceLine2Reference1.CFR_Code = "FR1";
			invoiceLine2Reference1.CFR_Reference = "REF1";
			var invoiceLine2Reference2 = invoiceLine2.CusSupplyChainActorReferences.AddNew();
			invoiceLine2Reference2.CFR_Code = "FR2";
			invoiceLine2Reference2.CFR_Reference = "REF1";

			var cusSupplyChainActorReferences = declaration.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>());
			var data = Extensions.GetAggregatedData(new[] { CusSupplyChainActorReference.Schema.CFR_Reference, CusSupplyChainActorReference.Schema.CFR_Code }, cusSupplyChainActorReferences);
			var refs = data.ToArray();
			AssertEquals("No of data", 3, refs.Length);
			AssertNoExceptionThrown(() =>
			{
				refs.Single(x => x.CFR_Code == "FR1" && x.CFR_Reference == "REF1");
				refs.Single(x => x.CFR_Code == "FR1" && x.CFR_Reference == "REF2");
				refs.Single(x => x.CFR_Code == "FR2" && x.CFR_Reference == "REF1");
			});
		}

		public void TestGetCachedAggregatedData()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine1Reference1 = invoiceLine1.CusSupplyChainActorReferences.AddNew();
			invoiceLine1Reference1.CFR_Code = "FR1";
			invoiceLine1Reference1.CFR_Reference = "REF1";
			var invoiceLine1Reference2 = invoiceLine1.CusSupplyChainActorReferences.AddNew();
			invoiceLine1Reference2.CFR_Code = "FR1";
			invoiceLine1Reference2.CFR_Reference = "REF2";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2Reference1 = invoiceLine2.CusSupplyChainActorReferences.AddNew();
			invoiceLine2Reference1.CFR_Code = "FR1";
			invoiceLine2Reference1.CFR_Reference = "REF1";
			var invoiceLine2Reference2 = invoiceLine2.CusSupplyChainActorReferences.AddNew();
			invoiceLine2Reference2.CFR_Code = "FR2";
			invoiceLine2Reference2.CFR_Reference = "REF1";

			CachedProperty<IEnumerable<CusSupplyChainActorReference>> property = null;
			var getBizObjsCalledCount = 0;
			Func<IEnumerable<CusSupplyChainActorReference>> getBizObjs = () =>
			{
				getBizObjsCalledCount++;
				return declaration.InvoiceLines.Cast<JobComInvoiceLine>().SelectMany(x => x.CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>());
			};
			var data = Factory.GetCachedAggregatedData(ref property, () => new[] { CusSupplyChainActorReference.Schema.CFR_Reference, CusSupplyChainActorReference.Schema.CFR_Code }, getBizObjs);
			AssertSame(data, property.Value);
			AssertSame(data, Factory.GetCachedAggregatedData(ref property, () => new[] { CusSupplyChainActorReference.Schema.CFR_Reference, CusSupplyChainActorReference.Schema.CFR_Code }, getBizObjs));
			AssertEquals("getBizObjsCalledCount", 1, getBizObjsCalledCount);
			var refs = data.ToArray();
			AssertEquals("No of data", 3, refs.Length);
			AssertNoExceptionThrown(() =>
			{
				refs.Single(x => x.CFR_Code == "FR1" && x.CFR_Reference == "REF1");
				refs.Single(x => x.CFR_Code == "FR1" && x.CFR_Reference == "REF2");
				refs.Single(x => x.CFR_Code == "FR2" && x.CFR_Reference == "REF1");
			});
		}

		public void TestAddMergeKey()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine1Reference1 = invoiceLine1.CusSupplyChainActorReferences.AddNew();
			invoiceLine1Reference1.CFR_Code = "FR1";
			invoiceLine1Reference1.CFR_Reference = "REF1";
			var invoiceLine1Reference2 = invoiceLine1.CusSupplyChainActorReferences.AddNew();
			invoiceLine1Reference2.CFR_Code = "FR1";
			invoiceLine1Reference2.CFR_Reference = "REF2";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2Reference1 = invoiceLine2.CusSupplyChainActorReferences.AddNew();
			invoiceLine2Reference1.CFR_Code = "FR1";
			invoiceLine2Reference1.CFR_Reference = "REF1";

			var dictionary = new Dictionary<MergeKey, CusSupplyChainActorReference>();
			var keys = new[] { CusSupplyChainActorReference.Schema.CFR_Reference, CusSupplyChainActorReference.Schema.CFR_Code };
			dictionary.AddMergeKey(keys, invoiceLine1Reference1);
			AssertEquals("dictionary.Count", 1, dictionary.Count);
			dictionary.AddMergeKey(keys, invoiceLine1Reference2);
			AssertEquals("dictionary.Count", 2, dictionary.Count);
			dictionary.AddMergeKey(keys, invoiceLine2Reference1);
			AssertEquals("dictionary.Count", 2, dictionary.Count);
			var values = dictionary.Values.ToArray();
			var value1 = values[0];
			var value2 = values[1];
			if (value2 == invoiceLine2Reference1)
			{
				value2 = values[0];
				value1 = values[1];
			}
			AssertSame(invoiceLine2Reference1, value1);
			AssertSame(invoiceLine1Reference2, value2);
		}

		public void TestIsPartOfTradeGroup()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

			var euctp = helper.CreateTradeGroup("EUN", "EUCTP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(euctp, "CH", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			Factory.Save();

			RefCountry country = null;
			Assert(!country.IsPartOfTradeGroup(""));

			country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Switzerland);
			Assert(country.IsPartOfTradeGroup("EUCTP"));
		}

		public void TestIsACountryEligibleToACommonTransitProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

			var euctp = helper.CreateTradeGroup("EUN", "EUCTP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(euctp, "CH", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			Factory.Save();

			RefCountry country = null;
			Assert(!country.IsACountryEligibleToACommonTransitProcedure());

			country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.Switzerland);
			Assert(country.IsACountryEligibleToACommonTransitProcedure());
		}

		public void TestIsASpecialTerritoryOfTheCommunity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

			var eusft = helper.CreateTradeGroup("EUN", UniversalReferenceConstants.RefCusTradeGroups.Groups.EUSpecialFiscalTerritories, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(eusft, "GF", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			Factory.Save();

			RefCountry country = null;
			Assert(!country.IsASpecialTerritoryOfTheCommunity());

			country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.FrenchGuyana);
			Assert(country.IsASpecialTerritoryOfTheCommunity());
		}

		public void TestHasSpecialTerritoriesOfTheCommunity()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

			var eusfr = helper.CreateTradeGroup("EUN", UniversalReferenceConstants.RefCusTradeGroups.Groups.EUSpecialFiscalTerritoryCountries, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(eusfr, "GB", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			Factory.Save();

			RefCountry country = null;
			Assert(!country.HasSpecialTerritoriesOfTheCommunity());

			country = RefCountry.LoadFromCountryCode(Factory, Core.Constants.CountryCodes.UnitedKingdom);
			Assert(country.HasSpecialTerritoriesOfTheCommunity());
		}

		[ExpectNoExceptions()]
		public void TestNullGetEoriNumber()
		{
			Assert(EU.Business.Extensions.GetEoriNumber(null, Core.Constants.CountryCodes.Venezuela).IsEmpty);
		}

		public void TestGetEoriNumber()
		{
			var header = Factory.New<OrgHeader>();
			Assert(header.GetEoriNumber(Core.Constants.CountryCodes.Bolivia).IsEmpty);
			var cusCode = header.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator;
			cusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Bolivia;
			cusCode.OK_CustomsRegNo = "EU1234567890";
			Assert(header.GetEoriNumber(Core.Constants.CountryCodes.Bolivia).IsEmpty);
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			AssertEquals("EU1234567890", header.GetEoriNumber(Core.Constants.CountryCodes.Bolivia));
		}

		public void TestGetEoriNumberWithFallback_Branch()
		{
			SetUpForTestEORIFallBack();
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = companyOrgProxy.PK;
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchOrgProxy.PK;
			AssertEquals("GRBRANCHEORI01", Extensions.GetEoriNumberWithFallback());
		}

		public void TestGetEoriNumberWithFallback_Company()
		{
			SetUpForTestEORIFallBack();
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = companyOrgProxy.PK;
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertEquals("GRCOMPANYEORI01", Extensions.GetEoriNumberWithFallback());
		}

		public void TestGetEoriNumberWithFallback_BranchWithNoEoriRecords()
		{
			SetUpForTestEORIFallBack();
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = companyOrgProxy.PK;
			branchOrgProxy.CustomsCodes.RemoveAndDeleteAll();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = branchOrgProxy.PK;
			Factory.Save();
			AssertEquals("GRCOMPANYEORI01", Extensions.GetEoriNumberWithFallback());
		}

		public void TestGetEoriDetails()
		{
			CombineAssertions(() =>
			{
				AssertEquals("OrgHeader Null", ZString.Empty, Extensions.GetEoriDetails(null));
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Latvia);
				AssertEquals("Single Eori", "LV123456789", orgHeader.GetEoriDetails());
				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "94567", Core.Constants.CountryCodes.France);
				AssertEquals("Multiple EORI", ZString.Empty, orgHeader.GetEoriDetails());
				AssertEquals("Multiple EORI with error value", "* multiple EOR *", orgHeader.GetEoriDetails(errorOnMultiple: true));
			});
		}

		public void TestGetEoriDetails_CountriesToIgnore()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Latvia);
				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "94567", Core.Constants.CountryCodes.UnitedKingdom);
				AssertEquals("Empty Eori", ZString.Empty, orgHeader.GetEoriDetails());
				AssertEquals("Latvia Eori, UK ignored", "LV123456789", orgHeader.GetEoriDetails(countriesToIgnore: [Core.Constants.CountryCodes.UnitedKingdom]));
			});
		}

		public void TestGetEORIFromAddress()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", Core.Constants.CountryCodes.France);
			var addressWithEORI = orgHeader.Addresses.AddNew();
			addressWithEORI.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			AssertEquals("Can get Eori Number", "123456", addressWithEORI.GetEORIFromAddress());

			var addressWithoutEORI = orgHeader.Addresses.AddNew();
			AssertEquals("Cannot get Eori Number", ZString.Empty, addressWithoutEORI.GetEORIFromAddress());
		}

		public void TestGetICS2EoriDetails()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Null OrgHeader", ZString.Empty, Extensions.GetICS2EoriDetails(null));

				var orgHeader1 = Factory.New<OrgHeader>();
				orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", Core.Constants.CountryCodes.UnitedKingdom);
				AssertEquals("Only UK EORI", ZString.Empty, orgHeader1.GetICS2EoriDetails());

				var orgHeader2 = Factory.New<OrgHeader>();
				orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "456", Core.Constants.CountryCodes.Germany);
				orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "999", Core.Constants.CountryCodes.UnitedKingdom);
				AssertEquals("One non-UK EORI", "DE456", orgHeader2.GetICS2EoriDetails());

				var orgHeader3 = Factory.New<OrgHeader>();
				orgHeader3.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123", Core.Constants.CountryCodes.Germany);
				orgHeader3.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "456", Core.Constants.CountryCodes.France);
				AssertEquals("Multiple non-UK EORI", ZString.Empty, orgHeader3.GetICS2EoriDetails());

				AssertEquals("Multiple non-UK EORI with error", "* multiple EOR *", orgHeader3.GetICS2EoriDetails(errorOnMultiple: true));
			});
		}

		public void TestGetConcatenatedSingleOrgCusCodeIgnoringCountry_Null()
		{
			AssertEquals(ZString.Empty, Extensions.GetConcatenatedSingleOrgCusCodeIgnoringCountry(null, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori));
		}

		public void TestGetConcatenatedSingleOrgCusCodeIgnoringCountry_EmptyCodeType()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Latvia);
			AssertEquals(ZString.Empty, orgHeader.GetConcatenatedSingleOrgCusCodeIgnoringCountry(ZString.Empty));
		}

		public void TestGetConcatenatedSingleOrgCusCodeIgnoringCountry_Duplicate()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Latvia);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "54321", Core.Constants.CountryCodes.Germany);
			AssertEquals(ZString.Empty, orgHeader.GetConcatenatedSingleOrgCusCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori));
		}

		public void TestGetConcatenatedSingleOrgCusCodeIgnoringCountry_DuplicateWithError()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Latvia);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "54321", Core.Constants.CountryCodes.Germany);
			AssertEquals("* multiple EOR *", orgHeader.GetConcatenatedSingleOrgCusCodeIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, errorOnMultiple: true));
		}

		public void TestGetConcatenatedSingleOrgCusCodeIgnoringCountry()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Latvia);
			AssertEquals("LV12345", orgHeader.GetConcatenatedSingleOrgCusCodeIgnoringCountry("EOR"));
		}

		public void TestGetConcatenatedSingleOrgCusCode()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Latvia);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Germany);
			AssertEquals("LV12345", orgHeader.GetConcatenatedSingleOrgCusCode("EOR"));

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "12345", Core.Constants.CountryCodes.Italy);
			AssertEquals("IT12345", orgHeader2.GetConcatenatedSingleOrgCusCode("EOR"));

			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			AssertEquals(ZString.Empty, orgHeader3.GetConcatenatedSingleOrgCusCode("EOR"));
		}

		public void TestIsIntegerRequiredUnitOfQuantity()
		{
			CombineAssertions(() =>
			{
				foreach (var validUQ in new[]
				{
					Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItems,
					Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfCells,
					Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfPairs,
					Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Number.NumberOfItemsPerFlask
				})
				{
					AssertEquals("Valid UQ: " + validUQ, true, Factory.IsIntegerRequiredUnitOfQuantity(validUQ));
				}
				AssertEquals("Invalid UQ", false, Factory.IsIntegerRequiredUnitOfQuantity(Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram));
			});
		}

		public void TestGetCustomsRegNoIgnoringCountry_OrgHeader()
		{
			CombineAssertions(() =>
			{
				AssertEquals("OrgHeader is null", ZString.Empty, Extensions.GetCustomsRegNoIgnoringCountry((OrgHeader)null, ZString.Empty));

				var orgHeader = Factory.New<OrgHeader>();
				AssertEquals("No Ten code", ZString.Empty, orgHeader.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber));

				var customsCode = orgHeader.CustomsCodes.AddNew();
				customsCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber;
				customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
				customsCode.OK_CustomsRegNo = "TEN001";
				AssertEquals("Country is IT", "TEN001", orgHeader.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber));

				customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
				AssertEquals("Country is DE", "TEN001", orgHeader.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber));
			});
		}

		public void TestGetCustomsRegNoIgnoringCountry_OrgAddress()
		{
			CombineAssertions(() =>
			{
				AssertEquals("OrgAddress is null", ZString.Empty, Extensions.GetCustomsRegNoIgnoringCountry((OrgAddress)null, ZString.Empty));

				var orgHeader = Factory.New<OrgHeader>();
				var orgAddress = orgHeader.MainAddress;
				AssertEquals("No TID code", ZString.Empty, orgAddress.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID));

				var customsCode = orgHeader.CustomsCodes.AddNew();
				customsCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID;
				customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
				customsCode.OK_CustomsRegNo = "TID001";
				customsCode.OK_OA_PremisesAddress = orgAddress.PK;
				AssertEquals("Country is IT", "TID001", orgAddress.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID));

				customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
				AssertEquals("Country is DE", "TID001", orgAddress.GetCustomsRegNoIgnoringCountry(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID));
			});
		}

		public void TestGetCustomsRegNoIgnoringCountryPrefixed_OrgHeader()
		{
			CombineAssertions(() =>
			{
				AssertEquals("OrgHeader is null", ZString.Empty, Extensions.GetCustomsRegNoIgnoringCountryPrefixed(null, ZString.Empty));

				var orgHeader = Factory.New<OrgHeader>();
				AssertEquals("No Ten code", ZString.Empty, orgHeader.GetCustomsRegNoIgnoringCountryPrefixed(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber));

				var customsCode = orgHeader.CustomsCodes.AddNew();
				customsCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber;
				customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Italy;
				customsCode.OK_CustomsRegNo = "TEN001";
				AssertEquals("Country is IT", "ITTEN001", orgHeader.GetCustomsRegNoIgnoringCountryPrefixed(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber));

				customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
				AssertEquals("Country is DE", "DETEN001", orgHeader.GetCustomsRegNoIgnoringCountryPrefixed(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber));
			});
		}

		public void TestGetVATCodeType()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Germany", GermanyOrgCusCodeInfo.OrgCusCodes.UST, Extensions.GetVATCodeType(Core.Constants.CountryCodes.Germany));
				AssertEquals("countryCode not found", ZString.Empty, Extensions.GetVATCodeType(Core.Constants.CountryCodes.Australia));
			});
		}

		public void TestGetVATRegistrationNumber()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "FRTVA1", Core.Constants.CountryCodes.France);

			CombineAssertions(() =>
			{
				AssertEquals("orgHeader null", ZString.Empty, Extensions.GetVATRegistrationNumber(null, Core.Constants.CountryCodes.Germany));
				AssertEquals("countryCode not found", ZString.Empty, orgHeader.GetVATRegistrationNumber(Core.Constants.CountryCodes.Australia));
				AssertEquals("No VAT number", ZString.Empty, orgHeader.GetVATRegistrationNumber(Core.Constants.CountryCodes.Germany));
				AssertEquals("Has VAT number", "FRTVA1", orgHeader.GetVATRegistrationNumber(Core.Constants.CountryCodes.France));
			});
		}

		public void TestGetVATRegistrationNumberWithCountryCodePrefix()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "TVA1", Core.Constants.CountryCodes.France);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.GreeceCodeTypes.AFM, "AFM1", Core.Constants.CountryCodes.Greece);

			CombineAssertions(() =>
			{
				AssertEquals("orgHeader null", ZString.Empty, Extensions.GetVATRegistrationNumberWithCountryCodePrefix(null, Core.Constants.CountryCodes.France));
				AssertEquals("countryCode not found", ZString.Empty, orgHeader.GetVATRegistrationNumberWithCountryCodePrefix(Core.Constants.CountryCodes.Australia));
				AssertEquals("No VAT number", ZString.Empty, orgHeader.GetVATRegistrationNumberWithCountryCodePrefix(Core.Constants.CountryCodes.Germany));
				AssertEquals("France", "FRTVA1", orgHeader.GetVATRegistrationNumberWithCountryCodePrefix(Core.Constants.CountryCodes.France));
				AssertEquals("Greece", "ELAFM1", orgHeader.GetVATRegistrationNumberWithCountryCodePrefix(Core.Constants.CountryCodes.Greece));
			});
		}

		public void TestCountryVatCodeType()
		{
			var countryVatCodeType = Extensions.CountryVatCodeType.Value;
			CombineAssertions(() =>
			{
				AssertEquals("France", OrgCusCode.FranceCodeTypes.TVA, countryVatCodeType[Core.Constants.CountryCodes.France]);
				AssertEquals("UnitedKingdom", OrgCusCode.CodeTypes.VATCode, countryVatCodeType[Core.Constants.CountryCodes.UnitedKingdom]);
			});
		}

		public void TestGetCorrectEUTypeForCountryCode()
		{
			var typeDecider = new JobDeclarationTypeDecider();
			var defaultType = typeof(JobDeclaration);
			AssertEquals(defaultType, typeDecider.GetCorrectEUTypeForCountryCode(Core.Constants.CountryCodes.Latvia, defaultType));
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			AssertEquals(defaultType, typeDecider.GetCorrectEUTypeForCountryCode(Core.Constants.CountryCodes.Australia, defaultType));
		}

		public void TestGetCorrectEUTypeForCountriesUnderSomeEUCustomsCountryOfJurisdiction()
		{
			var typeDecider = new JobDeclarationTypeDecider();
			var defaultType = typeof(JobDeclaration);
			AssertEquals("CorrectEUTypeForCountryCode should return GetTypeForCountryCode() for countries under jurisdiction of a country that is in EU or inherits from EU.", "Enterprise.Customs.FR.Business.Declaration.JobDeclaration", typeDecider.GetCorrectEUTypeForCountryCode(Core.Constants.CountryCodes.Martinique, defaultType).FullName);
		}

		public void TestGetAllVatNumbers()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, "BTW123", Core.Constants.CountryCodes.Netherlands);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "TVA123", Core.Constants.CountryCodes.France);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.NetherlandsCodeTypes.LFRVATNumberCode, "LFR123", Core.Constants.CountryCodes.Netherlands);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ABIRoutingCode, "ABI123", Core.Constants.CountryCodes.UnitedStates);

			var result = orgHeader.GetAllVatNumbers();

			CombineAssertions(() =>
			{
				Assert(!result.IsNullOrEmpty());
				AssertEquals("GetAllVatNumbers should return 2 results", 2, result.Length);
				AssertEquals("Country of first Vat number should be NL", "NL", result[0].OK_RN_NKCodeCountry);
				AssertEquals("First Vat number should be BTW123", "BTW123", result[0].OK_CustomsRegNo);
				AssertEquals("Country of second Vat number should be FR", "FR", result[1].OK_RN_NKCodeCountry);
			});
		}

		public void TestFallbackTo()
		{
			CombineAssertions("FallbackTo", () =>
			{
				AssertEquals("Return value when it is not empty", "value", new ZString("value").FallbackTo("fallbackValue"));
				AssertEquals("Return fallback value when value is empty", "fallbackValue", ZString.Empty.FallbackTo("fallbackValue"));
				AssertEquals("Return empty value when value and fallback are both empty", "", ZString.Empty.FallbackTo(ZString.Empty));
			});
		}

		public void TestEqualsAny()
		{
			ZString str1 = "Abc";
			ZString str2 = "abc";
			var arr = new ZString[]
			{
				"Abc", "Bcd", "Cde"
			};
			CombineAssertions(() =>
			{
				AssertEquals("Value found in array with same casing", true, str1.EqualsAny(arr));
				AssertEquals("Value not found in array, due to different casing", false, str2.EqualsAny(arr));
			});
		}

		public void TestDirection()
		{
			CombineAssertions(() =>
			{
				ICanBeImportOrExport nullImpExp = null;
				AssertEquals("Null", string.Empty, nullImpExp.Direction());

				var mock = new Mock<ICanBeImportOrExport>();

				mock.Setup(x => x.IsExport).Returns(true);
				mock.Setup(x => x.IsImport).Returns(true);
				AssertEquals("Both true", "Both", mock.Object.Direction());

				mock.Setup(x => x.IsExport).Returns(true);
				mock.Setup(x => x.IsImport).Returns(false);
				AssertEquals("Export true", "Export", mock.Object.Direction());

				mock.Setup(x => x.IsExport).Returns(false);
				mock.Setup(x => x.IsImport).Returns(true);
				AssertEquals("Import true", "Import", mock.Object.Direction());

				mock.Setup(x => x.IsExport).Returns(false);
				mock.Setup(x => x.IsImport).Returns(false);
				AssertEquals("Both false", "Import", mock.Object.Direction());
			});
		}

		public void TestHasSameEori_SameOrg()
		{
			var org = Factory.New<OrgHeader>();

			AssertEquals("Same organization", true, org.HasSameEori(org));
		}

		public void TestHasSameEori_DifferentOrgs()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			AssertEquals("Different organizations", false, org1.HasSameEori(org2));
		}

		public void TestHasSameEori_DifferentOrgsSameEori()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Italy);
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Italy);

			AssertEquals("Same EORI", true, org1.HasSameEori(org2));
		}

		public void TestHasSameEori_DifferentOrgsSameEoriNumberButDifferentIssuingCountry()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Italy);
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Germany);

			AssertEquals("Same EORI but different issuing country", false, org1.HasSameEori(org2));
		}

		public void TestHasSameEori_DifferentOrgsDifferentEori()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321", Core.Constants.CountryCodes.Germany);
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Germany);

			AssertEquals("Different EORI", false, org1.HasSameEori(org2));
		}

		public void TestHasSameEori_DifferentOrgsOneEori()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321", Core.Constants.CountryCodes.Germany);

			AssertEquals("Only one org has an EORI", false, org1.HasSameEori(org2));
		}

		public void TestHasSameEoriOrTcu_DifferentOrgsDifferentEoriSameTcu()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321", Core.Constants.CountryCodes.Germany);
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Germany);

			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.UnitedStates);
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.UnitedStates);

			AssertEquals("Different EORI but same TCU", true, org1.HasSameEoriOrTcu(org2));
		}

		public void TestHasSameEori_DifferentOrgsSameEoriDifferentTcu()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Italy);
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Italy);

			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "112233445566", Core.Constants.CountryCodes.UnitedStates);
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "665544332211", Core.Constants.CountryCodes.UnitedStates);

			AssertEquals("Same EORI, different TCU", true, org1.HasSameEori(org2));
		}

		public void TestHasSameEoriOrTcu_DifferentOrgsDifferentTcu()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "987654321", Core.Constants.CountryCodes.UnitedStates);
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.UnitedStates);

			AssertEquals("Different TCU", false, org1.HasSameEoriOrTcu(org2));
		}

		public void TestHasSameEoriOrTcu_DifferentOrgsSameTcu()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.UnitedStates);
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.UnitedStates);

			AssertEquals("Same TCU", true, org1.HasSameEoriOrTcu(org2));
		}

		public void TestHasSameEoriOrTcu_DifferentOrgsOneTcuSet()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.UnitedStates);

			AssertEquals("One Org has a TCU", false, org1.HasSameEoriOrTcu(org2));
		}

		public void TestHasSameEoriOrTcu_MultipleTcus()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.UnitedStates);
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.UnitedStates);

			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "987654321", Core.Constants.CountryCodes.Japan);
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "112233445", Core.Constants.CountryCodes.Japan);

			AssertEquals("US TCUs are equal", true, org1.HasSameEoriOrTcu(org2));
		}

		public void TestHasSameEoriOrTcu_DifferentTcuCountries()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.UnitedStates);

			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.Japan);

			AssertEquals("TCU Countries differ", false, org1.HasSameEoriOrTcu(org2));
		}

		public void TestHasSameEoriOrTcu_DifferentTcuNumbers()
		{
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "987654321", Core.Constants.CountryCodes.Japan);

			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.Japan);

			AssertEquals("TCU Countries differ", false, org1.HasSameEoriOrTcu(org2));
		}

		public void TestHasSameEoriOrTcu_NullArguments()
		{
			var org = Factory.New<OrgHeader>();

			CombineAssertions(() =>
			{
				AssertEquals(false, org.HasSameEori(null));

				AssertEquals(false, ((OrgHeader)null).HasSameEori(org));

				AssertEquals(false, ((OrgHeader)null).HasSameEori(null));

				AssertEquals(false, org.HasSameEoriOrTcu(null));

				AssertEquals(false, ((OrgHeader)null).HasSameEoriOrTcu(org));

				AssertEquals(false, ((OrgHeader)null).HasSameEoriOrTcu(null));
			});
		}

		public void TestGetNumberOfSignificantDigits()
		{
			CombineAssertions(() =>
			{
				AssertNumberOfSignificantDigits(1, "0.00");
				AssertNumberOfSignificantDigits(1, "1.00");
				AssertNumberOfSignificantDigits(3, "0.12000");
				AssertNumberOfSignificantDigits(3, "1.12000");
				AssertNumberOfSignificantDigits(4, "1,000.0000");
				AssertNumberOfSignificantDigits(8, "1,000.0001");
				AssertNumberOfSignificantDigits(11, "1,234,567.8901");
			});
		}

		public static OrgHeader SetupOrgHeaderWithEORICode(BusinessObjectFactory factory, ZString eoriCode)
		{
			var org = factory.New<OrgHeader>();
			var cusCodeEori = org.CustomsCodes.AddNew();
			cusCodeEori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCodeEori.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Greece;
			cusCodeEori.OK_CustomsRegNo = eoriCode;
			return org;
		}

		public void SetUpForTestEORIFallBack()
		{
			companyOrgProxy = SetupOrgHeaderWithEORICode(Factory, "COMPANYEORI01");
			companyOrgProxy.OH_Code = "COMPORGPROX";
			branchOrgProxy = SetupOrgHeaderWithEORICode(Factory, "BRANCHEORI01");
			branchOrgProxy.OH_Code = "BRANORGPROX";
			Factory.Save();
		}

		public void TestGetIdentificationNumber()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU;
			orgCusCode.OK_CustomsRegNo = "PartyID";
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Belgium;

			AssertEquals("BEPartyID", orgHeader.GetIdentificationNumber());
		}

		public void TestGetIdentificationNumber_NotFound()
		{
			var orgHeader = Factory.New<OrgHeader>();
			AssertNull("asked to return null", orgHeader.GetIdentificationNumber());
		}

		public void TestGetIdentificationNumberOrder()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var orgCusCodeWithoutPriority = orgHeader.CustomsCodes.AddNew();
			orgCusCodeWithoutPriority.OK_CodeType = OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU;
			orgCusCodeWithoutPriority.OK_CustomsRegNo = "NotThisOne";
			orgCusCodeWithoutPriority.OK_RN_NKCodeCountry = "NL";

			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			orgCusCode.OK_CustomsRegNo = "PartyID";
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Belgium;

			AssertEquals("BEPartyID", orgHeader.GetIdentificationNumber());
		}

		void AssertNumberOfSignificantDigits(int expectedNumberOfSignificantDigits, string inputValue)
		{
			var zDecimal = new ZDecimal(inputValue);
			AssertEquals($"when input value is {inputValue}", expectedNumberOfSignificantDigits, zDecimal.GetNumberOfSignificantDigits());
		}

		OrgHeader companyOrgProxy;
		OrgHeader branchOrgProxy;
	}
}
