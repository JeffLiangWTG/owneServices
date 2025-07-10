using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(eAdaptorNextOutboundConfigRegistryItem))]
	sealed class eAdaptorNextOutboundConfigRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<eAdaptorNextOutboundConfig>
	{
		protected override StronglyTypedRegistryItem<eAdaptorNextOutboundConfig, eAdaptorNextOutboundConfig> GetNewRegistryItem()
		{
			return new eAdaptorNextOutboundConfigRegistryItem(
						"eAdaptorNextOutboundConfig",
						null,
						null,
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						eAdaptorNextOutboundConfig.DefaultValue);
		}

		protected override eAdaptorNextOutboundConfig ValidValue
		{
			get
			{
				return new eAdaptorNextOutboundConfig
				{
					IsOAuth2Enabled = ZBool.False,
					AuthorizationGrantTypeCode = eAdaptorNextOutboundGrantTypesList.Codes.ClientCredentials,
					AuthorizationURL = ZString.Empty,
					ClientID = ZString.Empty,
					ClientSecret = ZString.Empty,
					Username = ZString.Empty,
					Password = ZString.Empty
				};
			}
		}
	}
}
