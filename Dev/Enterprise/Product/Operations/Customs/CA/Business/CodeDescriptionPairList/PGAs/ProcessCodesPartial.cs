using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	partial class ProcessCodes
	{
		public static CodeDescriptionPairList GetProcessCodesFor(string govAgencyCode, BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("ProcessCodesFor" + govAgencyCode, () =>
			{
				var result = new CodeDescriptionPairList();
				if (govAgencyCode == PGACodes.Codes.ECCC)
				{
					result.AddPair(Codes.XE01, Descriptions.XE01);
					result.AddPair(Codes.XE02, Descriptions.XE02);
					result.AddPair(Codes.XE03, Descriptions.XE03);
					result.AddPair(Codes.XE04, Descriptions.XE04);
				}
				return result;
			});
		}
	}
}
