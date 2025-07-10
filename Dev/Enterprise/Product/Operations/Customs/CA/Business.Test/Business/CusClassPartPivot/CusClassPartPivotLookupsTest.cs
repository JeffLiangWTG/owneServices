using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusClassPartPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDefaultOriginsAndStateLookups()
		{
			AssertEquals(typeof(RefCountryCollection), Lookups.DefaultOrigins.GetType());
			AssertEquals(typeof(USStatesList), Lookups.StatesOfExport.GetType());
		}

		public void TestParent()
		{
			AssertEquals(Pivot, Lookups.Parent);
		}

		public void TestClassificationTypes()
		{
			AssertEquals(typeof(ClassificationTypeList), Lookups.ClassificationTypes.GetType());
		}

		public void TestTariffs()
		{
			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals(typeof(CACClassCollection), Lookups.Tariffs.GetType());

			Pivot.CI_ChildType = ClassificationTypeList.Codes.SHB;
			AssertEquals(typeof(CACExportTariffCollection), Lookups.Tariffs.GetType());

			Pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			AssertEquals(typeof(CACClassCollection), Lookups.Tariffs.GetType());
		}

		public void TestManufacturers()
		{
			AssertEquals(typeof(OrgHeaderCollection), Lookups.Manufacturers.GetType());
		}

		public void TestAuthorityNumberList()
		{
			var part = Factory.New<OrgSupplierPart>();
			var pivot1 = part.PivotsForBinding.AddNew();
			pivot1.CCA_AuthorityNumber = "C0001";

			AssertType<CACusRulingFindBoxCollection>(pivot1.Lookups.AuthorityNumberList);
		}

		#region Implementation
		CusClassPartPivot Pivot
		{
			get { return pivot ?? (pivot = Factory.New<CusClassPartPivot>()); }
		}
		CusClassPartPivot pivot;

		CusClassPartPivotLookups Lookups
		{
			get { return Pivot.Lookups; }
		}
		#endregion
	}
}
