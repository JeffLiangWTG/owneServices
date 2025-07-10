using System.Collections.Immutable;
using CargoWise.Integration;

namespace Enterprise.Customs.CA.Business
{
	partial class ValueForDutyCodes : Integration.Customs.CA.ICAValuationBasisListProvider
	{
		#region ICAValuationBasisListProvider Members

		public ICodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return this;
		}

		#endregion

		public static bool IsRelatedFirms(string code)
		{
			return RelatedFirmsVFDCodes.Contains(code);
		}

		public static bool IsUnrelatedFirms(string code)
		{
			return UnrelatedFirmsVFDCodes.Contains(code);
		}

		static readonly ImmutableArray<string> RelatedFirmsVFDCodes = ImmutableArray.Create(
			Codes.RelatedFirmsPaidPayableWithoutAdjustments,
			Codes.RelatedFirmsPaidPayableWithAdjustments,
			Codes.RelatedFirmsIdenticalGoods,
			Codes.RelatedFirmsSimilarGoods,
			Codes.RelatedFirmsDeductiveValue,
			Codes.RelatedFirmsComputedValue,
			Codes.RelatedFirmsResidualMethodValue
			);

		static readonly ImmutableArray<string> UnrelatedFirmsVFDCodes = ImmutableArray.Create(
			Codes.UnrelatedFirmsPaidPayableWithoutAdjustments,
			Codes.UnrelatedFirmsPaidPayableWithAdjustments,
			Codes.UnrelatedFirmsIdenticalGoods,
			Codes.UnrelatedFirmsSimilarGoods,
			Codes.UnrelatedFirmsDeductiveValue,
			Codes.UnrelatedFirmsComputedValue,
			Codes.UnrelatedFirmsResidualMethodValue
			);
	}
}
