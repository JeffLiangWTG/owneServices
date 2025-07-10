namespace Enterprise.Integration.ComplianceWise
{
	public interface IComplianceWiseEnabledDetail
	{
		bool IsCustomsEnabledComplianceWise { get; }

		bool IsComplianceCommodityScreeningEnable { get; }
	}
}
