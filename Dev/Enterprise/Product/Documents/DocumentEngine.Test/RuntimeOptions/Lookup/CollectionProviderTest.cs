using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.Security;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class CollectionProviderTest : TestCase
	{
		[ExpectException(typeof(DocumentEngineException))]
		public void TestGetCollectionForFindboxCallsCreateCollection()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			TestCollectionProvider provider = new TestCollectionProvider(factory);
			IBusinessObjectCollection findBoxCollectoin = provider.CollectionForFindbox;
		}

		public void TestSetFilterCollection()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			CollectionProvider provider = new Enterprise.DocumentEngine.RuntimeOptions.OrgHeaderCollectionProvider(factory);
			provider.SetFilterCollection(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			((BusinessObjectCollection)provider.Collection).Load();
			((BusinessObjectCollection)provider.CollectionForFindbox).Load();
			AssertEquals("find box collection should be readonly", true, provider.CollectionForFindbox.ReadOnly);
			AssertEquals("Provider's collection should have values", true, provider.Collection.Count > 0);
			AssertEquals("Provider's find box collection should have values", true, provider.CollectionForFindbox.Count > 0);

			provider.SetFilterCollection(new ZQuery(OrgHeaderSchema.OH_Code, "AALSHI"));
			((BusinessObjectCollection)provider.Collection).Load();
			((BusinessObjectCollection)provider.CollectionForFindbox).Load();
			AssertEquals("find box collection should be readonly", true, provider.CollectionForFindbox.ReadOnly);
			AssertEquals("Providers collection should only have 1 item now", 1, provider.Collection.Count);
			AssertEquals("Providers find box collection should only have 1 item now", 1, provider.CollectionForFindbox.Count);
		}

		public static void AssertValidationAndDefaultAdded(CollectionProvider collectionProvider, SecurityCheckpoint securityCheckpoint, Type validationTypeExpected, ZGuid defaultValueExpected)
		{
			ValidatorPack validatorPack = new ValidatorPack();
			LookupField filterField = new LookupField(new BusinessObjectFactory());
			filterField.SetCollectionProvider(collectionProvider);

			securityCheckpoint.IsAllowed = true;
			collectionProvider.AddValidationAndDefault(filterField, validatorPack);
			AssertEquals("No conditional validator added", 0, filterField.Validators.Count);

			securityCheckpoint.IsAllowed = false;
			collectionProvider.AddValidationAndDefault(filterField, validatorPack);
			AssertEquals("Conditional validator has been added", 1, filterField.Validators.Count);
			AssertEquals("Conditional validator has expected type", validationTypeExpected, filterField.Validators[0].GetType());
			AssertEquals("Default value has been added", defaultValueExpected, filterField.Value);
		}
	}
}
