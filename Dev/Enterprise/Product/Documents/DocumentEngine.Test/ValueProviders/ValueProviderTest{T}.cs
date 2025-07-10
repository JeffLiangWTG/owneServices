using System;

namespace Enterprise.DocumentEngine.ValueProviders.Testing
{
	public abstract class ValueProviderTest<T> : ValueProviderTest where T : ValueProvider, new()
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new T();
		}

		protected override Type ValueProviderType
		{
			get { return typeof(T); }
		}

		protected new T ValueProviderToTest
		{
			get { return (T)base.ValueProviderToTest; }
		}
	}
}
