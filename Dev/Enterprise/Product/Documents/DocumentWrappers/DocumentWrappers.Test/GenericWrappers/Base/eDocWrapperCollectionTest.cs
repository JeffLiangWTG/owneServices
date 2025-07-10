using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Base.Testing
{
	[TestedType(typeof(eDocWrapperCollection))]
	sealed class eDocWrapperCollectionTest : GenericWrapperCollectionTest<eDocWrapperCollection>
	{
		protected override eDocWrapperCollection GetNewDocumentWrapperCollection()
		{
			return eDocWrapperCollection.New(shipment, Factory);
		}

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new eDocWrapper(eDoc, Factory);
		}

		public void TestGetRow()
		{
			var eDocs = ((IDocManagerSupport)shipment).DocManagerInfo.Documents;
			AssertNotNull(eDocs);

			var collection = new eDocWrapperCollectionForTest(eDocs, Factory);
			AssertNotNull(collection.GetRow("CDZ"));
			AssertNotNull(collection.GetRow("SIMG"));
		}

		protected override void SetUp()
		{
			base.SetUp();

			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentWrappers.Testing.Squares_100dpi.tif", "Squares_100dpi.tif");
			shipment = Factory.New<ForwardingShipment>();
			var shipmentDocManagerInfo = ((IEDocsProvider)shipment).DocManagerInfo;
			eDoc = shipmentDocManagerInfo.AddFileOrDocument(Path.GetFullPath(tempFileName), "CDZ");
			shipmentDocManagerInfo.AddFileOrDocument(Path.GetFullPath(tempFileName), "SIMG");
			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();

			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;
		ForwardingShipment shipment;
		IeDoc eDoc;

		class eDocWrapperCollectionForTest(IStorageDocsBaseCollection parentCollection, BusinessObjectFactory factory) : eDocWrapperCollection(parentCollection, factory)
		{
			public IBODocDataProvider GetRow(string id)
			{
				return base.GetRow(id);
			}
		}
	}
}
