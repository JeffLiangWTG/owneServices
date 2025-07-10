using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DataTransfer.Integration
{
	public struct OrgMatchingResult
	{
		public OrgMatchingResult(IOrgHeaderForMatching match, bool matchFound, bool tempOrgCouldNotBeCreated, CodeDescriptionPairList additionalInfo)
		{
			Match = match;
			MatchFound = matchFound;
			TempOrgCouldNotBeCreated = tempOrgCouldNotBeCreated;
			AdditionalInfo = additionalInfo;
		}

		public readonly IOrgHeaderForMatching Match;
		public readonly bool MatchFound;
		public readonly bool TempOrgCouldNotBeCreated;
		public readonly CodeDescriptionPairList AdditionalInfo;
	}
}
