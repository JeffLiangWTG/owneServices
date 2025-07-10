using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(GbCustomsOfficeCodeCollection))]
	public class GbCustomsOfficeCodeCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllUKCustomsOfficesWithRequiredRoles()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			new CustomsOfficeCodeTestHelper(helper);
			var minDate = ZDateTime.MinSmallDateTimeValue;
			var maxDate = ZDateTime.MaxSmallDateTimeValue;
			helper.CreateNewOrGetExistingDataGrouping(CountryCodes.UnitedKingdom);
			helper.CreateCusCodeList(CountryCodes.UnitedKingdom, RefCusCodeListTypes.CustomsOffice, "GB000001", minDate, maxDate);
			helper.CreateCusCodeList(CountryCodes.UnitedKingdom, RefCusCodeListTypes.CustomsOffice, "XI000001", minDate, maxDate);
			helper.CreateCusCodeListWithAttribute(CountryCodes.UnitedKingdom, RefCusCodeListTypes.CustomsOffice, "GB000002", "GB000002 EXT", minDate, maxDate, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfExit);
			Factory.Save();

			var collection = GbCustomsOfficeCodeCollection.AllUKCustomsOfficesWithRequiredRoles(Factory);
			AssertCollectionContainsCusCodes(collection, "GB000001", "GB000002");

			collection = GbCustomsOfficeCodeCollection.AllUKCustomsOfficesWithRequiredRoles(Factory, EuOfficeCodesTypes.Codes.OfficeOfExit);
			AssertCollectionContainsCusCodes(collection, "GB000002");
		}

		void AssertCollectionContainsCusCodes(GbCustomsOfficeCodeCollection collection, params ZString[] expectedCodes)
		{
			collection.Load();
			AssertContainsExactElementsInAnyOrder(expectedCodes, collection.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
		}

		protected override BusinessObjectCollection GetCollectionToTest() => GbCustomsOfficeCodeCollection.AllUKCustomsOfficesWithRequiredRoles(Factory);
	}
}
