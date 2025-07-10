using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;

namespace Enterprise.DocumentWrappers
{
	class CFRSpecialPermitNumberComponent : IUNDGSummaryWriterComponent
	{
		bool IUNDGSummaryWriterComponent.IsDefault => true;

		ZString IUNDGSummaryWriterComponent.Write(UNDGSubstanceWrapper wrapper)
		{
			var specialPermitNumber = wrapper.DGData?.DI_SpecialPermitNumber ?? ZString.Empty;
			if (specialPermitNumber.IsEmpty)
			{
				return ZString.Empty;
			}

			return string.Concat(SpecialPermitPrefixNotation, " ", specialPermitNumber);
		}

		const string SpecialPermitPrefixNotation = "DOT-SP";
	}
}
