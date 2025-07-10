using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ServerUsernamePasswordConfiguration : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string UserName = "UserName";
			public const string Password = "Password";
		}

		#endregion

		#region Persistent Properties

		#region UserName

		[MaxLength(255)]
		public ZString UserName
		{
			get { return fUserName; }
			set
			{
				if (fUserName != value)
				{
					CheckMaximumLength(UserNameInfo, value);
					fUserName = value;
					if (!IsValidationSuspended)
					{
						ValidateUserName();
					}
					UserNameInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo UserNameInfo
		{
			get { return GetZPropertyInfo(Schema.UserName); }
		}

		ZString fUserName;

		#endregion

		#region Password

		[MaxLength(255)]
		[Password]
		public ZString Password
		{
			get { return fPassword; }
			set
			{
				if (fPassword != value)
				{
					CheckMaximumLength(PasswordInfo, value);
					fPassword = value;
					if (!IsValidationSuspended)
					{
						ValidatePassword();
					}
					PasswordInfo.RefreshBinding();

					ClearConfirmPassword();
				}
			}
		}

		void ClearConfirmPassword()
		{
			using (GetValidationSuspender())
			{
				ConfirmPassword = "";
			}
		}

		public ZPropertyInfo PasswordInfo
		{
			get { return GetZPropertyInfo(Schema.Password); }
		}

		ZString fPassword;

		#endregion

		#endregion

		#region Non-Persistent Properties

		public string ServerName
		{
			get
			{
				int separatorIndex = UserName.IndexOf('\\');
				return (separatorIndex > -1) ? UserName.Left(separatorIndex) : ZString.Empty;
			}
		}

		public string UserNameWithoutServer
		{
			get
			{
				int separatorIndex = UserName.IndexOf('\\');
				return (separatorIndex > -1) ? UserName.SubstringSafe(separatorIndex + 1) : UserName;
			}
		}

		[BusinessObjectTestExclude] // not serialized
		[MaxLength(255)]
		[Password]
		public ZString ConfirmPassword
		{
			get { return fConfirmPassword; }
			set
			{
				if (fConfirmPassword != value)
				{
					CheckMaximumLength(ConfirmPasswordInfo, value);
					fConfirmPassword = value;
					if (!IsValidationSuspended)
					{
						ValidateConfirmPassword();
					}
					ConfirmPasswordInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ConfirmPasswordInfo
		{
			get { return GetZPropertyInfo(nameof(ConfirmPassword)); }
		}

		ZString fConfirmPassword;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.UserName, UserName);
			writer.WriteElementString(Schema.Password, Password);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			UserName = reader.ReadElementString(Schema.UserName);
			Password = reader.ReadElementString(Schema.Password);
			ConfirmPassword = Password;
		}

		#endregion

		#region GetClone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ServerUsernamePasswordConfiguration();
		}

		#endregion

		#region Validation

		public void ValidateUserName()
		{
			UserNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(UserNameInfo);
		}

		public void ValidatePassword()
		{
			PasswordInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PasswordInfo);
		}

		public void ValidateConfirmPassword()
		{
			ConfirmPasswordInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ConfirmPasswordInfo);

			if (!PasswordInfo.HasErrors() && !ConfirmPasswordInfo.HasErrors())
			{
				if (Password != ConfirmPassword)
				{
					ConfirmPasswordInfo.AddError(Res.GetString("112fa036-01ba-4683-accb-b6e43eaf6dc9", "The passwords you typed do not match. Type the same password into both text boxes."));
				}
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateUserName();
			ValidatePassword();
			ValidateConfirmPassword();
		}

		#endregion
	}
}
