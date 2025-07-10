using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CountryOfRouting))]
	sealed class CountryOfRoutingTest : Customs.Business.Testing.CusCodeDataTest<CountryOfRouting>
	{
		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CY_Type", CusCodeDataTypeList.Codes.CountryOfRouting, countryOfRouting.CY_Type);
				AssertEquals("CY_Code", CusCodeDataTypeList.Codes.CountryOfRouting, countryOfRouting.CY_Code);
			});
		}

		public void TestGetNewLookups()
		{
			AssertType<CountryOfRoutingLookups>(countryOfRouting.Lookups);
		}

		public void TestGetNewValidation()
		{
			AssertType<CountryOfRoutingValidation>(countryOfRouting.Validation);
		}

		public void TestValidationDecider()
		{
			AssertType<CountryOfRoutingDeparturePhase5ValidationDecider>(countryOfRouting.ValidationDecider);
		}

		public void TestHumanReadableName()
		{
			AssertEquals("Country/Region of Routing", countryOfRouting.HumanReadableName);
		}

		public void TestCY_Order_MaxLength()
		{
			AssertEquals("MaxLength", 2, countryOfRouting.CY_OrderInfo.MaxLength);
		}

		public void TestCY_Order_ReadOnly()
		{
			AssertEquals("ReadOnly", true, countryOfRouting.CY_OrderInfo.ReadOnly);
		}

		public void TestCY_Order_Caption()
		{
			NCTSTestHelper.AssertCaptions(countryOfRouting.CY_OrderInfo, "Sequence", string.Empty, "Seq.");
		}

		public void TestCY_Data_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(countryOfRouting.CY_DataInfo, multipleResourceKey: null, "Country/Region", "Ctry./Rgn.");
		}

		public void TestCY_Description_Caption()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(countryOfRouting.DescriptionInfo, multipleResourceKey: null, "Country/Region Description", "Ctry./Rgn. Desc.", "Ctry./Rgn. Description");
		}

		public void TestDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			Factory.Save();

			countryOfRouting.CY_Data = Core.Constants.CountryCodes.Australia;
			AssertEquals("Australien", countryOfRouting.Description);
		}

		public void TestDescription_Invalid()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			Factory.Save();

			countryOfRouting.CY_Data = "12";

			AssertEquals(string.Empty, countryOfRouting.Description);
		}

		public void TestCY_Order_SequenceNumber()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var countryOfRouting1 = Factory.New<CountryOfRouting>();
			var countryOfRouting2 = Factory.New<CountryOfRouting>();
			header.CountriesOfRouting.Add(countryOfRouting1);
			header.CountriesOfRouting.Add(countryOfRouting2);

			CombineAssertions(() =>
			{
				AssertEquals("#1", (short)1, countryOfRouting1.CY_Order);
				AssertEquals("#2", (short)2, countryOfRouting2.CY_Order);
			});
		}

		public void TestDefaultDataGroupingCode()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "XXX";
			company.GC_Name = "COMP TEST";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "YYY";
			branch.GB_BranchName = "BRANCH TEST";
			header.BH_GB = branch.PK;

			AssertEquals("PreCondition DE", Core.Constants.CountryCodes.Germany, header.DefaultDataGroupingCode);
			AssertEquals("PreCondition LV", Core.Constants.CountryCodes.Latvia, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			CombineAssertions(() =>
			{
				AssertEquals("Parent exists", Core.Constants.CountryCodes.Germany, countryOfRouting.DefaultDataGroupingCode);
				AssertEquals("No Parent", Core.Constants.CountryCodes.Latvia, Factory.New<CountryOfRouting>().DefaultDataGroupingCode);
			});
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		protected override IEnumerable<CountryOfRouting> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return (CountryOfRouting)GetNewBusinessObject(factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			countryOfRouting = header.CountriesOfRouting.AddNew();
		}
		NctsHeader header;
		CountryOfRouting countryOfRouting;

		BusinessObject GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var countryOfRouting = header.CountriesOfRouting.AddNew();
			return countryOfRouting;
		}
	}
}
