using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	class CusExitDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		[ExpectNoExceptions]
		public void TestOrgHeaderCollection()
		{
			NUnit.Framework.Assert.That(lookups.OrgHeaderCollection, Is.TypeOf<OrgHeaderCollection>());
		}

		[ExpectNoExceptions]
		public void TestStatusList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "Export Customs Status");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "123", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "515", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			NUnit.Framework.Assert.Multiple(() =>
			{
				var list = lookups.StatusList;
				NUnit.Framework.Assert.That(list.CodesAsString, Is.EqualTo("123, 515"), "CodesAsString");
				NUnit.Framework.Assert.That(lookups.StatusList, Is.SameAs(list), "Cached");
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var exitDetail = Factory.NewWithValidTestData<CusExitDetail>();
			lookups = exitDetail.Lookups;
		}
		CusExitDetailLookups lookups;
	}
}
