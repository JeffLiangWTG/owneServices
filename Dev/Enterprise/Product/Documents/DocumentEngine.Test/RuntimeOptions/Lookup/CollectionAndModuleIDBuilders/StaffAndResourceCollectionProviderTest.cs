using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(StaffAndResourceCollectionProvider))]
	sealed class StaffAndResourceCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(GlbStaffAndResourceCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.GlbStaff;

		protected override int ExpectedMaxLength => GlbStaffSchema.GS_Code.MaxLength;
	}
}
