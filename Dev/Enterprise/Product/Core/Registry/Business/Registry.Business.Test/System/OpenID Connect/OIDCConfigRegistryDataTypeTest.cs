using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OIDCConfigRegistryDataType))]
	sealed class OIDCConfigRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<OIDCConfigRegistryDataType>
	{
		protected override OIDCConfigRegistryDataType GetNewDataType()
		{
			return new OIDCConfigRegistryDataType(OIDCConfig.DefaultValue);
		}

		protected override string ExpectedEditorName
		{
			get { return "OIDCRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var oidcConfig1 = new OIDCConfig()
			{
				IsOIDCEnabled = true,
				OIDCServerType = OIDCServerTypes.Generic,
				AuthorityURL = "https://identityprovider.com",
				ClientIdentifier = "SomeId",
			};

			oidcConfig1.ClaimsMappings.Add(new OIDCClaimsMapping()
			{
				ClaimName = "SomeClaim",
				Identifier = "GlbStaff.GS_LoginName"
			});
			oidcConfig1.IsVerified = true;
			var oidcConfig2 = OIDCConfig.DefaultValue;
			var serializer = new OIDCConfigRegistryDataType(OIDCConfig.DefaultValue);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(oidcConfig1, serializer.Serialise(oidcConfig1)),
				new ValidSampleAndBinaryValueInDB(oidcConfig2, serializer.Serialise(oidcConfig2)),
			};
		}
	}
}
