namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EU
		{
			public interface IEUCustomsRegistry
			{
				IRegistryItem AllowExportAndImportOfOldStyleCusAddInfoRowsInUniversalAsWellAsNewStyleTaxOrFee { get; }
				IRegistryItem PNTSEnabled { get; }
				IRegistryItem PNTSEnabledDeveloperOnly { get; }
				IRegistryItem RegisterEnabled { get; }
				IRegistryItem RegisterEnabledDeveloperOnly { get; }
			}
		}
	}
}
