using System.Linq;

namespace Enterprise.Customs.Common.CA
{
	public partial class TariffTreatmentCodes : Integration.Customs.CA.ICATariffTreatmentCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public static bool IsCountryOfOriginAndExportRequiredForLVS(string treatmentCode)
		{
			return !new[]
					{
						Codes.MostFavouredNation,
						Codes.UnitedStates,
						string.Empty
					}.Contains(treatmentCode);
		}

		public static bool IsNeedCheckForValidCertificateOfOrigin(string treatmentCode)
		{
			return !new[]
					{
						Codes.MostFavouredNation,
						Codes.General,
						string.Empty
					}.Contains(treatmentCode);
		}

		public ZArchitecture.Core.ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return this;
		}
	}
}
