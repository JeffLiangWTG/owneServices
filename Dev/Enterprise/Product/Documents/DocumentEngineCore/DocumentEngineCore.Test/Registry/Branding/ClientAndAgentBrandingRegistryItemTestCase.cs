using System;
using System.Drawing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestsSubclassesOf(typeof(ClientAndAgentBrandingRegistryItem))]
	public abstract class ClientAndAgentBrandingRegistryItemTestCase : StronglyTypedRegistryItemTestCase<ClientAndAgentBrandingCollection>
	{
		public void TestReloadValuesIfDataIsOutdated()
		{
			ClientAndAgentBrandingCollection collection = ValidValue;
			collection.RemoveAll();
			ClientAndAgentBrandingBusinessObject brandingBizo = (ClientAndAgentBrandingBusinessObject)collection.AddNew();
			brandingBizo.Image = new Bitmap(3, 3);

			using (Item.DataType.SuspendValidation())
			{
				Item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
			}

			collection = Item.Value;
			AssertEquals(1, collection.Count);
			AssertEquals(3, collection[0].Image.Width);

			ClientAndAgentBrandingCollection collection1 = Item.Value;
			Assert("Should be same object", object.ReferenceEquals(collection, collection1));
			AssertEquals(1, collection1.Count);
			AssertEquals(3, collection1[0].Image.Width);

			collection[0].Image.Dispose();

			collection1 = Item.Value;
			Assert("Should reload object", !object.ReferenceEquals(collection, collection1));
			AssertEquals(1, collection1.Count);
			AssertEquals(3, collection1[0].Image.Width);
		}
	}
}
