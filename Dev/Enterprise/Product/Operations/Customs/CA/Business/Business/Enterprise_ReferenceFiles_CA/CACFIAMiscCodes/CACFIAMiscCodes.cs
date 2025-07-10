using CargoWise.Types;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CACFIAMiscCodes : ZZRefCusCodeListWrapper
	{
		public CACFIAMiscCodes(ZZRefCusCodeListCombined cusCodeList)
			: base(cusCodeList)
		{
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("52DBE1D4-B28D-484e-91AF-839CCBC43CC2", "Miscellaneous Code: '{0}'", Code); }
		}
	}
}
