using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Universal;
using UniversalRefSysConfigTypeCodes = Enterprise.Customs.CA.Business.UniversalReferenceConstants.RefSysConfigType.Codes;

namespace Enterprise.Customs.CA.Business
{
	public class AIRSWebpageConfiguration : NonPersistentBusinessObject
	{
		public AIRSWebpageConfiguration(BusinessObjectFactory factory) : base(factory)
		{
			isFrenchLanguageIndicator = CACustomsDataRegistry.Instance.FrenchLanguageIndicator.Value;
			effectiveDate = ZDateTime.Today;
		}
		readonly ZBool isFrenchLanguageIndicator;
		readonly ZDateTime effectiveDate;

		public ZString Url => GetRefSysConfigValue(isFrenchLanguageIndicator ? UniversalRefSysConfigTypeCodes.CAAIRSUriFrench : UniversalRefSysConfigTypeCodes.CAAIRSUriEnglish);
		public ZString EndUseTextID => GetRefSysConfigValue(UniversalRefSysConfigTypeCodes.CAAIRSEndUseTextID);
		public ZString ExtensionTextID => GetRefSysConfigValue(UniversalRefSysConfigTypeCodes.CAAIRSExtensionTextID);
		public ZString MiscTextID => GetRefSysConfigValue(UniversalRefSysConfigTypeCodes.CAAIRSMiscTextID);
		public ZString IIDTableID => GetRefSysConfigValue(UniversalRefSysConfigTypeCodes.CAAIRSIIDTableID);
		public ZString PostDataTemplate => GetRefSysConfigValue(UniversalRefSysConfigTypeCodes.CAAIRSPostDataTemplate);
		public ZString IIDWebPageTitle => GetRefSysConfigValue(isFrenchLanguageIndicator ? UniversalRefSysConfigTypeCodes.CAAIRSIIDWebPageTitleFrench : UniversalRefSysConfigTypeCodes.CAAIRSIIDWebPageTitleEnglish);
		public ZString MaterializedGridTitle => GetRefSysConfigValue(isFrenchLanguageIndicator ? UniversalRefSysConfigTypeCodes.CAAIRSMaterializedGridTitleFrench : UniversalRefSysConfigTypeCodes.CAAIRSMaterializedGridTitleEnglish);
		public ZString DeMaterializedGridTitle => GetRefSysConfigValue(isFrenchLanguageIndicator ? UniversalRefSysConfigTypeCodes.CAAIRSDeMaterializedGridTitleFrench : UniversalRefSysConfigTypeCodes.CAAIRSDeMaterializedGridTitleEnglish);
		public ZString AIRSRegistrationGridTitle => GetRefSysConfigValue(isFrenchLanguageIndicator ? UniversalRefSysConfigTypeCodes.CAAIRSRegistrationGridTitleFrench : UniversalRefSysConfigTypeCodes.CAAIRSRegistrationGridTitleEnglish);
		public ZString CodeText => GetRefSysConfigValue(isFrenchLanguageIndicator ? UniversalRefSysConfigTypeCodes.CAAIRSCodeTextFrench : UniversalRefSysConfigTypeCodes.CAAIRSCodeTextEnglish);
		public ZString RegistrationText => GetRefSysConfigValue(isFrenchLanguageIndicator ? UniversalRefSysConfigTypeCodes.CAAIRSRegistrationTextFrench : UniversalRefSysConfigTypeCodes.CAAIRSRegistrationTextEnglish);
		public ZString OrText => GetRefSysConfigValue(isFrenchLanguageIndicator ? UniversalRefSysConfigTypeCodes.CAAIRSOrTextFrench : UniversalRefSysConfigTypeCodes.CAAIRSOrTextEnglish);
		public ZString LPCOCodeAttributeKey => GetRefSysConfigValue(UniversalRefSysConfigTypeCodes.CAAIRSLPCOCodeAttributeKey);
		public ZString LPCOCodeAttributeCode => GetRefSysConfigValue(UniversalRefSysConfigTypeCodes.CAAIRSLPCOCodeAttributeCode);
		public ZString LPCOCodeAttributeDescription => GetRefSysConfigValue(UniversalRefSysConfigTypeCodes.CAAIRSLPCOCodeAttributeDescription);

		public ZBool IsValid
		{
			get
			{
				return !(Url.IsEmpty || EndUseTextID.IsEmpty || ExtensionTextID.IsEmpty
					|| MiscTextID.IsEmpty || IIDTableID.IsEmpty || PostDataTemplate.IsEmpty || IIDWebPageTitle.IsEmpty
					|| MaterializedGridTitle.IsEmpty || DeMaterializedGridTitle.IsEmpty || CodeText.IsEmpty || RegistrationText.IsEmpty
					|| OrText.IsEmpty || LPCOCodeAttributeKey.IsEmpty || LPCOCodeAttributeCode.IsEmpty || LPCOCodeAttributeDescription.IsEmpty || AIRSRegistrationGridTitle.IsEmpty);
			}
		}

		ZString GetRefSysConfigValue(ZString configCode)
		{
			return Factory.GetCachedValue(configCode + effectiveDate, () =>
			{
				var result = ZString.Empty;

				var config = RefSysConfigLoader.Load(configCode, effectiveDate);
				if (config != null)
				{
					result = config.ZRC_StringValue;
				}
				return result;
			});
		}

		RefSysConfig.Loader RefSysConfigLoader
		{
			get
			{
				return refSysConfigLoader ?? (refSysConfigLoader = new RefSysConfig.Loader(Factory));
			}
		}
		RefSysConfig.Loader refSysConfigLoader;
	}
}
