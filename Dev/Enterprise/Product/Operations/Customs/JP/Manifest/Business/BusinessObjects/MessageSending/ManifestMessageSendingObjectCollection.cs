using CargoWise.EntityFramework;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Manifest.Business;

public class ManifestMessageSendingObjectCollection : NonPersistentBusinessObjectCollection<ManifestMessageSendingObject>
{
	protected override bool AllowNewCore => false;

	public ManifestMessageSendingObjectCollection(BusinessObjectFactory factory) : base(factory) { }

	protected override BusinessObject CreateNonPersistentBusinessObject()
	{
		throw new System.NotImplementedException();
	}

	public void SortByBillStatus()
	{
		Sort<ManifestMessageSendingObject>(CompareByBillStatus);
	}

	int CompareByBillStatus(ManifestMessageSendingObject sendingObject1, ManifestMessageSendingObject sendingObject2)
	{
		var currentProcedureCode = sendingObject1.Parent.CurrentProcedureCode;

		return GetStatusOrderValue(sendingObject1.Bill.ABL_BillStatus, currentProcedureCode) - GetStatusOrderValue(sendingObject2.Bill.ABL_BillStatus, currentProcedureCode);

		int GetStatusOrderValue(string status, string currentProcedureCode)
		{
			return currentProcedureCode switch
			{
				JPProcedureCodeList.Codes.HDF01 => GetHDF01StatusOrderValue(status),
				JPProcedureCodeList.Codes.NVC01 => GetNVC01StatusOrderValue(status),
				_ => int.MaxValue,
			};

			int GetHDF01StatusOrderValue(string status)
			{
				return status switch
				{
					"" => 1,
					JPCustomsStatusList.Codes.REG => 2,
					JPCustomsStatusList.Codes.CAN => 3,
					JPCustomsStatusList.Codes.AWR => 4,
					JPCustomsStatusList.Codes.AWC => 5,
					JPCustomsStatusList.Codes.AWD => 6,
					JPCustomsStatusList.Codes.Deleted => 7,
					_ => int.MaxValue
				};
			}

			int GetNVC01StatusOrderValue(string status)
			{
				return status switch
				{
					"" => 1,
					JPCustomsStatusList.Codes.REG => 2,
					JPCustomsStatusList.Codes.AMD => 3,
					JPCustomsStatusList.Codes.DEL => 4,
					JPCustomsStatusList.Codes.AWA => 5,
					JPCustomsStatusList.Codes.AWD => 6,
					JPCustomsStatusList.Codes.AWR => 7,
					_ => int.MaxValue
				};
			}
		}
	}
}
