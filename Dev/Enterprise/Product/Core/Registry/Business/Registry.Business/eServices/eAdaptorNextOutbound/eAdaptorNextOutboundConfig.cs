using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	//This is deprecated 
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class eAdaptorNextOutboundConfig : RegistryBusinessObjectTemplate, IeAdaptorNextOutboundConfig
	{
		static class Schema
		{
			internal const string Version = "Version"; // In-case we ever have to modify the contents of this registry item
			internal const string IsOAuth2Enabled = "IsOAuth2Enabled";
			internal const string AuthorizationGrantTypeCode = "AuthorizationGrantTypeCode";
			internal const string AuthorizationURL = "AuthorizationURL";
			internal const string Username = "Username";
			internal const string Password = "Password";
			internal const string ClientID = "ClientID";
			internal const string ClientSecret = "ClientSecret";
		}

		public string CachingKey => "eAdaptorNextOutboundConfig";

		#region IsOAuth2Enabled

		[ResourceStringData("eAdaptorNextOutboundConfig.IsOAuth2Enabled", Caption = "Enabled")]
		public ZBool IsOAuth2Enabled
		{
			get { return isOAuth2Enabled; }
			set
			{
				SetNonPersistentPropertyValue(IsOAuth2EnabledInfo, ref isOAuth2Enabled, value);
				Scopes.SetReadOnlyIncludingChildren(!value);
			}
		}
		ZBool isOAuth2Enabled;

		public ZPropertyInfo IsOAuth2EnabledInfo => GetZPropertyInfo(nameof(IsOAuth2Enabled));

		#endregion

		[List("eAdaptorNextOutboundGrantTypesListInstance")]
		[ResourceStringData("eAdaptorNextOutboundConfig.AuthorizationGrantTypeCode", Caption = "OAuth2.0 Grant Type")]
		[MaxLength(3)]
		public ZString AuthorizationGrantTypeCode
		{
			get { return authorizationGrantTypeCode; }
			set
			{
				SetNonPersistentPropertyValue(AuthorizationGrantTypeCodeInfo, ref authorizationGrantTypeCode, value);
				if (!IsValidationSuspended)
				{
					ValidateAuthorizationGrantTypeCode();
				}
			}
		}
		ZString authorizationGrantTypeCode;

		public ZPropertyInfo AuthorizationGrantTypeCodeInfo => GetZPropertyInfo(nameof(AuthorizationGrantTypeCode));

		internal void ValidateAuthorizationGrantTypeCode()
		{
			ListValidation.ErrorIfInvalidCode(AuthorizationGrantTypeCodeInfo, eAdaptorNextOutboundGrantTypesListInstance);
			MandatoryValidation.CheckEntered(AuthorizationGrantTypeCodeInfo);
		}

		public CodeDescriptionPairList eAdaptorNextOutboundGrantTypesListInstance => new eAdaptorNextOutboundGrantTypesList();

		#region AuthorizationURL

		[ResourceStringData("eAdaptorNextOutboundConfig.AuthorizationURL", Caption = "OAuth2.0 Authorization URL")]
		public ZString AuthorizationURL
		{
			get { return authorizationURL; }
			set
			{
				SetNonPersistentPropertyValue(AuthorizationURLInfo, ref authorizationURL, value);
				if (!IsValidationSuspended)
				{
					ValidateAuthorizationURL();
				}
			}
		}
		ZString authorizationURL;

		public ZPropertyInfo AuthorizationURLInfo => GetZPropertyInfo(nameof(AuthorizationURL));

		protected bool AuthorizationURL_ReadOnly => !IsOAuth2Enabled;

		ZString AuthorizationURLIsMalformed => Res.GetString("b167a608-362b-43ea-a9ac-7c9c82e9c4ea", "Malformed Authorization URL.");
		ZString AuthorizationURLIsNotHttps => Res.GetString("d85ac2ab-a915-4d85-ae8b-70b867ac074c", "Authorization URL must be https.");

		internal void ValidateAuthorizationURL()
		{
			AuthorizationURLInfo.ClearAllNotifications();
			if (IsOAuth2Enabled)
			{
				MandatoryValidation.CheckEntered(AuthorizationURLInfo);
				if (AuthorizationURLInfo.HasErrors())
				{
					return;
				}
			}

			bool valid = Uri.TryCreate(AuthorizationURL, UriKind.Absolute, out var uriResult);
			bool isHttps = valid && (uriResult.Scheme == Uri.UriSchemeHttps);

			if (!valid && IsOAuth2Enabled)
			{
				AuthorizationURLInfo.AddError(AuthorizationURLIsMalformed);
			}
			else if (!isHttps && IsOAuth2Enabled)
			{
				AuthorizationURLInfo.AddError(AuthorizationURLIsNotHttps);
			}
		}

		#endregion

		#region IsVerified

		public ZBool IsVerified
		{
			get
			{
				if (lastVerifiedConfig == null)
				{
					lastVerifiedConfig = this.Clone();
				}

				return this.Equals(lastVerifiedConfig);
			}
			set
			{
				if (value)
				{
					lastVerifiedConfig = this.Clone();
				}

				// Refresh binding is not used but required by unit tests
				IsVerifiedInfo.RefreshBinding();
				ValidateIsVerified();
			}
		}
		eAdaptorNextOutboundConfig lastVerifiedConfig;

		internal void ValidateIsVerified()
		{
			IsVerifiedInfo.ClearAllNotifications();
			if (IsOAuth2Enabled && !IsVerified)
			{
				IsVerifiedInfo.AddError(Res.GetString("e7d0faac-213e-4e83-a11a-40edbab5db50", "Configuration must be verified before changes can be saved"));
			}
		}

		public ZPropertyInfo IsVerifiedInfo => GetZPropertyInfo(nameof(IsVerified));

		#endregion

		#region ClientID

		[ResourceStringData("eAdaptorNextOutboundConfig.ClientID", Caption = "Client ID")]
		public ZString ClientID
		{
			get { return clientID; }
			set
			{
				SetNonPersistentPropertyValue(ClientIDInfo, ref clientID, value);
				if (!IsValidationSuspended)
				{
					ValidateClientID();
				}
			}
		}
		ZString clientID;

		public ZPropertyInfo ClientIDInfo => GetZPropertyInfo(nameof(ClientID));

		protected bool ClientID_ReadOnly => !IsOAuth2Enabled;

		internal void ValidateClientID()
		{
			ClientIDInfo.ClearAllNotifications();
			if (IsOAuth2Enabled)
			{
				MandatoryValidation.CheckEntered(ClientIDInfo);
			}
		}

		#endregion

		#region ClientSecret

		[ResourceStringData("eAdaptorNextOutboundConfig.ClientSecret", Caption = "Client Secret")]
		public ZString ClientSecret
		{
			get { return clientSecret; }
			set
			{
				SetNonPersistentPropertyValue(ClientSecretInfo, ref clientSecret, value);
				if (!IsValidationSuspended)
				{
					ValidateClientSecret();
				}
			}
		}
		ZString clientSecret;

		public ZPropertyInfo ClientSecretInfo => GetZPropertyInfo(nameof(ClientSecret));

		protected bool ClientSecret_ReadOnly => !IsOAuth2Enabled;

		internal void ValidateClientSecret()
		{
			ClientSecretInfo.ClearAllNotifications();
			if (IsOAuth2Enabled && AuthorizationGrantTypeCode != eAdaptorNextOutboundGrantTypesList.Codes.ClientCertificate)
			{
				MandatoryValidation.CheckEntered(ClientSecretInfo);
			}
		}

		#endregion

		#region Username

		[ResourceStringData("eAdaptorNextOutboundConfig.Username", Caption = "Username")]
		public ZString Username
		{
			get { return username; }
			set { SetNonPersistentPropertyValue(UsernameInfo, ref username, value); }
		}
		ZString username;

		public ZPropertyInfo UsernameInfo => GetZPropertyInfo(nameof(Username));

		protected bool Username_ReadOnly => !IsOAuth2Enabled;

		#endregion

		#region Password

		[ResourceStringData("eAdaptorNextOutboundConfig.Password", Caption = "Password")]
		public ZString Password
		{
			get { return password; }
			set { SetNonPersistentPropertyValue(PasswordInfo, ref password, value); }
		}
		ZString password;

		public ZPropertyInfo PasswordInfo => GetZPropertyInfo(nameof(Password));

		protected bool Password_ReadOnly => !IsOAuth2Enabled;

		#endregion

		#region Scopes

		[ChildEditable]
		public OAuth2ScopesCollection Scopes
		{
			get
			{
				if (scopes == null)
				{
					scopes = new OAuth2ScopesCollection();
					RegisterEditableChildObject(scopes);
				}

				return scopes;
			}
		}
		OAuth2ScopesCollection scopes;

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new eAdaptorNextOutboundConfig
			{
				IsOAuth2Enabled = IsOAuth2Enabled,
				AuthorizationGrantTypeCode = AuthorizationGrantTypeCode,
				AuthorizationURL = AuthorizationURL,
				ClientID = ClientID,
				ClientSecret = ClientSecret,
				Username = Username,
				Password = Password
			};

			foreach (var scope in Scopes.Cast<OAuth2Scope>())
			{
				var scopeClone = new OAuth2Scope()
				{
					ScopeName = scope.ScopeName,
				};

				result.Scopes.Add(scopeClone);
			}

			return result;
		}

		public new eAdaptorNextOutboundConfig Clone() => (eAdaptorNextOutboundConfig)GetClone(null, null);

		#region Serializers

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			var version = reader.ReadElementStringAsZInt(Schema.Version);
			IsOAuth2Enabled = reader.ReadElementStringAsZBool(Schema.IsOAuth2Enabled);
			AuthorizationGrantTypeCode = reader.ReadElementString(Schema.AuthorizationGrantTypeCode);
			AuthorizationURL = reader.ReadElementString(Schema.AuthorizationURL);
			ClientID = reader.ReadElementString(Schema.ClientID);
			ClientSecret = reader.ReadElementString(Schema.ClientSecret);
			Username = reader.ReadElementString(Schema.Username);
			Password = reader.ReadElementString(Schema.Password);
			scopes = (OAuth2ScopesCollection)ScopesSerializer.Deserialize(reader);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Version, Version.ToString());
			writer.WriteElementString(Schema.IsOAuth2Enabled, IsOAuth2Enabled.ToString());
			writer.WriteElementString(Schema.AuthorizationGrantTypeCode, AuthorizationGrantTypeCode);
			writer.WriteElementString(Schema.AuthorizationURL, AuthorizationURL);
			writer.WriteElementString(Schema.ClientID, ClientID);
			writer.WriteElementString(Schema.ClientSecret, ClientSecret);
			writer.WriteElementString(Schema.Username, Username);
			writer.WriteElementString(Schema.Password, Password);
			ScopesSerializer.Serialize(writer, Scopes);
		}

		ZXmlSerializer ScopesSerializer
		{
			get { return scopesSerializer ?? (scopesSerializer = ZXmlSerializer.New(typeof(OAuth2ScopesCollection))); }
		}
		ZXmlSerializer scopesSerializer;

		ZInt Version { get; } = 1;

		#endregion

		public static eAdaptorNextOutboundConfig DefaultValue
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

		IeAdaptorNextOutboundConfig IeAdaptorNextOutboundConfig.Clone()
		{
			return this.Clone();
		}

		string IClientCertificateGrant.PrivateKey => Username;

		string IClientCertificateGrant.Certificate => Password;

		string IClientSecret.ClientSecret => ClientSecret;

		string IPasswordGrant.Username => Username;

		string IPasswordGrant.Password => Password;

		string ICommonOAuth2Parameters.AuthorizationURL => AuthorizationURL;

		string ICommonOAuth2Parameters.ClientID => ClientID;

		IEnumerable<string> ICommonOAuth2Parameters.Scopes => Scopes.Cast<OAuth2Scope>().Select(x => (string)x.ScopeName);

		string ICommonOAuth2Parameters.FlowCode => AuthorizationGrantTypeCode;

		public override bool Equals(object obj)
		{
			var config = obj as eAdaptorNextOutboundConfig;
			return config != null
				&& config.IsOAuth2Enabled == IsOAuth2Enabled
				&& config.AuthorizationGrantTypeCode == AuthorizationGrantTypeCode
				&& config.AuthorizationURL.Equals(AuthorizationURL)
				&& config.ClientID.Equals(ClientID)
				&& config.ClientSecret.Equals(ClientSecret)
				&& config.Username.Equals(Username)
				&& config.Password.Equals(Password)
				&& string.Join("", config.Scopes.Cast<OAuth2Scope>().Select(x => x.ScopeName)) == string.Join("", Scopes.Cast<OAuth2Scope>().Select(x => x.ScopeName));
		}

		public override int GetHashCode()
		{
			return
				IsOAuth2Enabled.GetHashCode()
				^ AuthorizationGrantTypeCode.GetHashCode()
				^ AuthorizationURL.GetHashCode()
				^ ClientID.GetHashCode()
				^ ClientSecret.GetHashCode()
				^ Username.GetHashCode()
				^ Password.GetHashCode()
				^ Scopes.GetHashCode();
		}
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public sealed class OAuth2Scope : RegistryBusinessObjectTemplate, IOAuth2Scope
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
			if (scopeName.ToString().Any((c) => char.IsWhiteSpace(c) || c == ','))
			{
				ScopeNameInfo.AddError(Res.GetString("0faaf44c-c445-4c33-80c4-d60b441bd16f", "Scope Value cannot contain whitespace : {0}", scopeName));
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OAuth2Scope()
			{
				ScopeName = ScopeName,
			};
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ScopeName, ScopeName);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ScopeName = reader.ReadElementString(Schema.ScopeName);
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			var other = obj as OAuth2Scope;
			return other.ScopeName == this.ScopeName;
		}

		static class Schema
		{
			internal const string ScopeName = "ScopeName";
		}

		public override int GetHashCode()
		{
			return ScopeName.GetHashCode();
		}
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public sealed class OAuth2ScopesCollection : RegistryBusinessObjectCollectionTemplate<OAuth2Scope>
	{
		public OAuth2ScopesCollection() : base(null, null)
		{
		}

		public override int GetHashCode()
		{
			int hash = 0;
			foreach (var scope in Elements.Cast<OAuth2Scope>())
			{
				hash ^= scope.GetHashCode();
			}
			return hash;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			var other = obj as OAuth2ScopesCollection;
			var set1 = new HashSet<OAuth2Scope>(other);
			var set2 = new HashSet<OAuth2Scope>(this);

			return set1.SetEquals(set2);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OAuth2Scope();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new OAuth2ScopesCollection();
		}
	}
}
