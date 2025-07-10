using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	class JobDeclarationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentGrouping = helper.CreateNewOrGetExistingDataGrouping("ZZ", "ParentGrouping");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Botswana, "Botswana", parentGrouping);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsUQ");
			var cusCode = helper.CreateCusCodeList(Core.Constants.CountryCodes.Botswana, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ARIA", "Ariamsvlei", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var cusCode1 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Botswana, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "LUDE", "Luderitz", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "ZZxx", "ZZxxx", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var dec = Factory.New<JobDeclaration>();
			var list = dec.Lookups.CustomsOfficeList;
			AssertEquals(2, list.Count);
			AssertEquals("Ariamsvlei", list.GetDescriptionFromCode(cusCode.ZZD_Code));
			AssertEquals("Luderitz", list.GetDescriptionFromCode(cusCode1.ZZD_Code));
		}

		public void TestPaymentPartyList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, "ParentGrouping");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Botswana, "Botswana", parentGrouping);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "MOP");
			var cusCode = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "C", "Cash", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var cusCode1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "D", "Defer", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var dec = Factory.New<JobDeclaration>();
			var list = dec.Lookups.PaymentPartyList;
			AssertEquals(2, list.Count);
			AssertEquals("Cash", list.GetDescriptionFromCode(cusCode.ZZD_Code));
			AssertEquals("Defer", list.GetDescriptionFromCode(cusCode1.ZZD_Code));
		}

		public void TestRepresentativeList()
		{
			var lookups = new JobDeclarationLookups(Factory.New<JobDeclaration>());
			var list = lookups.RepresentativeList;
			AssertType<BrokerCollection>(list);
			AssertEquals("RepresentativeList should not be loaded by the property", false, list.IsLoaded);
		}

		public void TestEntryStatusList()
		{
			Factory.SetupEntryStatusList();
			var dec = Factory.New<JobDeclaration>();
			var lookups = new JobDeclarationLookups(dec);
			var entryStyleList = lookups.EntryStatusList;
			AssertContainsExactElementsInAnyOrder(new ZString[] { "ST1", "ST2", "ST3" }, entryStyleList.GetAllCodes());
		}
	}
}
