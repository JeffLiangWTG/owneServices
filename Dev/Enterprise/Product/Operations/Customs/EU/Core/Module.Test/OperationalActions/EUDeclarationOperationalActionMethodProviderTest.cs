using System;
using System.Linq;
using Enterprise.Customs.EU.Module.OperationalActions;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing.OperationalActions
{
	[TestedType(typeof(EUDeclarationOperationalActionMethodProvider))]
	public class EUDeclarationOperationalActionMethodProviderTest : OperationalActionMethodProviderTest
	{
		protected override ActionMethodProviderID ID => ActionMethodProviderIDs.EUJobDeclaration;

		public void TestNewMethods()
		{
			var provider = new EUDeclarationOperationalActionMethodProvider();
			AssertContainsExactElementsInAnyOrder(new Type[] { typeof(DeclarationUpdateSupportingDocumentsOperationalActionMethod), typeof(DeclarationUpdatePreviousDocumentsOperationalActionMethod) }, provider.NewMethods(null).Select(x => x.GetType()));
		}
	}
}
