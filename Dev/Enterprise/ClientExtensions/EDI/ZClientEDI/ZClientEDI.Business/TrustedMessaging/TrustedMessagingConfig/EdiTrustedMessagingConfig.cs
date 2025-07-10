using System;
using System.Data;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.TrustedMessaging.Business
{
	[CodeProperty(Schema.ETM_Product)]
	public class EdiTrustedMessagingConfig : AutoEdiTrustedMessagingConfig
	{
		public EdiTrustedMessagingConfig(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ETM_CertificateType = CertificateTypeList.Codes.TrustedSystemCertificate;
		}

		[List("Lookups.ProductTypeList")]
		public override ZString ETM_Product
		{
			get => base.ETM_Product;
			set => base.ETM_Product = value;
		}

		[List("Lookups.CertificateTypeList")]
		public override ZString ETM_CertificateType
		{
			get => base.ETM_CertificateType;
			set => base.ETM_CertificateType = value;
		}

		public static EdiTrustedMessagingConfig GetGlobalCertificateConfig(BusinessObjectFactory factory, ZString product, ZString certificateType)
		{
			var query = new ZQuery(EdiTrustedMessagingConfigSchema.ETM_Product, product);
			query.AddToFilter(EdiTrustedMessagingConfigSchema.ETM_CertificateType, certificateType);
			return factory.LoadTop1<EdiTrustedMessagingConfig>(query);
		}

		public bool TryGenerateCertificate(string subjectName, out string errorMessage)
		{
			errorMessage = null;
			var soapTemplate = EDIDataRegistry.Instance.MyAccountCertificateAuthoritySoapRequestTemplate.Value;
			var userAccount = EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccount.Value;
			var userAccountPassword = EDIDataRegistry.Instance.MyAccountCertificateAuthorityUserAccountPassword.Value;

			if (string.IsNullOrWhiteSpace(subjectName))
			{
				errorMessage = "Invalid Subject Name";
				return false;
			}

			if (string.IsNullOrWhiteSpace(soapTemplate) || string.IsNullOrWhiteSpace(userAccount) || string.IsNullOrWhiteSpace(userAccountPassword))
			{
				errorMessage = "Invalid Registry Settings";
				return false;
			}

			using (var rsa = GetRASProvider())
			{
				if (GetCertRequest().TrySubmitSafe(soapTemplate, subjectName, rsa.ExportCspBlob(false),
					new NetworkCredential(userAccount, userAccountPassword), null, out var output))
				{
					var password = ZGuid.NewZGuid().ToString();
					var pfxBytes = new X509Certificate2(Convert.FromBase64String(output), "", X509KeyStorageFlags.MachineKeySet)
						.CopyWithPrivateKey(rsa).Export(X509ContentType.Pfx, password);

					ETM_CertificatePassword = password;
					ETM_CertificateData = pfxBytes;

					return GetCertificate().GetRSAPrivateKey() != null;
				}
				else
				{
					errorMessage = output;
				}
			}

			return false;
		}

		public X509Certificate2 GetCertificate() =>
			!ETM_CertificateData.IsEmpty ? new X509Certificate2(ETM_CertificateData, ETM_CertificatePassword, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable) : null;

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override ZString HumanReadableNameCore => "Trusted Messaging Configuration";

		protected virtual IEDICertRequest GetCertRequest() => new EDICertRequest();

		protected virtual RSACryptoServiceProvider GetRASProvider() => new RSACryptoServiceProvider(4096);
	}
}
