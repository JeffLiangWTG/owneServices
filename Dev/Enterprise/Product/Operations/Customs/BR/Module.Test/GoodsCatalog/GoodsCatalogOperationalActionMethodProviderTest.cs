using System;
using System.Linq;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(GoodsCatalogOperationalActionMethodProvider))]
	class GoodsCatalogOperationalActionMethodProviderTest : OperationalActionMethodProviderTest
	{
		protected override ActionMethodProviderID ID => ActionMethodProviderIDs.BRGoodsCatalog;

		public void TestNewMethods()
		{
			var provider = new GoodsCatalogOperationalActionMethodProvider();
			var expectedMethodTypes = new Type[]
			{
				typeof(SendCatalogCreateDraftActionMethod),
				typeof(SendCatalogUpdateDraftActionMethod),
				typeof(SendCatalogActivateActionMethod),
				typeof(SendCatalogCreateNewVersionActionMethod),
				typeof(SendCatalogDeactivateActionMethod),
			};
			AssertContainsExactElementsInAnyOrder(expectedMethodTypes, provider.NewMethods(null).Select(x => x.GetType()));
		}
	}
}
