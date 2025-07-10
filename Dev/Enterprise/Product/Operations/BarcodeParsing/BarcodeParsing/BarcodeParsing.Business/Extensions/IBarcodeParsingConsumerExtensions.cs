using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.BarcodeParsing.Business
{
	public static class IBarcodeParsingConsumerExtensions
	{
		public static bool IsBuyerRequiredForRelatedEntity(this IBarcodeParsingConsumer consumer)
		{
			return consumer != null && consumer.RelatedEntityRequirements.HasFlag(RelatedEntityRequirements.MustHaveBuyer);
		}

		// tested in BarcodeRuleSetTest & BarcodeParsingDiagnosticTest
		internal static string GetBuyerCaption(this IBarcodeParsingConsumer consumer, string fallbackCaption)
		{
			Argument.NotNull(consumer, nameof(consumer));
			return string.IsNullOrEmpty(consumer.BuyerCaption) ? fallbackCaption : consumer.BuyerCaption;
		}

		internal static string GetSupplierCaption(this IBarcodeParsingConsumer consumer, string fallbackCaption)
		{
			Argument.NotNull(consumer, nameof(consumer));
			return string.IsNullOrEmpty(consumer.SupplierCaption) ? fallbackCaption : consumer.SupplierCaption;
		}

		internal static string GetRelatedEntityCaption(this IBarcodeParsingConsumer consumer, string fallbackCaption)
		{
			Argument.NotNull(consumer, nameof(consumer));
			return string.IsNullOrEmpty(consumer.RelatedEntityCaption) ? fallbackCaption : consumer.RelatedEntityCaption;
		}

		internal static bool IsRelatedEntityEditable(this IBarcodeParsingConsumer consumer, ZGuid buyerPK)
		{
			Argument.NotNull(consumer, nameof(consumer));
			return consumer.IsRelatedEntityAvailable && (!consumer.IsBuyerRequiredForRelatedEntity() || buyerPK.IsValid);
		}
	}
}
