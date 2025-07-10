namespace Enterprise.Customs.JP.AFR.Business
{
	public enum ActionCode
	{
		Registering = 0,
		AmendingAdd = 1,
		AmendingUpdate = 2,
		AmendingDelete = 3,

		RegisterCompletionByRegistration = 4,
		RegisterCompletionByAmendment = 5,

		CorrectVesselInformationByRegistration = 6,
		CorrectVesselInformationByAmendment = 7,
		CorrectMasterInformation = 8,
		NewBill = 9,

		RegisterDepartureTime = 10,
		ReRegisterMasterAfterATD = 11,
		ReRegisterMasterBeforeATD = 12,
		ChangeDepartureTimeAfterATD = 13,
	}

	public static class ActionCodeTool
	{
		public static bool IsForceSendingInGrid(ActionCode actionCode)
		{
			return actionCode == ActionCode.CorrectVesselInformationByRegistration ||
				actionCode == ActionCode.CorrectVesselInformationByAmendment ||
				actionCode == ActionCode.CorrectMasterInformation ||
				actionCode == ActionCode.ReRegisterMasterAfterATD ||
				actionCode == ActionCode.ReRegisterMasterBeforeATD;
		}

		public static bool IsLimitingActionInGrid(ActionCode actionCode)
		{
			return actionCode == ActionCode.CorrectVesselInformationByRegistration ||
				actionCode == ActionCode.CorrectVesselInformationByAmendment ||
				actionCode == ActionCode.Registering ||
				actionCode == ActionCode.RegisterCompletionByRegistration ||
				actionCode == ActionCode.RegisterCompletionByAmendment ||
				actionCode == ActionCode.RegisterDepartureTime;
		}
	}
}
