using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	class EMCSDocumentLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDocumentTypesList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSDocumentTypes, "Excise Movement Control System (EMCS) Doc Type");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSDocumentTypes, "0", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSDocumentTypes, "1", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSDocumentTypes, "11", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			CombineAssertions(() =>
			{
				var list = lookups.DocumentTypesList;
				list.Load();
				AssertContainsExactElementsInAnyOrder("LV-Codes", new ZString[] { "0", "1" }, list.Select(x => x.ZZD_Code));
				AssertSame("Cached", list, lookups.DocumentTypesList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<EMCSJobDeclaration>();
			var document = declaration.Documents.AddNew();
			lookups = document.Lookups;
		}
		EMCSDocumentLookups lookups;
	}
}
