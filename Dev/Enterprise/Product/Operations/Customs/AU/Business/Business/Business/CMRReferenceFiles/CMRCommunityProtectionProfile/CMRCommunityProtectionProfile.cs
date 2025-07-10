
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCommunityProtectionProfile : AutoCMRCommunityProtectionProfile
	{
		public const string NotConstants = "NOT=";

		public CMRCommunityProtectionProfile(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new CMRCommunityProtectionProfileFetchStrategy(this);
		}

		public static CMRCommunityProtectionProfile New(BusinessObjectFactory factory)
		{
			return factory.New<CMRCommunityProtectionProfile>();
		}

		public bool IsMatchingOtherThanTariff(CPQuestionKeys cPQuestionKey)
		{
			return IsMatchingOriginOrNatureOrModeOfTransport(cPQuestionKey)
				&& IsMatchingOtherThanTariff(cPQuestionKey.StatCode, CP_StatisticalClassificationCodefield);
		}

		bool IsMatchingOriginOrNatureOrModeOfTransport(CPQuestionKeys cPQuestionKey)
		{
			return !cPQuestionKey.HasValidOriginOrNatureOrModeOfTransport
				|| (IsMatchingOtherThanTariff(cPQuestionKey.ModeOfTransport, CP_ModeofTransportfield)
					&& IsMatchingOtherThanTariff(cPQuestionKey.Nature, CP_LineNatureTypefield)
					&& IsMatchingOtherThanTariff(cPQuestionKey.OriginCode, CP_OriginCountryCodefield)
				);
		}

		bool IsMatchingOtherThanTariff(ZString valueFromCPQuestionKey, ZString valueFromThisObject)
		{
			return valueFromThisObject.IsEmpty
				|| valueFromThisObject == valueFromCPQuestionKey
				|| (valueFromThisObject.ToUpper().StartsWith(NotConstants) && !valueFromThisObject.Contains(valueFromCPQuestionKey));
		}
	}
}
