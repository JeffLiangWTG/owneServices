using System;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class DocumentSigningServiceCredentialsConfiguration : DocumentSigningServiceCredentials
	{
		[List("EmptyProviderCodesList")]
		public override ZString ProviderCode { get; set; }

		public CodeDescriptionPairList EmptyProviderCodesList => new CodeDescriptionPairList();

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DocumentSigningServiceCredentialsConfiguration();
		}
	}

	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public abstract class DocumentSigningServiceCredentials : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string ClientID = "ClientID";
			public const string AccessKey = "AccessKey";
			public const string KeyID = "KeyID";
		}

		#endregion

		#region Properties

		[List("ProviderCodesList")]
		public abstract ZString ProviderCode { get; set; }

		string clientID;
		public ZString ClientID
		{
			get { return clientID; }
			set
			{
				if (clientID != value)
				{
					CheckMaximumLength(ClientIDInfo, value);
					clientID = value;
					if (!IsValidationSuspended)
					{
						ValidateClientID();
					}
					ClientIDInfo.RefreshBinding();
					HasChanges = true;
				}
			}
		}

		public ZPropertyInfo ClientIDInfo
		{
			get { return GetZPropertyInfo(Schema.ClientID); }
		}

		string accessKey;
		public ZString AccessKey
		{
			get { return accessKey; }
			set
			{
				if (accessKey != value)
				{
					CheckMaximumLength(AccessKeyInfo, value);
					accessKey = value;
					if (!IsValidationSuspended)
					{
						ValidateAccessKey();
					}
					AccessKeyInfo.RefreshBinding();
					HasChanges = true;
				}
			}
		}

		public ZPropertyInfo AccessKeyInfo
		{
			get { return GetZPropertyInfo(Schema.AccessKey); }
		}

		string keyID;
		public ZString KeyID
		{
			get { return keyID; }
			set
			{
				if (keyID != value)
				{
					CheckMaximumLength(KeyIDInfo, value);
					keyID = value;
					if (!IsValidationSuspended)
					{
						ValidateKeyID();
					}
					KeyIDInfo.RefreshBinding();
					HasChanges = true;
				}
			}
		}

		public ZPropertyInfo KeyIDInfo
		{
			get { return GetZPropertyInfo(Schema.KeyID); }
		}

		#endregion

		#region Validation

		public void ValidateClientID()
		{
			ClientIDInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ClientIDInfo);
		}

		public void ValidateAccessKey()
		{
			AccessKeyInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AccessKeyInfo);
		}

		public void ValidateKeyID()
		{
			KeyIDInfo.ClearAllNotifications();
			if (!SuspendValidationForKeyID_ForTestOnly)
			{
				MandatoryValidation.CheckEntered(KeyIDInfo);
			}
		}

		public bool SuspendValidationForKeyID_ForTestOnly { set; get; }

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateClientID();
			ValidateAccessKey();
			ValidateKeyID();
		}

		#endregion

		#region Xml Serialisation

		TwoWayEncoder Encoder => encoder ?? (encoder = TwoWayEncoder.NewWithStandardInitialisationVector());
		TwoWayEncoder encoder;

		public string Encode(string value)
		{
			return Encoder.Encrypt(value);
		}

		public string Decode(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return string.Empty;
			}

			Exception reportException = null;
			try
			{
				value = Encoder.Decrypt(value);
			}
			catch (CryptographicException ex) when (!ex.IsCriticalException())
			{
				reportException = ex;
			}
			catch (FormatException ex) when (!ex.IsCriticalException())
			{
				reportException = ex;
			}

			if (reportException != null)
			{
				ErrorReporter.ReportOnce("BadEncryptedRegistryValue", $"Cannot decrypt value of the encrypted registry", reportException);
				value = string.Empty;
			}

			return value;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.ClientID, ClientID);
			writer.WriteElementString(Schema.AccessKey, Encode(AccessKey));
			writer.WriteElementString(Schema.KeyID, Encode(KeyID));
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ClientID = reader.ReadElementString(Schema.ClientID);
			AccessKey = Decode(reader.ReadElementString(Schema.AccessKey));
			KeyID = Decode(reader.ReadElementString(Schema.KeyID));
		}

		#endregion
	}
}
