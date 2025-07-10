namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CA
		{
			public interface ICACustomsDataRegistry
			{
				IRegistryItem AccountSecurityNo { get; }

				IRegistryItem RNSActive { get; }

				IRegistryItem DefaultExciseTaxFromCustomsTariff { get; }
			}
		}
	}
}
