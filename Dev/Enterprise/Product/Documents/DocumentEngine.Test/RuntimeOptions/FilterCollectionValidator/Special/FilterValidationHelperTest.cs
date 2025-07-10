using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class FilterValidationHelperTest : TestCase
	{
		public void TestContainsOnlyAllowed()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject allowedDummy = factory.New<DummyBusinessObject>();

			LookupField lookupField = new LookupField(factory);
			lookupField.Value = Guid.Empty;
			AssertEquals(false, FilterValidationHelper.FilterContainsOnlyAllowed(lookupField, allowedDummy));

			lookupField.Value = Guid.NewGuid();
			AssertEquals(false, FilterValidationHelper.FilterContainsOnlyAllowed(lookupField, allowedDummy));

			Assert(!FilterValidationHelper.FilterContainsOnlyAllowed(lookupField, allowedDummy, null));

			lookupField.Value = allowedDummy.PK.ToGuid();
			AssertEquals(true, FilterValidationHelper.FilterContainsOnlyAllowed(lookupField, allowedDummy));

			MultipleSelectionLookup multipleSelectionLookup = new MultipleSelectionLookup(factory);
			multipleSelectionLookup.SetCollectionProvider(new DummyCollectionProvider(factory));
			AssertEquals(false, FilterValidationHelper.FilterContainsOnlyAllowed(multipleSelectionLookup, allowedDummy));

			DummyBusinessObject disallowedDummy = factory.New<DummyBusinessObject>();
			multipleSelectionLookup.BindToList.Add(disallowedDummy);
			AssertEquals(false, FilterValidationHelper.FilterContainsOnlyAllowed(multipleSelectionLookup, allowedDummy));

			multipleSelectionLookup.BindToList.Add(allowedDummy);
			AssertEquals(false, FilterValidationHelper.FilterContainsOnlyAllowed(multipleSelectionLookup, allowedDummy));

			multipleSelectionLookup.BindToList.RemoveFromRelationship(disallowedDummy);
			AssertEquals(true, FilterValidationHelper.FilterContainsOnlyAllowed(multipleSelectionLookup, allowedDummy));

			DummyBusinessObject anotherAllowedDummy = factory.New<DummyBusinessObject>();
			multipleSelectionLookup.BindToList.Add(anotherAllowedDummy);
			AssertEquals(true, FilterValidationHelper.FilterContainsOnlyAllowed(multipleSelectionLookup, allowedDummy, anotherAllowedDummy));

			multipleSelectionLookup.BindToList.Add(disallowedDummy);
			AssertEquals(false, FilterValidationHelper.FilterContainsOnlyAllowed(multipleSelectionLookup, allowedDummy, anotherAllowedDummy));
		}

		#region Implementation

		class DummyCollectionProvider : CollectionProvider
		{
			public DummyCollectionProvider(BusinessObjectFactory businessObjectFactory)
				: base(businessObjectFactory)
			{
			}

			protected override IBusinessObjectCollection CreateCollection()
			{
				return new DummyBusinessObjectCollection(BusinessObjectFactory);
			}

			public override Enterprise.ZArchitecture.Modules.ModuleIdentifier ModuleID
			{
				get { throw new NotImplementedException(); }
			}
		}

		#endregion
	}
}
