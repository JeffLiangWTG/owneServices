using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.SystemToSystemTrust;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class SystemToSystemTrustInfo : RegistryBusinessObjectTemplate, ISystemToSystemTrustInfo
	{
		record ProtectedStringWithFallback
		{
			string NameForClearValue { get; }
			string NameForEncryptedValue { get; }
			string valueInClear;
			string valueEncrypted;

			internal ProtectedStringWithFallback(string nameForClearValue, string nameForEncryptedValue)
			{
				NameForClearValue = nameForClearValue;
				NameForEncryptedValue = nameForEncryptedValue;
			}

			internal string GetValue(Lazy<IDataProtectorService> lazyDataProtectorService)
			{
				if (!string.IsNullOrEmpty(valueEncrypted))
				{
					try
					{
						return lazyDataProtectorService.Value.Decrypt(valueEncrypted);
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						ErrorReporter.ReportOnce($"Decryption failed for {NameForClearValue} value.", e);
					}
				}
				return valueInClear;
			}

			internal void SetValue(Lazy<IDataProtectorService> lazyDataProtectorService, string value)
			{
				valueInClear = value;
				if (!string.IsNullOrEmpty(value))
				{
					try
					{
						valueEncrypted = lazyDataProtectorService.Value.Encrypt(value);
					}
					catch (Exception e) when (!e.IsCriticalException())
					{
						ErrorReporter.ReportOnce($"Encryption failed for {NameForClearValue} value.", e);
						valueEncrypted = null;
					}
				}
				else
				{
					valueEncrypted = null;
				}
			}

			internal void ReadElement(XmlReaderWrapper reader)
			{
				valueInClear = reader.ReadElementString(NameForClearValue);
				valueEncrypted = reader.ReadElementString(NameForEncryptedValue);
			}

			internal void WriteElement(XmlWriter writer)
			{
				writer.WriteElementString(NameForClearValue, valueInClear);
				writer.WriteElementString(NameForEncryptedValue, valueEncrypted);
			}

			internal bool IsEmpty() => string.IsNullOrEmpty(valueInClear) && string.IsNullOrEmpty(valueEncrypted);
		}
		#region Schema

		public static class Schema
		{
			public const string PrivateKeyInClear = "PrivateKey";
			public const string PrivateKeyEncrypted = "PrivateKeyEncrypted";
			public const string LegacyPrivateKeyInClear = "LegacyPrivateKey";
			public const string LegacyPrivateKeyEncrypted = "LegacyPrivateKeyEncrypted";
			public const string RolloverPrivateKeyInClear = "RolloverPrivateKey";
			public const string RolloverPrivateKeyEncrypted = "RolloverPrivateKeyEncrypted";
			public const string Certificate = "Certificate";
			public const string LegacyCertificate = "LegacyCertificate";
			public const string CertificateSigningRequest = "CSR";
			public const string ClientId = "ClientId";
			public const string TenantId = "TenantId";
			public const string OperationId = "OperationId";
		}

		#endregion

		#region Fields

		ProtectedStringWithFallback privateKey;
		ProtectedStringWithFallback legacyPrivateKey;
		ProtectedStringWithFallback rolloverPrivateKey;

		readonly Lazy<IDataProtectorService> lazyDataProtectorService;

		#endregion

		#region Constructors

		public SystemToSystemTrustInfo() : this(new DataProtectorServiceFactory())
		{
		}

		internal SystemToSystemTrustInfo(IDataProtectorServiceFactory factory) : this(new Lazy<IDataProtectorService>(factory.Create))
		{
		}

		SystemToSystemTrustInfo(Lazy<IDataProtectorService> lazyDataProtectorService)
		{
			this.lazyDataProtectorService = lazyDataProtectorService;
			privateKey = new ProtectedStringWithFallback(Schema.PrivateKeyInClear, Schema.PrivateKeyEncrypted);
			legacyPrivateKey = new ProtectedStringWithFallback(Schema.LegacyPrivateKeyInClear, Schema.LegacyPrivateKeyEncrypted);
			rolloverPrivateKey = new ProtectedStringWithFallback(Schema.RolloverPrivateKeyInClear, Schema.RolloverPrivateKeyEncrypted);
		}

		#endregion

		#region Properties

		[Obsolete("Only the S2ST library and the token service should access it")]
		public string PrivateKey
		{
			get => privateKey.GetValue(lazyDataProtectorService);
			set => privateKey.SetValue(lazyDataProtectorService, value);
		}

		[Obsolete("Only the S2ST library and the token service should access it")]
		public string LegacyPrivateKey
		{
			get => legacyPrivateKey.GetValue(lazyDataProtectorService);
			set => legacyPrivateKey.SetValue(lazyDataProtectorService, value);
		}

		[Obsolete("Only the S2ST library and the token service should access it")]
		public string RolloverPrivateKey
		{
			get => rolloverPrivateKey.GetValue(lazyDataProtectorService);
			set => rolloverPrivateKey.SetValue(lazyDataProtectorService, value);
		}

		public ZBlob Certificate { get; set; }
		public ZBlob LegacyCertificate { get; set; }
		public string CertificateSigningRequest { get; set; }
		public string ClientId { get; set; }
		public string TenantId { get; set; }
		public string OperationId { get; set; }

		public string CertificateThumbprint => X509Certificate?.Thumbprint;

		public ZDateTime CertificateValidFrom => new ZDateTime(X509Certificate?.NotBefore);

		public ZDateTime CertificateValidTo => new ZDateTime(X509Certificate?.NotAfter);

		X509Certificate2 X509Certificate
		{
			get
			{
				if (Certificate.IsEmpty)
				{
					return null;
				}
				try
				{
					return new X509Certificate2(Certificate);
				}
				catch (Exception ex) when(!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce("SystemToSystemTrustInfo_X509Certificate", "Error when converting certificate data to X509Certificate2", ex);
					return null;
				}
			}
		}

		#endregion

		#region interface implementation

		byte[] ITokenConfigInfo.CertificateBytes => (this as ISystemToSystemTrustInfo).CertificateBytes;
		byte[] ISystemToSystemTrustInfo.CertificateBytes
		{
			get => Certificate;
			set => Certificate = value;
		}
		byte[] ISystemToSystemTrustInfo.LegacyCertificateBytes
		{
			get => LegacyCertificate;
			set => LegacyCertificate = value;
		}
		public bool HasPrivateKey() => !privateKey.IsEmpty();

		#endregion

		#region Serialization

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Certificate = Convert.FromBase64String(reader.ReadElementString(Schema.Certificate));
			privateKey.ReadElement(reader);
			legacyPrivateKey.ReadElement(reader);
			rolloverPrivateKey.ReadElement(reader);
			LegacyCertificate = Convert.FromBase64String(reader.ReadElementString(Schema.LegacyCertificate));
			CertificateSigningRequest = reader.ReadElementString(Schema.CertificateSigningRequest);
			ClientId = reader.ReadElementString(Schema.ClientId);
			TenantId = reader.ReadElementString(Schema.TenantId);
			OperationId = reader.ReadElementString(Schema.OperationId);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.Certificate, Convert.ToBase64String(Certificate));
			privateKey.WriteElement(writer);
			legacyPrivateKey.WriteElement(writer);
			rolloverPrivateKey.WriteElement(writer);
			writer.WriteElementString(Schema.LegacyCertificate, Convert.ToBase64String(LegacyCertificate));
			writer.WriteElementString(Schema.CertificateSigningRequest, CertificateSigningRequest);
			writer.WriteElementString(Schema.ClientId, ClientId);
			writer.WriteElementString(Schema.TenantId, TenantId);
			writer.WriteElementString(Schema.OperationId, OperationId);
			base.WriteElements(writer);
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SystemToSystemTrustInfo(lazyDataProtectorService)
			{
				Certificate = Certificate,
				LegacyCertificate = LegacyCertificate,
				privateKey = privateKey,
				legacyPrivateKey = legacyPrivateKey,
				rolloverPrivateKey = rolloverPrivateKey,
				CertificateSigningRequest = CertificateSigningRequest,
				ClientId = ClientId,
				TenantId = TenantId,
				OperationId = OperationId
			};
		}

		#endregion
	}
}
