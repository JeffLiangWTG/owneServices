using System;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Management;

namespace Enterprise.Customs.CA.Business;

public class PreMessagingActionResult
{
	public bool IsBondedWarehouse { get; set; }
	public ZString ErrorMessageForCheckFieldsForBondedWarehous { get; set; }
	public Func<PublishToUniversalResult> PreMessagingAction { get; set; }
	public Action RestoreToPreMessagingState { get; set; }
}
