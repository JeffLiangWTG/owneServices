using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions.Outbound;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Business
{
	sealed class MoveInDestinationProvider : IMoveInDestination
	{
		public MoveInDestinationProvider(MoveInDestination moveInDestination)
		{
			this.moveInDestination = Argument.NotNull(moveInDestination, nameof(moveInDestination));
		}

		readonly MoveInDestination moveInDestination;

		public string MoveInDestination => moveInDestination.CSI_Code;

		public DateTime? MoveInDate
		{
			get
			{
				var dateTime = moveInDestination.CSI_DateOfIssue;
				return dateTime.IsValid ? dateTime.ToDateTime() : null;
			}
		}

		public string VanningLocationCode => moveInDestination.CSI_ReferenceNumber;

		public decimal? MoveInQuantity => moveInDestination.CSI_Quantity.Truncate();

		public decimal? MoveInWeight => moveInDestination.CSI_Quantity2.Truncate(3);

		public decimal? MoveInVolumn => moveInDestination.CSI_Quantity3.Truncate(3);

		public string MarksAndNumbers => moveInDestination.CSI_Description;

		public IEnumerable<IInventory> Inventories => moveInDestination.InventoryNumbers.Select(TryGetInventoryProvider);

		InventoryProvider TryGetInventoryProvider(CusSupportingInfo info) => info != null && info.IsInDatabase ? new InventoryProvider(info) : null;
	}
}
