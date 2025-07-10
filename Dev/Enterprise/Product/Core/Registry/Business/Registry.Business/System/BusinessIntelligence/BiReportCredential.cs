using System;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class BiReportCredential : RegistryBusinessObjectTemplate
	{
		public static class Schema
		{
			public const string Domain = "Domain";
			public const string UserName = "UserName";
			public const string Password = "Password";
		}

		public ZString Domain
		{
			get => domain;
			set
			{
				var oldValue = Domain;
				SetNonPersistentPropertyValue(DomainInfo, ref domain, value);

				if (!IsValidationSuspended)
				{
					if (oldValue != value)
					{
						ValidateDomain();
					}
				}
			}
		}
		ZString domain;

		public ZPropertyInfo DomainInfo => GetZPropertyInfo(Schema.Domain);

		public void ValidateDomain()
		{
			DomainInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DomainInfo);
		}

		public ZString UserName
		{
			get => userName;
			set
			{
				var oldValue = UserName;
				SetNonPersistentPropertyValue(UserNameInfo, ref userName, value);

				if (!IsValidationSuspended)
				{
					if (oldValue != value)
					{
						ValidateUserName();
					}
				}
			}
		}
		ZString userName;

		public ZPropertyInfo UserNameInfo => GetZPropertyInfo(Schema.UserName);

		public void ValidateUserName()
		{
			UserNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(UserNameInfo);
		}

		[Password]
		public ZString Password
		{
			get => password;
			set
			{
				var oldValue = Password;
				SetNonPersistentPropertyValue(PasswordInfo, ref password, value);

				if (oldValue != value)
				{
					ValidatePassword();
				}
			}
		}
		ZString password;

		public ZPropertyInfo PasswordInfo => GetZPropertyInfo(Schema.Password);

		internal bool IsEmpty()
		{
			if (this == null)
			{
				return true;
			}
			else
			{
				return string.IsNullOrEmpty(this.UserName);
			}
		}

		public void ValidatePassword()
		{
			PasswordInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PasswordInfo);
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new BiReportCredential();
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Domain = reader.ReadElementString(Schema.Domain);
			UserName = reader.ReadElementString(Schema.UserName);
			var passwordStr = reader.ReadElementString(Schema.Password);
			Password = String.IsNullOrWhiteSpace(passwordStr) ? passwordStr : Encoder.Decrypt(passwordStr);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Domain, Domain);
			writer.WriteElementString(Schema.UserName, UserName);
			writer.WriteElementString(Schema.Password, Password.IsEmpty ? Password : (ZString)Encoder.Encrypt(Password));
		}

		TwoWayEncoder Encoder => encoder ?? (encoder = TwoWayEncoder.NewWithStandardInitialisationVector());
		TwoWayEncoder encoder;
	}
}
