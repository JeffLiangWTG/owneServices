using System;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMReleaseSequenceFilterBusinessObject))]
	class BMReleaseSequenceFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new BMReleaseSequenceFilterBusinessObject();
		}

		public void TestNameFilter()
		{
			var filter = new BMReleaseSequenceFilterBusinessObject();
			((ModuleTextFilter)filter["Name"]).Property = "Name1";
			((ModuleTextFilter)filter["Name"]).IsActive = true;

			var results = Factory.Load<BMReleaseSequence>(filter.Filter);
			AssertCollectionContains(seq1, results);
			AssertCollectionNotContains(seq2, results);
		}

		public void TestReleaseGroupFilter()
		{
			var filter = new BMReleaseSequenceFilterBusinessObject();
			((ModuleGuidFilter)filter["ReleaseGroup"]).Property = group2.PK;
			((ModuleGuidFilter)filter["ReleaseGroup"]).IsActive = true;

			var results = Factory.Load<BMReleaseSequence>(filter.Filter);
			AssertCollectionContains(seq2, results);
			AssertCollectionNotContains(seq1, results);
		}

		public void TestFilters()
		{
			var filter = new BMReleaseSequenceFilterBusinessObject();
			((ModuleGuidFilter)filter["Capability"]).Property = capability.PK;
			((ModuleGuidFilter)filter["Capability"]).IsActive = true;

			var results = Factory.Load<BMReleaseSequence>(filter.Filter);
			AssertCollectionContains(seq1, results);
			AssertCollectionNotContains(seq2, results);
		}

		BMReleaseSequence seq1, seq2;
		GlbGroup group1, group2;
		GlbCapability capability;

		protected override void SetUp()
		{
			base.SetUp();

			var (system, buffer) = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG");
			capability = BMSTestHelper.CreateCapability(Factory, code: "CAP", isGroupScope: false);
			group1 = BMSTestHelper.CreateGroup(Factory, code: "GR1");
			group2 = BMSTestHelper.CreateGroup(Factory, code: "GR2");

			seq1 = Factory.NewWithValidTestData<BMReleaseSequence>();
			seq2 = Factory.NewWithValidTestData<BMReleaseSequence>();

			seq1.BMR_Name = "Name1";
			seq1.BMR_GG_ReleaseGroup = group1.PK;
			seq1.BMR_G4_Capability = capability.PK;
			seq1.BMR_IsActive = true;

			seq2.BMR_Name = "Name2";
			seq2.BMR_GG_ReleaseGroup = group2.PK;
			seq2.BMR_G4_Capability = ZGuid.Empty;
			seq2.BMR_IsActive = false;

			Factory.Save();
		}
	}
}
