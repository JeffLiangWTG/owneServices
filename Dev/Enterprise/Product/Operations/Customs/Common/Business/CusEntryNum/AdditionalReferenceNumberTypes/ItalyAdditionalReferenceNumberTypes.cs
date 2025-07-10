using System.Diagnostics.CodeAnalysis;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common
{
	public partial class ItalyAdditionalReferenceNumberTypes : CodeDescriptionPairList
	{
		[SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public static class Codes
		{
			public const string CIG = "CIG";
			public const string CUP = "CUP";
		}

		[SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public static class Descriptions
		{
			public static MultilingualString CIG { get { return ResString.GetMultilingualString("ItalyAdditionalReferenceNumberTypes|CIG", "CODICE IDENTIFICATIVO DELLA GARA"); } }
			public static MultilingualString CUP { get { return ResString.GetMultilingualString("ItalyAdditionalReferenceNumberTypes|CUP", "CODICE UNITARIO PROGETTO"); } }
		}

		public ItalyAdditionalReferenceNumberTypes()
		{
			Add(new CustomsNumberTypeCodeDescription(Codes.CIG, Descriptions.CIG, false));
			Add(new CustomsNumberTypeCodeDescription(Codes.CUP, Descriptions.CUP, false));
		}
	}
}
