using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers
{
	public class WhsPickableDocketHelper
	{
		public WhsPickableDocketHelper(WhsPickableDocket pickableDocket)
		{
			PickableDocket = pickableDocket;
		}

		readonly WhsPickableDocket PickableDocket;

		#region GetCartageDropModeFromWhsOrder

		public ZString GetCartageDropModeFromWhsOrder()
		{
			ZString result = "";

			if (!PickableDocket.WD_DropMode.IsEmpty)
			{
				var dropMode = PickableDocket.WD_DropMode;
				result = dropMode + " - " + PickableDocket.Lookups.DropModes.GetDescriptionFromCode(dropMode);
			}
			else if (PickableDocket.ConsigneeDocAddress != null)
			{
				var address = PickableDocket.ConsigneeDocAddress.Address;
				if (address != null)
				{
					result = (PickableDocket.Containers.Count == 0)
						? address.OA_LCLEquipmentNeeded + " - " + address.Lookups.CartageEquipmentNeededLCL.GetDescriptionFromCode(address.OA_LCLEquipmentNeeded)
						: address.OA_FCLEquipmentNeeded + " - " + address.Lookups.CartageEquipmentNeededFCL.GetDescriptionFromCode(address.OA_FCLEquipmentNeeded);

					if (result == " - ")
					{
						result = "";
					}
				}
			}

			return result;
		}

		#endregion
	}
}
