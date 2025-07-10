using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocForwardingShipmentCollection))]
	sealed class DocForwardingShipmentCollectionTest : DocShipmentCollectionTestCase<DocForwardingShipmentCollection>
	{
		public void TestIndexer()
		{
			DocForwardingShipmentCollection collection = GetNewDocForwardingCollection();
			DocForwardingShipment element = GetNewDocForwardingCollectionElement();
			collection.Add(element);

			AssertEquals("DocForwardingShipmentCollection indexer did not return the correct element.", element, collection[0]);
		}

		[ExpectExceptionMessage(typeof(ArgumentException), "Cannot add a Wrapper of type 'DocShipment' to DocForwardingShipmentCollection (must be a DocForwardingShipment).")]
		public void TestAddingNonDocForwardingShipmentBlowsUp()
		{
			DocForwardingShipmentCollection collection = GetNewDocForwardingCollection();
			DocShipment docShipment = DocShipment.New(CommonShipment.New(Factory), Factory);

			collection.Add(docShipment);
		}

		[ExpectNoExceptions("DocForwardingShipmentCollection.Add(DocForwardingShipment) blew up.")]
		public void TestAddWithDocForwardingShipment()
		{
			DocForwardingShipmentCollection collection = GetNewDocForwardingCollection();
			DocForwardingShipment docForwardingShipment = GetNewDocForwardingCollectionElement();

			collection.Add(docForwardingShipment);
		}

		#region Implementation

		DocForwardingShipmentCollection GetNewDocForwardingCollection()
		{
			return new DocForwardingShipmentCollection(Factory);
		}

		DocForwardingShipment GetNewDocForwardingCollectionElement()
		{
			return DocForwardingShipment.New(Factory.New<ForwardingShipment>(), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return GetNewDocForwardingCollectionElement();
		}

		protected override DocForwardingShipmentCollection GetCollectionToTest()
		{
			return GetNewDocForwardingCollection();
		}

		#endregion
	}
}
