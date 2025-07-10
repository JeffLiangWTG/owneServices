using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.CO.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.CO.Manifest.Business
{
	internal class BillItemWrapper : IItem
	{
		internal BillItemWrapper(AsycudaBill bill)
		{
			this.bill = Argument.NotNull(bill, "AsycudaBill cannot be null");
		}
		readonly AsycudaBill bill;

		string IItem.BulkType => bill.ABL_ContainerMode;

		string IItem.ContainerIDNumber => ZString.Empty;

		string IItem.Size => ZString.Empty;

		string IItem.EquipmentType => ZString.Empty;

		string IItem.SealNumber => ZString.Empty;

		decimal IItem.GrossWeight => Core.Constants.Weight.ConvertSafe(bill.ABL_GrossWeight, bill.ABL_GrossWeightUQ, Core.Constants.Weight.Kilograms);

		int IItem.BulkQty => bill.ABL_ManifestQty;

		decimal IItem.Volume => ((ZDecimal)Core.Constants.Volume.ConvertSafe(bill.ABL_Volume, bill.ABL_VolumeUQ, Core.Constants.Volume.CubicMetres)).Truncate(2);

		IReadOnlyCollection<IPack> IItem.Packs
		{
			get
			{
				var result = new List<IPack>();
				foreach (AsycudaPack pack in bill.Packs)
				{
					result.Add(new PackWrapper(pack));
				}
				return result.ToArray();
			}
		}
	}
}
