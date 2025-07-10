using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class FilterDefaultsHelperTest : TestCase
	{
		public void TestAddDefaultValue()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummy = factory.New<DummyBusinessObject>();

			LookupField lookupField = new LookupField(factory);
			AssertEquals("Precondition", Guid.Empty, lookupField.Value);

			FilterDefaultsHelper.AddDefaultValue(lookupField, dummy);
			AssertEquals("Default was added", dummy.PK, lookupField.Value);

			MultipleSelectionLookup multipleSelectionLookup = new MultipleSelectionLookup(factory);
			multipleSelectionLookup.SetCollectionProvider(new DummyCollectionProvider(factory));
			AssertEquals("Precondition", 0, multipleSelectionLookup.BindToList.Count);
			AssertEquals(false, multipleSelectionLookup.IsHidden);

			FilterDefaultsHelper.AddDefaultValue(multipleSelectionLookup, dummy);
			AssertEquals("Default was added", 1, multipleSelectionLookup.BindToList.Count);
			AssertEquals("Default was added", true, multipleSelectionLookup.BindToList.Contains(dummy));
		}

		public void TestAddDefaultValueWithLookupStyle()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();

			var multipleSelectionLookup = new MultipleSelectionLookup(factory);
			multipleSelectionLookup.SetCollectionProvider(new DummyCollectionProvider(factory));
			multipleSelectionLookup.Style = MultipleSelectionLookup.Styles.None;

			AssertEquals("Precondition", 0, multipleSelectionLookup.BindToList.Count);
			AssertEquals(true, multipleSelectionLookup.IsHidden);

			FilterDefaultsHelper.AddDefaultValue(multipleSelectionLookup, dummy);

			AssertEquals("Default was NOT added", 0, multipleSelectionLookup.BindToList.Count);
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
