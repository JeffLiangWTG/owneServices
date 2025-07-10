using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.GB.Registry.XmlSerializers")]
	public class McpIslCredentialsSetting : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string McpIslCompanyCode = "McpIslCompanyCode";
			public const string McpIslUsername = "McpIslUsername";
			public const string McpIslDevice = "McpIslDevice";
			public const string McpIslPassword = "McpIslPassword";
		}

		#endregion

		public McpIslCredentialsSetting()
		{
		}

		public McpIslCredentialsSetting(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new McpIslCredentialsSetting(fallbackLevel, factory);
		}

		#endregion

		#region McpIslCompanyCode

		ZString fCompanyCode;
		[MaxLength(3)]
		public ZString McpIslCompanyCode
		{
			get { return fCompanyCode; }
			set
			{
				SetNonPersistentPropertyValue(McpIslCompanyCodeInfo, ref fCompanyCode, value);
				if (!IsValidationSuspended)
				{
					ValidateMcpIslCompanyCode();
				}
			}
		}

		public ZPropertyInfo McpIslCompanyCodeInfo
		{
			get { return GetZPropertyInfo(Schema.McpIslCompanyCode); }
		}

		public void ValidateMcpIslCompanyCode()
		{
			McpIslCompanyCodeInfo.ClearAllNotifications();
			if (McpIslCompanyCode.IsEmpty)
			{
				McpIslCompanyCodeInfo.AddError("Company Code cannot be empty.");
			}
		}

		#endregion

		#region McpIslUsername

		ZString fUsername;
		[MaxLength(255)]
		public ZString McpIslUsername
		{
			get { return fUsername; }
			set
			{
				SetNonPersistentPropertyValue(McpIslUsernameInfo, ref fUsername, value);
				if (!IsValidationSuspended)
				{
					ValidateMcpIslUsername();
				}
			}
		}

		public ZPropertyInfo McpIslUsernameInfo
		{
			get { return GetZPropertyInfo(Schema.McpIslUsername); }
		}

		public void ValidateMcpIslUsername()
		{
			McpIslUsernameInfo.ClearAllNotifications();
			if (McpIslUsername.IsEmpty)
			{
				McpIslUsernameInfo.AddError("Username cannot be empty.");
			}
		}

		#endregion

		#region McpIslDevice

		ZString fDevice;
		[MaxLength(4)]
		public ZString McpIslDevice
		{
			get { return fDevice; }
			set
			{
				SetNonPersistentPropertyValue(McpIslDeviceInfo, ref fDevice, value);
				if (!IsValidationSuspended)
				{
					ValidateMcpIslDevice();
				}
			}
		}

		public ZPropertyInfo McpIslDeviceInfo
		{
			get { return GetZPropertyInfo(Schema.McpIslDevice); }
		}

		public void ValidateMcpIslDevice()
		{
			McpIslDeviceInfo.ClearAllNotifications();
			if (McpIslDevice.IsEmpty)
			{
				McpIslDeviceInfo.AddError("Device cannot be empty.");
			}
		}

		#endregion

		#region McpIslPassword

		ZString fPassword;
		[Password]
		[MaxLength(255)]
		public ZString McpIslPassword
		{
			get { return fPassword; }
			set
			{
				SetNonPersistentPropertyValue(McpIslPasswordInfo, ref fPassword, value);
				if (!IsValidationSuspended)
				{
					ValidateMcpIslPassword();
				}
			}
		}

		public ZPropertyInfo McpIslPasswordInfo
		{
			get { return GetZPropertyInfo(Schema.McpIslPassword); }
		}

		public void ValidateMcpIslPassword()
		{
			McpIslPasswordInfo.ClearAllNotifications();
			if (McpIslPassword.IsEmpty)
			{
				McpIslPasswordInfo.AddError("Password cannot be empty.");
			}
		}

		#endregion

		public void ValidateAll()
		{
			ValidateMcpIslCompanyCode();
			ValidateMcpIslUsername();
			ValidateMcpIslDevice();
			ValidateMcpIslPassword();
		}

		#region Xml Serialisation

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.McpIslCompanyCode, McpIslCompanyCode);
			writer.WriteElementString(Schema.McpIslUsername, McpIslUsername);
			writer.WriteElementString(Schema.McpIslDevice, McpIslDevice);
			writer.WriteElementString(Schema.McpIslPassword, McpIslPassword);
		}

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			McpIslCompanyCode = reader.ReadElementString(Schema.McpIslCompanyCode);
			McpIslUsername = reader.ReadElementString(Schema.McpIslUsername);
			McpIslDevice = reader.ReadElementString(Schema.McpIslDevice);
			McpIslPassword = reader.ReadElementString(Schema.McpIslPassword);
		}

		#endregion
	}
}
