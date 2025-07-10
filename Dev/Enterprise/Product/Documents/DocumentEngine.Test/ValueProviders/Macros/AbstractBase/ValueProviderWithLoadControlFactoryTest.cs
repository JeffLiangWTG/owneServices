using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestsSubclassesOf(typeof(ValueProviderWithLoadControlFactory))]
	public abstract class ValueProviderWithLoadControlFactoryTest<T> : ValueProviderTest where T : ValueProviderWithLoadControlFactory, new()
	{
		public void TestIfUsingFactoryExceedsTheMaximumTheCounterRestartsAndFactoryGetsNew()
		{
			ValueProvider = GetNewValueProviderWithLoadControlFactory();
			ValueProvider.MaxNumberOfObjectsCanBeHeldByFactory = 2;
			BusinessObjectFactory factory = ValueProvider.FactoryForTesting;
			factory.New(typeof(MasterFiles.Business.OrgHeader));
			factory.New(typeof(MasterFiles.Business.OrgHeader));
			factory.New(typeof(MasterFiles.Business.OrgHeader));
			ValueProvider.ReplaceFactoryIfRequired();
			AssertEquals(ValueProvider.ShouldResetFactory ? 0 : 3, ValueProvider.NumberOfObjectsInFactory);
			AssertEquals(!ValueProvider.ShouldResetFactory, factory.Equals(ValueProvider.FactoryForTesting));
		}

		public void TestMaxNumberOfRuns()
		{
			var provider = GetNewValueProviderWithLoadControlFactory();
			AssertEquals("Default value should be 100", 100, provider.MaxNumberOfObjectsCanBeHeldByFactory);

			provider.MaxNumberOfObjectsCanBeHeldByFactory = 2;
			AssertEquals(2, provider.MaxNumberOfObjectsCanBeHeldByFactory);
		}

		public abstract void TestReplacement();
		public abstract void TestIsResponsibleForReplacing();

		#region Implementation

		protected ValueProviderWithLoadControlFactory ValueProvider;
		protected BusinessObjectFactory ValueProviderFactory
		{
			get { return ValueProvider.FactoryForTesting; }
		}

		protected int ValueProviderMaxNumberOfObjectsCanBeHeldByFactory
		{
			get { return ValueProvider.MaxNumberOfObjectsCanBeHeldByFactory; }
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return GetNewValueProviderWithLoadControlFactory();
		}

		protected virtual ValueProviderWithLoadControlFactory GetNewValueProviderWithLoadControlFactory()
		{
			return new T();
		}

		protected override void TearDown()
		{
			base.TearDown();
			ValueProviderWithLoadControlFactory.ResetFactoryForTesting();
		}

		#endregion
	}
}
