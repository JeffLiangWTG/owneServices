using System;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	class CapacityReservationTrackerOldCalculatorTest : CapacityReservationTrackerTest
	{
		[TestDate(2013, 2, 6, 14, 31, 0)]
		public void TestRule_ThrottleZoneCapacity_FlatZoneMultipliers_ReleaseGroupHasOverride()
		{
			var throttleTester = new ThrottleZoneCapacityTester(Factory, system);

			var zoneModifier = Factory.New<BMZoneCapacityMultiplier>();
			zoneModifier.BZC_FC_Component = throttleTester.Buffer.PK;
			zoneModifier.BZC_Zone0Multiplier = 1;
			zoneModifier.BZC_Zone1Multiplier = 1;
			zoneModifier.BZC_Zone2Multiplier = 2;
			zoneModifier.BZC_Zone3Multiplier = 3;

			throttleTester.AssertTransferForZone(3, false, false);
			throttleTester.AssertTransferForZone(2, false, false);
			throttleTester.AssertTransferForZone(1, true, false);
			throttleTester.AssertTransferForZone(0, true, false);

			var releaseGroupZoneModifier = Factory.New<BMZoneCapacityMultiplier>();
			releaseGroupZoneModifier.BZC_FC_Component = throttleTester.Buffer.PK;
			releaseGroupZoneModifier.BZC_GG_ReleaseGroup = throttleTester.Group.PK;
			throttleTester.WorkflowInBuffer.FH_GG_ReleaseGroup = throttleTester.Group.PK;
			releaseGroupZoneModifier.BZC_Zone0Multiplier = 1;
			releaseGroupZoneModifier.BZC_Zone1Multiplier = 5;
			releaseGroupZoneModifier.BZC_Zone2Multiplier = 2;
			releaseGroupZoneModifier.BZC_Zone3Multiplier = 1;

			throttleTester.AssertTransferForZone(3, true, false);
			throttleTester.AssertTransferForZone(2, false, false);
			throttleTester.AssertTransferForZone(1, false, false);
			throttleTester.AssertTransferForZone(0, true, false);
		}

		[TestDate(2013, 2, 6, 14, 31, 0)]
		public void TestRule_ThrottleZoneCapacity_ReversedZoneMultipliers_ShouldUseDefaultBMComponentForReleaseGroup()
		{
			var throttleTester = new ThrottleZoneCapacityTester(Factory, system);

			var zoneModifier = Factory.New<BMZoneCapacityMultiplier>();
			zoneModifier.BZC_FC_Component = throttleTester.Buffer.PK;
			zoneModifier.BZC_Zone0Multiplier = 20;
			zoneModifier.BZC_Zone1Multiplier = 20;
			zoneModifier.BZC_Zone2Multiplier = 20;
			zoneModifier.BZC_Zone3Multiplier = 20;

			Factory.Save();

			throttleTester.AssertTransferForZone(3, false, false);
			throttleTester.AssertTransferForZone(2, false, false);
			throttleTester.AssertTransferForZone(1, false, false);
			throttleTester.AssertTransferForZone(0, false, false);

			throttleTester.WorkflowInBuffer.FH_GG_ReleaseGroup = throttleTester.Group.PK;

			throttleTester.AssertTransferForZone(3, false, false);
			throttleTester.AssertTransferForZone(2, false, false);
			throttleTester.AssertTransferForZone(1, false, false);
			throttleTester.AssertTransferForZone(0, false, false);
		}

		[TestDate(2013, 2, 6, 14, 31, 0)]
		public void TestRule_ThrottleZoneCapacity_StupidZoneMultipliers_withFlatReleaseGroup()
		{
			var throttleTester = new ThrottleZoneCapacityTester(Factory, system);

			var zoneModifier = Factory.New<BMZoneCapacityMultiplier>();
			zoneModifier.BZC_FC_Component = throttleTester.Buffer.PK;
			zoneModifier.BZC_GG_ReleaseGroup = ZGuid.Empty;
			zoneModifier.BZC_Zone0Multiplier = 20;
			zoneModifier.BZC_Zone1Multiplier = 20;
			zoneModifier.BZC_Zone2Multiplier = 20;
			zoneModifier.BZC_Zone3Multiplier = 20;

			throttleTester.AssertTransferForZone(3, false, false);
			throttleTester.AssertTransferForZone(2, false, false);
			throttleTester.AssertTransferForZone(1, false, false);
			throttleTester.AssertTransferForZone(0, false, false);

			var releaseGroupZoneModifier = Factory.New<BMZoneCapacityMultiplier>();
			releaseGroupZoneModifier.BZC_FC_Component = throttleTester.Buffer.PK;
			releaseGroupZoneModifier.BZC_GG_ReleaseGroup = throttleTester.Group.PK;
			throttleTester.WorkflowInBuffer.FH_GG_ReleaseGroup = throttleTester.Group.PK;
			releaseGroupZoneModifier.BZC_Zone0Multiplier = 1;
			releaseGroupZoneModifier.BZC_Zone1Multiplier = 1;
			releaseGroupZoneModifier.BZC_Zone2Multiplier = 1;
			releaseGroupZoneModifier.BZC_Zone3Multiplier = 1;

			throttleTester.AssertTransferForZone(3, true, false);
			throttleTester.AssertTransferForZone(2, true, false);
			throttleTester.AssertTransferForZone(1, true, false);
			throttleTester.AssertTransferForZone(0, true, false);
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSRegistry.Instance.EnableSimpleCapacityCalculationForAllSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
	}
}
