using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(BMZoneCapacityMultiplier))]
	public class BMZoneCapacityMultiplierTest : EnterpriseBusinessObjectTestCase
	{
		public void TestClone()
		{
			var component = Factory.New<BMComponent>();
			var group = Factory.New<GlbGroup>();

			var zoneMultiplier = component.ZoneCapacityMultipliers.AddNew();
			zoneMultiplier.BZC_GG_ReleaseGroup = group.PK;
			zoneMultiplier.BZC_Zone0Multiplier = 90;

			var clone = (BMZoneCapacityMultiplier)zoneMultiplier.Clone();

			AssertEquals(group.PK, clone.BZC_GG_ReleaseGroup);
			AssertEquals(component.PK, clone.BZC_FC_Component);
			AssertEquals(90m, clone.BZC_Zone0Multiplier);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
