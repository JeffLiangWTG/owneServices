namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class TR
		{
			public interface ITRCustomsDataRegistry
			{
				IRegistryItem ExposeETradeModule { get; }
				IRegistryItem ExposeSimplifiedProcedureTransitSystemModule { get; }
				IRegistryItem EnableTRNCTS { get; }
				IRegistryItem SendTRNCTSMessageWithoutSign { get; }
				IRegistryItem SendTRDeclarationWithComma { get; }
			}
		}
	}
}
