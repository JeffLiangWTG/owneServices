using System;
using System.Linq;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.OperationalActions.Testing
{
	[TestedType(typeof(FrDeclarationOperationalActionMethodProvider))]
	class FrDeclarationOperationalActionMethodProviderTest : OperationalActionMethodProviderTest
	{
		protected override ActionMethodProviderID ID => ActionMethodProviderIDs.FrJobDeclaration;

		public void TestNewMethods()
		{
			var provider = new FrDeclarationOperationalActionMethodProvider();
			AssertContainsExactElementsInAnyOrder(new Type[] {
				typeof(FrDeclarationDeltaSecondStepMessageOperationalActionMethod),
				typeof(FrDeclarationSendValideeMessageOperationalActionMethod),
				typeof(FrDeclarationCreditD48OperationalActionMethod),
				typeof(FrDeclarationSendPrelodgeAmendmentOperationalActionMethod),
				typeof(CreditCODOperationalActionMethod),
				typeof(SendCancellationMessageOperationalActionMethod)
			}, provider.NewMethods(null).Select(x => x.GetType()));
		}
	}
}
