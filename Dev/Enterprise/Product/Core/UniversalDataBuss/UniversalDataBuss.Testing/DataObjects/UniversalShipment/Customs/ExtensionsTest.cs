using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Application.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.Testing
{
	class ExtensionsTest : TestCase
	{
		public void TestGetWarehouseType()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var contextMock = new Mock<IDataContextDataObject>();
			shipment.DataContext = contextMock.Object;
			contextMock.Setup(m => m.CountryCodeToImportInto).Returns("US");
			AssertEquals(WarehouseType.Default, shipment.GetWarehouseType());
			shipment.MessageType = new CodeDescriptionPair() { Code = "FTZ" };
			AssertEquals(WarehouseType.FreeTradeZone, shipment.GetWarehouseType());
			contextMock.Reset();
			contextMock.Setup(m => m.CountryCodeToImportInto).Returns("");
			AssertEquals(WarehouseType.Default, shipment.GetWarehouseType());
			contextMock.Reset();
			contextMock.Setup(m => m.CountryCodeToImportInto).Returns("AU");
			AssertEquals(WarehouseType.Default, shipment.GetWarehouseType());
			contextMock.VerifyAll();
		}

		public void TestGetLineDetails()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CommercialInfo = new CommercialInfo();
			shipment.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>();
			var invoice = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CommercialInfo.CommercialInvoiceCollection.Add(invoice);
			invoice.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>());
			var invoiceLine = new CommercialInvoiceLine();
			invoice.CommercialInvoiceLineCollection.Add(invoiceLine);
			invoiceLine.BondedWarehouseQuantity = 1;

			var contextMock = new Mock<IDataContextDataObject>();
			contextMock.Setup(m => m.CountryCodeToImportInto).Returns("AU");
			var line = new List<IWarehouseCustomsLineDetails>(shipment.GetWarehouseCustomsLineDetails(contextMock.Object))[0];
			AssertEquals("Enterprise.Customs.AU.Declaration.Business", line.GetType().Namespace);
			contextMock = new Mock<IDataContextDataObject>();
			contextMock.Setup(m => m.CountryCodeToImportInto).Returns("ER");
			line = new List<IWarehouseCustomsLineDetails>(shipment.GetWarehouseCustomsLineDetails(contextMock.Object))[0];
			AssertEquals("Enterprise.Customs.DataTransfer.Universal", line.GetType().Namespace);
			contextMock = new Mock<IDataContextDataObject>();
			contextMock.Setup(m => m.CountryCodeToImportInto).Returns("US");
			line = new List<IWarehouseCustomsLineDetails>(shipment.GetWarehouseCustomsLineDetails(contextMock.Object))[0];
			AssertEquals("Enterprise.Customs.US.DataTransfer.Universal", line.GetType().Namespace);

			contextMock.VerifyAll();
		}

		public void TestGetLineDetails_NctsHeader()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CommercialInfo = new CommercialInfo();
			shipment.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>();
			var invoice = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.CommercialInfo.CommercialInvoiceCollection.Add(invoice);
			invoice.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>());
			var invoiceLine = new CommercialInvoiceLine();
			invoice.CommercialInvoiceLineCollection.Add(invoiceLine);
			invoiceLine.BondedWarehouseQuantity = 1;

			var nctsDataSource = new Mock<IDataSourceDataObject>();
			nctsDataSource.Setup(e => e.Type).Returns(nameof(DataContextType.NctsHeader));

			var contextMock = new Mock<IDataContextDataObject>();
			contextMock.Setup(e => e.DataSourceCollection).Returns(
				new List<IDataSourceDataObject>()
				{
					nctsDataSource.Object,
				});
			contextMock.Setup(m => m.CountryCodeToImportInto).Returns("LV");
			var nctsHashTable = new Hashtable();
			var warehouseCustomsLineDetailsProviderMock = new Mock<IWarehouseCustomsLineDetailsProvider>();
			nctsHashTable["EUN"] = new TestObjectHandle(warehouseCustomsLineDetailsProviderMock.Object);
			ObjectFactory.Substitute("WarehouseNctsCustomsLineDetailsProviders", nctsHashTable);
			_ = shipment.GetWarehouseCustomsLineDetails(contextMock.Object).SingleOrDefault();
			warehouseCustomsLineDetailsProviderMock.Verify(m => m.GetLineDetails());
			contextMock.VerifyAll();
			Assert("relying on verify", true);
		}
	}
}
