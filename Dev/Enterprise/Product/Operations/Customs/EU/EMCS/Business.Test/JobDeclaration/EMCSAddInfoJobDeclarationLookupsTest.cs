using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	class EMCSAddInfoJobDeclarationLookupsTest : EUEMCSAddInfoLookupsTest
	{
		public void TestDeferredSubmissionList()
		{
			CombineAssertions(() =>
			{
				var deferredSubmissionList = lookups.DeferredSubmissionList;
				AssertEquals("Codes", "0, 1", deferredSubmissionList.CodesAsString);
				AssertSame("Cached", deferredSubmissionList, lookups.DeferredSubmissionList);
			});
		}

		public void TestGuarantorTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSGuarantorTypes, "Excise Movement Control System (EMCS) Guarantor Type");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.IsJointGuarantor, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSGuarantorTypes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

			var code1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSGuarantorTypes, "1", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			code1.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.IsJointGuarantor, YesNoList.Codes.No);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSGuarantorTypes, "2", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1));
			Factory.Save();
			CombineAssertions(() =>
			{
				var list = lookups.GuarantorTypeList;
				AssertEquals("CodesAsString", "1, 2", list.CodesAsString);
				AssertSame("Cached", list, lookups.GuarantorTypeList);
			});
		}

		public void TestTransportArrangementList()
		{
			CombineAssertions(() =>
			{
				var transportArrangementList = lookups.TransportArrangementList;
				AssertEquals("Codes", "1, 2, 3, 4", transportArrangementList.CodesAsString);
				AssertSame("Cached", transportArrangementList, lookups.TransportArrangementList);
			});
		}

		public void TestOriginTypeList()
		{
			CombineAssertions(() =>
			{
				var originTypeList = lookups.OriginTypeList;
				AssertEquals("Codes", "1, 2, 3", originTypeList.CodesAsString);
				AssertSame("Cached", originTypeList, lookups.OriginTypeList);
			});
		}

		public void TestSubmissionTypeList()
		{
			CombineAssertions(() =>
			{
				var submissionTypeList = lookups.SubmissionTypeList;
				AssertEquals("Codes", "1, 2, 3", submissionTypeList.CodesAsString);
				AssertSame("Cached", submissionTypeList, lookups.SubmissionTypeList);
			});
		}

		[TestDate(2019, 12, 12)]
		public void TestEuropeanUnionCountryList()
		{
			var yesterday = ZDate.Today.AddDays(-1);
			var tomorrow = ZDate.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUC", yesterday, tomorrow);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Monaco, yesterday, tomorrow);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.IsleOfMan, yesterday, tomorrow);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Greece, yesterday, tomorrow);
			helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.Germany, yesterday, tomorrow);

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("ZZ Details are only 3", new[] { Core.Constants.CountryCodes.Monaco, Core.Constants.CountryCodes.IsleOfMan, Core.Constants.CountryCodes.Greece, Core.Constants.CountryCodes.Germany }, Factory.GetEuropeanUnionForCustomsMembers());
				var euorpeanUnionCountryList = lookups.EuropeanUnionCountryList.CodesAsString;
				AssertNotContains("Assert No Monaco", Core.Constants.CountryCodes.Monaco, euorpeanUnionCountryList);
				AssertNotContains("Assert No Isle Of Man", Core.Constants.CountryCodes.IsleOfMan, euorpeanUnionCountryList);
				AssertNotContains("Assert No Greece", Core.Constants.CountryCodes.Greece, euorpeanUnionCountryList);
				AssertContains("Assert Converted code for Greece", "EL", euorpeanUnionCountryList);
				AssertContains("Assert Germany", Core.Constants.CountryCodes.Germany, euorpeanUnionCountryList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
			lookups = declaration.AddInfoLookups;
		}
		EMCSJobDeclaration declaration;
		EMCSAddInfoJobDeclarationLookups lookups;
	}
}
