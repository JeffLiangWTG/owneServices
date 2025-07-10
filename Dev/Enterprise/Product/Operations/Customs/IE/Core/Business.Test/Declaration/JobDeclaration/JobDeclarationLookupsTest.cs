using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	abstract class JobDeclarationLookupsTest<T> : EU.Business.Declaration.Testing.JobDeclarationLookupsTest<T, JobDeclaration>
		where T : JobDeclarationLookups
	{
		protected override void SetUp()
		{
			base.SetUp();
			helper = new UniversalReferenceTestDataHelper(Factory);
		}

		protected UniversalReferenceTestDataHelper helper;
	}

	sealed class BaseJobDeclarationLookupsTest : JobDeclarationLookupsTest<JobDeclarationLookups>
	{
		public void TestDeclarantTypeList_Codes()
		{
			AssertEquals("DIR, IND", ((CodeDescriptionPairList)lookups.DeclarantTypeList).CodesAsString);
		}

		public void TestDeclarantTypeList_Cached()
		{
			AssertSame(lookups.DeclarantTypeList, lookups.DeclarantTypeList);
		}

		public void TestPaymentPartyList_Cached()
		{
			AssertSame(lookups.PaymentPartyList, lookups.PaymentPartyList);
		}

		public void TestPaymentPartyList_CodesAsString()
		{
			AssertEquals("A, E, J, M", lookups.PaymentPartyList.CodesAsString);
		}

		public void TestLocationOfGoodsCollection()
		{
			jobDeclaration.JE_LocationOfGoods = "AUSYD";
			var locationOfGoodsCollection = lookups.LocationOfGoodsCollection;
			AssertType<RefUNLOCOCollection>(locationOfGoodsCollection);
			AssertHasDefault((IFilterBusinessObjectDefaultsProvider)locationOfGoodsCollection, "Code", "Property", new ZString("AUSYD"));
		}

		public void TestLocationQualifierList()
		{
			var qualifierType = Constants.RefCusCodeListTypes.IrelandQualifierType;
			helper.CreateNewOrGetExistingCusCodeType(qualifierType, "Ireland Qualifier Type");
			var eunCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var grouping = helper.CreateNewOrGetExistingDataGrouping(eunCode);
			var irelandCode = Core.Constants.CountryCodes.Ireland;
			var italyCode = Core.Constants.CountryCodes.Italy;
			helper.CreateNewOrGetExistingDataGrouping(irelandCode, parent: grouping);
			helper.CreateNewOrGetExistingDataGrouping(italyCode, parent: grouping);
			helper.CreateCusCodeList(irelandCode, qualifierType, "IEQUA1", "Ireland Qualifier Type 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(irelandCode, qualifierType, "IEQUA2", "Ireland Qualifier Type 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(italyCode, qualifierType, "ITQUA", "Italy Qualifier Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(eunCode, qualifierType, "ENUQUA", "EUN Qualifier Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var locationQualifierList = lookups.LocationQualifierList;

			CombineAssertions(() =>
			{
				AssertEquals("List should have 2 items.", 2, locationQualifierList.Count);
				AssertEquals("List should have item IELOC1.", true, locationQualifierList.ContainsCode("IEQUA1"));
				AssertEquals("List should have item IELOC2.", true, locationQualifierList.ContainsCode("IEQUA2"));

				AssertEquals("List should NOT have IT item.", false, locationQualifierList.ContainsCode("ITQUA"));
				AssertEquals("List should NOT have EU item.", false, locationQualifierList.ContainsCode("ENUQUA"));

				var newDeclaration = Factory.New<JobDeclaration>();
				newDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertSame("List should have been cached.", locationQualifierList, newDeclaration.Lookups.LocationQualifierList);
			});
		}

		public void TestLocationTypeList()
		{
			var locationType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.GoodsOfLocationType;
			helper.CreateNewOrGetExistingCusCodeType(locationType, "Location Types");
			var eunCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var grouping = helper.CreateNewOrGetExistingDataGrouping(eunCode);
			var irelandCode = Core.Constants.CountryCodes.Ireland;
			var italyCode = Core.Constants.CountryCodes.Italy;
			helper.CreateNewOrGetExistingDataGrouping(irelandCode, parent: grouping);
			helper.CreateNewOrGetExistingDataGrouping(italyCode, parent: grouping);
			helper.CreateCusCodeList(irelandCode, locationType, "IELOC1", "IE Location Type 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(irelandCode, locationType, "IELOC2", "IE Location Type 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(italyCode, locationType, "ITLOC", "Italy Location Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(eunCode, locationType, "ENULOC", "EUN Location", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var locationTypeList = lookups.LocationTypeList;

			CombineAssertions(() =>
			{
				AssertEquals("List should have 2 items.", 2, locationTypeList.Count);

				AssertEquals("List should have item IELOC1.", true, locationTypeList.ContainsCode("IELOC1"));
				AssertEquals("List should have item IELOC2.", true, locationTypeList.ContainsCode("IELOC2"));

				AssertEquals("List should NOT have IT item.", false, locationTypeList.ContainsCode("ITLOC"));
				AssertEquals("List should NOT have EU item.", false, locationTypeList.ContainsCode("ENULOC"));

				AssertSame("List should have been cached.", locationTypeList, Factory.New<JobDeclaration>().Lookups.LocationTypeList);
			});
		}
	}
}
