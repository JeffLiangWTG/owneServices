using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.JP.AFR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.JP.AFR.Business.XmlSerializers")]
	public class AFRReporterID : RegistryBusinessObjectTemplate
	{
		public static class Schema
		{
			public const string ReporterID = "ReporterID";
			public const int ReporterIDMaxLength = 8;
			public const string Password = "Password";
			public const int PasswordMaxLength = 15;
		}

		[MaxLength(Schema.ReporterIDMaxLength)]
		public ZString ReporterID
		{
			get => reporterID;
			set
			{
				var oldValue = ReporterID;
				SetNonPersistentPropertyValue(ReporterIDInfo, ref reporterID, value);
				if (!IsValidationSuspended)
				{
					if (oldValue != value)
					{
						ValidateReporterID();
						ValidatePassword();
					}
				}
			}
		}
		ZString reporterID;

		public ZPropertyInfo ReporterIDInfo => GetZPropertyInfo(Schema.ReporterID);

		public void ValidateReporterID()
		{
			ReporterIDInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(ReporterIDInfo);
			if (!ReporterID.IsEmpty && !ReporterID.IsLettersAndNumbersOnlyOrEmpty)
			{
				ReporterIDInfo.AddError(Res.GetString("DDA3CE83-12AA-4176-B329-73EA953634D5", "AFR Reporter ID should only contain uppercase characters and numbers"));
			}
		}

		[MaxLength(Schema.PasswordMaxLength)]
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

		public void ValidatePassword()
		{
			PasswordInfo.ClearAllNotifications();
			if (!ReporterID.IsEmpty)
			{
				MandatoryValidation.CheckEntered(PasswordInfo);
			}
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			ReporterID = ZString.Empty;
			Password = ZString.Empty;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AFRReporterID();
		}

		TwoWayEncoder TwoWayEncoder => twoWayEncoder ?? (twoWayEncoder = TwoWayEncoder.NewWithStandardInitialisationVector());
		TwoWayEncoder twoWayEncoder;

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ReporterID = reader.ReadElementString(Schema.ReporterID);
			var passwordStr = reader.ReadElementString(Schema.Password);
			Password = string.IsNullOrEmpty(passwordStr) ? passwordStr : TwoWayEncoder.Decrypt(passwordStr);
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ReporterID, ReporterID);
			writer.WriteElementString(Schema.Password, Password.IsEmpty ? Password : (ZString)TwoWayEncoder.Encrypt(Password));
		}
	}
}
