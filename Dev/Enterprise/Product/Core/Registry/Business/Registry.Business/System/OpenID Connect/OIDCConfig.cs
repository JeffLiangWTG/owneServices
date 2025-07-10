using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	public static class OIDCConfigFactory
	{
		public static IOIDCConfig GetOIDCConfig()
		{
			if (Globals.IsWinzor)
			{
				var oidcConfig = SystemDataRegistry.Instance.WinzorOIDCConfig.Value;
				if (oidcConfig.IsOIDCEnabled)
				{
					return oidcConfig;
				}
			}
			return SystemDataRegistry.Instance.OIDCConfig.Value;
		}
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class OIDCConfig : RegistryBusinessObjectTemplate, IOIDCConfig
	{
		[ResourceStringData("OIDCConfig.IsOIDCEnabled", Caption = "Enabled")]
		public ZBool IsOIDCEnabled
		{
			get { return isOIDCEnabled; }
			set
			{
				SetNonPersistentPropertyValue(IsOIDCEnabledInfo, ref isOIDCEnabled, value);
				ClaimsMappings.SetReadOnlyIncludingChildren(!value);
				Scopes.SetReadOnlyIncludingChildren(!value);
				if (!value)
				{
					this.ClearRowNotifications();
				}
			}
		}
		ZBool isOIDCEnabled;

		public ZPropertyInfo IsOIDCEnabledInfo => GetZPropertyInfo(nameof(IsOIDCEnabled));

		public ZBool IsVerified
		{
			get
			{
				return this.Equals(lastVerifiedConfig);
			}
			set
			{
				if (value)
				{
					lastVerifiedConfig = this.Clone();
				}
				RefreshBinding();
			}
		}
		OIDCConfig lastVerifiedConfig;

		public OIDCServerTypes OIDCServerType
		{
			get
			{
				switch (OIDCServerTypeCode)
				{
					case "":
					case Business.OIDCServerTypesList.Codes.Generic:
						return OIDCServerTypes.Generic;
					case Business.OIDCServerTypesList.Codes.Okta:
						return OIDCServerTypes.Okta;
					case Business.OIDCServerTypesList.Codes.Azure:
						return OIDCServerTypes.Azure;
					case Business.OIDCServerTypesList.Codes.OneLogin:
						return OIDCServerTypes.OneLogin;
					case Business.OIDCServerTypesList.Codes.WiseTechIdP:
						return OIDCServerTypes.WiseTechIdP;
					default:
						return OIDCServerTypes.Generic;
				}
			}
			set
			{
				switch (value)
				{
					case OIDCServerTypes.Generic:
						OIDCServerTypeCode = Business.OIDCServerTypesList.Codes.Generic;
						break;

					case OIDCServerTypes.Okta:
						OIDCServerTypeCode = Business.OIDCServerTypesList.Codes.Okta;
						break;

					case OIDCServerTypes.Azure:
						OIDCServerTypeCode = Business.OIDCServerTypesList.Codes.Azure;
						break;

					case OIDCServerTypes.OneLogin:
						OIDCServerTypeCode = Business.OIDCServerTypesList.Codes.OneLogin;
						break;

					case OIDCServerTypes.WiseTechIdP:
						OIDCServerTypeCode = Business.OIDCServerTypesList.Codes.WiseTechIdP;
						break;
				}
			}
		}

		[List("OIDCServerTypesList")]
		[ResourceStringData("OIDCConfig.OIDCServerTypeCode", Caption = "OpenID Connect Server")]
		[MaxLength(3)]
		public ZString OIDCServerTypeCode
		{
			get { return oidcServerTypeCode; }
			set
			{
				SetNonPersistentPropertyValue(OIDCServerTypeCodeInfo, ref oidcServerTypeCode, value);
				if (!IsValidationSuspended)
				{
					ValidateOIDCServerTypeCode();
				}
			}
		}
		ZString oidcServerTypeCode;

		public ZPropertyInfo OIDCServerTypeCodeInfo => GetZPropertyInfo(nameof(OIDCServerTypeCode));

		protected bool OIDCServerTypeCode_ReadOnly => !IsOIDCEnabled;

		public CodeDescriptionPairList OIDCServerTypesList => new OIDCServerTypesList();

		[ChildEditable]
		public OIDCClaimsMappingCollection ClaimsMappings
		{
			get
			{
				if (claimsMappings == null)
				{
					claimsMappings = new OIDCClaimsMappingCollection();
					RegisterEditableChildObject(claimsMappings);
				}

				return claimsMappings;
			}
		}
		OIDCClaimsMappingCollection claimsMappings;

		[ChildEditable]
		public OIDCScopesCollection Scopes
		{
			get
			{
				if (scopes == null)
				{
					scopes = new OIDCScopesCollection();
					RegisterEditableChildObject(scopes);
				}

				return scopes;
			}
		}
		OIDCScopesCollection scopes;

		[ResourceStringData("OIDCConfig.AuthorityURL", Caption = "OpenID Connect Authority URL")]
		public ZString AuthorityURL
		{
			get { return authorityURL; }
			set
			{
				SetNonPersistentPropertyValue(AuthorityURLInfo, ref authorityURL, value);
				if (!IsValidationSuspended)
				{
					ValidateAuthorityURL();
				}
			}
		}
		ZString authorityURL;

		public ZPropertyInfo AuthorityURLInfo => GetZPropertyInfo(nameof(AuthorityURL));

		protected bool AuthorityURL_ReadOnly => !IsOIDCEnabled;

		ZString authorityURLIsMalformed => Res.GetString("649BAC6A-4685-4C5B-8441-EED40CDA8764", "Malformed Authority URL.");
		ZString authorityURLIsNotHttps => Res.GetString("8A3163A7-76DD-4586-8724-E2DB41DE8CB7", "Authority URL must be https.");

		[ResourceStringData("OIDCConfig.ClientIdentifier", Caption = "Client Identifier")]
		public ZString ClientIdentifier
		{
			get { return clientIdentifier; }
			set
			{
				SetNonPersistentPropertyValue(ClientIdentifierInfo, ref clientIdentifier, value);
				if (!IsValidationSuspended)
				{
					ValidateClientIdentifier();
				}
			}
		}
		ZString clientIdentifier;

		public ZPropertyInfo ClientIdentifierInfo => GetZPropertyInfo(nameof(ClientIdentifier));

		protected bool ClientIdentifier_ReadOnly => !IsOIDCEnabled;

		internal void ValidateOIDCServerTypeCode()
		{
			if (new OIDCServerTypesList().ContainsCode(OIDCServerTypeCode))
			{
				OIDCServerTypeCodeInfo.ClearAllNotifications();
			}
			else
			{
				OIDCServerTypeCodeInfo.AddError(serverTypeInvalidErrorMessage);
			}
		}
		ZString serverTypeInvalidErrorMessage => Res.GetString("B8A24483-5A3E-4D3A-9C09-A05293C2F75C", "Invalid Selection.");

		internal void ValidateAuthorityURL()
		{
			AuthorityURLInfo.ClearAllNotifications();
			if (IsOIDCEnabled)
			{
				MandatoryValidation.CheckEntered(AuthorityURLInfo);
				if (AuthorityURLInfo.HasErrors())
				{
					return;
				}
			}

			bool valid = Uri.TryCreate(AuthorityURL, UriKind.Absolute, out var uriResult);
			bool isHttps = valid && (uriResult.Scheme == Uri.UriSchemeHttps);

			if (!valid && IsOIDCEnabled)
			{
				AuthorityURLInfo.AddError(authorityURLIsMalformed);
			}
			else if (!isHttps && IsOIDCEnabled)
			{
				AuthorityURLInfo.AddError(authorityURLIsNotHttps);
			}
		}

		internal void ValidateClientIdentifier()
		{
			ClientIdentifierInfo.ClearAllNotifications();
			if (IsOIDCEnabled)
			{
				MandatoryValidation.CheckEntered(ClientIdentifierInfo);
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateOIDCServerTypeCode();
			ValidateAuthorityURL();
			ValidateClientIdentifier();
		}

		public override bool Equals(object obj)
		{
			var config = obj as OIDCConfig;
			return config != null
				&& config.IsOIDCEnabled == IsOIDCEnabled
				&& config.OIDCServerTypeCode == OIDCServerTypeCode
				&& config.AuthorityURL.Equals(AuthorityURL)
				&& config.ClientIdentifier.Equals(ClientIdentifier)
				&& string.Join("", config.ClaimsMappings.Cast<OIDCClaimsMapping>().Select(x => x.ClaimName + x.Identifier)) == string.Join("", ClaimsMappings.Cast<OIDCClaimsMapping>().Select(x => x.ClaimName + x.Identifier))
				&& string.Join("", config.Scopes.Cast<OIDCScope>().Select(x => x.ScopeName)) == string.Join("", Scopes.Cast<OIDCScope>().Select(x => x.ScopeName));
		}

		public override int GetHashCode()
		{
			return
				(IsOIDCEnabled.ToString()
				+ OIDCServerTypeCode
				+ AuthorityURL
				+ ClientIdentifier
				+ string.Join("", ClaimsMappings.Cast<OIDCClaimsMapping>().Select(x => x.ClaimName + x.Identifier))
				+ string.Join("", Scopes.Cast<OIDCScope>().Select(x => x.ScopeName))
				).GetHashCode();
		}

		public static OIDCConfig DefaultValue
		{
			get
			{
				return new OIDCConfig
				{
					IsOIDCEnabled = ZBool.False,
					OIDCServerType = OIDCServerTypes.Generic,
					AuthorityURL = ZString.Empty,
					ClientIdentifier = ZString.Empty,
				};
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new OIDCConfig
			{
				IsOIDCEnabled = IsOIDCEnabled,
				OIDCServerTypeCode = OIDCServerTypeCode,
				AuthorityURL = AuthorityURL,
				ClientIdentifier = ClientIdentifier,
			};

			foreach (var mapping in ClaimsMappings.Cast<OIDCClaimsMapping>())
			{
				var mappingClone = new OIDCClaimsMapping()
				{
					ClaimName = mapping.ClaimName,
					Identifier = mapping.Identifier,
				};

				result.ClaimsMappings.Add(mappingClone);
			}

			foreach (var scope in Scopes.Cast<OIDCScope>())
			{
				var scopeClone = new OIDCScope()
				{
					ScopeName = scope.ScopeName,
				};

				result.Scopes.Add(scopeClone);
			}

			return result;
		}

		public new OIDCConfig Clone() => (OIDCConfig)GetClone(null, null);

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			var version = reader.ReadElementStringAsZInt(Schema.Version);
			IsOIDCEnabled = reader.ReadElementStringAsZBool(Schema.IsOIDCEnabled);
			OIDCServerTypeCode = reader.ReadElementString(Schema.OIDCServerTypeCode);
			AuthorityURL = reader.ReadElementString(Schema.AuthorityURL);
			ClientIdentifier = reader.ReadElementString(Schema.ClientIdentifier);
			claimsMappings = (OIDCClaimsMappingCollection)ClaimsMappingSerializer.Deserialize(reader);
			scopes = (OIDCScopesCollection)ScopesSerializer.Deserialize(reader);

			if (IsOIDCEnabled)
			{
				IsVerified = true;
			}
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Version, Version.ToString());
			writer.WriteElementString(Schema.IsOIDCEnabled, IsOIDCEnabled.ToString());
			writer.WriteElementString(Schema.OIDCServerTypeCode, OIDCServerTypeCode);
			writer.WriteElementString(Schema.AuthorityURL, AuthorityURL);
			writer.WriteElementString(Schema.ClientIdentifier, ClientIdentifier);
			ClaimsMappingSerializer.Serialize(writer, ClaimsMappings);
			ScopesSerializer.Serialize(writer, Scopes);
		}

		ZInt Version { get; } = 1;

		ZXmlSerializer ClaimsMappingSerializer
		{
			get { return claimsMappingSerializer ?? (claimsMappingSerializer = ZXmlSerializer.New(typeof(OIDCClaimsMappingCollection))); }
		}
		ZXmlSerializer claimsMappingSerializer;

		ZXmlSerializer ScopesSerializer
		{
			get { return scopesSerializer ?? (scopesSerializer = ZXmlSerializer.New(typeof(OIDCScopesCollection))); }
		}

		IEnumerable<IOIDCClaimsMapping> IOIDCConfig.ClaimsMappings => ClaimsMappings;

		IEnumerable<ZString> IOIDCConfig.OIDCServerTypesList
		{
			get
			{
				foreach (var codePair in OIDCServerTypesList.Cast<ICodeDescription>())
				{
					yield return codePair.Code;
				}
			}
		}

		IEnumerable<IOIDCScope> IOIDCConfig.Scopes => Scopes;

		ZXmlSerializer scopesSerializer;

		static class Schema
		{
			internal const string Version = "Version"; // In-case we ever have to modify the contents of this registry item
			internal const string IsOIDCEnabled = "IsOIDCEnabled";
			internal const string OIDCServerTypeCode = "OIDCServerTypeCode";
			internal const string AuthorityURL = "AuthorityURL";
			internal const string ClientIdentifier = "ClientIdentifier";
		}
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public sealed class OIDCClaimsMapping : RegistryBusinessObjectTemplate, IOIDCClaimsMapping
	{
		ZString claimName;
		ZString identifier;

		public ZString ClaimName
		{
			get
			{
				return claimName;
			}
			set
			{
				if (value != claimName)
				{
					SetNonPersistentPropertyValue(ClaimNameInfo, ref claimName, value);
					if (!IsValidationSuspended)
					{
						ValidateClaimName();
					}
					HasChanges = true;
				}
			}
		}
		public ZPropertyInfo ClaimNameInfo => GetZPropertyInfo(nameof(ClaimName));

		internal void ValidateClaimName()
		{
			ClaimNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ClaimNameInfo);
		}

		[List("OIDCClaimMappingIdentifiers")]
		public ZString Identifier
		{
			get
			{
				return identifier;
			}
			set
			{
				if (value != identifier)
				{
					SetNonPersistentPropertyValue(IdentifierInfo, ref identifier, value);
					if (!IsValidationSuspended)
					{
						ValidateIdentifier();
					}
					HasChanges = true;
				}
			}
		}
		public ZPropertyInfo IdentifierInfo => GetZPropertyInfo(nameof(Identifier));

		internal void ValidateIdentifier()
		{
			if (new OIDCClaimMappingIdentifiers().ContainsCode(Identifier))
			{
				IdentifierInfo.ClearAllNotifications();
			}
			else
			{
				IdentifierInfo.AddError(identifierInvalidErrorMessage);
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OIDCClaimsMapping()
			{
				ClaimName = ClaimName,
				Identifier = Identifier
			};
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Version, Version.ToString());
			writer.WriteElementString(Schema.ClaimName, ClaimName);
			writer.WriteElementString(Schema.Identifier, Identifier);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			var version = reader.ReadElementStringAsZInt(Schema.Version);
			ClaimName = reader.ReadElementString(Schema.ClaimName);
			Identifier = reader.ReadElementString(Schema.Identifier);
		}

		ZString identifierInvalidErrorMessage => Res.GetString("A3CE3AE8-0CF4-4847-8604-E3953930714E", "Invalid Selection.");
		public CodeDescriptionPairList OIDCClaimMappingIdentifiers => new OIDCClaimMappingIdentifiers();

		ZInt Version { get; } = 1;

		IEnumerable<ZString> IOIDCClaimsMapping.OIDCClaimMappingIdentifiers
		{
			get
			{
				foreach (var codePair in OIDCClaimMappingIdentifiers.Cast<ICodeDescription>())
				{
					yield return codePair.Code;
				}
			}
		}

		static class Schema
		{
			internal const string Version = "Version"; // In-case we ever have to modify the contents of this registry item
			internal const string ClaimName = "ClaimName";
			internal const string Identifier = "Identifier";
		}
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public sealed class OIDCClaimsMappingCollection : RegistryBusinessObjectCollectionTemplate<OIDCClaimsMapping>, IBusinessObjectCollection<OIDCClaimsMapping>
	{
		public OIDCClaimsMappingCollection() : base(null, null)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OIDCClaimsMapping();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OIDCClaimsMappingCollection();
		}
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public sealed class OIDCScope : RegistryBusinessObjectTemplate, IOIDCScope
	{
		ZString scopeName;

		public ZString ScopeName
		{
			get
			{
				return scopeName;
			}
			set
			{
				if (value != scopeName)
				{
					SetNonPersistentPropertyValue(ScopeNameInfo, ref scopeName, value);
					if (!IsValidationSuspended)
					{
						ValidateScopeName();
					}
					HasChanges = true;
				}
			}
		}
		public ZPropertyInfo ScopeNameInfo => GetZPropertyInfo(nameof(ScopeName));

		internal void ValidateScopeName()
		{
			ScopeNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ScopeNameInfo);
			var reservedScopes = new HashSet<string>() { (NoResString)"openid", "offline_access" };
			if (reservedScopes.Contains(ScopeName))
			{
				ScopeNameInfo.AddError(Res.GetString("4645782C-3C3D-4155-BA74-4AEF351AD93E", "Scope values \"{0}\" are reserved. Please remove them from this list.", string.Join(", ", reservedScopes)));
			}

			if (scopeName.ToString().Any(char.IsWhiteSpace))
			{
				ScopeNameInfo.AddError(Res.GetString("2CB928D9-0203-4775-804C-BF80DB38FFAD", "Scope Value cannot contain whitespace : {0}", scopeName));
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OIDCScope()
			{
				ScopeName = ScopeName,
			};
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Version, Version.ToString());
			writer.WriteElementString(Schema.ScopeName, ScopeName);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			var version = reader.ReadElementStringAsZInt(Schema.Version);
			ScopeName = reader.ReadElementString(Schema.ScopeName);
		}

		ZInt Version { get; } = 1;

		static class Schema
		{
			internal const string Version = "Version"; // In-case we ever have to modify the contents of this registry item
			internal const string ScopeName = "ScopeName";
		}
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public sealed class OIDCScopesCollection : RegistryBusinessObjectCollectionTemplate<OIDCScope>
	{
		public OIDCScopesCollection() : base(null, null)
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OIDCScope();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OIDCScopesCollection();
		}
	}
}
