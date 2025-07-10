using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AccAlternateGLAccountCollectionProvider))]
	sealed class AccAlternateGLAccountCollectionProviderTest : CollectionProviderBaseTest
	{
		public void TestValidationAndDefaultAdded()
		{
			var filterField = new LookupField(new BusinessObjectFactory());
			filterField.SetCollectionProvider(Provider);

			Provider.AddValidationAndDefault(filterField, null);
			AssertEquals("Conditional validator has been added", 1, filterField.Validators.Count);
			AssertEquals("Conditional validator has expected type", typeof(AccAlternateGLAccountLookupTypeValidator), filterField.Validators[0].GetType());
		}

		protected override Type ExpectedCollectionType => typeof(AlternateGLAccountCombineParentAccountCollection);

		protected override ModuleIdentifier ExpectedModuleID => ModuleIDs.AlternateGLAccounts;
	}
}
