using System;
using System.Linq;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Module.Testing
{
	[TestedType(typeof(EDICommunicationsModeOperationalActionMethodProvider))]
	public class EDICommunicationsModeOperationalActionMethodProviderTest : OperationalActionMethodProviderTest
	{
		protected override ActionMethodProviderID ID => ActionMethodProviderIDs.EDICommunicationsMode;

		public void TestNewMethods()
		{
			var provider = new EDICommunicationsModeOperationalActionMethodProvider();
			var expectedMethodTypes = new Type[]
			{
				typeof(EDICommunicationsModeOperationalActionMethod),
			};
			AssertContainsExactElementsInAnyOrder(expectedMethodTypes, provider.NewMethods(null).Select(x => x.GetType()));
		}
	}
}
