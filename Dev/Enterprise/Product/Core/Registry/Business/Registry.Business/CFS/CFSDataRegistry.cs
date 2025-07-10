using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class CFSDataRegistry : RegistryItemSet
	{
		CFSDataRegistry()
		{
		}

		public static CFSDataRegistry Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new CFSDataRegistry();
				}
				return fInstance;
			}
		}

		[ThreadStatic]
		static CFSDataRegistry fInstance;

		public override bool IsForProductivityWise => false;

		#region Gatepass Copies to Print

		public IntRegistryItem GatepassCopiesToPrint
		{
			get
			{
				return GetItem<IntRegistryItem>("GatepassCopiesToPrint", delegate
				{
					return new IntRegistryItem(
						"GatepassCopiesToPrint",
						RawDataRegistry.Categories.CFS,
						ResString.GetMultilingualString("dce5338e-2444-4131-aaea-cf1588593af5", "Gatepass Copies to Print"),
						ResString.GetMultilingualString("554871fa-9a96-458d-bb18-df928cb436bc", "The number of copies of the Gate Pass document that will be printed when the Gate Pass is saved and printed."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						1);
				});
			}
		}

		public BooleanRegistryItem IgnoreCSAWithNoIMP
		{
			get
			{
				return GetItem<BooleanRegistryItem>("IgnoreCSAWithNoIMP", delegate
				{
					return new BooleanRegistryItem(
						"IgnoreCSAWithNoIMP",
						RawDataRegistry.Categories.CFS,
						ResString.GetMultilingualString("203e88b8-52ac-450a-8a35-91a240fba142", "Deliverance Conversion"),
						ResString.GetMultilingualString("3cad4d94-1399-4540-b3c1-51d3823a95aa", "Deliverance Depot Conversion.  Cargo Status Advice ignored if no impending arrival exists."),
						RegistryStorageFlags.Company,
						false);
				});
			}
		}

		#endregion

		public BooleanRegistryItem RequireTransportDetails
		{
			get
			{
				return GetItem("RequireTransportDetails", delegate
				{
					return new BooleanRegistryItem(
						"RequireTransportDetails",
						RawDataRegistry.Categories.CFS,
						ResString.GetMultilingualString("15e191b0-fc86-454f-bd29-34bbb8b640bd", "Require Transport Details"),
						null,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		public BooleanRegistryItem AutopackContainers
		{
			get
			{
				return GetItem("AutopackContainers", delegate
				{
					return new BooleanRegistryItem(
						"AutopackContainers",
						RawDataRegistry.Categories.CFS,
						ResString.GetMultilingualString("bda5e9bb-e40f-4823-904a-7fe31b37e77d", "Auto-pack Containers"),
						null,
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						false);
				});
			}
		}

		public GuidRegistryItem DepotCashSalesOrg
		{
			get
			{
				return GetItem("DepotCashSales", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"DepotCashSales",
						RawDataRegistry.Categories.CFS,
						ResString.GetMultilingualString("ae0324a7-5ce8-44cb-994e-5ccb895a57d7", "Depot Cash Sales Organization"),
						ResString.GetMultilingualString("b1391c09-6ef3-43ea-9faf-661a859847ab", "For temporary and non A/R registered clients"),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						Guid.Empty);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.Debtor);
					return result;
				});
			}
		}

		#region Use Client Free Days

		public BooleanRegistryItem CFSAirFreightUseClientFreeDays
		{
			get
			{
				return GetItem<BooleanRegistryItem>("CFSAirFreightUseClientFreeDays", delegate
				{
					return new BooleanRegistryItem("CFSAirFreightUseClientFreeDays",
						RawDataRegistry.Categories.CFS_AirFreight_General,
						ResString.GetMultilingualString("598ad03f-861d-4793-aab8-00dbefe8e73d", "Use Client Free Days "),
						ResString.GetMultilingualString("79062ded-8eea-4743-a183-01dd0988eaff", "The default number of free days for storage for LCL cargo will use the Consignee Organization's set number of days from the Consignee > Air Freight Storage Free Days Setting."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		public BooleanRegistryItem CFSSeaFreightUseClientFreeDays
		{
			get
			{
				return GetItem<BooleanRegistryItem>("CFSSeaFreightUseClientFreeDays", delegate
				{
					return new BooleanRegistryItem("CFSSeaFreightUseClientFreeDays",
						RawDataRegistry.Categories.CFS_SeaFreight_General,
						ResString.GetMultilingualString("598ad03f-861d-4793-aab8-00dbefe8e73d", "Use Client Free Days "),
						ResString.GetMultilingualString("5495a61f-93a5-4b15-b360-b3667d76f727", "The default number of free days for storage for LCL cargo will use the Consignee Organization's set number of days from the Consignee > Sea Freight Storage Free Days Setting."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						true);
				});
			}
		}

		#endregion
	}
}
