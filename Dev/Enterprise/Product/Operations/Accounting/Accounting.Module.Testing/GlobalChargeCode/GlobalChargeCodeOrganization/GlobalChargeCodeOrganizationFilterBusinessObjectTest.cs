using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(GlobalChargeCodeOrganizationFilterBusinessObject))]
	public class GlobalChargeCodeOrganizationFilterBusinessObjectTest : GlobalChargeCodeFilterBusinessObjectTest
	{
		#region Implementation

		protected OrgHeader Org1;

		protected override void SetUp()
		{
			Org1 = Factory.NewWithValidTestData<OrgHeader>();
			base.SetUp();
		}

		protected override GlobalChargeCodeMap GetGlobalChargeCode
		{
			get
			{
				GlobalChargeCodeMapOrganization globalChargeCode = Factory.NewWithValidTestData<GlobalChargeCodeMapOrganization>();
				globalChargeCode.YG_OH = Org1.PK;
				return globalChargeCode;
			}
		}

		protected override BusinessObjectCollection GetGlobalChargeCodeCollection
		{
			get { return new GlobalChargeCodeMapOrganizationCollection(Factory); }
		}

		protected override BusinessObjectCollection GetGlobalChargeCodePivotCollection
		{
			get { return new GlobalChargeCodeMapPivotOrganizationCollection(Factory); }
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlobalChargeCodeOrganizationFilterBusinessObject();
		}

		#endregion

		public void TestOrganizationFilter()
		{
			ModuleGuidFilter filter = (ModuleGuidFilter)FilterBO["Organization"];

			filter.Property = Org1.PK;
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
