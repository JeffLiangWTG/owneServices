using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(BankAccountCollectionProvider))]
	sealed class BankAccountCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(AccBankAccountCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.AccBankAccount;
	}
}
