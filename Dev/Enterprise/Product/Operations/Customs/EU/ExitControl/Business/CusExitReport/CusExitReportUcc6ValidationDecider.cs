namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusExitReportUcc6ValidationDecider : ICusExitReportUcc6ValidationDecider
{
	public bool ValidateCER_LocationLookups => true;

	public bool ValidateCER_CXC_ConsignmentMrnIsAlreadyBeingUsed => true;
}
