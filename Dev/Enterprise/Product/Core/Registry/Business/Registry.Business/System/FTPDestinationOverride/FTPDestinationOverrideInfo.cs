using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public partial class FTPDestinationOverrideInfo : RegistryBusinessObjectTemplate
	{
		public FTPDestinationOverrideInfo() { }
		public FTPDestinationOverrideInfo(string ftpAddress, string userName, string password)
		{
			FtpAddress = ftpAddress;
			UserName = userName;
			Password = password;
		}

		#region SuppressResourceStringsCheckRegion

		public static class Schema
		{
			public const string FtpAddress = "FtpAddress";
			public const string UserName = "UserName";
			public const string Password = "Password";
		}

		#endregion

		#region Properties

		#region FtpAddress

		public ZString FtpAddress
		{
			get { return ftpAddress; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(FtpAddressInfo, ref ftpAddress, value);
				if (!IsValidationSuspended)
				{
					ValidateFtpAddress(FtpAddressInfo);
				}
			}
		}
		ZString ftpAddress;

		public ZPropertyInfo FtpAddressInfo
		{
			get { return GetZPropertyInfo(Schema.FtpAddress); }
		}

		#endregion

		#region UserName

		public ZString UserName
		{
			get { return userName; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(UserNameInfo, ref userName, value);
				if (!IsValidationSuspended)
				{
					ValidateUserName(UserNameInfo);
				}
			}
		}
		ZString userName;

		public ZPropertyInfo UserNameInfo
		{
			get { return GetZPropertyInfo(Schema.UserName); }
		}

		#endregion

		#region Password

		[Password]
		public ZString Password
		{
			get { return password; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(PasswordInfo, ref password, value);
				if (!IsValidationSuspended)
				{
					ValidatePassword(PasswordInfo);
				}
			}
		}
		ZString password;

		public ZPropertyInfo PasswordInfo
		{
			get { return GetZPropertyInfo(Schema.Password); }
		}

		#endregion

		#endregion //Properties

		#region XMLSerialisation

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			FtpAddress = reader.ReadElementString(Schema.FtpAddress);
			UserName = reader.ReadElementString(Schema.UserName);
			Password = reader.ReadElementString(Schema.Password);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.FtpAddress, FtpAddress);
			writer.WriteElementString(Schema.UserName, UserName);
			writer.WriteElementString(Schema.Password, Password);
		}

		#endregion //XMLSerialisation

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateFtpAddress(FtpAddressInfo);
			ValidateUserName(UserNameInfo);
			ValidatePassword(PasswordInfo);
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new FTPDestinationOverrideInfo();
		}

		void ValidateFtpAddress(ZPropertyInfo ftpAddressInfo)
		{
			ftpAddressInfo.ClearAllNotifications();
			if (!FtpAddress.IsValid)
			{
				var error = ResString.GetMultilingualString("3956A00B-8DCD-4B13-8542-55FBFA25CA8B", "Please enter a valid value");
				ftpAddressInfo.AddError(error);
			}
		}

		void ValidateUserName(ZPropertyInfo userNameInfo)
		{
			userNameInfo.ClearAllNotifications();
			if (!UserName.IsValid)
			{
				var error = ResString.GetMultilingualString("3956A00B-8DCD-4B13-8542-55FBFA25CA8B", "Please enter a valid value");
				userNameInfo.AddError(error);
			}
		}

		void ValidatePassword(ZPropertyInfo passwordInfo)
		{
			passwordInfo.ClearAllNotifications();
			if (!Password.IsValid)
			{
				var error = ResString.GetMultilingualString("3956A00B-8DCD-4B13-8542-55FBFA25CA8B", "Please enter a valid value");
				passwordInfo.AddError(error);
			}
		}
	}
}
