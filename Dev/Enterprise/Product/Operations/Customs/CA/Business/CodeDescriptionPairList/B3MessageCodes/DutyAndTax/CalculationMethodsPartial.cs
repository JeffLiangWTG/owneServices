using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public partial class CalculationMethods : Integration.Customs.CA.ICARemissionTypeCodeDescriptionPairProvider,
		DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			return this;
		}

		public static ZBool IsTemporaryImport(ZString code)
		{
			return code == Codes.RepairsRemission
				|| code == Codes.WarrantyRepairsRemission
				|| code == Codes.OneOneTwentiethRemission
				|| code == Codes.OneSixtiethRemission;
		}
	}
}
