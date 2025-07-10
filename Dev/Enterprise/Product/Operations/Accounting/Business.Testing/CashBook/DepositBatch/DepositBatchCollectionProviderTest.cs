using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch.Testing
{
	[TestedType(typeof(DepositBatchCollectionProvider))]
	public class DepositBatchCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(DepositBatchModuleCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.DepositBatch;
	}
}
