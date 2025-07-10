using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace ZClientEDI.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class WTGActiveDirectoryCredentials : RegistryBusinessObjectTemplate, IDomainCredentials
	{
		#region Domain Name

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

		#region Domain Username

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

		#region Domain User Password

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

		#endregion

		#region Organizational Unit Path

		public ZString OrganizationalUnitPath
		{
			get { return organizationalUnitPath; }
			set
			{
				SetNonPersistentPropertyValue(OrganizationalUnitPathInfo, ref organizationalUnitPath, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateOrganizationalUnitPath();
				}
			}
		}

		ZString organizationalUnitPath;

		public ZPropertyInfo OrganizationalUnitPathInfo => GetZPropertyInfo(nameof(OrganizationalUnitPath));

		#endregion

		#region IsEnabled

		public ZBool IsEnabled
		{
			get { return isEnabled; }
			set
			{
				SetNonPersistentPropertyValue(IsEnabledInfo, ref isEnabled, value);
			}
		}
		ZBool isEnabled;

		public ZPropertyInfo IsEnabledInfo => GetZPropertyInfo(nameof(IsEnabled));

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new WTGActiveDirectoryCredentials
			{
				DomainName = DomainName,
				DomainUserName = DomainUserName,
				DomainUserPassword = DomainUserPassword,
				OrganizationalUnitPath = OrganizationalUnitPath,
				IsEnabled = IsEnabled,
			};
			return result;
		}

		#endregion

		#region Xml Serialisation

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			IsEnabled = reader.ReadElementStringAsZBool(Schema.IsEnabled);
			DomainName = reader.ReadElementString(Schema.DomainName);
			DomainUserName = reader.ReadElementString(Schema.DomainUserName);
			DomainUserPassword = reader.ReadElementString(Schema.DomainUserPassword);
			OrganizationalUnitPath = reader.ReadElementString(Schema.OrganizationalUnitPath);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.IsEnabled, IsEnabled.ToString());
			writer.WriteElementString(Schema.DomainName, DomainName);
			writer.WriteElementString(Schema.DomainUserName, DomainUserName);
			writer.WriteElementString(Schema.DomainUserPassword, DomainUserPassword);
			writer.WriteElementString(Schema.OrganizationalUnitPath, OrganizationalUnitPath);
		}

		#endregion

		#region Default Value

		public static WTGActiveDirectoryCredentials DefaultValue
		{
			get
			{
				return new WTGActiveDirectoryCredentials
				{
					OrganizationalUnitPath = "root/Accounts/Token Based Authentication",
				};
			}
		}

		#endregion

		#region Schema

		static class Schema
		{
			internal const string DomainName = "DomainName";
			internal const string DomainUserName = "DomainUserName";
			internal const string DomainUserPassword = "DomainUserPassword";
			internal const string OrganizationalUnitPath = "OrganizationalUnitPath";
			internal const string IsEnabled = "IsEnabled";
		}

		#endregion

		#region IDomainCredentials

		string IDomainCredentials.DomainName { get => DomainName; set => DomainName = value; }
		string IDomainCredentials.DomainUserName { get => DomainUserName; set => DomainUserName = value; }
		string IDomainCredentials.DomainUserPassword { get => DomainUserPassword; set => DomainUserPassword = value; }
		string IDomainCredentials.UserOrganisationalUnit { get => OrganizationalUnitPath; set => OrganizationalUnitPath = value; }
		string IDomainCredentials.GroupOrganisationalUnit { get => OrganizationalUnitPath; set => OrganizationalUnitPath = value; }
		string IDomainCredentials.DefaultPassword { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		bool IDomainCredentials.IsDefaultDomain { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
		bool IDomainCredentials.DefaultPasswordFailsToMeetDomainPolicy { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			if (!IsValidationSuspended)
			{
				Validation.ValidateDomainName();
				Validation.ValidateDomainUserName();
				Validation.ValidateDomainUserPassword();
				Validation.ValidateOrganizationalUnitPath();
			}
		}
		public WTGActiveDirectoryCredentialsValidation Validation => validation ?? (validation = new WTGActiveDirectoryCredentialsValidation(this));

		WTGActiveDirectoryCredentialsValidation validation;

		#endregion
	}
}
