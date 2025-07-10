using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business.Testing
{
	public abstract class AmountBasedAuthorisationRequirementCollectionTest<T> : RegistryBusinessObjectCollectionTemplateTestCase<T> where T : AmountBasedAuthorisationRequirementCollection
	{
		public void TestSort()
		{
			AmountBasedMultiLevelAuthorisationRequirement setting1 = Collection.AddNew();
			AmountBasedMultiLevelAuthorisationRequirement setting2 = Collection.AddNew();
			AmountBasedMultiLevelAuthorisationRequirement setting3 = Collection.AddNew();
			AmountBasedMultiLevelAuthorisationRequirement setting4 = Collection.AddNew();

			setting1.Range = "Up to";
			setting1.Amount = 100;
			setting2.Range = "Above";
			setting2.Amount = 300;
			setting3.Range = "Up to";
			setting3.Amount = 300;
			setting4.Range = "Up to";
			setting4.Amount = 200;

			AssertEquals("First row should be setting 1", setting1, Collection[0]);
			AssertEquals("Second row should be setting 2", setting2, Collection[1]);
			AssertEquals("Third row should be setting 3", setting3, Collection[2]);
			AssertEquals("Forth row should be setting 4", setting4, Collection[3]);

			Collection.Sort();

			AssertEquals("First row should be setting 1", setting1, Collection[0]);
			AssertEquals("Second row should be setting 4", setting4, Collection[1]);
			AssertEquals("Third row should be setting 3", setting3, Collection[2]);
			AssertEquals("Forth row should be setting 2", setting2, Collection[3]);
		}

		public void TestCurrentFallbackCompanyCurrencyDecimals()
		{
			BusinessObject company = (BusinessObject)Factory.LoadTop1<IGlbCompany>(new ZQuery());
			BusinessObject currency = (BusinessObject)Factory.LoadTop1<IRefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, company[GlbCompanySchema.GC_RX_NKLocalCurrency]));
			DummyAmountBasedAuthorisationRequirementCollection collection = new DummyAmountBasedAuthorisationRequirementCollection(new FallbackLevel(company.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			AssertEquals("Before update", 2, collection.CurrentFallbackCompanyCurrencyDecimals);
			currency[RefCurrencySchema.RX_SubUnitRatio] = 10000;
			Factory.Save();
			collection = new DummyAmountBasedAuthorisationRequirementCollection(new FallbackLevel(company.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			AssertEquals("After update", 4, collection.CurrentFallbackCompanyCurrencyDecimals);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
