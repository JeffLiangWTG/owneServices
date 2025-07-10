using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(MeetingResourceCollectionProvider))]
	sealed class MeetingResourceCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(GlbResourceCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.GlbStaff;

		protected override int ExpectedMaxLength => GlbStaffSchema.GS_Code.MaxLength;
	}
}
