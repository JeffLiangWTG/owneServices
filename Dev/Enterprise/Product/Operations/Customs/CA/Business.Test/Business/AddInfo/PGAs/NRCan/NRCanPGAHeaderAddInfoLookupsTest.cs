using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class NRCanPGAHeaderAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUNDGCodeList()
		{
			AssertEquals(typeof(UNDGSubstanceCollection), header.AddInfoLookups.UNDGCodeList.GetType());
		}

		public void TestProperties()
		{
			AssertEquals(typeof(NRCanIntendedUseCodes), header.AddInfoLookups.IntendedUseCodeList.GetType());
			AssertEquals(typeof(LPCOHolderPartyTypeCodes), header.AddInfoLookups.AuthorizedPartyTypeCodes.GetType());
			AssertEquals(typeof(CodeDescriptionPairList), header.AddInfoLookups.CustomsUQList.GetType());
		}

		public void TestCustomsUQList()
		{
			header.B7_ParentID = ZGuid.NewZGuid();
			header.B7_ParentTableCode = "B7";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"AAA", "AAAAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BG", "BG DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BBB", "BBB DESC", new ZDateTime(1994, 3, 3), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			Factory.Save();

			// Test if new added code exists in list
			Assert("AAA", header.AddInfoLookups.CustomsUQList.ContainsCode("AAA"));
			Assert("BAG should not appear as it is not in the list", !header.AddInfoLookups.CustomsUQList.ContainsCode("BAG"));
			Assert("BBB should not appear as it is too new", !header.AddInfoLookups.CustomsUQList.ContainsCode("BBB"));
		}

		public void TestProgramCodesList()
		{
			AssertEquals(typeof(NRCanPGADepartmentCodes), header.AddInfoLookups.ProgramCodesList.GetType());
		}

		public void TestCountryOfOriginsLookupAndStateCodeListLookup()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_NRCanInd = "Y";
			var header = invoiceLine.NRCanPGAHeader;

			AssertEquals(typeof(RefCountryCollection), header.AddInfoLookups.CountryOfOriginsLookup.GetType());

			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedStates;
			AssertEquals(typeof(USStatesList), header.AddInfoLookups.StateCodeListLookup.GetType());
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Canada;
			AssertEquals(typeof(CanadianProvinceList), header.AddInfoLookups.StateCodeListLookup.GetType());
		}

		public void TestThereIsNoExceptionWhenRequirementsParentIsNull()
		{
			AssertNoExceptionThrown(() =>
			{
				_ = header.AddInfoLookups.CountryOfOriginsLookup;
				_ = header.AddInfoLookups.StateCodeListLookup;
			});
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NRCanPGAHeader>();
		}
		NRCanPGAHeader header;

		#endregion
	}
}
