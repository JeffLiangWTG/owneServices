using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(WebCustomCssBusinessObjectCollection))]
	sealed class WebCustomCssBusinessObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<WebCustomCssBusinessObjectCollection>
	{
		protected override WebCustomCssBusinessObjectCollection GetCollectionToTest() => new WebCustomCssBusinessObjectCollection();
		protected override BusinessObject GetNewElementToAddToTheCollection() => new WebCustomCssBusinessObject();
	}
}
