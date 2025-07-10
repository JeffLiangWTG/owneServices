using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.CLE.OrdersDataImport.Testing
{
	class CLEOrderValueObjectDataAdapterTest : TestCaseWithFactory
	{
		const string SourceLocalOrgCode = "SOURCECD";
		const string SourceForeignOrgCode = "SOURs";
		const string OrderNumber = "TST101";
		public void TestCreateOrUpdateFromValueObjectFromValueObject()
		{
			CreateValueObject(OrderValue);
			Adapter.CreateOrUpdateFromValueObject(OrderValue, Context);
			Order order = Factory.LoadTop1<Order>(new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, OrderNumber));
			AssertNotNull(order);
			AssertEquals(ZDateTime.BrettsBirthday, order.JD_ExWorksRequiredBy);
			AssertEquals("SEA", order.JD_TransportMode);
			AssertEquals("FCL", order.JD_ContainerMode);
			AssertEquals("FOB", order.JD_IncoTerm);
			AssertEquals("FOB", order.JD_IncoTerm);
			AssertEquals(SupplierOrg, order.Supplier);
			AssertEquals(BuyerOrg, order.Buyer);
			AssertEquals("USD", order.JD_RX_NKOrderCurrency);
			AssertEquals("AUSYD", order.JD_RL_NKPortOfLoading);
			AssertEquals("UAIEV", order.JD_RL_NKPortOfDischarge);
			AssertEquals("PLC", order.JD_OrderStatus);
			AssertNotNull(order.OrderLines);
			AssertEquals(1, order.OrderLines.Count);
			OrderLine orderLine = order.OrderLines[0];
			AssertEquals(1, orderLine.JO_LineNo);
			AssertEquals("325.235", orderLine.JO_Partno);
			AssertEquals("vodka", orderLine.JO_Description);
			AssertEquals(223.35m, orderLine.JO_QtyReceived);
			AssertEquals("NO", orderLine.JO_F3_NKPackType);
			AssertEquals(236.31m, orderLine.JO_LinePrice);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-45), orderLine.JO_LineDropDate);
			AssertEquals("special instructions", orderLine.JO_SpecialInstructions);
			AssertEquals(1, orderLine.Deliveries.Count);
			AssertEquals("delivery point", orderLine.Deliveries[0].J4_OA_NKDeliveryPoint);
		}

		public void TestCreateOrUpdateFromValueObject_AdditionalDetails()
		{
			CreateValueObject(OrderValue);
			Adapter.CreateOrUpdateFromValueObject(OrderValue, Context);
			var order = Factory.LoadTop1<Order>(new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, OrderNumber));
			AssertEquals(1, order.OrderLines[0].Deliveries.Count);
			AssertEquals("delivery point", order.OrderLines[0].Deliveries[0].J4_OA_NKDeliveryPoint);
			AssertEquals("UAAAR", order.JD_RL_NKGoodsDeliveredTo);
			AssertEquals("PLC", order.JD_OrderStatus);
		}

		public void TestCreateOrUpdateFromValueObject_Ports()
		{
			CreateValueObject(OrderValue);
			OrgSupplierBuyerLink link = null;
			foreach (OrgSupplierBuyerLink buyerLink in SupplierOrg.BuyerLinks)
			{
				if (buyerLink.Buyer == BuyerOrg)
				{
					link = buyerLink;
				}
			}

			if (link == null)
			{
				link = SupplierOrg.BuyerLinks.AddNew(BuyerOrg);
			}

			OrgSupBuyLinkTrnMode mode = link.OrgSupBuyLinkTrnModes.Find("SEA", "FCL");
			if (mode == null)
			{
				link.OrgSupBuyLinkTrnModes.AddNew();
				mode.PF_TransportMode = "SEA";
				mode.PF_ContainerMode = "FCL";
			}

			mode.PF_RL_NKLoadPort = "CAYYZ";
			mode.PF_RL_NKPlaceOfReceivalPort = "CATOR";
			OrderValue.OrderDetail.ShipmentPlanning.LoadPort = null;
			OrderValue.OrderDetail.ShipmentPlanning.DischargePort = null;
			Factory.Save();
			Adapter.CreateOrUpdateFromValueObject(OrderValue, Context);
			Order order = Factory.LoadTop1<Order>(new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, OrderNumber));
			AssertEquals("CATOR", order.JD_RL_NKGoodsAvailableAt);
			AssertEquals("UAAAR", order.JD_RL_NKGoodsDeliveredTo);
			AssertEquals("CAYYZ", order.JD_RL_NKPortOfLoading);
			AssertEquals("UAAAR", order.JD_RL_NKPortOfDischarge);
		}

		public void TestOrderStatusWhenExFactoryDateNoteEmpty()
		{
			CreateValueObject(OrderValue);
			OrderValue.OrderDetail.Milestones.ExFactory.Estimated = new ZDateTime(2009, 11, 11, 11, 11, 11);
			Adapter.CreateOrUpdateFromValueObject(OrderValue, Context);
			Order order = Factory.LoadTop1<Order>(new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, OrderNumber));
			AssertNotNull(order);
			AssertEquals("CNF", order.JD_OrderStatus);
		}

		public void TestExWorksRequiredBy()
		{
			OrgSupplierBuyerLink link = SupplierOrg.BuyerLinks.AddNew();
			link.OL_OH_Buyer = BuyerOrg.PK;
			Assert("Precondition: shoud be at least one element", link.OrgSupBuyLinkTrnModes.Count > 0);
			link.OrgSupBuyLinkTrnModes[0].PF_EstDeliveryDays = 23;
			link.OrgSupBuyLinkTrnModes[0].PF_TransportMode = OrderValue.OrderDetail.TransportMode.ToString();
			link.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = OrderValue.OrderDetail.ContainerMode.ToString();
			Factory.Save();
			OrderValue.OrderDetail.ExWorksRequiredBy = ZDateTime.Empty;
			Adapter.CreateOrUpdateFromValueObject(OrderValue, Context);
			Order order = Factory.LoadTop1<Order>(new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, OrderNumber));
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(-68), order.JD_ExWorksRequiredBy);
		}

		public void TestExWorksRequiredBy_OutOfMaxRange()
		{
			var outOfRangeDay = new ZDateTime(2085, 2, 12);
			Assert(OrderValue.OrderLines.Count > 0);
			var lineValue = OrderValue.OrderLines[0];
			lineValue.OrderLineDetail.DropDate = outOfRangeDay;
			var link = SupplierOrg.BuyerLinks.AddNew();
			link.OL_OH_Buyer = BuyerOrg.PK;
			Assert("Precondition: shoud be at least one element", link.OrgSupBuyLinkTrnModes.Count > 0);
			link.OrgSupBuyLinkTrnModes[0].PF_EstDeliveryDays = 23;
			link.OrgSupBuyLinkTrnModes[0].PF_TransportMode = OrderValue.OrderDetail.TransportMode.ToString();
			link.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = OrderValue.OrderDetail.ContainerMode.ToString();
			Factory.Save();
			OrderValue.OrderDetail.ExWorksRequiredBy = outOfRangeDay;
			AssertExceptionThrown<System.Xml.XmlException>($"ExWorksRequiredBy import should throw XmlException", $"ExWorksRequiredBy field is invalid. {outOfRangeDay.Day}/{outOfRangeDay.Month:00}/{outOfRangeDay.Year:0000} 12:00:00 AM is not in range [01-Jan-1900 - 06-Jun-2079].", () => Adapter.CreateOrUpdateFromValueObject(OrderValue, Context));
		}

		public void TestExWorksRequiredBy_OutOfMinRange()
		{
			var outOfRangeDay = new ZDateTime(1800, 2, 12);
			Assert(OrderValue.OrderLines.Count > 0);
			var lineValue = OrderValue.OrderLines[0];
			lineValue.OrderLineDetail.DropDate = outOfRangeDay;
			var link = SupplierOrg.BuyerLinks.AddNew();
			link.OL_OH_Buyer = BuyerOrg.PK;
			Assert("Precondition: shoud be at least one element", link.OrgSupBuyLinkTrnModes.Count > 0);
			link.OrgSupBuyLinkTrnModes[0].PF_EstDeliveryDays = 23;
			link.OrgSupBuyLinkTrnModes[0].PF_TransportMode = OrderValue.OrderDetail.TransportMode.ToString();
			link.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = OrderValue.OrderDetail.ContainerMode.ToString();
			Factory.Save();
			OrderValue.OrderDetail.ExWorksRequiredBy = outOfRangeDay;
			AssertExceptionThrown<System.Xml.XmlException>($"ExWorksRequiredBy import should throw XmlException", $"ExWorksRequiredBy field is invalid. {outOfRangeDay.Day}/{outOfRangeDay.Month:00}/{outOfRangeDay.Year:0000} 12:00:00 AM is not in range [01-Jan-1900 - 06-Jun-2079].", () => Adapter.CreateOrUpdateFromValueObject(OrderValue, Context));
		}

		public void TestExWorksRequiredBy_OutOfMinRange_AfterApplyingDifferenceWithPF_EstDeliveryDays()
		{
			var outOfRangeDay = new ZDateTime(1800, 3, 10);
			Assert(OrderValue.OrderLines.Count > 0);
			var lineValue = OrderValue.OrderLines[0];
			lineValue.OrderLineDetail.DropDate = new ZDateTime(1900, 1, 2);
			var link = SupplierOrg.BuyerLinks.AddNew();
			link.OL_OH_Buyer = BuyerOrg.PK;
			Assert("Precondition: shoud be at least one element", link.OrgSupBuyLinkTrnModes.Count > 0);
			link.OrgSupBuyLinkTrnModes[0].PF_EstDeliveryDays = 23;
			link.OrgSupBuyLinkTrnModes[0].PF_TransportMode = OrderValue.OrderDetail.TransportMode.ToString();
			link.OrgSupBuyLinkTrnModes[0].PF_ContainerMode = OrderValue.OrderDetail.ContainerMode.ToString();
			Factory.Save();
			OrderValue.OrderDetail.ExWorksRequiredBy = outOfRangeDay;
			AssertExceptionThrown<System.Xml.XmlException>($"ExWorksRequiredBy import should throw XmlException", $"ExWorksRequiredBy field is invalid. {outOfRangeDay.Day}/{outOfRangeDay.Month:00}/{outOfRangeDay.Year:0000} 12:00:00 AM is not in range [01-Jan-1900 - 06-Jun-2079].", () => Adapter.CreateOrUpdateFromValueObject(OrderValue, Context));
		}

		public void TestOrderLineCancelled_AcordingLineNotProvidedInValueObject()
		{
			Xsd.OrderOrderLine lineValue = OrderValue.OrderLines.AddNew();
			lineValue.OrderLineNo = 2;
			lineValue.OrderLineDetail.Product = "325.22355";
			lineValue.OrderLineDetail.Description = "pivo";
			lineValue.OrderLineDetail.QtyReceived.Value = 229.39m;
			lineValue.OrderLineDetail.QtyReceived.DimensionType = "NO";
			lineValue.OrderLineDetail.QtyOrdered.Value = 229.39m;
			lineValue.OrderLineDetail.QtyOrdered.DimensionType = "NO";
			lineValue.OrderLineDetail.LinePrice.Value = 2446.34m;
			lineValue.OrderLineDetail.DropDate = ZDateTime.BrettsBirthday.AddDays(-45);
			lineValue.OrderLineDetail.SpecialInstructions = "special instructions";
			Xsd.OrderOrderLineOrderLineDelivery delivery = lineValue.OrderLineDeliveries.AddNew();
			delivery.DeliveryDetails.Address.AddressSequenceRef = 1;
			delivery.DeliveryDetails.Address.Organisation.OwnerCode = OrderValue.OrderDetail.Buyer.OwnerCode;
			delivery.DeliveryDetails.Address.Organisation.OrganisationDetails.Addresses.AddNew().AddressCode = "delivery point";
			Adapter.CreateOrUpdateFromValueObject(OrderValue, Context);
			Factory.Save();
			OrderValue.OrderLines.RemoveAt(1);
			Adapter.CreateOrUpdateFromValueObject(OrderValue, Context);
			Factory.Save();
			Order order = Factory.LoadTop1<Order>(new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, OrderNumber));
			AssertEquals(1, order.OrderLines.Count);
		}

		public void TestOrderLineCancelled_QuantityEqualZero()
		{
			Xsd.OrderOrderLine lineValue = OrderValue.OrderLines.AddNew();
			lineValue.OrderLineNo = 2;
			lineValue.OrderLineDetail.Product = "325.22355";
			lineValue.OrderLineDetail.Description = "pivo";
			lineValue.OrderLineDetail.QtyReceived.Value = 229.39m;
			lineValue.OrderLineDetail.QtyReceived.DimensionType = "NO";
			lineValue.OrderLineDetail.QtyOrdered.Value = 229.39m;
			lineValue.OrderLineDetail.QtyOrdered.DimensionType = "NO";
			lineValue.OrderLineDetail.LinePrice.Value = 2446.34m;
			lineValue.OrderLineDetail.DropDate = ZDateTime.BrettsBirthday.AddDays(-45);
			lineValue.OrderLineDetail.SpecialInstructions = "special instructions";
			Xsd.OrderOrderLineOrderLineDelivery delivery = lineValue.OrderLineDeliveries.AddNew();
			delivery.DeliveryDetails.Address.AddressSequenceRef = 1;
			delivery.DeliveryDetails.Address.Organisation.OwnerCode = OrderValue.OrderDetail.Buyer.OwnerCode;
			delivery.DeliveryDetails.Address.Organisation.OrganisationDetails.Addresses.AddNew().AddressCode = "delivery point";
			Adapter.CreateOrUpdateFromValueObject(OrderValue, Context);
			Factory.Save();
			lineValue.OrderLineDetail.QtyOrdered.Value = ZDecimal.Zero;
			Adapter.CreateOrUpdateFromValueObject(OrderValue, Context);
			Factory.Save();
			Order order = Factory.LoadTop1<Order>(new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, OrderNumber));
			AssertEquals(1, order.OrderLines.Count);
		}

		public void TestUpdateRejected_OrderLinkedWithShipmentWhichHasDeclaration()
		{
			Adapter.CreateOrUpdateFromValueObject(OrderValue, Context);
			Factory.Save();
			Order order = Factory.LoadTop1<Order>(new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, OrderNumber));
			order.JD_JS = Factory.NewWithValidTestData<ForwardingShipment>().PK;
			Factory.Save();
			Adapter.CreateOrUpdateFromValueObject(OrderValue, Context);
			Assert(Notifications.AsString.Contains("Order TST101 updated"));
			Factory.NewWithValidTestData<BaseJobDeclaration>().JE_JS = order.JD_JS;
			Factory.Save();
			int emailsCreated = Env.OutgoingMailManager.EmailsCreated.Count;
			Adapter.CreateOrUpdateFromValueObject(OrderValue, Context);
			Assert(Notifications.AsString.Contains("Order TST101 is linked to a shipment which has declaration and cannot be updated."));
			AssertEquals(emailsCreated, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestUpdateRejected_OrderLinkedWithDeclaration()
		{
			Adapter.CreateOrUpdateFromValueObject(OrderValue, Context);
			Factory.Save();
			Order order = Factory.LoadTop1<Order>(new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, OrderNumber));
			order.JD_JE = Factory.NewWithValidTestData<BaseJobDeclaration>().PK;
			Factory.Save();
			int emailsCreated = Env.OutgoingMailManager.EmailsCreated.Count;
			Adapter.CreateOrUpdateFromValueObject(OrderValue, Context);
			Assert(Notifications.AsString.Contains("Order TST101 is linked to a declaration and cannot be updated."));
			AssertEquals(emailsCreated, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		void CreateValueObject(Xsd.Order orderValue)
		{
			orderValue.OrderIdentifier.OrderNumber = OrderNumber;
			orderValue.OrderDetail.ExWorksRequiredBy = ZDateTime.BrettsBirthday;
			orderValue.OrderDetail.TransportMode = Xsd.OrderTransportMode.SEA;
			orderValue.OrderDetail.TransportModeSpecified = true;
			orderValue.OrderDetail.ContainerMode = Xsd.OrderContainerMode.FCL;
			orderValue.OrderDetail.ContainerModeSpecified = true;
			orderValue.OrderDetail.OrderDateTime = ZDateTime.BrettsBirthday.AddDays(3);
			orderValue.OrderDetail.Incoterm = "FOB";
			orderValue.OrderDetail.Supplier.EDICode = "AAAAAAAA";
			orderValue.OrderDetail.Supplier.OrganisationDetails.Name = "AAAA PTY LTD";
			orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses = new Xsd.OrgAddressCollection();
			Xsd.OrgAddress supplierAddress = orderValue.OrderDetail.Supplier.OrganisationDetails.Addresses.AddNew();
			supplierAddress.AddressLine1 = "Addr1";
			supplierAddress.AddressLine2 = "Addr2";
			supplierAddress.CityOrSuburb = "SYDNEY";
			supplierAddress.StateOrProvince = "NSW";
			supplierAddress.PostCode = "04213";
			supplierAddress.Location = Xsd.UNLOCO.FromPortCode(Factory, "AUSYD");
			orderValue.OrderDetail.Buyer.EDICode = "BBBBBBBB";
			orderValue.OrderDetail.Buyer.OrganisationDetails.Name = "BBBB PTY LTD";
			orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses = new Xsd.OrgAddressCollection();
			Xsd.OrgAddress buyerAddress = orderValue.OrderDetail.Buyer.OrganisationDetails.Addresses.AddNew();
			buyerAddress.AddressLine1 = "Addr1";
			buyerAddress.AddressLine2 = "Addr2";
			buyerAddress.CityOrSuburb = "KIEV";
			buyerAddress.StateOrProvince = "STST";
			buyerAddress.PostCode = "23462";
			buyerAddress.Location = Xsd.UNLOCO.FromPortCode(Factory, "UAIEV");
			orderValue.OrderDetail.OrderTotal.Value = 236.32m;
			orderValue.OrderDetail.OrderTotal.CurrencyCode = "USD";
			orderValue.OrderDetail.Custom.Text2 = SourceForeignOrgCode;
			orderValue.OrderDetail.ShipmentPlanning.LoadPort = Xsd.UNLOCO.FromPortCode(Factory, "AUSY");
			orderValue.OrderDetail.ShipmentPlanning.DischargePort = Xsd.UNLOCO.FromPortCode(Factory, "UAIE");
			Xsd.OrderOrderLine lineValue = orderValue.OrderLines.AddNew();
			lineValue.OrderLineNo = 1;
			lineValue.OrderLineDetail.Product = "325.235";
			lineValue.OrderLineDetail.Description = "vodka";
			lineValue.OrderLineDetail.QtyReceived.Value = 223.35m;
			lineValue.OrderLineDetail.QtyReceived.DimensionType = "NO";
			lineValue.OrderLineDetail.QtyOrdered.Value = 223.35m;
			lineValue.OrderLineDetail.QtyOrdered.DimensionType = "NO";
			lineValue.OrderLineDetail.LinePrice.Value = 236.31m;
			lineValue.OrderLineDetail.DropDate = ZDateTime.BrettsBirthday.AddDays(-45);
			lineValue.OrderLineDetail.SpecialInstructions = "special instructions";
			Xsd.OrderOrderLineOrderLineDelivery delivery = lineValue.OrderLineDeliveries.AddNew();
			delivery.DeliveryDetails.Address.AddressSequenceRef = 1;
			delivery.DeliveryDetails.Address.Organisation.OwnerCode = orderValue.OrderDetail.Buyer.OwnerCode;
			delivery.DeliveryDetails.Address.Organisation.OrganisationDetails.Addresses.AddNew().AddressCode = "delivery point";
		}

		#region Implementation
		CLEOrderValueObjectDataAdapter Adapter
		{
			get
			{
				if (fAdapter == null)
				{
					fAdapter = new CLEOrderValueObjectDataAdapter();
				}

				return fAdapter;
			}
		}

		CLEOrderValueObjectDataAdapter fAdapter;
		ValueObjectImportContext Context
		{
			get
			{
				if (fContext == null)
				{
					Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
					interchange.InterchangeInfo.EDIOrganisation.OwnerCode = SourceForeignOrgCode;
					fContext = new ValueObjectImportContext(Factory, interchange, Notifications);
				}

				return fContext;
			}
		}

		ValueObjectImportContext fContext;
		readonly NotificationBuffer Notifications = new NotificationBuffer();
		OrgHeader Source;
		OrgHeader BuyerOrg;
		OrgHeader SupplierOrg;
		Xsd.Order OrderValue;
		#endregion
		#region Set Up
		protected override void SetUp()
		{
			base.SetUp();
			DataTransferSwitchRegistryBusinessObject b = new DataTransferSwitchRegistryBusinessObject();
			GlbGroup g = Factory.NewWithValidTestData<GlbGroup>();
			g.GG_Code = "TMP";
			var staff = g.Staff.AddNew();
			staff.GS_EmailAddress = "bbb@ccc.com";
			staff.GS_Code = "ZAC";
			Factory.Save();
			b.GroupPK = g.PK;
			b.NextRunDateTime = ZDateTime.Now.AddMinutes(-20);
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			b.Directory = resourceRetriever.SaveAllResourcesToFiles();
			b.Interval = 25;
			b.EnableInterface = true;
			CLEDataRegistry.Instance.SwitchOrderImportItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, b);
			Source = Factory.NewWithValidTestData<OrgHeader>();
			Source.OH_Code = SourceLocalOrgCode;
			Source.OH_FullName = "Source Organisation";
			OrgPatternMatchOverride match = Factory.NewWithValidTestData<OrgPatternMatchOverride>();
			match.OO_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			match.OO_ForeignCode = SourceForeignOrgCode;
			match.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Organisation;
			match.OO_LocalCode = SourceLocalOrgCode;
			match.OO_LocalGuid = Source.PK;
			OrgPatternMatchOverride matchLoadPort = Source.CreatePatternMatchOverrideForTest();
			matchLoadPort.OO_ForeignCode = "AUSY";
			matchLoadPort.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			matchLoadPort.OO_LocalCode = "AUSYD";
			matchLoadPort.OO_LocalGuid = RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "SYDNEY", "AU").PK;
			OrgPatternMatchOverride matchDischargePort = Source.CreatePatternMatchOverrideForTest();
			matchDischargePort.OO_ForeignCode = "UAIE";
			matchDischargePort.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			matchDischargePort.OO_LocalCode = "UAIEV";
			matchDischargePort.OO_LocalGuid = RefUNLOCO.GetPortFromNameAndCountryCode(Factory, "Kiev", "UA").PK;
			BuyerOrg = Factory.NewWithValidTestData<OrgHeader>();
			BuyerOrg.OH_Code = "BBBBBBBB";
			BuyerOrg.OH_FullName = "BBBB PTY LTD";
			OrgAddress address = BuyerOrg.Addresses.AddNew();
			address.OA_Code = "delivery point";
			address.OA_Address1 = "delivery point";
			address.OA_RL_NKRelatedPortCode = "UAAAR";
			SupplierOrg = Factory.NewWithValidTestData<OrgHeader>();
			SupplierOrg.OH_Code = "AAAAAAAA";
			SupplierOrg.OH_FullName = "AAAA PTY LTD";
			Factory.Save();
			OrderValue = new Xsd.Order();
			CreateValueObject(OrderValue);
		}

		EmbeddedResourceRetriever resourceRetriever;
		#endregion

		protected override void TearDown()
		{
			base.TearDown();
			resourceRetriever.Dispose();
		}
	}
}
