using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.Asycuda;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing;
using CustomsChargeTypeList = Enterprise.Customs.Common.CustomsChargeTypeList;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	partial class AsycudaWriterTest
	{
		public void TestExportBillForDeclaration()
		{
			var bill = CreateBillForExportBillForDeclaration(Core.Constants.CountryCodes.Eritrea, "SGVLI", "MGI", Core.Constants.TransportModes.Air);
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			var writer = new AsycudaForCustomsDeclarationDataObjectWriterForTesting(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, bill)), new AsycudaManifestHeaderDataObjectWriterHelper(bill.Header));
			var shipment = AssertExportBillForDeclaration(writer, bill, ShipmentTypeList.Codes.Import23, null, "HAWB1234", WayBillTypeList.Codes.House, null, null, Core.Constants.TransportModes.Air, null, null, "", "BA123");
			AssertEquals("AdditionalBillCollection.Count", 2, shipment.AdditionalBillCollection.Count);
			AssertContents(shipment.AdditionalBillCollection[0], "HAWB1234", WayBillTypeList.Codes.House, "MB2");
			AssertContents(shipment.AdditionalBillCollection[1], "MB2", WayBillTypeList.Codes.Master, null);

			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			writer = new AsycudaForCustomsDeclarationDataObjectWriterForTesting(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, bill)), new AsycudaManifestHeaderDataObjectWriterHelper(bill.Header));
			shipment = AssertExportBillForDeclaration(writer, bill, ShipmentTypeList.Codes.Export22, null, "HAWB1234", WayBillTypeList.Codes.House, null, null, Core.Constants.TransportModes.Air, null, null, "", "BA123");
			AssertEquals("AdditionalBillCollection.Count", 2, shipment.AdditionalBillCollection.Count);
			AssertContents(shipment.AdditionalBillCollection[0], "HAWB1234", WayBillTypeList.Codes.House, "MB2");
			AssertContents(shipment.AdditionalBillCollection[1], "MB2", WayBillTypeList.Codes.Master, null);
		}

		public void TestExportBillForDeclarationForSeaTransport()
		{
			var bill = CreateBillForExportBillForDeclaration(Core.Constants.CountryCodes.Eritrea, "SGVLI", "MGI", Core.Constants.TransportModes.Sea);
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			var writer = new AsycudaForCustomsDeclarationDataObjectWriterForTesting(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, bill)), new AsycudaManifestHeaderDataObjectWriterHelper(bill.Header));
			var shipment = AssertExportBillForDeclaration(writer, bill, ShipmentTypeList.Codes.Import23, null, "HAWB1234", WayBillTypeList.Codes.House, null, null, Core.Constants.TransportModes.Sea, null, null, "MURSK VENTURA", "157E");
			AssertEquals("AdditionalBillCollection.Count", 2, shipment.AdditionalBillCollection.Count);
			AssertContents(shipment.AdditionalBillCollection[0], "HAWB1234", WayBillTypeList.Codes.House, "MB2");
			AssertContents(shipment.AdditionalBillCollection[1], "MB2", WayBillTypeList.Codes.Master, null);

			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			writer = new AsycudaForCustomsDeclarationDataObjectWriterForTesting(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, bill)), new AsycudaManifestHeaderDataObjectWriterHelper(bill.Header));
			shipment = AssertExportBillForDeclaration(writer, bill, ShipmentTypeList.Codes.Export22, null, "HAWB1234", WayBillTypeList.Codes.House, null, null, Core.Constants.TransportModes.Sea, null, null, "MURSK VENTURA", "157E");
			AssertEquals("AdditionalBillCollection.Count", 2, shipment.AdditionalBillCollection.Count);
			AssertContents(shipment.AdditionalBillCollection[0], "HAWB1234", WayBillTypeList.Codes.House, "MB2");
			AssertContents(shipment.AdditionalBillCollection[1], "MB2", WayBillTypeList.Codes.Master, null);
		}

		public void TestExportBillForDeclarationForRoadTransport()
		{
			var bill = CreateBillForExportBillForDeclaration(Core.Constants.CountryCodes.Eritrea, "SGVLI", "MGI", Core.Constants.TransportModes.Road);
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			var writer = new AsycudaForCustomsDeclarationDataObjectWriterForTesting(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, bill)), new AsycudaManifestHeaderDataObjectWriterHelper(bill.Header));
			var shipment = AssertExportBillForDeclaration(writer, bill, ShipmentTypeList.Codes.Import23, null, "HAWB1234", WayBillTypeList.Codes.House, null, null, Core.Constants.TransportModes.Road, null, null, "", "YKK729");
			AssertEquals("AdditionalBillCollection.Count", 2, shipment.AdditionalBillCollection.Count);
			AssertContents(shipment.AdditionalBillCollection[0], "HAWB1234", WayBillTypeList.Codes.House, "MB2");
			AssertContents(shipment.AdditionalBillCollection[1], "MB2", WayBillTypeList.Codes.Master, null);

			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Export22;
			writer = new AsycudaForCustomsDeclarationDataObjectWriterForTesting(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, bill)), new AsycudaManifestHeaderDataObjectWriterHelper(bill.Header));
			shipment = AssertExportBillForDeclaration(writer, bill, ShipmentTypeList.Codes.Export22, null, "HAWB1234", WayBillTypeList.Codes.House, null, null, Core.Constants.TransportModes.Road, null, null, "", "YKK729");
			AssertEquals("AdditionalBillCollection.Count", 2, shipment.AdditionalBillCollection.Count);
			AssertContents(shipment.AdditionalBillCollection[0], "HAWB1234", WayBillTypeList.Codes.House, "MB2");
			AssertContents(shipment.AdditionalBillCollection[1], "MB2", WayBillTypeList.Codes.Master, null);
		}

		public void TestExportBillUsesFreightValueForInvoiceAmount()
		{
			var bill = CreateBillForExportBillForDeclaration(Core.Constants.CountryCodes.Singapore, "SGVLI", "MGI", Core.Constants.TransportModes.Air);
			bill.ABL_ShipmentType = ShipmentTypeList.Codes.Import23;
			var writer = new AsycudaForCustomsDeclarationDataObjectWriterForTesting(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, bill)), new AsycudaManifestHeaderDataObjectWriterHelper(bill.Header));
			var shipment = AssertExportBillForDeclaration(writer, bill, ShipmentTypeList.Codes.Import23, null, "HAWB1234", WayBillTypeList.Codes.House, null, null, Core.Constants.TransportModes.Air, null, null, "Wrights' Flyer", "BA123");
			AssertEquals("AdditionalBillCollection.Count", 2, shipment.AdditionalBillCollection.Count);
			AssertContents(shipment.AdditionalBillCollection[0], "HAWB1234", WayBillTypeList.Codes.House, "MB2");
			AssertContents(shipment.AdditionalBillCollection[1], "MB2", WayBillTypeList.Codes.Master, null);
		}

		public void TestCommercialInvoiceLineCollectionWriterStrategy()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var writer = new AsycudaForCustomsDeclarationDataObjectWriterForTesting(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, bill),
					writerStrategy: new DataObjectWriterStrategyTestClass(s => s != nameof(CommercialInvoiceHeader.CommercialInvoiceLineCollection))),
				new AsycudaManifestHeaderDataObjectWriterHelper(bill.Header));
			var shipment = writer.GetDataObject(bill);
			var commercialInvoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
			AssertNull("CommercialInvoiceLineCollection - writerStrategy not allow", commercialInvoiceHeader.CommercialInvoiceLineCollection);

			writer = new AsycudaForCustomsDeclarationDataObjectWriterForTesting(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, bill)),
				new AsycudaManifestHeaderDataObjectWriterHelper(bill.Header));
			shipment = writer.GetDataObject(bill);
			commercialInvoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
			AssertNotNull("CommercialInvoiceLineCollection - writerStrategy allow", commercialInvoiceHeader.CommercialInvoiceLineCollection);
		}

		public void AssertContents(AdditionalBill additionalBill, ZString billNumber, ZString billType, string parentBillNumber)
		{
			CombineAssertions(() =>
			{
				AssertEquals("BillNumber", billNumber, additionalBill.BillNumber);
				AssertEquals("BillType", billType, additionalBill.BillType.Code);
				AssertEquals("ParentBillNumber", parentBillNumber, additionalBill.ParentBillNumber);
			});
		}

		public AsycudaBill CreateBillForExportBillForDeclaration(string countryCode, ZString localPortCode, ZString manifestType, ZString transportMode)
		{
			PrepareCusCodeDataForTesting();
			AsycudaBillForRegularBillValidationTest.FindOrMakeRefPackAndCusCodeList("DJC", "AAA", countryCode, Factory.BOFactory);

			Factory.SaveForTesting();

			var user = Factory.BOFactory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			user.GS_EmailAddress = "recipient.user@forwarder.com";

			var header = AsycudaManifestHeaderTestHelper.CreateNicelyPopulatedImportManifestForTesting(localPortCode, manifestType, Factory.BOFactory);
			header.AMA_MasterBill = "MB2";
			header.AMA_TransportMode = transportMode;
			if (transportMode == Core.Constants.TransportModes.Road)
			{
				header.AMA_VehicleRegistration = "YKK729";
			}
			else if (transportMode == Core.Constants.TransportModes.Air)
			{
				header.AMA_Voyage = "BA123";
			}
			else
			{
				header.AMA_VesselName = "MURSK VENTURA";
				header.AMA_Voyage = "157E";
			}

			header.AMA_Nature = ShipmentTypeList.Codes.Import23;

			var bill = header.Bills[0];
			bill.ABL_ManifestUQ = "DJC";
			bill.OtherChargesValue = 15m;
			bill.OtherChargesValueCurrency = Core.Constants.CurrencyCodes.Afghanistan;
			bill.DiscountValue = 24m;
			bill.DiscountValueCurrency = Core.Constants.CurrencyCodes.Bahamas;
			bill.ABL_FreightValue = 5000m;
			bill.ABL_RX_NKFreightValueCurrency = "SGD";
			bill.ABL_InsuranceValue = 175.20;
			bill.ABL_RX_NKInsuranceValueCurrency = "AUD";
			var billSG = bill as Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill;
			if (billSG != null)
			{
				billSG.SG_PartyID = "PartyID";
				billSG.SG_PayeeIndicator = SGPayeeIndicatorList.Codes.Q;
				billSG.SG_PartyStatus = "A";
			}
			var pack = bill.Packs.AddNew();
			pack.APA_PackQty = 30;
			pack.APA_PackUQ = "DJC";
			pack.ContainerPK = header.Containers[0].PK;
			pack.MatchingReference = "BT123";
			pack.APA_GoodsDescription = "PACK DESC";
			pack.APA_Weight = 10m;
			pack.APA_WeightUQ = Core.Constants.Weight.Tonnes;
			pack.APA_Volume = 1.5m;
			pack.APA_VolumeUQ = Core.Constants.Volume.CubicFeet;
			pack.APA_CommodityCode = "COM3";
			pack.LinePrice = 500m;

			var packedItem = pack.PackedItemForTesting();
			packedItem.API_Tariff = "123456";
			packedItem.API_CustomsQty = 20m;
			packedItem.API_CustomsUQ = "BOX";
			packedItem.API_CustomsValue = 500m;
			packedItem.API_GoodsDescription = "Toys";
			packedItem.API_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;

			bill.ApportionmentDirty = false;
			Factory.SaveForTesting();
			return bill;
		}

		public Shipment AssertExportBillForDeclaration(IAsycudaForCustomsDeclarationDataObjectWriter writer, AsycudaBill bill, string messageType, string messageSubType, string wayBillNumber, string waybillType, string containerMode, ZInt? containerCount, string transportMode, KeyValuePair<ZString, ZString>[] addInfos, string partNo, string vessel, string voyage, int orgAddressCount = 3)
		{
			var shipment = writer.GetDataObject(bill);
			AssertExportBillForDeclaration(shipment, messageType, messageSubType, wayBillNumber, waybillType, containerMode, containerCount, transportMode, addInfos, partNo, vessel, voyage, orgAddressCount);
			return shipment;
		}

		void AssertExportBillForDeclaration(Shipment shipment, string messageType, string messageSubType, string wayBillNumber, string waybillType, string containerMode, ZInt? containerCount, string transportMode, KeyValuePair<ZString, ZString>[] addInfos, string partNo, string vessel, string voyage, int orgAddressCount)
		{
			AssertEquals("MessageType", messageType, shipment.MessageType.Code);
			if (messageSubType == null)
			{
				AssertNull("MessageSubType", shipment.MessageSubType);
			}
			else
			{
				AssertEquals("MessageSubType.Code", messageSubType, shipment.MessageSubType.Code);
			}

			AssertEquals("ServiceLevel", "STD", shipment.ServiceLevel.Code);

			if (wayBillNumber == null)
			{
				AssertNull("WayBillNumber", shipment.WayBillNumber);
			}
			else
			{
				AssertEquals("WayBillNumber", wayBillNumber, shipment.WayBillNumber);
			}

			if (waybillType == null)
			{
				AssertNull("WayBillType", shipment.WayBillType);
			}
			else
			{
				AssertEquals("WayBillType", waybillType, shipment.WayBillType.Code);
			}

			if (transportMode == Core.Constants.TransportModes.Air || transportMode == Core.Constants.TransportModes.Road)
			{
				AssertEquals("VesselName is not used for Air or Road Transport", ZString.Empty, shipment.VesselName);
			}
			else
			{
				if (string.IsNullOrEmpty(vessel))
				{
					AssertNull("VesselName", shipment.VesselName);
				}
				else
				{
					AssertEquals("VesselName", vessel, shipment.VesselName);
				}
			}

			if (string.IsNullOrEmpty(voyage))
			{
				AssertNull("VoyageFlightNo", shipment.VoyageFlightNo);
			}
			else
			{
				AssertEquals("VoyageFlightNo - is used for Road vehicle registration value also", voyage, shipment.VoyageFlightNo);
			}

			AssertEquals("GoodsDescription", "Books", shipment.GoodsDescription);
			AssertEquals("PortOfOrigin", "USATL", shipment.PortOfOrigin.Code);
			AssertEquals("PortOfDestination", "SGVLI", shipment.PortOfDestination.Code);
			AssertEquals("PortOfLoading", "USATL", shipment.PortOfLoading.Code);
			AssertEquals("PortOfDischarge", "SGVLI", shipment.PortOfDischarge.Code);
			AssertEquals("ContainerCount", containerCount, shipment.ContainerCount);
			if (containerMode == null)
			{
				AssertNull("CustomsContainerMode", shipment.CustomsContainerMode);
			}
			else
			{
				AssertEquals("CustomsContainerMode.Code", containerMode, shipment.CustomsContainerMode.Code);
			}

			AssertEquals("TotalWeight", 3m, shipment.TotalWeight);
			AssertEquals("TotalWeightUnit", Core.Constants.Weight.Pounds, shipment.TotalWeightUnit.Code);
			AssertEquals("TotalWeight", 7m, shipment.TotalVolume);
			AssertEquals("TotalWeightUnit", Core.Constants.Volume.CubicFeet, shipment.TotalVolumeUnit.Code);
			if (transportMode == null)
			{
				AssertNull("TransportMode", shipment.TransportMode);
			}
			else
			{
				AssertEquals("TransportMode", transportMode, shipment.TransportMode.Code);
			}

			AssertEquals("Est.ArrivalDate", ZDateTime.BrettsBirthday, shipment.DateCollection.FirstOrDefault(x => x.Type == DateType.Arrival).Value);
			AssertEquals("Est.DepartureDate", ZDateTime.BrettsBirthday.AddDays(-1), shipment.DateCollection.FirstOrDefault(x => x.Type == DateType.Departure).Value);
			if (addInfos == null)
			{
				AssertNull("AddInfoCollection", shipment.AddInfoCollection);
			}
			else
			{
				addInfos = addInfos.OrderBy(x => x.Key).ToArray();
				var addInfoCollection = shipment.AddInfoCollection.OrderBy(x => x.Key).ToArray();
				AssertEquals("AddInfoCollection", addInfos.Length, addInfoCollection.Length);
				for (var i = 0; i < addInfos.Length; i++)
				{
					var addInfo = addInfos[i];
					AssertContents(addInfoCollection[i], addInfo.Key, addInfo.Value);
				}
			}

			AssertEquals("OrganizationAddressCollection.Count", orgAddressCount, shipment.OrganizationAddressCollection.Count);
			AssertOrganizationAddressContents(shipment.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.Value == nameof(DocAddressType.ConsignorDocumentaryAddress)), "LSA PRODUCTOS TECNICOS PARA A INDUSTRIA", "20A INDUSTRIAL DAS ERVOSAS LUT9", "", "HAVO", "IL", "", "NL", "");
			AssertOrganizationAddressContents(shipment.OrganizationAddressCollection.FirstOrDefault(x => x.AddressType.Value == nameof(DocAddressType.ConsigneeDocumentaryAddress)), "RAYMOND LUKE TURNER", "6 HIXON COURT", "ALEXANDRIA HILLS  QLD", "", "", "4161", "AU", "");

			AssertNull("shipment.CommercialInfo.CommercialChargeCollection", shipment.CommercialInfo.CommercialChargeCollection);
			AssertEquals("shipment.CommercialInfo.CommercialInvoiceCollection.Count", 1, shipment.CommercialInfo.CommercialInvoiceCollection.Count);
			var invoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection[0];

			AssertEquals("InvoiceNumber", "HAWB1234", invoiceHeader.InvoiceNumber);
			AssertEquals("InvoiceAmount", 5000m, invoiceHeader.InvoiceAmount);
			AssertEquals("InvoiceCurrency", "SGD", invoiceHeader.InvoiceCurrency.Code);
			AssertNull("invoiceHeader.Supplier", invoiceHeader.Supplier);
			AssertNull("invoiceHeader.Buyer", invoiceHeader.Buyer);
			AssertEquals("invoiceHeader.CommercialChargeCollection.Count", 4, invoiceHeader.CommercialChargeCollection.Count);
			AssertContents(invoiceHeader.CommercialChargeCollection[0], CustomsChargeTypeList.Codes.Discount, 24m, Core.Constants.CurrencyCodes.Bahamas);
			AssertContents(invoiceHeader.CommercialChargeCollection[1], CustomsChargeTypeList.Codes.OverseasFreight, 6m, Core.Constants.CurrencyCodes.HongKong);
			AssertContents(invoiceHeader.CommercialChargeCollection[2], CustomsChargeTypeList.Codes.OverseasInsurance, 175.2m, Core.Constants.CurrencyCodes.Australia);
			AssertContents(invoiceHeader.CommercialChargeCollection[3], CustomsChargeTypeList.Codes.OtherCharges, 15m, Core.Constants.CurrencyCodes.Afghanistan);

			AssertEquals("invoiceHeader.CommercialInvoiceLineCollection.Count", 1, invoiceHeader.CommercialInvoiceLineCollection.Count);
			var invoiceLine = invoiceHeader.CommercialInvoiceLineCollection.FirstOrDefault();
			AssertEquals("HarmonisedCode", "123456", invoiceLine.HarmonisedCode);
			AssertEquals("InvoiceQuantity", 30m, invoiceLine.InvoiceQuantity);
			AssertEquals("InvoiceQuantityUnit", "DJC", invoiceLine.InvoiceQuantityUnit.Code);
			AssertEquals("CustomsQuantity", 20m, invoiceLine.CustomsQuantity);
			AssertEquals("CustomsQuantityUnit", "BOX", invoiceLine.CustomsQuantityUnit.Code);
			AssertEquals("Weight", 10m, invoiceLine.Weight);
			AssertEquals("WeightUnit", Core.Constants.Weight.Tonnes, invoiceLine.WeightUnit.Code);
			AssertEquals("Volume", 1.5m, invoiceLine.Volume);
			AssertEquals("VolumeUnit", Core.Constants.Volume.CubicFeet, invoiceLine.VolumeUnit.Code);
			AssertEquals("LinePrice", 500m, invoiceLine.LinePrice);
			AssertEquals("Description", "PACK DESC", invoiceLine.Description);
			AssertEquals("CountryOfOrigin ", Core.Constants.CountryCodes.Australia, invoiceLine.CountryOfOrigin.Code);
			AssertEquals("PartNo", partNo, invoiceLine.PartNo);
			AssertEquals("Commodity.Code", "COM3", invoiceLine.Commodity.Code);
		}

		public void AssertContents(AddInfo addInfo, ZString key, ZString value)
		{
			CombineAssertions(() =>
			{
				AssertEquals("addInfo.Key", key, addInfo.Key);
				AssertEquals("addInfo.Value", value, addInfo.Value);
			});
		}

		void AssertContents(CommercialCharge commercialCharge, ZString chargeType, ZDecimal amount, ZString currencyCode)
		{
			CombineAssertions(() =>
			{
				AssertEquals("commercialCharge.ChargeType.Code", chargeType, commercialCharge.ChargeType.Code);
				AssertEquals("commercialCharge.Amount", amount, commercialCharge.Amount);
				AssertEquals("commercialCharge.Currency.Code", currencyCode, commercialCharge.Currency.Code);
			});
		}

		sealed class AsycudaForCustomsDeclarationDataObjectWriterForTesting : AsycudaForCustomsDeclarationDataObjectWriter<AsycudaBill, AsycudaPack, AsycudaPackedItem>
		{
			public AsycudaForCustomsDeclarationDataObjectWriterForTesting(IDataWritingManager manager, AsycudaManifestHeaderDataObjectWriterHelper helper)
				: base(manager, helper)
			{ }
		}
	}
}
