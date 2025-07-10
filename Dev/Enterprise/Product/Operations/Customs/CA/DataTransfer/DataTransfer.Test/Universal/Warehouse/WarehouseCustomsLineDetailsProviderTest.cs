using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	sealed class WarehouseCustomsLineDetailsProviderTest : TestCaseWithFactory
	{
		public void TestGetLineDetails()
		{
			var shipment = CreateShipment(B3EntryTypeList.Codes.Warehouse10, DataContextType.WarehouseReceive);
			var provider = new WarehouseCustomsLineDetailsProvider(shipment) as IWarehouseCustomsLineDetailsProvider;
			var lines = new List<IWarehouseCustomsLineDetails>(provider.GetLineDetails());
			WarehouseCustomsLineDetailsTest.AssertWarehouseCustomsLineDetails(lines[0], "12345000000023", 1, "12345000000012", 3, "99TariffCode=9902*RN_NKExport=US");
			WarehouseCustomsLineDetailsTest.AssertWarehouseCustomsLineDetails(lines[1], "12345000000034", 2, "12345000000012", 4, "99TariffCode=9902*RN_NKExport=US");

			shipment = CreateShipment(CADEntryTypeList.Codes.Warehouse101, DataContextType.WarehouseReceive);
			provider = new WarehouseCustomsLineDetailsProvider(shipment);
			lines = new List<IWarehouseCustomsLineDetails>(provider.GetLineDetails());
			WarehouseCustomsLineDetailsTest.AssertWarehouseCustomsLineDetails(lines[0], "12345000000023", 1, "12345000000012", 3, "99TariffCode=9902*RN_NKExport=US");
			WarehouseCustomsLineDetailsTest.AssertWarehouseCustomsLineDetails(lines[1], "12345000000034", 2, "12345000000012", 4, "99TariffCode=9902*RN_NKExport=US");

			shipment = CreateShipment(CADEntryTypeList.Codes.Warehouse102, DataContextType.WarehouseReceive);
			provider = new WarehouseCustomsLineDetailsProvider(shipment);
			lines = new List<IWarehouseCustomsLineDetails>(provider.GetLineDetails());
			WarehouseCustomsLineDetailsTest.AssertWarehouseCustomsLineDetails(lines[0], "12345000000023", 1, "12345000000012", 3, "99TariffCode=9902*RN_NKExport=US");
			WarehouseCustomsLineDetailsTest.AssertWarehouseCustomsLineDetails(lines[1], "12345000000034", 2, "12345000000012", 4, "99TariffCode=9902*RN_NKExport=US");

			shipment = CreateShipment(B3EntryTypeList.Codes.ExWarehouse20, DataContextType.WarehouseOrder);
			provider = new WarehouseCustomsLineDetailsProvider(shipment);
			lines = new List<IWarehouseCustomsLineDetails>(provider.GetLineDetails());
			WarehouseCustomsLineDetailsTest.AssertWarehouseCustomsLineDetails(lines[0], "12345000000023", 1, "12345000000012", 3, "");
			WarehouseCustomsLineDetailsTest.AssertWarehouseCustomsLineDetails(lines[1], "12345000000034", 2, "12345000000012", 4, "");

			shipment = CreateShipment(CADEntryTypeList.Codes.ExWarehouse201, DataContextType.WarehouseOrder);
			provider = new WarehouseCustomsLineDetailsProvider(shipment);
			lines = new List<IWarehouseCustomsLineDetails>(provider.GetLineDetails());
			WarehouseCustomsLineDetailsTest.AssertWarehouseCustomsLineDetails(lines[0], "12345000000023", 1, "12345000000012", 3, "");
			WarehouseCustomsLineDetailsTest.AssertWarehouseCustomsLineDetails(lines[1], "12345000000034", 2, "12345000000012", 4, "");
		}

		Shipment CreateShipment(ZString messageSubType, DataContextType dataContextType)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(dataContextType, null);

			return new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				MessageSubType = new CodeDescriptionPair() { Code = messageSubType },
				DataContext = dataContext,
				CommercialInfo = new CommercialInfo()
				{
					Name = "GROUPINV",
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV123",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>(new[] { InvoiceLine1, InvoiceLine2 })))
					})
				}
			};
		}

		CommercialInvoiceLine InvoiceLine1
		{
			get
			{
				if (invoiceLine1 == null)
				{
					invoiceLine1 = new CommercialInvoiceLine()
					{
						LineNo = 1,
						EntryLineNumber = 1,
						EntryNumber = "12345000000023",
						BondedWarehouseQuantity = 2000m,
						BondedWarehouseQuantityUnit = new CodeDescriptionPair() { Code = "KGM" },
						PreviousEntryNumber = "12345000000012",
						PreviousEntryLineNumber = 3,
						AddInfoCollection = new List<UniversalAddInfo>(new[]
						{
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.CA_99TariffCode.Substring(3), Value = "9902" },
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.CA_RN_NKExport.Substring(3), Value = "US" }
						})
					};
				}
				return invoiceLine1;
			}
		}
		CommercialInvoiceLine invoiceLine1;

		CommercialInvoiceLine InvoiceLine2
		{
			get
			{
				if (invoiceLine2 == null)
				{
					invoiceLine2 = new CommercialInvoiceLine()
					{
						LineNo = 1,
						EntryLineNumber = 2,
						EntryNumber = "12345000000034",
						BondedWarehouseQuantity = 1m,
						BondedWarehouseQuantityUnit = new CodeDescriptionPair() { Code = "KGM" },
						PreviousEntryNumber = "12345000000012",
						PreviousEntryLineNumber = 4,
						AddInfoCollection = new List<UniversalAddInfo>(new[]
						{
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.CA_99TariffCode.Substring(3), Value = "9902" },
							new UniversalAddInfo() { Key = JobComInvoiceLine.Schema.CA_RN_NKExport.Substring(3), Value = "US" }
						})
					};
				}
				return invoiceLine2;
			}
		}
		CommercialInvoiceLine invoiceLine2;
	}
}
