using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos
{
	public class AdditionalInfoValidation : EU.Business.Declaration.MultiLineAddInfos.AdditionalInfoValidation
	{
		protected new AdditionalInfo Parent => (AdditionalInfo)base.Parent;
		public AdditionalInfoValidation(AdditionalInfo parent) : base(parent)
		{
		}

		protected static Dictionary<ZString, IEnumerable<ZString>> ExclusionRiskAIStatementRules => new ()
		{
			{ GBCommonConstants.AdditonalInfoCodes.NIAID, RiskingAIStatementCodeList.Except(new ZString[] { GBCommonConstants.AdditonalInfoCodes.NIPRO, GBCommonConstants.AdditonalInfoCodes.NIREM, GBCommonConstants.AdditonalInfoCodes.NIAID }) },
			{ GBCommonConstants.AdditonalInfoCodes.NIQUO, RiskingAIStatementCodeList.Except(new ZString[] { GBCommonConstants.AdditonalInfoCodes.NIHIS, GBCommonConstants.AdditonalInfoCodes.NIQUO }) },
			{ GBCommonConstants.AdditonalInfoCodes.NIHIS, System.Array.Empty<ZString>() },
			{ GBCommonConstants.AdditonalInfoCodes.NIREM, new ZString[] { GBCommonConstants.AdditonalInfoCodes.NIQUO } },
			{ GBCommonConstants.AdditonalInfoCodes.NIPRO, new ZString[] { GBCommonConstants.AdditonalInfoCodes.NIQUO } },
			{ GBCommonConstants.AdditonalInfoCodes.NIOVR, new ZString[] { GBCommonConstants.AdditonalInfoCodes.NIQUO } },
			{ GBCommonConstants.AdditonalInfoCodes.NIIMP, new ZString[] { GBCommonConstants.AdditonalInfoCodes.NIDOM } },
			{ GBCommonConstants.AdditonalInfoCodes.NIDOM, new ZString[] { GBCommonConstants.AdditonalInfoCodes.NIIMP } }
		};

		protected static readonly ImmutableDictionary<ZString, MultilingualString> ExclusionRiskAIStatementErrors = new Dictionary<ZString, MultilingualString>()
		{
			{ GBCommonConstants.AdditonalInfoCodes.NIAID , NIAIDExclusionMessage },
			{ GBCommonConstants.AdditonalInfoCodes.NIQUO , NIQUOExclusionMessage },
			{ GBCommonConstants.AdditonalInfoCodes.NIHIS , NIHISExclusionMessage },
			{ GBCommonConstants.AdditonalInfoCodes.NIIMP , RiskingStatementWithoutIMPorDOM_Message },
			{ GBCommonConstants.AdditonalInfoCodes.NIDOM , RiskingStatementWithoutIMPorDOM_Message }
		}.ToImmutableDictionary();

		public static IEnumerable<ZString> RiskingAIStatementCodeList => new ZString[]
		{
			GBCommonConstants.AdditonalInfoCodes.NIOVR,
			GBCommonConstants.AdditonalInfoCodes.NIAID,
			GBCommonConstants.AdditonalInfoCodes.NIPRO,
			GBCommonConstants.AdditonalInfoCodes.NIREM,
			GBCommonConstants.AdditonalInfoCodes.NIHIS,
			GBCommonConstants.AdditonalInfoCodes.NIQUO
		};

		public static MultilingualString RiskingStatementWithoutIMPorDOM_Message => ResString.GetMultilingualString("601E6C99-D86A-42F0-AA9D-0AA822A83323", "Cannot use any of the risking statements without also one of NIIMP or NIDOM.");
		public static MultilingualString NIAIDExclusionMessage => ResString.GetMultilingualString("79C0E320-6274-44D3-88AD-6B6E12C554C2", "NIAID can be used with NIPRO or NIREM or on its own");
		public static MultilingualString NIQUOExclusionMessage => ResString.GetMultilingualString("2FF394EB-37DA-4449-834B-FADB8DCA0ED0", "NIQUO cannot also have any of NIAID, NIPRO, NIOVR or NIREM");
		public static MultilingualString NIHISExclusionMessage => ResString.GetMultilingualString("11572E7A-C954-40F1-A12F-C9F887BB00E3", "NIHIS may be used on its own or with any other risking statement.");
	}
}
