using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class UXmlLinkManagerTest : TestCaseWithFactory
	{
		public void TestNotifyAndGetId()
		{
			var linkManager = new UXmlLinkManager();
			var metaDataProvider = new UXmlMetaDataProvider(linkManager);

			var dummy = Factory.New<DummyBusinessObject>();
			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var dynamicData = dataObject.MakeDynamic(metaDataProvider);

			AssertExportAndRetrieveId(null, linkManager, metaDataProvider, null, dummy, dynamicData);
			AssertExportAndRetrieveId(null, linkManager, metaDataProvider, dataObject, null, dynamicData);
			AssertExportAndRetrieveId(dummy.PK, linkManager, metaDataProvider, dataObject, dummy, dynamicData);
		}

		void AssertExportAndRetrieveId(object expected, IDataWritingInformationCollector linkManager, IMetaDataProvider metaDataProvider, IDataObject dataObject, BusinessObject bizO, IDynamicData dynamicData)
		{
			linkManager.NotifyExported(dataObject, bizO);
			var result = metaDataProvider.GetMetaData(dynamicData, MetaDataType.Identifier);

			AssertEquals(expected, result);
		}

		public void TestNaturalKeyAsIdentifier()
		{
			var linkManager = new UXmlLinkManager();
			var metaDataProvider = new UXmlMetaDataProvider(linkManager);

			var dynamicData = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance).MakeDynamic();
			dynamicData.Properties.GetOrCreate("AddressType", "TestAddressType", typeof(string));
			dynamicData.SetValue(null);

			var id = ((IMetaDataProvider)metaDataProvider).GetMetaData(dynamicData, MetaDataType.Identifier);

			AssertEquals("TestAddressType", null, id);
		}
	}
}
