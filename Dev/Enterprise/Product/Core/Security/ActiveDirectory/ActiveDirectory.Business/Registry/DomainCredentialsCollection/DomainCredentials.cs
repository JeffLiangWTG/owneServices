using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.ActiveDirectory
{
	[XmlSerializerAssembly("Enterprise.Security.ActiveDirectory.XmlSerializers")]
	public sealed class DomainCredentials : RegistryBusinessObjectTemplate, IDomainCredentials
	{
		#region Properties

		#region DomainName

		[ResourceStringData("DomainCredentials.DomainName", Caption = "Domain Name", FullDescription = "The fully-qualified URI of the domain controller to connect to, rather than the domain the current machine is a part of.")]
		public ZString DomainName
		{
			get { return domainName; }
			set
			{
				SetNonPersistentPropertyValue(DomainNameInfo, ref domainName, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDomainName();
				}
			}
		}
		ZString domainName;

		public ZPropertyInfo DomainNameInfo => GetZPropertyInfo(nameof(DomainName));

		#endregion

		#region DomainUserName

		[ResourceStringData("DomainCredentials.DomainUserName", Caption = "Domain User Name", FullDescription = "The user to authenticate with when connecting to a domain controller (either the current domain, or the one specified in this registry item).")]
		public ZString DomainUserName
		{
			get { return domainUserName; }
			set
			{
				SetNonPersistentPropertyValue(DomainUserNameInfo, ref domainUserName, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDomainUserName();
				}
			}
		}
		ZString domainUserName;

		public ZPropertyInfo DomainUserNameInfo => GetZPropertyInfo(nameof(DomainUserName));

		#endregion

		#region DomainUserPassword

		[ResourceStringData("DomainCredentials.DomainUserPassword", Caption = "Domain User Password", FullDescription = "The password of the user to authenticate with when connecting to a domain controller (either the current domain, or the one specified in this registry item).")]
		[Password]
		public ZString DomainUserPassword
		{
			get { return domainUserPassword; }
			set
			{
				SetNonPersistentPropertyValue(DomainUserPasswordInfo, ref domainUserPassword, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDomainUserPassword();
				}
			}
		}
		ZString domainUserPassword;

		public ZPropertyInfo DomainUserPasswordInfo => GetZPropertyInfo(nameof(DomainUserPassword));

		public static readonly string InitialisationVector = "3f09c9df-1bb8-494f-a6d1-0daee81a7be4";

		#endregion

		#region IsDefaultDomain

		[ResourceStringData("DomainCredentials.IsDefaultDomain", Caption = "Default Domain", FullDescription = "The default domain to be used to create new AD objects if domain is not specified.")]
		public ZBool IsDefaultDomain
		{
			get { return isDefaultDomain; }
			set
			{
				SetNonPersistentPropertyValue(IsDefaultDomainInfo, ref isDefaultDomain, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateIsDefaultDomain();
				}
			}
		}
		ZBool isDefaultDomain;
		public ZPropertyInfo IsDefaultDomainInfo => GetZPropertyInfo(nameof(IsDefaultDomain));

		#endregion

		#region User Organisational Unit

		[BusinessObjectTestExclude] // The [List] attribute must be applied to the property with a valid list property name
		[ResourceStringData("DomainCredentials.UserOrganisationalUnit", Caption = "Users' Organizational Unit", FullDescription = "The Organizational Unit (OU) in the Active Directory structure where user records will be created and searched for.")]
		public ZString UserOrganisationalUnit
		{
			get { return userOrganisationalUnit; }
			set
			{
				SetNonPersistentPropertyValue(UserOrganisationalUnitInfo, ref userOrganisationalUnit, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateUserOrganisationalUnit();
				}
			}
		}
		ZString userOrganisationalUnit;

		public ZPropertyInfo UserOrganisationalUnitInfo => GetZPropertyInfo(nameof(UserOrganisationalUnit));

		#endregion

		#region Group Organisational Unit

		[BusinessObjectTestExclude] // The [List] attribute must be applied to the property with a valid list property name
		[ResourceStringData("DomainCredentials.GroupOrganisationalUnit", Caption = "Groups' Organizational Unit", FullDescription = "The Organizational Unit (OU) in the Active Directory structure where group records will be created and searched for.")]
		public ZString GroupOrganisationalUnit
		{
			get { return groupOrganisationalUnit; }
			set
			{
				SetNonPersistentPropertyValue(GroupOrganisationalUnitInfo, ref groupOrganisationalUnit, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateGroupOrganisationalUnit();
				}
			}
		}
		ZString groupOrganisationalUnit;

		public ZPropertyInfo GroupOrganisationalUnitInfo => GetZPropertyInfo(nameof(GroupOrganisationalUnit));

		#endregion

		#region Default Password

		[ResourceStringData("DomainCredentials.DefaultPassword", Caption = "Default Password", FullDescription = "The default password to use when creating new Active Directory users. All staff records will use this password in the event that Active Directory integration is disabled. Please make sure this password matches your domain password policy.")]
		[Password]
		public ZString DefaultPassword
		{
			get { return defaultPassword; }
			set
			{
				SetNonPersistentPropertyValue(DefaultPasswordInfo, ref defaultPassword, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDefaultPassword();
				}
			}
		}
		ZString defaultPassword = DefaultPasswordValue;

		public ZPropertyInfo DefaultPasswordInfo => GetZPropertyInfo(nameof(DefaultPassword));

		public static readonly string DefaultPasswordValue = "Ch4ng3m3!@34";

		// This is a flag to indicate the default password does not meet domain policy but not to be serialised to the registry
		public bool DefaultPasswordFailsToMeetDomainPolicy { get; set; }

		#endregion

		#endregion

		#region IDomainCredentials

		string IDomainCredentials.DomainName { get => DomainName; set => DomainName = value; }
		string IDomainCredentials.DomainUserName { get => DomainUserName; set => DomainUserName = value; }
		string IDomainCredentials.DomainUserPassword { get => DomainUserPassword; set => DomainUserPassword = value; }
		string IDomainCredentials.UserOrganisationalUnit { get => UserOrganisationalUnit; set => UserOrganisationalUnit = value; }
		string IDomainCredentials.GroupOrganisationalUnit { get => GroupOrganisationalUnit; set => GroupOrganisationalUnit = value; }
		string IDomainCredentials.DefaultPassword { get => DefaultPassword; set => DefaultPassword = value; }
		bool IDomainCredentials.IsDefaultDomain { get => IsDefaultDomain; set => IsDefaultDomain = value; }
		bool IDomainCredentials.DefaultPasswordFailsToMeetDomainPolicy { get => DefaultPasswordFailsToMeetDomainPolicy; set => DefaultPasswordFailsToMeetDomainPolicy = value; }

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new DomainCredentials();

		#region Xml Serialisation
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		protected override void ReadElements(XmlReaderWrapper reader)
		{
			reader.ReadElementString("DomainCredentials");
			var encoder = new TwoWayEncoder(new Guid(InitialisationVector));

			DomainName = reader.ReadElementString("DomainName").ToLowerInvariant();
			DomainUserName = reader.ReadElementString("DomainUserName");
			DomainUserPassword = encoder.Decrypt(reader.ReadElementString("DomainUserPassword"));
			IsDefaultDomain = reader.ReadElementStringAsZBool("IsDefaultDomain");
			UserOrganisationalUnit = reader.ReadElementString("UserOrganisationalUnit");
			GroupOrganisationalUnit = reader.ReadElementString("GroupOrganisationalUnit");
			DefaultPassword = reader.ReadElementString("DefaultPassword");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			var encoder = new TwoWayEncoder(new Guid(InitialisationVector));

			writer.WriteElementString("DomainName", DomainName.ToString().ToLowerInvariant());
			writer.WriteElementString("DomainUserName", DomainUserName);
			writer.WriteElementString("DomainUserPassword", encoder.Encrypt(DomainUserPassword));
			writer.WriteElementString("IsDefaultDomain", IsDefaultDomain.ToString());
			writer.WriteElementString("UserOrganisationalUnit", UserOrganisationalUnit);
			writer.WriteElementString("GroupOrganisationalUnit", GroupOrganisationalUnit);
			writer.WriteElementString("DefaultPassword", DefaultPassword);
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			if (!IsValidationSuspended)
			{
				Validation.ValidateAll();
			}
		}

		public DomainCredentialsValidation Validation => validation ?? (validation = new DomainCredentialsValidation(this));

		DomainCredentialsValidation validation;

		#endregion
	}
}
