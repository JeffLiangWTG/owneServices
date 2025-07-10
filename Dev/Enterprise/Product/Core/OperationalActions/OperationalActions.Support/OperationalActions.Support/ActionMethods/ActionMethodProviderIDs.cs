using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Support
{
	public static class ActionMethodProviderIDs
	{
#if DEBUG
		public readonly static ActionMethodProviderID DummyWithMethods = new ActionMethodProviderID(new ZGuid("{7194CB51-BE6A-423d-A57A-E8ACC3387167}"), (NoResString)"Dummy With Methods", "Enterprise.Services.OperationalActions.Module.Testing.DummyActionMethodControllerWithMethods,Enterprise.Services.OperationalActions.Module.Test");
		public readonly static ActionMethodProviderID DummyWithoutMethods = new ActionMethodProviderID(new ZGuid("{5E9FF3E2-20EF-45fb-B35A-EA6CF3431279}"), (NoResString)"Dummy Without Methods", "Enterprise.Services.OperationalActions.Module.Testing.DummyActionMethodControllerWithoutMethods,Enterprise.Services.OperationalActions.Module.Test");
#endif

		public readonly static ActionMethodProviderID Accounting = new ActionMethodProviderID(new ZGuid("{29FFE72C-8553-4536-AD08-DE6AFEA2735B}"), ResString.GetMultilingualString("29ffe72c-8553-4536-ad08-de6afea2735b", "Accounting"), (NoResString)"Enterprise.Accounting.Module.AccountingActionMethodProvider,Enterprise.Accounting.Module");
		public readonly static ActionMethodProviderID GbJobDeclaration = new ActionMethodProviderID(new ZGuid("2F2C0043-8C73-43c1-8F44-2A19A4E5E8D4"), ResString.GetMultilingualString("2F2C0043-8C73-43c1-8F44-2A19A4E5E8D4", "Customs Operational Actions"), (NoResString)"Enterprise.Customs.GB.Module.OperationalActions.GbDeclarationOperationalActionMethodProvider, Enterprise.Customs.GB.Module");
		public readonly static ActionMethodProviderID GbPickupDropOff = new ActionMethodProviderID(new ZGuid("12345678-8C73-43c1-8F44-2A19A4E5E8D4"), ResString.GetMultilingualString("12345678-8C73-43c1-8F44-2A19A4E5E8D4", "Drop Off and Pick Up Actions"), (NoResString)"Enterprise.Customs.GB.Module.OperationalActions.DropOff.GbDeclarationDropOffMessageOperationalActionMethodProvider, Enterprise.Customs.GB.Module");
		public readonly static ActionMethodProviderID GbCcsuk = new ActionMethodProviderID(new ZGuid("33F7CFB0-014C-4D97-84F8-5FE08DDA84FF"), ResString.GetMultilingualString("33F7CFB0-014C-4D97-84F8-5FE08DDA84FF", "CCSUK Operational Actions"), (NoResString)"Enterprise.Customs.GB.Module.OperationalActions.Ccsuk.CcsukOperationalActionMethodProvider, Enterprise.Customs.GB.Module");
		public readonly static ActionMethodProviderID Shipping = new ActionMethodProviderID(new ZGuid("{7933180C-616D-452b-8E8B-0F59C8A1B13C}"), ResString.GetMultilingualString("7933180C-616D-452b-8E8B-0F59C8A1B13C", "Shipping"), (NoResString)"Enterprise.Freight.Agency.Module.ShippingManagerActionMethodProvider,Enterprise.Freight.Agency.Module");
		public readonly static ActionMethodProviderID Warehouse = new ActionMethodProviderID(new ZGuid("{A0549318-D4A5-43d6-8CF2-6FEA21EBADCC}"), ResString.GetMultilingualString("a0549318-d4a5-43d6-8cf2-6fea21ebadcc", "Warehouse"), (NoResString)"Enterprise.Warehouse.Transactions.Module.WhsOperationalActionMethodProvider,Enterprise.Warehouse.Transactions.Module");
		public readonly static ActionMethodProviderID USJobDeclaration = new ActionMethodProviderID(new ZGuid("D6876049-CEAD-4432-A82A-38EB9D253554"), ResString.GetMultilingualString("8C50D252-07D4-4DEF-9F1A-7DF5BA7A9A9E", "US Customs Operational Actions"), (NoResString)"Enterprise.Customs.US.Module.OperationalActions.USDeclarationOperationalActionMethodProvider, Enterprise.Customs.US.Module");
		public readonly static ActionMethodProviderID CAJobDeclaration = new ActionMethodProviderID(new ZGuid("CE446149-A611-464D-9BD5-F8FEC071243D"), ResString.GetMultilingualString("0DAD7662-C3B0-4614-9EA3-E93F6BB13814", "CA Customs Operational Actions"), (NoResString)"Enterprise.Customs.CA.Module.OperationalActions.CADeclarationOperationalActionMethodProvider, Enterprise.Customs.CA.Module");
		public readonly static ActionMethodProviderID TWJobDeclaration = new ActionMethodProviderID(new ZGuid("EAE60464-653C-4516-A71D-ED385869162F"), ResString.GetMultilingualString("DC35577C-4F46-42A1-973C-0162F407585A", "TW Customs Operational Actions"), (NoResString)"Enterprise.Customs.TW.Module.OperationalActions.TWDeclarationOperationalActionMethodProvider, Enterprise.Customs.TW.Module");
		public readonly static ActionMethodProviderID B2Adjustments = new ActionMethodProviderID(new ZGuid("2d55f12a-9f2f-4873-9040-2637126e8788"), ResString.GetMultilingualString("98be01bf-0d17-4a76-9b20-230237ce35d2", "CA B2/B3X Operational Actions"), (NoResString)"Enterprise.Customs.CA.Module.B2AdjustmentsOperationalActionMethodProvider, Enterprise.Customs.CA.Module");
		public readonly static ActionMethodProviderID AWB = new ActionMethodProviderID(new ZGuid("{3DE08E3F-639C-417A-BAF7-33008F9A90EA}"), ResString.GetMultilingualString("937b48e4-25df-4097-9546-f2b979ff1cb3", "AWB"), (NoResString)"Enterprise.Freight.Forwarding.Module.AWBActionMethodProvider,Enterprise.Freight.Forwarding.Module");
		public readonly static ActionMethodProviderID General = new ActionMethodProviderID(new ZGuid("{198814F2-91C0-4574-B848-17138097AEA6}"), ResString.GetMultilingualString("0e02b008-24f4-4d2f-b8d9-620da42786a0", "General"), (NoResString)"Enterprise.Services.OperationalActions.Module.GeneralActionMethodProvider,Enterprise.Services.OperationalActions.Module");
		public readonly static ActionMethodProviderID HVLV = new ActionMethodProviderID(new ZGuid("80857E76-13D3-4930-A6B4-D463C885657B"), ResString.GetMultilingualString("29a5f286-c365-4424-9a10-b0b2bdc126dd", "HVLV"), (NoResString)"Enterprise.eTail.Module.HVLVActionMethodProvider,Enterprise.eTail.Module");
		public readonly static ActionMethodProviderID DtbBooking = new ActionMethodProviderID(new ZGuid("{565FE8EC-E379-49CF-9FAF-718D4D44F577}"), ResString.GetMultilingualString("777511EB-6AD8-4552-8128-792D4F51B4A4", "Transport Booking"), (NoResString)"Enterprise.TransportBookings.Module.DtbBookingOperationalActionMethodProvider,Enterprise.TransportBookings.Module");
		public readonly static ActionMethodProviderID DtbBookingParent = new ActionMethodProviderID(new ZGuid("3C168D1B-13C0-4E78-8F3B-F398FC5B7497"), ResString.GetMultilingualString("68C621D7-4860-47D2-ABEE-B9E913364258", "Transport Booking Parent"), (NoResString)"Enterprise.TransportBookings.Module.DtbBookingParentOperationalActionMethodProvider, Enterprise.TransportBookings.Module");
		public readonly static ActionMethodProviderID MasterFiles = new ActionMethodProviderID(new ZGuid("{6B5B6FC9-DF8C-4E6B-8729-D03A5AD71DEE}"), ResString.GetMultilingualString("C74CDAEA-91C1-470B-BBD1-EFC89FEFE45F", "Master Files"), (NoResString)"Enterprise.MasterFiles.Module.MasterFilesOperationalActionMethodProvider,Enterprise.MasterFiles.Module");
		public readonly static ActionMethodProviderID PRAMessage = new ActionMethodProviderID(new ZGuid("{2932065B-16A9-4549-B905-F0778B3FFF20}"), ResString.GetMultilingualString("D0FEDF16-47AB-4AF8-B7B1-72E496350414", "PRA Message"), (NoResString)"Enterprise.Freight.Forwarding.Module.PRAActionMethodProvider,Enterprise.Freight.Forwarding.Module");
		public readonly static ActionMethodProviderID JobDeclaration = new ActionMethodProviderID(new ZGuid("{BC031BF3-9DD0-426E-A1B9-A34F640553FC}"), ResString.GetMultilingualString("73332CD3-F314-41B0-8513-6F20E447789B", "Integrated Countries/Regions Customs Operational Actions"), (NoResString)"Enterprise.Customs.Module.OperationalActions.DeclarationOperationalActionMethodProvider,Enterprise.Customs.Module");
		public readonly static ActionMethodProviderID Brokerage = new ActionMethodProviderID(new ZGuid("{6F3EDF6D-11EB-4708-B89C-5A2D5709F6CE}"), ResString.GetMultilingualString("914A3DF0-6E5B-478A-8A75-5C30AE1217A3", "Brokerage Operational Actions"), (NoResString)"Enterprise.Customs.Module.OperationalActions.BrokerageOperationalActionMethodProvider,Enterprise.Customs.Module");
		public readonly static ActionMethodProviderID USStatement = new ActionMethodProviderID(new ZGuid("0095FE98-D800-4841-AEFE-AB3F20D86279"), ResString.GetMultilingualString("{74CC843-5E89-47A4-A92A-48D19C73FC0A", "US Statement Operational Actions"), (NoResString)"Enterprise.Customs.US.Module.USStatementOperationalActionMethodProvider, Enterprise.Customs.US.Module");
		public readonly static ActionMethodProviderID FrJobDeclaration = new ActionMethodProviderID(new ZGuid("12345678-8C73-43c1-8F44-2A19A4E5A2E4"), ResString.GetMultilingualString("12345678-8C73-43c1-8F44-2A19A4E5A2E4", "FR Customs Operational Actions"), (NoResString)"Enterprise.Customs.FR.Module.FrDeclarationOperationalActionMethodProvider, Enterprise.Customs.FR.Module");
		public readonly static ActionMethodProviderID CNCusEntry = new ActionMethodProviderID(new ZGuid("77C41424-859B-4C92-AEF6-1F951DCF80C0"), ResString.GetMultilingualString("FC4E36C2-044A-4D0A-8B53-9E6EE8F7C10D", "CN Customs Entries Operational Actions"), (NoResString)"Enterprise.Customs.CN.Module.EntryHeaderOperationalActionMethodProvider, Enterprise.Customs.CN.Module");
		public readonly static ActionMethodProviderID USLowValueBill = new ActionMethodProviderID(new ZGuid("f188654d-a0c0-4dd9-9aaa-110e3ba24d35"), ResString.GetMultilingualString("9c57ca9f-599e-4fc9-955f-f422cd4d1477", "US Low Value Entries by Bill Operational Actions"), (NoResString)"Enterprise.Customs.US.LVS.Module.LowValueEntriesBillActionMethodProvider, Enterprise.Customs.US.LVS.Module");
		public readonly static ActionMethodProviderID Shipment = new ActionMethodProviderID(new ZGuid("48ea01b1-07ac-4e0c-afda-6c54a33f3fa4"), ResString.GetMultilingualString("da1875f2-f881-4110-ab84-88c16e0c78f2", "Shipment Operational Actions"), (NoResString)"Enterprise.Freight.Forwarding.Module.ShipmentActionMethodProvider, Enterprise.Freight.Forwarding.Module");
		public readonly static ActionMethodProviderID SendAdvancedAirCargoReport = new ActionMethodProviderID(new ZGuid("{C8DADE4E-4F31-4958-99B7-95A9F7DF4816}"), ResString.GetMultilingualString("224F8B00-7845-42BB-9DF4-A65A10C805CF", "Send Advanced Air Cargo Report"), (NoResString)"Enterprise.Freight.Forwarding.Module.SendAdvancedAirCargoReportActionMethodProvider,Enterprise.Freight.Forwarding.Module");
		public readonly static ActionMethodProviderID USInBond = new ActionMethodProviderID(new ZGuid("{61E3207A-EDCA-4953-8F6E-6DFBCC981CA8}"), ResString.GetMultilingualString("8FD41D92-C34E-4958-807D-0F097B9E909C", "US In-Bond Operational Actions"), (NoResString)"Enterprise.Customs.US.InBond.Module.OperationalActions.USInBondOperationalActionMethodProvider,Enterprise.Customs.US.InBond.Module");
		public readonly static ActionMethodProviderID EUJobDeclaration = new ActionMethodProviderID(new ZGuid("31802C42-102E-4D4C-952B-A9DA4199C0E8"), ResString.GetMultilingualString("3F5BA67C-31F2-4899-A3E1-C37C3F7B21C5", "EU Customs Operational Actions"), (NoResString)"Enterprise.Customs.EU.Module.OperationalActions.EUDeclarationOperationalActionMethodProvider, Enterprise.Customs.EU.Module");
		public readonly static ActionMethodProviderID FRCusEntry = new ActionMethodProviderID(new ZGuid("7C09E4F1-AEE1-453A-A4F2-EE901657F3E6"), ResString.GetMultilingualString("6179C637-69E2-4EE5-9056-41BDDACAF3DA", "FR Customs Entries Operational Actions"), (NoResString)"Enterprise.Customs.FR.Module.EntryHeaderOperationalActionMethodProvider, Enterprise.Customs.FR.Module");
		public readonly static ActionMethodProviderID WarehouseOperatorTransactions = new ActionMethodProviderID(new ZGuid("67234BAD-19BA-4155-B309-4CCC269E433F"), ResString.GetMultilingualString("D4CC320D-9ED3-43AA-99F9-4C80D1AF18BF", "Warehouse Operator Transactions Operational Actions"), (NoResString)"Enterprise.Customs.ZA.Module.WarehouseOperatorTransactionsOperationalActionMethodProvider, Enterprise.Customs.ZA.Module");
		public readonly static ActionMethodProviderID BRGoodsCatalog = new ActionMethodProviderID(new ZGuid("5F5963C4-0ED5-4A37-927C-02721486E190"), ResString.GetMultilingualString("31EBE50E-C56E-4EDD-A726-1C26F50DC376", "Goods Catalog Operational Actions"), (NoResString)"Enterprise.Customs.BR.Module.GoodsCatalogOperationalActionMethodProvider, Enterprise.Customs.BR.Module");
		public readonly static ActionMethodProviderID INManifest = new ActionMethodProviderID(new ZGuid("D9C62CCB-B9FD-4C2C-9243-D98F9027A008"), ResString.GetMultilingualString("BAD4FA86-6339-48BD-9576-E9B2D2B69E8D", "IN Customs Operational Actions"), (NoResString)"Enterprise.Customs.IN.Manifest.Module.INManifestOperationalActionMethodProvider, Enterprise.Customs.IN.Manifest.Module");
		public readonly static ActionMethodProviderID EUH7 = new ActionMethodProviderID(new ZGuid("8707f99f-8fc4-4fb4-84d1-0b28a6095114"), ResString.GetMultilingualString("22e29c7c-68e1-48ae-8393-c77e35509afc", "EU H7 Operational Actions"), (NoResString)"Enterprise.Customs.EU.H7.Module.EUH7ActionMethodProvider, Enterprise.Customs.EU.H7.Module");
		public readonly static ActionMethodProviderID EDICommunicationsMode = new ActionMethodProviderID(new ZGuid("e4803b38-cad1-4bae-9d6d-54d598a68c95"), ResString.GetMultilingualString("e3feb219-b59c-4b89-b1d0-99ac4f4ed5c5", "EDI Communications Mode Operational Actions"), (NoResString)"Enterprise.Messaging.Module.EDICommunicationsModeOperationalActionMethodProvider, Enterprise.Messaging.Module");
		public readonly static ActionMethodProviderID FRProduct = new ActionMethodProviderID(new ZGuid("C7CAC60E-EB7D-40EE-AF15-54B13441D634"), ResString.GetMultilingualString("89B211A3-FA11-4029-9FFC-20809C24D531", "FR Product Customs Operational Actions"), (NoResString)"Enterprise.Customs.FR.Module.ProductOperationalActionMethodProvider, Enterprise.Customs.FR.Module");

		#region Find

		public static ActionMethodProviderID FindByGuid(ZGuid providerID)
		{
			return LookupHolder.Instance[providerID];
		}

		[ImmutableObject(true)]
		class LookupHolder
		{
			static readonly Lazy<LookupHolder> instance = new Lazy<LookupHolder>(() => new LookupHolder());
			public static LookupHolder Instance
			{
				get { return instance.Value; }
			}

			LookupHolder()
			{
				lookup = new Dictionary<ZGuid, ActionMethodProviderID>();

				foreach (FieldInfo info in typeof(ActionMethodProviderIDs).GetFields(BindingFlags.Public | BindingFlags.Static))
				{
					ActionMethodProviderID id = info.GetValue(null) as ActionMethodProviderID;

					if (id != null)
					{
						lookup[id.Guid] = id;
					}
				}
			}

			public ActionMethodProviderID this[ZGuid providerID]
			{
				get
				{
					ActionMethodProviderID result;
					lookup.TryGetValue(providerID, out result);
					return result;
				}
			}

			[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
			readonly Dictionary<ZGuid, ActionMethodProviderID> lookup;
		}

		#endregion
	}
}
