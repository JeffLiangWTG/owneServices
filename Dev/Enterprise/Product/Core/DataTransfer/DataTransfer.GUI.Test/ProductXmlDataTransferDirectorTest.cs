using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;

namespace Enterprise.DataTransfer.GUI.Testing
{
	sealed class ProductXmlDataTransferDirectorTest : TestCaseWithFactory
	{
		public void TestAdapter()
		{
			var adapter = new ProductValueObjectDataAdapter();
			var director = new TestProductXmlDataTransferDirector(adapter);
			AssertEquals("Adapter", typeof(ProductValueObjectDataAdapter), director.Adapter.GetType());
		}

		public void TestSerializer()
		{
			var adapter = new ProductValueObjectDataAdapter();
			var director = new TestProductXmlDataTransferDirector(adapter);
			AssertEquals("Serializer", typeof(ProductXmlValueObjectSerializer), director.Serializer.GetType());
		}

		class TestProductXmlDataTransferDirector : ProductXmlDataTransferDirector
		{
			public TestProductXmlDataTransferDirector(ProductValueObjectDataAdapter adapter)
				: base(adapter)
			{
			}

			public new IValueObjectDataAdapter Adapter
			{
				get { return base.Adapter; }
			}
		}
	}
}
