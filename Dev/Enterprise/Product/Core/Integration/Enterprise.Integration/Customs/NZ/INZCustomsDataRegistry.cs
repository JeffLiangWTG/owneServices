namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class NZ
		{
			public interface INZCustomsDataRegistry
			{
				IRegistryItem UpdateAttachedManifestedECIsWhenConsolDetailsChange { get; }
				IRegistryItem ExportEntryFeeChargeCode { get; }
				IRegistryItem EnableInwardCargoReportManifest { get; }
				IRegistryItem MaxNumberOfECIManifestLinesAccepted { get; }
				IRegistryItem MaxNumberOfECIManifestLinesAcceptedCRE { get; }
			}
		}
	}
}
