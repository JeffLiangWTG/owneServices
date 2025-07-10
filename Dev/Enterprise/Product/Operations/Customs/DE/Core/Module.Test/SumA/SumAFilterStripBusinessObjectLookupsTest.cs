using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.Module.Testing
{
	public class SumAFilterStripBusinessObjectLookupsTest : TestCaseWithFactory
	{
		public void TestOrganisationList()
		{
			AssertEquals("OrganisationList should not be loaded by the property", false, ((IBusinessObjectCollection)lookups.OrganisationList).IsLoaded);
		}

		public void TestBranchList()
		{
			AssertEquals("BranchList should not be loaded by the property", false, ((IBusinessObjectCollection)lookups.BranchList).IsLoaded);
		}

		public void TestLoadingList()
		{
			AssertEquals("LoadingList should not be loaded by the property", false, ((IBusinessObjectCollection)lookups.LoadingList).IsLoaded);
		}

		public void TestCustomsOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE003401", "Bad Hersfeld", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE006206", "Rendsburg", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE001514", "Hannover", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			CombineAssertions(() =>
			{
				var customsOfficeList = lookups.CustomsOfficeList;
				AssertSame("Cached", customsOfficeList, lookups.CustomsOfficeList);
				AssertEquals("Ordered by Description", "DE003401, DE001514, DE006206", customsOfficeList.CodesAsString);
			});
		}

		public void TestTransportTypeList()
		{
			AssertSame(lookups.TransportTypeList, lookups.TransportTypeList);
		}

		public void TestApplicationCodeList()
		{
			AssertSame(lookups.ApplicationCodeList, lookups.ApplicationCodeList);
		}

		public void TestDeclarationTypeList()
		{
			AssertSame(lookups.DeclarationTypeList, lookups.DeclarationTypeList);
		}

		public void TestMessageStatusList()
		{
			AssertSame(lookups.MessageStatusList, lookups.MessageStatusList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			lookups = new SumAFilterStripBusinessObjectLookups(new SumAFilterStripBusinessObject());
		}
		SumAFilterStripBusinessObjectLookups lookups;
	}
}
