using System;
using System.Linq;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.OperationalActions.Testing
{
	[TestedType(typeof(EntryHeaderOperationalActionMethodProvider))]
	public class EntryHeaderOperationalActionMethodProviderTest : OperationalActionMethodProviderTest
	{
		protected override ActionMethodProviderID ID => ActionMethodProviderIDs.FRCusEntry;

		public void TestNewMethods()
		{
			var provider = new EntryHeaderOperationalActionMethodProvider();
			AssertContainsExactElementsInAnyOrder(new Type[] {
				typeof(CreditCODOperationalActionMethod), typeof(SendCancellationMessageOperationalActionMethod)
			}, provider.NewMethods(null).Select(x => x.GetType()));
		}
	}
}
