namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class AU
		{
			public interface IAUCustomsRegistry
			{
				IRegistryItem EnableValidationTool { get; }
				IRegistryItem PrintEntryWhenPrintingInvoice { get; }
				IRegistryItem UseCustomsReferenceData { get; }
				IRegistryItem EnableCWRefForAHECC { get; }
			}
		}
	}
}
