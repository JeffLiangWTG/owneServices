using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class AWSPrivateCA : RegistryBusinessObjectTemplate
	{
		#region Schema

		[SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
		public static class Schema
		{
			public const string IssuingCA = "IssuingCA";
			public const string Arn = "Arn";
			public const string IsEnabled = "IsEnabled";
			public const string AccessKey = "AccessKey";
			public const string SecretKey = "SecretKey";
		}

		#endregion

		#region CAList

		public CARootCodeDescriptionList CAList => caList ?? (caList = new CARootCodeDescriptionList());
		CARootCodeDescriptionList caList;

		#endregion

		#region IssuingCA
		[List("CAList")]
		public ZString IssuingCA
		{
			get => issuingCA;
			set
			{
				SetNonPersistentPropertyValue(IssuingCAInfo, ref issuingCA, value);
				if (!IsValidationSuspended)
				{
					ValidateIssuingCA();
				}
			}
		}

		public ZPropertyInfo IssuingCAInfo => GetZPropertyInfo(Schema.IssuingCA);

		ZString issuingCA;

		#endregion

		#region Arn

		public ZString Arn
		{
			get => arn;
			set
			{
				SetNonPersistentPropertyValue(ArnInfo, ref arn, value);
				if (!IsValidationSuspended)
				{
					ValidateArn();
				}
			}
		}

		public ZPropertyInfo ArnInfo => GetZPropertyInfo(Schema.Arn);

		ZString arn;

		#endregion

		#region AccessKey
		[Password]
		public ZString AccessKey
		{
			get => accessKey;
			set
			{
				SetNonPersistentPropertyValue(AccessKeyInfo, ref accessKey, value);
				if (!IsValidationSuspended)
				{
					ValidateAccessKey();
				}
			}
		}

		public ZPropertyInfo AccessKeyInfo => GetZPropertyInfo(Schema.AccessKey);

		ZString accessKey;

		#endregion

		#region SecretKey
		[Password]
		public ZString SecretKey
		{
			get => secretKey;
			set
			{
				SetNonPersistentPropertyValue(SecretKeyInfo, ref secretKey, value);
				if (!IsValidationSuspended)
				{
					ValidateSecretKey();
				}
			}
		}

		public ZPropertyInfo SecretKeyInfo => GetZPropertyInfo(Schema.SecretKey);

		ZString secretKey;

		#endregion

		#region IsEnabled

		public ZBool IsEnabled
		{
			get => isEnabled;
			set => SetNonPersistentPropertyValue(IsEnabledInfo, ref isEnabled, value);
		}

		public ZPropertyInfo IsEnabledInfo => GetZPropertyInfo(Schema.IsEnabled);

		ZBool isEnabled;

		#endregion

		#region clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AWSPrivateCA()
			{
				IssuingCA = IssuingCA,
				Arn = Arn,
				AccessKey = AccessKey,
				SecretKey = SecretKey,
				IsEnabled = IsEnabled
			};
		}

		#endregion

		#region Xml Serialisation

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			IssuingCA = reader.ReadElementString(Schema.IssuingCA);
			Arn = reader.ReadElementString(Schema.Arn);
			AccessKey = reader.ReadElementString(Schema.AccessKey);
			SecretKey = reader.ReadElementString(Schema.SecretKey);
			IsEnabled = new ZBool(reader.ReadElementString(Schema.IsEnabled));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.IssuingCA, IssuingCA);
			writer.WriteElementString(Schema.Arn, Arn);
			writer.WriteElementString(Schema.AccessKey, AccessKey);
			writer.WriteElementString(Schema.SecretKey, SecretKey);
			writer.WriteElementString(Schema.IsEnabled, IsEnabled.ToString());
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			if (!IsValidationSuspended)
			{
				ValidateIssuingCA();
				ValidateArn();
				ValidateAccessKey();
				ValidateSecretKey();
			}
		}

		public void ValidateIssuingCA()
		{
			IssuingCAInfo.ClearAllNotifications();
			if (IsEnabled)
			{
				MandatoryValidation.CheckEntered(IssuingCAInfo);
				ListValidation.ErrorIfInvalidCode(IssuingCAInfo);
				if (!IssuingCA.IsEmpty)
				{
					PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(IssuingCAInfo);
				}
			}
		}

		public void ValidateArn()
		{
			ArnInfo.ClearAllNotifications();
			if (IsEnabled)
			{
				MandatoryValidation.CheckEntered(ArnInfo);
			}
		}

		public void ValidateAccessKey()
		{
			AccessKeyInfo.ClearAllNotifications();
			if (IsEnabled)
			{
				MandatoryValidation.CheckEntered(AccessKeyInfo);
			}
		}

		public void ValidateSecretKey()
		{
			SecretKeyInfo.ClearAllNotifications();
			if (IsEnabled)
			{
				MandatoryValidation.CheckEntered(SecretKeyInfo);
			}
		}

		#endregion
	}
}
