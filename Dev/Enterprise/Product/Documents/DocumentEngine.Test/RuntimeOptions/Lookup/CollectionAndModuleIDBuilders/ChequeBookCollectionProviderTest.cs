using System;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.LookupCollectionProviderTesting
{
	[TestedType(typeof(ChequeBookCollectionProvider))]
	sealed class ChequeBookCollectionProviderTest : CollectionProviderBaseTest
	{
		protected override Type ExpectedCollectionType => typeof(AccChequeBookCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.AccChequeBook;
	}
}
