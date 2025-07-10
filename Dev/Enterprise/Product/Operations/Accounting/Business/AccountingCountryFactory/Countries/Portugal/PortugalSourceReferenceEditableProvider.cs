using static Enterprise.MasterFiles.Business.CountryCompliance.PortugalComplianceInfo;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class PortugalSourceReferenceEditableProvider : ISourceReferenceEditableProvider
	{
		bool ISourceReferenceEditableProvider.CheckReferenceSourceIsEditable(bool isReversed, string complianceSubType) => isReversed && (complianceSubType == ComplianceSubTypeCodes.TCM || complianceSubType == ComplianceSubTypeCodes.LCR);
	}
}
