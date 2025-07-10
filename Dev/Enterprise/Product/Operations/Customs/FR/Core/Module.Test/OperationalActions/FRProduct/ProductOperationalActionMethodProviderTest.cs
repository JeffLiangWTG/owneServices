using System;
using System.Linq;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Module.OperationalActions.Testing
{
	[TestedType(typeof(ProductOperationalActionMethodProvider))]
	public class ProductOperationalActionMethodProviderTest : OperationalActionMethodProviderTest
	{
		protected override ActionMethodProviderID ID => ActionMethodProviderIDs.FRProduct;

		public void TestNewMethods()
		{
			var provider = new ProductOperationalActionMethodProvider();
			AssertContainsExactElementsInAnyOrder(new Type[] { typeof(UpdateProductAdditionalInformationMethod)
			}, provider.NewMethods(null).Select(x => x.GetType()));
		}
	}
}
