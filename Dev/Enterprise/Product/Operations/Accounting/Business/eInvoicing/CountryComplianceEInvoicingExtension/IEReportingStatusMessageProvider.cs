using CargoWise.Types;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public interface IEReportingStatusMessageProvider
	{
		string GetStatusMessage(ZString complianceSubType, ZString approvalStatus, ZString pivotStatus, ZString pivotActionType, string currentMessage);
	}
}
