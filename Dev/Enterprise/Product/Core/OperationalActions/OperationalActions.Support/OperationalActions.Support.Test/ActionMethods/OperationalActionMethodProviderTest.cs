using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Services.OperationalActions.Support.Testing
{
	[TestsSubclassesOf(typeof(OperationalActionMethodProvider))]
	public abstract class OperationalActionMethodProviderTest : TestCase
	{
		public void TestCreate()
		{
			OperationalActionMethodProvider newProvider = OperationalActionMethodProvider.New(ID);
			AssertNotNull(newProvider);
			AssertEquals("method provider is of the wrong type", TestedTypeHelper.GetTestedType(GetType()), newProvider.GetType());
		}

		#region Implementation
		public OperationalActionMethodProvider Provider
		{
			get
			{
				return provider ?? (provider = OperationalActionMethodProvider.New(ID));
			}
		}

		OperationalActionMethodProvider provider;

		protected abstract ActionMethodProviderID ID { get; }

		#endregion
	}
}
