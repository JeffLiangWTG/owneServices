using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.Wow.CASSKIRKOrderIntegration
{
	class CASSKIRKTestHelper
	{
		public CASSKIRKTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public OrgHeader Importer
		{
			get
			{
				if (importer == null)
				{
					importer = Factory.LoadTop1<OrgHeader>(new ZQuery());
					OrgAddress addr = importer.Addresses.AddNew();
					addr.OA_Code = "5210";
					addr.OA_Address1 = "addr 1";
					addr.OA_RL_NKRelatedPortCode = "AUSYD";
				}
				return importer;
			}
		}
		OrgHeader importer;

		public OrgHeader Supplier
		{
			get
			{
				return (supplier) ?? (supplier = Factory.LoadTop1<OrgHeader>(new ZQuery()));
			}
		}
		OrgHeader supplier;

		public Order GetOrder()
		{
			Order order = Factory.NewWithValidTestData<Order>(TestBusinessObjectKind.MinimumRequiredToSave);
			order.JD_OrderNumber = "1001";
			order.BuyerPK = Importer.PK;
			order.SupplierPK = Supplier.PK;
			order.JD_RL_NKPortOfDischarge = "AUSYD";
			order.JD_CustomDecimal1 = 1m;

			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineStatus = "DUE";
			orderLine.JO_LineNo = 1;
			orderLine.JO_Partno = "Product";
			orderLine.JO_Description = "Pencil";
			orderLine.JO_ItemPrice = 12.5m;

			OrderLineDelivery delivery = orderLine.Deliveries.AddNew();
			delivery.J4_OA_NKDeliveryPoint = "5210";
			delivery.J4_RL_NKDestinationPort = order.JD_RL_NKPortOfDischarge;

			OrderLineDeliverContainer container1 = delivery.Containers.AddNew();
			container1.J5_QuantityInvoiced = 300m;
			container1.J5_ContainerNum = "CONT1";

			Factory.Save();
			return order;
		}

		public void SetupAllRegistry()
		{
			SetupNoteType();
			SetupOrgMapping();
			WowDataRegistry.Instance.SetDeclarationImporter(GlbBranch.CurrentBranch, GlbCompany.CurrentCompany.OrgProxy.PK.ToGuid());
		}

		public void SetupOrgMapping()
		{
			OrgHeader orgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery());
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = orgProxy.PK;

			Factory.Save();
			OrgPatternMatchOverride buyerPatthern = orgProxy.CreatePatternMatchOverrideForTest();
			buyerPatthern.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			buyerPatthern.OO_ForeignCode = "Importer";
			buyerPatthern.OO_LocalCode = Importer.OH_Code;
			buyerPatthern.OO_LocalGuid = Importer.PK;

			OrgPatternMatchOverride supplierPatthern = orgProxy.CreatePatternMatchOverrideForTest();
			supplierPatthern.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
			supplierPatthern.OO_ForeignCode = "Supplier";
			supplierPatthern.OO_LocalCode = Supplier.OH_Code;
			supplierPatthern.OO_LocalGuid = Supplier.PK;

			Factory.Save();
		}

		public void SetupNoteType()
		{
			CustomNoteModuleAndCountry orderModule = new CustomNoteModuleAndCountry();
			orderModule.ModuleIDName = ModuleIDs.Orders.Name;
			orderModule.CountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AddNewNoteType(CASSKIRKConstant.ReplenishersEmailNoteType, orderModule.CustomNoteTypesList);
			AddNewNoteType(CASSKIRKConstant.ReplenishersFaxNoteType, orderModule.CustomNoteTypesList);
			AddNewNoteType(CASSKIRKConstant.VendorB2BAddressNoteType, orderModule.CustomNoteTypesList);

			CustomNoteTypes collection = new CustomNoteTypes();
			collection.NoteModuleAndCountryList.Add(orderModule);

			SystemDataRegistry.Instance.CustomNotes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);
		}

		void AddNewNoteType(ZString noteType, CustomNoteTypeItemCollection list)
		{
			CustomNoteTypeItem item = list.AddNew();
			item.IsTextOnly = true;
			item.DefaultVisibility = nameof(StmNoteVisibility.INT);
			item.NoteName = noteType;
		}

		readonly BusinessObjectFactory Factory;
	}
}
