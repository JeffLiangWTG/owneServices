namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusExitConsignmentConfiguration
{
	public CusExitConsignmentItemConfiguration CusExitConsignmentItemConfiguration => cusExitConsignmentItemConfiguration ??= GetNewCusExitConsignmentItemConfigurationConfiguration();
	CusExitConsignmentItemConfiguration cusExitConsignmentItemConfiguration;

	protected virtual CusExitConsignmentItemConfiguration GetNewCusExitConsignmentItemConfigurationConfiguration() => new();
}
