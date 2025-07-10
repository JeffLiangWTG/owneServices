using static CargoWise.EventReference.Constants;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave
{
	static class JobQueueReferenceParameters
	{
		public const string TriggerAction = "TRA";
		public const string TriggerCompanyPK = EventReferenceParameters.Codes.Company;
		public const string ParentPK = "PPK";
		public const string ParentType = "PTP";
	}
}
