using CargoWise.Types;

namespace Enterprise.Customs.IL.Business
{
	public interface IDeclarationConfigurationProvider
	{
		ZBool UseUniversalFeeCalculation { get; }
	}

	class DefaultDeclarationConfigurationProvider : IDeclarationConfigurationProvider
	{
		public ZBool UseUniversalFeeCalculation => true;
	}

	public static class DeclarationConfigurationProvider
	{
		public static IDeclarationConfigurationProvider Provider { get; set; } = new DefaultDeclarationConfigurationProvider();

		public static ZBool UseUniversalFeeCalculation => Provider.UseUniversalFeeCalculation;
	}
}
