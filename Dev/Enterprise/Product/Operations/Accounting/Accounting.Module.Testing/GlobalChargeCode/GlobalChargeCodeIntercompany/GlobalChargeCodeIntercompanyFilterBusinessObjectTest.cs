using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GlobalChargeCodeIntercompanyFilterBusinessObject))]
	public class GlobalChargeCodeIntercompanyFilterBusinessObjectTest : GlobalChargeCodeFilterBusinessObjectTest
	{
		OrgHeader localclient1;
		OrgHeader localclient2;
		OrgHeader localclient3;

		protected override void SetUp()
		{
			base.SetUp();
			// add some setup to test the local client override
			localclient1 = Factory.NewWithValidTestData<OrgHeader>();
			localclient2 = Factory.NewWithValidTestData<OrgHeader>();
			localclient3 = Factory.NewWithValidTestData<OrgHeader>();
			pivot1.YP_OH_LocalClientOverride = localclient1.PK;
			pivot2.YP_OH_LocalClientOverride = localclient2.PK;
			Factory.Save();
		}

		public void TestLocalClientOverrideFilter()
		{
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Job Local Client Override"];

			filter.Property = localclient1.PK;
			filter.IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(globalChargeCode));

			filter.Property = localclient3.PK;
			filter.IsActive = true;
			collection.Load(FilterBO.Filter);
			AssertEquals(0, collection.Count);
		}

		#region Implementation

		protected override GlobalChargeCodeMap GetGlobalChargeCode
		{
			get { return Factory.NewWithValidTestData<GlobalChargeCodeMapIntercompany>(); }
		}

		protected override BusinessObjectCollection GetGlobalChargeCodeCollection
		{
			get { return new GlobalChargeCodeMapIntercompanyCollection(Factory); }
		}

		protected override BusinessObjectCollection GetGlobalChargeCodePivotCollection
		{
			get { return new GlobalChargeCodeMapPivotIntercompanyCollection(Factory); }
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlobalChargeCodeIntercompanyFilterBusinessObject();
		}

		#endregion
	}
}
