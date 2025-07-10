using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(AccComplianceSequenceCollectionProvider))]
	sealed class AccComplianceSequenceCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(AccComplianceSequenceCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.AccComplianceSequence;

		protected override int ExpectedMaxLength => AccComplianceSequenceSchema.XD_Code.MaxLength;
	}
}
