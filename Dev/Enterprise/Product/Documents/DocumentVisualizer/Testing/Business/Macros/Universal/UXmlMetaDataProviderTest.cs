using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing.Business.Macros.Universal
{
	sealed class UXmlMetaDataProviderTest : TestCase
	{
		public void TestGetIdentifier()
		{
			var linkManager = new UXmlLinkManager();
			IMetaDataProvider metaDataProvider = new UXmlMetaDataProvider(linkManager);

			var address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);

			var data = address.MakeDynamic();

			AssertNull("no identifier", metaDataProvider.GetMetaData(data, MetaDataType.Identifier));

			var bizObj = new DummyNonPersistentBusinessObject();

			((IDataWritingInformationCollector)linkManager).NotifyExported(address, bizObj);

			AssertEquals("business object PK identifier", bizObj.PK, metaDataProvider.GetMetaData(data, MetaDataType.Identifier));
		}

		public void TestGetNaturalKey_UnmappedDataObject()
		{
			var linkManager = new UXmlLinkManager();
			IMetaDataProvider metaDataProvider = new UXmlMetaDataProvider(linkManager);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var data = shipment.MakeDynamic();

			AssertNull("no natural key", metaDataProvider.GetMetaData(data, MetaDataType.NaturalKey));
		}

		public void TestGetNaturalKey_Collection()
		{
			var linkManager = new UXmlLinkManager();
			IMetaDataProvider metaDataProvider = new UXmlMetaDataProvider(linkManager);

			var orgAddresses = new List<OrganizationAddress>();

			var data = orgAddresses.MakeDynamic();

			AssertEquals("natural key", "AddressType", metaDataProvider.GetMetaData(data, MetaDataType.NaturalKey));
		}

		public void TestGetMaxLength()
		{
			var linkManager = new UXmlLinkManager();
			IMetaDataProvider metaDataProvider = new UXmlMetaDataProvider(linkManager);

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.VesselName = "Steven is awesome";
			var data = shipment.MakeDynamic();
			var vesselNameData = data.GetDynamicProperty("VesselName");

			AssertEquals(35, metaDataProvider.GetMetaData(vesselNameData, MetaDataType.MaxLength));
		}
	}
}
