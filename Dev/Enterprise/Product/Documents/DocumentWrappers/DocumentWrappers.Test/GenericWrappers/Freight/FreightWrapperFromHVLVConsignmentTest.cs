using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromHVLVConsignment))]
	sealed class FreightWrapperFromHVLVConsignmentTest : FreightWrapperTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			if (Item == null)
			{
				Item = Consignment.Items.AddNew();
			}

			return new FreightWrapperFromHVLVConsignment(Consignment, Item, Factory);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
			var consignment = consignmentHeader.Consignments.AddNew();

			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			consignment.HVC_HVH_BookingHeader = bookingHeader.PK;

			consignment.HVC_ConsignmentId = "HVCTEST00001";
			return consignment;
		}

		HVLVConsignment Consignment => (HVLVConsignment)WrappedBO;
		HVLVItem Item;
		FreightWrapperFromHVLVConsignment HVLVConsignmentWrapper => (FreightWrapperFromHVLVConsignment)Wrapper;

		public void TestAdditionalCopyInfo()
		{
			var wrapperForConsignmentOnly = new FreightWrapperFromHVLVConsignment(Consignment, Factory);
			var copyInfo = ((IBODocDataProvider)wrapperForConsignmentOnly).AdditionalCopyInfo;
			AssertNull(copyInfo);

			Item = Consignment.Items.AddNew();
			Item.HVI_ItemId = "HVI_001";
			Consignment.HVC_ConsignmentId = "HVC_001";
			var wrapperForConsignmentAndItems = new FreightWrapperFromHVLVConsignment(Consignment, Item, Factory);

			copyInfo = ((IBODocDataProvider)HVLVConsignmentWrapper).AdditionalCopyInfo;
			AssertNotNull(copyInfo);
			AssertEquals("Enterprise.DocumentWrappers.GenericWrappers.FreightWrapperFromHVLVConsignment+HVLVItemDocWrapperCopyInfo", copyInfo.GetType().FullName);
			AssertEquals("HVLV Item (HVC_001/HVI_001)", copyInfo.TitleCopyCountPair.Title);
		}

		public void TestAvalibleLocalTransportLabels()
		{
			const string documentTitle = "HVLV Delivery Label";
			const string prefix = "\"<LocalTransportCompanyLabel>\" == \"";
			var query = new ZQuery(StmMenuTemplatePivotSchema.SI_DocumentTitle, documentTitle);
			var labels = Factory.Load<StmMenuTemplatePivot>(query).Select(p => p.SI_MenuTemplateFilter.Replace(prefix, ZString.Empty).TrimEnd('"'));

			var labelList = new LabelNameList();

			AssertCollectionContains("LocalTransportCompanyLabel should exist in LabelNameList", labels, label => labelList.ContainsCode(label));
		}

		public void TestDocumentNumberAndTotal()
		{
			Item = Consignment.Items.AddNew();
			Item.HVI_ItemId = "9";
			AssertEquals(1, Wrapper.DocumentTotal);
			AssertEquals(1, Wrapper.DocumentNumber);
			Consignment.Items.AddNew().HVI_ItemId = "8";
			AssertEquals(2, Wrapper.DocumentNumber);
			AssertEquals(2, Wrapper.DocumentTotal);
			Consignment.Items.AddNew().HVI_ItemId = "A";
			AssertEquals(2, Wrapper.DocumentNumber);
			AssertEquals(3, Wrapper.DocumentTotal);
		}

		public void TestGetHVLVConsignment()
		{
			var wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;
			AssertEquals("Should return HVLV consignment", Consignment.PK, wrapper.HVLVConsignment.PK);
		}

		public void TestGetHVLVItem()
		{
			var wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;
			AssertEquals("Should return HVLV item", Consignment.Items.First().PK, wrapper.HVLVItem.PK);
		}

		public void TestGetCarrier()
		{
			var lastMileCarrier = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrier.OH_IsShippingProvider = true;
			lastMileCarrier.OH_IsLocalTransport = true;

			var registryValue = new LocalTransportCompanyBrandingCollection();
			var branding = registryValue.AddNew();
			branding.LabelName = LabelNames.EParcelLabel;
			branding.LocalTransportCompanyPK = lastMileCarrier.PK;
			branding.Code = lastMileCarrier.OH_Code;

			Factory.Save();

			Consignment.HVC_OH_LastMileCarrier = lastMileCarrier.PK;
			using (DocumentsDataRegistry.Instance.LocalTransportCompanyBrand.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				AssertEquals(LabelNames.EParcelLabel, Wrapper.Carrier.CompanyCode);
			}
		}

		public void TestGetConsignor()
		{
			const string tempConsignorTerminology = "Mail man";
			const string shipperAddress1 = "DHL HQ";
			using (FreightDataRegistry.Instance.ConsignorShipperTerminology.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tempConsignorTerminology))
			{
				Consignment.HVC_ShipperAddress1 = shipperAddress1;

				AssertEquals(shipperAddress1, Wrapper.Consignor.MainAddress.AddressLine1);

				AssertEquals(tempConsignorTerminology, Wrapper.Consignor.TypeDescription);
			}
		}

		public void TestGetIsPODRequired()
		{
			var wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;
			Assert(!wrapper.IsPODRequired);

			wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;
			Consignment.HVC_IsSignatureRequired = true;
			Assert(wrapper.IsPODRequired);
		}

		public void TestGetConsignee()
		{
			const string consigneeAddress1 = "WTG";
			const string consigneeName = "BZG";
			Consignment.HVC_ConsigneeAddress1 = consigneeAddress1;
			Consignment.HVC_ConsigneeContact = consigneeName;

			AssertEquals(consigneeAddress1, Wrapper.Consignee.MainAddress.AddressLine1);
			AssertEquals(consigneeName, Wrapper.Consignee.ContactName);
			AssertEquals(CommonResourceStrings.Consignee, Wrapper.Consignee.TypeDescription);
		}

		public void TestGetVolume()
		{
			var wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;

			AssertEquals("0.000 M3", wrapper.Volume.ValueAndUnitCode);

			wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;

			Consignment.HVC_VolumeUQ = "GA";
			Item.HVI_ManifestedVolume = 123.45;

			AssertEquals("123.450 GA", wrapper.Volume.ValueAndUnitCode);

			wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;

			Consignment.HVC_VolumeUQ = "GA";
			Item.HVI_ManifestedVolume = 123.45;
			Item.HVI_ActualVolume = 321.09;
			AssertEquals("321.090 GA", Wrapper.Volume.ValueAndUnitCode);
		}

		public void TestGetWeight()
		{
			var wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;

			AssertEquals("0.000 KG", wrapper.Weight.ValueAndUnitCode);

			wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;

			Consignment.HVC_WeightUQ = "LB";
			Item.HVI_ManifestedWeight = 123.45;

			AssertEquals("123.450 LB", wrapper.Weight.ValueAndUnitCode);

			wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;

			Consignment.HVC_WeightUQ = "LB";
			Item.HVI_ManifestedWeight = 123.45;
			Item.HVI_ActualWeight = 321.09;
			AssertEquals("321.090 LB", Wrapper.Weight.ValueAndUnitCode);
		}

		public void TestGetGoodsValue()
		{
			var wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;
			AssertEquals(ZString.Empty, wrapper.GoodsValue.AmountAndCurrencyCode);

			wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;
			Consignment.HVC_GoodsValue = 123;
			AssertEquals("123.00", wrapper.GoodsValue.AmountAndCurrencyCode);
		}

		public void TestGetServiceLevel()
		{
			var wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;
			AssertEquals(ZString.Empty, wrapper.ServiceLevel.CodeAndDescription);

			wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;
			Consignment.BookingHeader.HVH_RS_NKBookingServiceLevel = "D2D";
			AssertEquals("D2D - Door to Door", wrapper.ServiceLevel.CodeAndDescription);
		}

		public void TestGetCarrierServiceLevel()
		{
			var wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;
			AssertEquals(ZString.Empty, wrapper.CarrierServiceLevel.CodeAndDescription);

			wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingProvider = true;
			Consignment.HVC_OH_LastMileCarrier = carrier.PK;

			Consignment.HVC_PL_NKLastMileCarrierServiceLevel = OrgCarrierServiceLevel.StandardCode;
			AssertEquals("STD - Standard", wrapper.CarrierServiceLevel.CodeAndDescription);
		}

		public void TestGetDeliveryAgent()
		{
			var wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;
			AssertEquals(ZString.Empty, wrapper.DeliveryAgent.CompanyCode);

			wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;

			var lmc = Factory.New<OrgHeader>();
			lmc.OH_Code = "DELIVER";

			Consignment.HVC_OH_LastMileCarrier = lmc.PK;
			AssertEquals("DELIVER", wrapper.DeliveryAgent.CompanyCode);
		}

		public void TestGetGoodsDescription()
		{
			const string goodsDescription = "Garbage";
			AssertEquals(ZString.Empty, Wrapper.GoodsDescription);
			Consignment.HVC_GoodsDescription = goodsDescription;
			AssertEquals(goodsDescription, Wrapper.GoodsDescription);
		}

		public void TestGetOrigin()
		{
			var wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;
			AssertEquals(ZString.Empty, wrapper.Destination.Location.UNLOCOAndPortName);

			wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			Consignment.ConsignmentHeader.HCH_JS_Shipment = shipment.PK;

			var originDepot = Factory.New<OrgAddress>();
			originDepot.OA_RL_NKRelatedPortCode = "AUMEL";
			Consignment.BookingHeader.HVH_OA_OriginDepot = originDepot.PK;

			AssertEquals("AUSYD - Sydney", wrapper.Origin.Location.UNLOCOAndPortName);

			wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;
			Consignment.ConsignmentHeader.HCH_JS_Shipment = ZGuid.Empty;

			AssertEquals("AUMEL - Melbourne", wrapper.Origin.Location.UNLOCOAndPortName);
		}

		public void TestGetDestination()
		{
			var wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;
			AssertEquals(ZString.Empty, wrapper.Destination.Location.UNLOCOAndPortName);

			wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";
			Consignment.ConsignmentHeader.HCH_JS_Shipment = shipment.PK;

			var destinationDepot = Factory.New<OrgAddress>();
			destinationDepot.OA_RL_NKRelatedPortCode = "AUMEL";
			Consignment.HVC_OA_DestinationDepot = destinationDepot.PK;

			AssertEquals("AUSYD - Sydney", wrapper.Destination.Location.UNLOCOAndPortName);

			wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;
			Consignment.ConsignmentHeader.HCH_JS_Shipment = ZGuid.Empty;

			AssertEquals("AUMEL - Melbourne", wrapper.Destination.Location.UNLOCOAndPortName);
		}

		public void TestGetDeliveryAddress()
		{
			const string address1 = "Under the bridge";
			Consignment.HVC_ConsigneeAddress1 = address1;
			AssertEquals(address1.ToUpper(), Wrapper.DeliveryAddress.AddressLine1);
		}

		public void TestGetJobNumberHeading()
		{
			AssertEquals("Consignment", Wrapper.JobNumberHeading);
		}

		public void TestGetJobNumber()
		{
			Consignment.HVC_ConsignmentId = "TestHVC123";
			AssertEquals("TestHVC123", Wrapper.JobNumber);
		}

		public void TestGetPickupAddress()
		{
			const string address1 = "Above the sky";
			Consignment.HVC_ShipperAddress1 = address1;
			AssertEquals(address1.ToUpper(), Wrapper.PickupAddress.AddressLine1);
		}

		public void TestGetOrderTrackingNumber()
		{
			Consignment.HVC_ShipperReference = "1111111";
			AssertEquals("1111111", Wrapper.OrderTrackingNumber);

			Consignment.HVC_ShipperReference = ZString.Empty;
			Consignment.HVC_WaybillNumber = "2222222";
			AssertEquals("2222222", Wrapper.OrderTrackingNumber);

			Consignment.HVC_ShipperReference = ZString.Empty;
			Consignment.HVC_WaybillNumber = ZString.Empty;
			Consignment.HVC_ConsignmentId = "0000000";
			AssertEquals("0000000", Wrapper.OrderTrackingNumber);
		}

		public void TestGetUnpackCFSAddress()
		{
			var wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;
			AssertEquals(ZString.Empty, wrapper.UnpackCFSAddress.AddressLine1);

			wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;
			var destinationDepot = Factory.New<OrgAddress>();
			destinationDepot.OA_Address1 = "Somewhere";
			Consignment.HVC_OA_DestinationDepot = destinationDepot.PK;

			AssertEquals("SOMEWHERE", wrapper.UnpackCFSAddress.AddressLine1);

			wrapper = GetNewDocumentWrapper() as FreightWrapperFromHVLVConsignment;
			var shipment = Factory.New<ForwardingShipment>();
			var docAddress = shipment.DocAddresses.AddNew(Enterprise.MasterFiles.Integration.DocAddressType.ConsignorDocumentaryAddress);
			docAddress.OrganisationPK = Factory.New<OrgHeader>().PK;

			var relatedOrg = Factory.New<OrgHeader>();
			relatedOrg.MainAddress.OA_Address1 = "Nowhere";

			docAddress.Organisation.AddRelatedParty(relatedOrg.PK, "RTA", ZString.Empty, ZString.Empty, ZString.Empty, GlbCompany.CurrentCompany);

			Consignment.ConsignmentHeader.HCH_JS_Shipment = shipment.PK;

			Consignment.HVC_OA_DestinationDepot = destinationDepot.PK;

			AssertEquals("NOWHERE", wrapper.UnpackCFSAddress.AddressLine1);
		}

		#region Overrides

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "DocumentNumber", "1" },
					{ "DocumentTotal", "1" },
					{ "JobNumber", "HVCTEST00001" },
					{ "OrderTrackingNumber", "HVCTEST00001" },
					{ "JobNumberBarcodeTextWithoutDocManagerCodes", "ÈHVCTEST00001eÊ" },
					{ "JobNumberHeading", "Consignment" }
				};
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			Item = Consignment.Items.AddNew();
			return new FreightWrapperFromHVLVConsignment(Consignment, Item, Factory);
		}

		#endregion
	}
}
