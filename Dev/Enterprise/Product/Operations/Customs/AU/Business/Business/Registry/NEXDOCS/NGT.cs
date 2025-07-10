using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.AU.Declaration.Business.XmlSerializers")]
	public class NGT : RegistryBusinessObjectTemplate
	{
		public static class Schema
		{
			public const string Password = "Password";
			public const int PasswordMaxLength = 32;
		}

		public ZString PasswordStatus => Password.IsEmpty ? ZString.Empty : (ZString)Core.Constants.PasswordOK;

		[MaxLength(NGT.Schema.PasswordMaxLength)]
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

		public void ValidatePassword()
		{
			PasswordInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(PasswordInfo);
		}

		public ZPropertyInfo PasswordInfo => GetZPropertyInfo(NGT.Schema.Password);

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new NGT();
		}

		TwoWayEncoder TwoWayEncoder => twoWayEncoder ?? (twoWayEncoder = TwoWayEncoder.NewWithStandardInitialisationVector());
		TwoWayEncoder twoWayEncoder;

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			var passwordStr = reader.ReadElementString(Schema.Password);
			Password = string.IsNullOrEmpty(passwordStr) ? passwordStr : TwoWayEncoder.Decrypt(passwordStr);
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.Password, Password.IsEmpty ? Password : (ZString)TwoWayEncoder.Encrypt(Password));
		}
	}
}
