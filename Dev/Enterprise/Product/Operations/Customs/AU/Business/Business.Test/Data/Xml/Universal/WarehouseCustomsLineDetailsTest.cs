using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class WarehouseCustomsLineDetailsTest : TestCaseWithFactory
	{
		public void TestEntryDetails()
		{
			CombineAssertions(() =>
			{
				var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				var entryHeader = new EntryHeader(DefaultDataObjectWriterStrategy.TestInstance);
				var entryNumber = new UniversalDataBuss.DataObjects.Universal.Customs.EntryNumber() { Number = "ENT1" };
				entryHeader.EntryNumberCollection = new List<UniversalDataBuss.DataObjects.Universal.Customs.EntryNumber>(new[] { entryNumber });
				var entryLine = new EntryLine() { LineNumber = 1 };
				entryHeader.EntryLineCollection = new List<EntryLine>(new[] { entryLine });
				var invoiceHeaderOrgAddInfo = new UniversalAddInfo() { Key = "ORG", Value = "AUST" };
				var invoiceHeader = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance) { InvoiceNumber = "INV123", AddInfoCollection = new List<UniversalAddInfo>(new[] { invoiceHeaderOrgAddInfo }) };
				var invoiceLine = new CommercialInvoiceLine() { EntryLineNumber = 1, EntryNumber = "ENT1", BondedWarehouseQuantity = 1, CustomsQuantity = 10m, CustomsQuantityUnit = new CodeDescriptionPair6Char() { Code = "!K1", Description = "WHAT CODE" }, PreviousEntryLineNumber = 3, PreviousEntryNumber = "ENT3" };
				var wrnAddInfo = new UniversalAddInfo() { Key = "WRN", Value = "ENT2" };
				var wrlAddInfo = new UniversalAddInfo() { Key = "WRL", Value = "2" };
				var wrqAddInfo = new UniversalAddInfo() { Key = "WRQ", Value = "0" };
				var wruAddInfo = new UniversalAddInfo() { Key = "WRU", Value = "" };
				invoiceLine.AddInfoCollection = new List<UniversalAddInfo>(new[] { wrnAddInfo, wrlAddInfo, wrqAddInfo, wruAddInfo });
				invoiceHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>(new[] { invoiceLine }));
				shipment.CommercialInfo = new CommercialInfo() { CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[] { invoiceHeader }) };
				IWarehouseCustomsLineDetailsProvider provider = new WarehouseCustomsLineDetailsProvider(shipment);
				var details = provider.GetLineDetails().First();
				AssertEquals("CountryOfOrigin.Code", "AU", details.CountryOfOrigin.Code);
				AssertNull("CountryOfOrigin.Name", details.CountryOfOrigin.Name);
				AssertEquals("CustomsQuantity", 10m, details.CustomsQuantity);
				AssertEquals("CustomsQuantityUnit.Code", "!K1", details.CustomsQuantityUnit.Code);
				AssertEquals("CustomsQuantityUnit.Description", "WHAT CODE", details.CustomsQuantityUnit.Description);
				AssertEquals("EntryNumber", "ENT1", details.EntryNumber.GetValueOrDefault());
				AssertEquals("EntryLineNumber", (ZShort)1, details.EntryLineNumber.GetValueOrDefault());
				AssertEquals("PreviousEntryNumber", "", details.PreviousEntryNumber.GetValueOrDefault());
				AssertEquals("PreviousEntryLineNumber", ZShort.Zero, details.PreviousEntryLineNumber.GetValueOrDefault());

				invoiceHeaderOrgAddInfo.Value = Core.Constants.CountryCodes.Canada;
				wrqAddInfo.Value = "15";
				provider = new WarehouseCustomsLineDetailsProvider(shipment);
				details = provider.GetLineDetails().First();
				AssertEquals("CountryOfOrigin.Code", "CA", details.CountryOfOrigin.Code);
				AssertNull("CountryOfOrigin.Name", details.CountryOfOrigin.Name);
				AssertEquals("CustomsQuantity", 10m, details.CustomsQuantity);
				AssertEquals("CustomsQuantityUnit.Code", "!K1", details.CustomsQuantityUnit.Code);
				AssertEquals("CustomsQuantityUnit.Description", "WHAT CODE", details.CustomsQuantityUnit.Description);

				wruAddInfo.Value = "PK";
				invoiceLine.CountryOfOrigin = new Country() { Code = "FP", Name = "FOOD PLACE" };
				provider = new WarehouseCustomsLineDetailsProvider(shipment);
				details = provider.GetLineDetails().First();
				AssertEquals("CountryOfOrigin.Code", "FP", details.CountryOfOrigin.Code);
				AssertEquals("CountryOfOrigin.Name", "FOOD PLACE", details.CountryOfOrigin.Name);
				AssertEquals("CustomsQuantity", 15m, details.CustomsQuantity);
				AssertEquals("CustomsQuantityUnit.Code", "PK", details.CustomsQuantityUnit.Code);
				AssertNull("CustomsQuantityUnit.Description", details.CustomsQuantityUnit.Description);

				wrqAddInfo.Value = "0";
				shipment.MessageType = new CodeDescriptionPair() { Code = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.ExWarehouse };
				provider = new WarehouseCustomsLineDetailsProvider(shipment);
				details = provider.GetLineDetails().First();
				AssertEquals("CountryOfOrigin.Code", "FP", details.CountryOfOrigin.Code);
				AssertEquals("CountryOfOrigin.Name", "FOOD PLACE", details.CountryOfOrigin.Name);
				AssertEquals("CustomsQuantity", 10m, details.CustomsQuantity);
				AssertEquals("CustomsQuantityUnit.Code", "!K1", details.CustomsQuantityUnit.Code);
				AssertEquals("CustomsQuantityUnit.Description", "WHAT CODE", details.CustomsQuantityUnit.Description);
				AssertEquals("EntryNumber", "ENT1", details.EntryNumber.GetValueOrDefault());
				AssertEquals("EntryLineNumber", (ZShort)1, details.EntryLineNumber.GetValueOrDefault());
				AssertEquals("PreviousEntryNumber", "ENT2", details.PreviousEntryNumber.GetValueOrDefault());
				AssertEquals("PreviousEntryLineNumber", (ZShort)2, details.PreviousEntryLineNumber.GetValueOrDefault());

				invoiceHeader.AddInfoCollection = null;
				invoiceLine.AddInfoCollection = null;
				provider = new WarehouseCustomsLineDetailsProvider(shipment);
				details = provider.GetLineDetails().First();
				AssertEquals("AddInfos", ZString.Empty, details.AddInfos);
				AssertEquals("CountryOfOrigin.Code", "FP", details.CountryOfOrigin.Code);
				AssertEquals("CountryOfOrigin.Name", "FOOD PLACE", details.CountryOfOrigin.Name);
				AssertEquals("CustomsQuantity", 10m, details.CustomsQuantity);
				AssertEquals("CustomsQuantityUnit.Code", "!K1", details.CustomsQuantityUnit.Code);
				AssertEquals("CustomsQuantityUnit.Description", "WHAT CODE", details.CustomsQuantityUnit.Description);
				AssertEquals("EntryNumber", "ENT1", details.EntryNumber.GetValueOrDefault());
				AssertEquals("EntryLineNumber", (ZShort)1, details.EntryLineNumber.GetValueOrDefault());
				AssertEquals("PreviousEntryNumber", "", details.PreviousEntryNumber.GetValueOrDefault());
				AssertEquals("PreviousEntryLineNumber", ZShort.Zero, details.PreviousEntryLineNumber.GetValueOrDefault());

				shipment.MessageType = new CodeDescriptionPair() { Code = Enterprise.Customs.AU.Declaration.Business.JobMessageTypeList.Codes.WarehousedByExternalAgent };
				provider = new WarehouseCustomsLineDetailsProvider(shipment);
				details = provider.GetLineDetails().First();
				AssertEquals("AddInfos", ZString.Empty, details.AddInfos);
				AssertEquals("CountryOfOrigin.Code", "FP", details.CountryOfOrigin.Code);
				AssertEquals("CountryOfOrigin.Name", "FOOD PLACE", details.CountryOfOrigin.Name);
				AssertEquals("CustomsQuantity", 10m, details.CustomsQuantity);
				AssertEquals("CustomsQuantityUnit.Code", "!K1", details.CustomsQuantityUnit.Code);
				AssertEquals("CustomsQuantityUnit.Description", "WHAT CODE", details.CustomsQuantityUnit.Description);
				AssertEquals("EntryNumber", Enterprise.Customs.DataTransfer.Universal.Constants.EntryNumberPlaceHolder, details.EntryNumber.GetValueOrDefault());
				AssertEquals("EntryLineNumber", ZShort.Zero, details.EntryLineNumber.GetValueOrDefault());
				AssertEquals("PreviousEntryNumber", "", details.PreviousEntryNumber.GetValueOrDefault());
				AssertEquals("PreviousEntryLineNumber", ZShort.Zero, details.PreviousEntryLineNumber.GetValueOrDefault());

				invoiceLine.AddInfoCollection = new List<UniversalAddInfo>(new[] { wrnAddInfo, wrlAddInfo });
				provider = new WarehouseCustomsLineDetailsProvider(shipment);
				details = provider.GetLineDetails().First();
				AssertEquals("AddInfos", "WRN=ENT2*WRL=2", details.AddInfos);
				AssertEquals("CountryOfOrigin.Code", "FP", details.CountryOfOrigin.Code);
				AssertEquals("CountryOfOrigin.Name", "FOOD PLACE", details.CountryOfOrigin.Name);
				AssertEquals("CustomsQuantity", 10m, details.CustomsQuantity);
				AssertEquals("CustomsQuantityUnit.Code", "!K1", details.CustomsQuantityUnit.Code);
				AssertEquals("CustomsQuantityUnit.Description", "WHAT CODE", details.CustomsQuantityUnit.Description);
				AssertEquals("EntryNumber", "ENT2", details.EntryNumber.GetValueOrDefault());
				AssertEquals("EntryLineNumber", (ZShort)2, details.EntryLineNumber.GetValueOrDefault());
				AssertEquals("PreviousEntryNumber", "", details.PreviousEntryNumber.GetValueOrDefault());
				AssertEquals("PreviousEntryLineNumber", ZShort.Zero, details.PreviousEntryLineNumber.GetValueOrDefault());
			});
		}
	}
}
