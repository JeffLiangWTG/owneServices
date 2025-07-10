using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public sealed class DigitalSignatureRegistry : RegistryBusinessObjectTemplate
	{
		ZString certificateFileName;
		ZString certificatePassword;
		ZString signatureDetailsName;
		ZString signatureDetailsLocation;
		ZString signatureDetailsEmail;

		#region Certificate

		public ZString CertificateFileName
		{
			get { return certificateFileName; }
			set { SetNonPersistentPropertyValue(CertificateFileNameInfo, ref certificateFileName, value); }
		}

		public ZPropertyInfo CertificateFileNameInfo => GetZPropertyInfo(nameof(CertificateFileName));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public byte[] DigitalSignature
		{
			get
			{
				if (data == null)
				{
					return Array.Empty<byte>();
				}
				return data;
			}
			set { data = value; }
		}
		byte[] data;

		[Password]
		public ZString CertificatePassword
		{
			get { return certificatePassword; }
			set { SetNonPersistentPropertyValue(CertificatePasswordInfo, ref certificatePassword, value); }
		}

		public ZPropertyInfo CertificatePasswordInfo => GetZPropertyInfo(nameof(CertificatePassword));

		#endregion

		#region Complementary Informations

		public ZString SignatureDetailsName
		{
			get { return signatureDetailsName; }
			set
			{
				SetNonPersistentPropertyValue(SignatureDetailsNameInfo, ref signatureDetailsName, value);
				if (!IsValidationSuspended)
				{
					ValidationSignatureDetailsName();
				}
				SignatureDetailsNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SignatureDetailsNameInfo => GetZPropertyInfo(nameof(SignatureDetailsName));

		public ZString SignatureDetailsLocation
		{
			get { return signatureDetailsLocation; }
			set { SetNonPersistentPropertyValue(SignatureDetailsLocationInfo, ref signatureDetailsLocation, value); }
		}

		public ZPropertyInfo SignatureDetailsLocationInfo => GetZPropertyInfo(nameof(SignatureDetailsLocation));

		public ZString SignatureDetailsEmail
		{
			get { return signatureDetailsEmail; }
			set
			{
				SetNonPersistentPropertyValue(SignatureDetailsEmailInfo, ref signatureDetailsEmail, value);
				if (!IsValidationSuspended)
				{
					ValidationSignatureDetailsEmail();
				}
				SignatureDetailsEmailInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SignatureDetailsEmailInfo => GetZPropertyInfo(nameof(SignatureDetailsEmail));

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new DigitalSignatureRegistry();
			using (clone.GetValidationSuspender())
			{
				clone.CertificateFileName = certificateFileName;
				clone.CertificatePassword = certificatePassword;
				clone.DigitalSignature = data;
				clone.SignatureDetailsName = signatureDetailsName;
				clone.SignatureDetailsLocation = signatureDetailsLocation;
				clone.SignatureDetailsEmail = signatureDetailsEmail;
			}
			return clone;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			var encoder = new TwoWayEncoder(new Guid(InitialisationVector));

			WriteCertificateDataElement(writer, encoder);
			writer.WriteElementString("CertificateFileName", CertificateFileName);
			writer.WriteElementString("CertificatePassword", encoder.Encrypt(CertificatePassword));
			writer.WriteElementString("SignatureDetailsName", SignatureDetailsName);
			writer.WriteElementString("SignatureDetailsLocation", SignatureDetailsLocation);
			writer.WriteElementString("SignatureDetailsEmail", SignatureDetailsEmail);
		}

		void WriteCertificateDataElement(XmlWriter writer, TwoWayEncoder encoder)
		{
			writer.WriteStartElement("DigitalSignature");
			var encrypted = encoder.Encrypt(DigitalSignature);
			if (DigitalSignature.Length > 0)
			{
				writer.WriteBase64(encrypted, 0, encrypted.Length);
			}
			writer.WriteEndElement();
		}
		const int bufferSize = 1000;

		const string InitialisationVector = "58179ea0-4dd4-40e5-a43d-fbd123e84f32";

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			var encoder = new TwoWayEncoder(new Guid(InitialisationVector));
			DigitalSignature = encoder.Decrypt(ReadSignatureElement(reader));
			CertificateFileName = reader.ReadElementString("CertificateFileName");
			CertificatePassword = encoder.Decrypt(reader.ReadElementString("CertificatePassword"));
			SignatureDetailsName = reader.ReadElementString("SignatureDetailsName");
			SignatureDetailsLocation = reader.ReadElementString("SignatureDetailsLocation");
			SignatureDetailsEmail = reader.ReadElementString("SignatureDetailsEmail");
		}

		byte[] ReadSignatureElement(XmlReaderWrapper reader)
		{
			var result = Array.Empty<byte>();
			if (reader.Reader.Name == "DigitalSignature")
			{
				using (var output = new MemoryStream(bufferSize))
				{
					var buffer = new byte[bufferSize];
					int read;
					while ((read = reader.Reader.ReadElementContentAsBase64(buffer, 0, bufferSize)) > 0)
					{
						output.Write(buffer, 0, read);
					}

					result = output.ToArray();
				}
			}
			return result;
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidationSignatureDetailsName();
			ValidationSignatureDetailsEmail();
		}

		void ValidationSignatureDetailsName()
		{
			SignatureDetailsNameInfo.ClearAllNotifications();

			if (signatureDetailsName.Contains(".", StringComparison.OrdinalIgnoreCase))
			{
				SignatureDetailsNameInfo.AddError(Res.GetString("c2e575db-c83f-4f0e-bb21-6f285397253a", "Name can not contain the character {0}", "'.'"));
			}
			else if (string.IsNullOrEmpty(signatureDetailsName))
			{
				MandatoryValidation.CheckEntered(SignatureDetailsNameInfo);
			}
		}

		void ValidationSignatureDetailsEmail()
		{
			SignatureDetailsEmailInfo.ClearAllNotifications();

			if (!SignatureDetailsEmail.IsEmpty)
			{
				EmailAddressValidation.ValidateEmailAddress(SignatureDetailsEmailInfo);
			}
		}
	}
}
