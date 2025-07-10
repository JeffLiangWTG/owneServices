namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EUExitControl
		{
			public interface IExitControlCustomsDataRegistry
			{
				bool IsExitControlPluginEnabledForCurrentCompany { get; }
				bool IsExitControlModuleEnabledForCurrentCompany { get; }

				IRegistryItem EnableExitControlPlugin { get; }
				IRegistryItem EnableExitControlModule { get; }
			}
		}
	}
}
