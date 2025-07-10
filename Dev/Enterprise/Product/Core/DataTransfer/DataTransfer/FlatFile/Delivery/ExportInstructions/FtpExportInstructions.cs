using System.IO;
using System.Net;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Business
{
	public class FtpExportInstructions : NonPersistentBusinessObject, IObsoleteValidation
	{
		public FtpExportInstructions()
		{
		}

		public FtpExportInstructions(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Bound Proprerties

		#region ServerAddress

		[MaxLength(EDICommunicationsMode.Schema.EK_ServerAddressSubjectMaxLength)]
		public ZString ServerAddress
		{
			get
			{
				return fServerAddress;
			}
			set
			{
				value = value.TrimEnd(' ');
				if (fServerAddress != value)
				{
					SetNonPersistentPropertyValue<ZString>(ServerAddressInfo, ref fServerAddress, value);
					if (!IsValidationSuspended)
					{
						ValidateServerAddress();
					}
				}
			}
		}

		protected virtual void ValidateServerAddress()
		{
			ServerAddressInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ServerAddressInfo);
		}

		ZString fServerAddress;

		public ZPropertyInfo ServerAddressInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(ServerAddress));
			}
		}

		#endregion

		#region ServerAddress

		[MaxLength(EDICommunicationsMode.Schema.EK_DestinationMaxLength)]
		public ZString DestinationPath
		{
			get { return fDestinationPath; }
			set { SetNonPersistentPropertyValue<ZString>(DestinationPathInfo, ref fDestinationPath, value); }
		}

		ZString fDestinationPath;

		public ZPropertyInfo DestinationPathInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(DestinationPath));
			}
		}

		#endregion

		#region Username

		[MaxLength(EDICommunicationsMode.Schema.EK_LoginNameMaxLength)]
		public ZString Username
		{
			get
			{
				return fUsername;
			}
			set
			{
				value = value.TrimEnd(' ');
				if (fUsername != value)
				{
					SetNonPersistentPropertyValue<ZString>(UsernameInfo, ref fUsername, value);
					if (!IsValidationSuspended)
					{
						ValidateUsername();
					}
				}
			}
		}

		protected virtual void ValidateUsername()
		{
			UsernameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(UsernameInfo);
		}

		ZString fUsername;

		public ZPropertyInfo UsernameInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(Username));
			}
		}

		#endregion

		#region Password

		[MaxLength(EDICommunicationsMode.Schema.EK_PasswordMaxLength)]
		public ZString Password
		{
			get
			{
				return fPassword;
			}
			set
			{
				value = value.TrimEnd(' ');
				if (fPassword != value)
				{
					SetNonPersistentPropertyValue<ZString>(PasswordInfo, ref fPassword, value);
					if (!IsValidationSuspended)
					{
						ValidatePassword();
					}
				}
			}
		}

		protected virtual void ValidatePassword()
		{
			PasswordInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PasswordInfo);
		}

		ZString fPassword;

		public ZPropertyInfo PasswordInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(Password));
			}
		}

		#endregion

		#region PortNumber

		public ZInt PortNumber
		{
			get
			{
				return fPortNumber;
			}
			set
			{
				if (fPortNumber != value)
				{
					SetNonPersistentPropertyValue<ZInt>(PortNumberInfo, ref fPortNumber, value);
					if (!IsValidationSuspended)
					{
						ValidatePortNumber();
					}
				}
			}
		}

		protected virtual void ValidatePortNumber()
		{
			PortNumberInfo.ClearAllNotifications();
			MandatoryValidation.CheckNotNegative(PortNumberInfo);
		}

		ZInt fPortNumber;

		public ZPropertyInfo PortNumberInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(PortNumber));
			}
		}

		#endregion

		#region Proxy

		public IWebProxy Proxy
		{
			get
			{
				return fProxy;
			}
			set
			{
				if (fProxy != value)
				{
					fProxy = value;
				}
			}
		}

		IWebProxy fProxy;

		#endregion

		#endregion

		public string UriString
		{
			get
			{
				ZString result = FixWebDirectoryPath(ServerAddress);
				if (PortNumber > 0)
				{
					result += ":" + PortNumber.ToString().Trim();
				}

				if (!DestinationPath.IsEmpty)
				{
					result += Path.AltDirectorySeparatorChar + FixWebDirectoryPath(DestinationPath);
				}

				if (!result.Contains("://"))
				{
					result = "ftp://" + result;
				}
				return result;
			}
		}

		ZString FixWebDirectoryPath(ZString directory)
		{
			return directory.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Trim(Path.AltDirectorySeparatorChar);
		}

		#region Implementation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateServerAddress();
			ValidateUsername();
			ValidatePassword();
		}

		#endregion
	}
}
