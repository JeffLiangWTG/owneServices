using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("Name")]
	public class WarehouseBOWrapper : GenericWrapper
	{
		public WarehouseBOWrapper(ZString useageBasedTitle, WarehouseBOWrapper wrapperBeingCloned)
			: base(wrapperBeingCloned.WrappedBO, wrapperBeingCloned.Factory)
		{
			this.useageBasedTitle = useageBasedTitle;
			WarehouseBO = wrapperBeingCloned.WarehouseBO;
		}

		public WarehouseBOWrapper(ZString useageBasedTitle, WhsWarehouse whsWarehouse, BusinessObjectFactory factory)
			: base(whsWarehouse, factory)
		{
			this.useageBasedTitle = useageBasedTitle;
			WarehouseBO = whsWarehouse;
		}

		public ZString Code
		{
			get { return WarehouseBO != null ? WarehouseBO.WW_WarehouseCode : ZString.Empty; }
		}

		public MultilingualString Name
		{
			get { return WarehouseBO != null ? WarehouseBO.WW_WarehouseNameMultilingual : (NoResString)ZString.Empty; }
		}

		public MultilingualString NameAndAddress
		{
			get
			{
				MultilingualString result = (NoResString)ZString.Empty;
				if (WarehouseBO != null)
				{
					result = MultilingualString.Join(System.Environment.NewLine, WarehouseBO.WW_WarehouseNameMultilingual, (NoResString)Address.ToString());
				}
				return result;
			}
		}

		#region Address

		public AddressWrapper Address
		{
			get
			{
				if (WarehouseBO != null && address == null)
				{
					address = new AddressWrapper(WarehouseBO.WarehouseAddress, ContactType.Warehouse, Factory);
				}

				return address;
			}
		}

		AddressWrapper address;

		#endregion

		#region PhoneAndFax

		public ZString PhoneAndFax
		{
			get
			{
				var result = ZString.Empty;
				var warehouseAddress = WarehouseBO?.WarehouseAddress;
				if (warehouseAddress != null)
				{
					if (!warehouseAddress.OA_Phone_Formatted.IsEmpty)
					{
						if (!warehouseAddress.OA_Fax_Formatted.IsEmpty)
						{
							result = Res.GetString("a0bd2a84-e111-4b87-b719-20618ccf03b9", "Tel: {0}   Fax: {1}", warehouseAddress.OA_Phone_Formatted, warehouseAddress.OA_Fax_Formatted);
						}
						else
						{
							result = Res.GetString("935ab635-a1db-4058-8c2b-7718758834ee", "Tel: {0}", warehouseAddress.OA_Phone_Formatted);
						}
					}
					else
					{
						if (!warehouseAddress.OA_Fax.IsEmpty)
						{
							result = Res.GetString("34ca2468-1042-47cc-bbb7-55d5561f61c3", "Fax: {0}", warehouseAddress.OA_Fax_Formatted);
						}
					}
				}

				return result;
			}
		}

		#endregion

		public ZString TypeDescription
		{
			get { return useageBasedTitle; }
		}

		#region AutoPrintFields

		public ZBool AutoPrintPickingSlip
		{
			get { return WarehouseBO != null ? WarehouseBO.WW_AutoPrintPickingSlip : ZBool.False; }
		}

		public ZBool AutoPrintOrderSummary
		{
			get { return WarehouseBO != null ? WarehouseBO.WW_AutoPrintOrderSummaryOnPick : ZBool.False; }
		}

		public ZBool AutoPrintNonPickedItems
		{
			get { return WarehouseBO != null ? WarehouseBO.WW_AutoPrintPickingNonPickedItems : ZBool.False; }
		}

		public ZBool AutoPrintShortfallItems
		{
			get { return WarehouseBO != null ? WarehouseBO.WW_AutoPrintPickingShortfallItems : ZBool.False; }
		}

		public ZBool AutoPrintOrderCopyForMOP
		{
			get { return WarehouseBO != null ? WarehouseBO.WW_AutoPrintOrderCopyForMOPOnPick : ZBool.False; }
		}

		#endregion

		readonly WhsWarehouse WarehouseBO;
		readonly ZString useageBasedTitle;
	}
}
