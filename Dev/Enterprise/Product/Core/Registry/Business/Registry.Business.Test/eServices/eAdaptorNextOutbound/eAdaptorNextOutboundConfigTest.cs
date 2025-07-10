using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(eAdaptorNextOutboundConfig))]
	sealed class eAdaptorNextOutboundConfigTest : RegistryBusinessObjectTemplateTestCase<eAdaptorNextOutboundConfig>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override eAdaptorNextOutboundConfig GetBusinessObjectToClone()
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

		protected override eAdaptorNextOutboundConfig GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		public override void TestBizObjectFields()
		{
			// Tested elsewhere
			Assert(true);
		}
	}
}
