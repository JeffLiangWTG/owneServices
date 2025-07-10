using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AvsWebServiceUriRegistryBusinessObjectCollection))]
	public class AvsWebServiceUriRegistryBusinessObjectCollectionTest : RegistryBusinessObjectCollectionTestCase<AvsWebServiceUriRegistryBusinessObjectCollection>
	{
		public void TestDoNotAllowNewAndRemove()
		{
			var collection = new AvsWebServiceUriRegistryBusinessObjectCollection();
			AssertEquals(false, collection.AllowNew);
			AssertEquals(false, collection.AllowRemove);
		}

		public void TestGetServiceUriFromType()
		{
			var collection = new AvsWebServiceUriRegistryBusinessObjectCollection();
			AssertNull(collection.Primary);
			AssertNull(collection.Secondary);
			AssertNull(collection.Background);

			collection.AddRange(new []
			{
				new AvsWebServiceUriRegistryBusinessObject { Type = "Primary", ServiceUri = "http://fake.domain/url1/" },
				new AvsWebServiceUriRegistryBusinessObject { Type = "Secondary", ServiceUri = "http://fake.domain/url2/" },
				new AvsWebServiceUriRegistryBusinessObject { Type = "Background", ServiceUri = "http://fake.domain/url3/" },
			});

			AssertEquals("http://fake.domain/url1/", collection.Primary.ServiceUri);
			AssertEquals("http://fake.domain/url2/", collection.Secondary.ServiceUri);
			AssertEquals("http://fake.domain/url3/", collection.Background.ServiceUri);

			collection.Add(new AvsWebServiceUriRegistryBusinessObject { Type = "Primary", ServiceUri = "http://fake.domain/url4/" });
			AssertNull(collection.Primary);
		}

		protected override bool RequiresFactory => false;
		protected override bool RequiresFallbackLevel => false;

		protected override AvsWebServiceUriRegistryBusinessObjectCollection GetCollectionToTest()
		{
			return new AvsWebServiceUriRegistryBusinessObjectCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AvsWebServiceUriRegistryBusinessObject();
		}
	}
}
