using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(AccInvMsgCollectionProvider))]
	sealed class AccInvMsgCollectionProviderTest : CollectionProviderWithCodeSupportBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(AccInvMsgCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.AccInvMsg;

		protected override int ExpectedMaxLength => AccInvMsgSchema.A9_Code.MaxLength;
	}
}
