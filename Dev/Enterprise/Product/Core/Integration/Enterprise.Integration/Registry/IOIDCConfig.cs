using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public enum OIDCServerTypes
	{
		Generic,
		Okta,
		Azure,
		OneLogin,
		WiseTechIdP
	}

	public interface IOIDCConfig
	{
		ZString AuthorityURL { get; }
		IEnumerable<IOIDCClaimsMapping> ClaimsMappings { get; }
		ZString ClientIdentifier { get; }
		ZBool IsOIDCEnabled { get; }
		OIDCServerTypes OIDCServerType { get; }
		ZString OIDCServerTypeCode { get; }
		IEnumerable<ZString> OIDCServerTypesList { get; }
		IEnumerable<IOIDCScope> Scopes { get; }
	}

	public interface IOIDCScope
	{
		ZString ScopeName { get; }
	}

	public interface IOIDCClaimsMapping
	{
		ZString ClaimName { get; }
		ZString Identifier { get; }
		IEnumerable<ZString> OIDCClaimMappingIdentifiers { get; }
	}
}
