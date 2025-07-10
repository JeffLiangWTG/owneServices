namespace Enterprise.Customs.EU.ExitControl.Business;

public interface ICusExitReportValidationDecider
{
}

public interface ICusExitReportUcc6ValidationDecider : ICusExitReportValidationDecider
{
	bool ValidateCER_CXC_ConsignmentMrnIsAlreadyBeingUsed { get; }

	bool ValidateCER_LocationLookups { get; }
}
