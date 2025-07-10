using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public sealed class AutomaticSignatureExternalPasswordLookups : MasterFiles.Business.GlbExternalPasswordLookups
{
	public AutomaticSignatureExternalPasswordLookups(AutomaticSignatureExternalPassword parent) : base(parent)
	{
	}

	public CodeDescriptionPairList DelegateList => Factory.GetCachedValue("IT.AutomaticSignatureExternalPasswordLookups.DelegateList", () => GetDelegateList());

	#region Implementation

	CodeDescriptionPairList GetDelegateList()
	{
		var now = ZDateTime.Now;
		var result = new CodeDescriptionPairList();

		var sysConfigLoader = new RefSysConfig.Loader(Factory);

		AddSysConfigValue(UniversalReferenceConstants.RefSysConfigTypes.ItalyAutomaticRemoteDigitalSignatureDelegate01);
		AddSysConfigValue(UniversalReferenceConstants.RefSysConfigTypes.ItalyAutomaticRemoteDigitalSignatureDelegate02);

		return result;

		void AddSysConfigValue(string sysConfigCode)
		{
			var sysConfigValue = sysConfigLoader.GetStringValue(sysConfigCode, now);
			if (!sysConfigValue.IsEmpty)
			{
				result.AddPair(sysConfigCode, sysConfigValue);
			}
		}
	}

	#endregion
}
