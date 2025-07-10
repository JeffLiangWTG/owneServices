using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class WarehouseEntryDataEventContextCreator : Customs.DataTransfer.Universal.WarehouseEntryDataEventContextCreator
	{
		public WarehouseEntryDataEventContextCreator(CusEntryHeader entry) : base(entry)
		{
		}

		protected override void AddCusEntryHeaderContextValuesCore(List<KeyValuePair<TypeWithDescription, IZType>> contextValues)
		{
			base.AddCusEntryHeaderContextValuesCore(contextValues);

			contextValues.AddOrUpdateIfNotEmpty(UniversalEvent.ContextTypes.ClearanceReferenceNumber, entry.EntryNumber);
			contextValues.AddOrUpdateIfNotEmpty(UniversalEvent.ContextTypes.OuterPackQty, entry.PackagesCount);
			contextValues.AddOrUpdateIfNotEmpty(UniversalEvent.ContextTypes.InnerPackQty, entry.EntryInstruction?.CEI_TotalInnerPackages ?? ZInt.Zero);
		}
	}
}
