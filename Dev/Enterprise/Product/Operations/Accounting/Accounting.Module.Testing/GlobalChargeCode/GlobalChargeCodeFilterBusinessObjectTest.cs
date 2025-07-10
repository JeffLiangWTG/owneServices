using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class GlobalChargeCodeFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected abstract GlobalChargeCodeMap GetGlobalChargeCode { get; }
		protected abstract BusinessObjectCollection GetGlobalChargeCodeCollection { get; }
		protected abstract BusinessObjectCollection GetGlobalChargeCodePivotCollection { get; }

		protected AccChargeCode chargeCode1;
		protected AccChargeCode chargeCode2;
		protected GlobalChargeCodeMap globalChargeCode;
		protected GlobalChargeCodeMapPivot pivot1;
		protected GlobalChargeCodeMapPivot pivot2;
		protected GlobalChargeCodeMapPivot pivot3;
		protected GlobalChargeCodeFilterBusinessObject FilterBO;
		protected BusinessObjectCollection collection;
		protected BusinessObjectCollection pivotsCollection;

		protected override void SetUp()
		{
			base.SetUp();

			chargeCode1 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode1.AC_Code = "TEST1";

			chargeCode2 = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode2.AC_Code = "TEST2";

			globalChargeCode = GetGlobalChargeCode;
			globalChargeCode.YG_Code = "GLBCRGCD1";
			globalChargeCode.YG_Desc = "Global Charge Code 1";

			collection = GetGlobalChargeCodeCollection;
			pivotsCollection = GetGlobalChargeCodePivotCollection;

			pivot1 = (GlobalChargeCodeMapPivot)pivotsCollection.AddNew();
			pivot1.YP_AC = chargeCode1.PK;
			pivot1.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			pivot1.YP_YG = globalChargeCode.PK;

			pivot2 = (GlobalChargeCodeMapPivot)pivotsCollection.AddNew();
			pivot2.YP_AC = chargeCode2.PK;
			pivot2.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsReceivable;
			pivot2.YP_YG = globalChargeCode.PK;

			pivot3 = (GlobalChargeCodeMapPivot)pivotsCollection.AddNew();
			pivot3.YP_AC = chargeCode1.PK;
			pivot3.YP_TYPE = ZArchitecture.Core.LedgerTypes.AccountsPayable;
			pivot3.YP_YG = globalChargeCode.PK;

			Factory.Save();

			FilterBO = (GlobalChargeCodeFilterBusinessObject)GetNewFilterStripBusinessObject();
		}

		public void TestCodeFilter()
		{
			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Code"];

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "GLBCRGCD1";
			filter.IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(globalChargeCode));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "XXX";
			filter.IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals(0, collection.Count);
		}

		public void TestDescriptionFilter()
		{
			ModuleTextFilter filter = (ModuleTextFilter)FilterBO["Description"];

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "Global Charge Code 1";
			filter.IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(globalChargeCode));

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = "XXX";
			filter.IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals(0, collection.Count);
		}

		public void TestAPChargeCodeFilter()
		{
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["AP Charge Code"];

			filter.Property = chargeCode1.PK;
			filter.IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(globalChargeCode));

			filter.Property = chargeCode2.PK;
			filter.IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals(0, collection.Count);
		}

		public void TestARChargeCodeFilter()
		{
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["AR Charge Code"];

			filter.Property = chargeCode1.PK;
			filter.IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(globalChargeCode));

			filter.Property = chargeCode2.PK;
			filter.IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(globalChargeCode));

			filter.Property = ZGuid.NewZGuid();
			filter.IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals(0, collection.Count);
		}
	}
}
