using System;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ZArchitecture.Core;
using FlexCel.Core;
using FlexCel.Pdf;
using static Enterprise.DocumentEngineCore.Registry.DocumentSigningRegistryConstants;

namespace Enterprise.DocumentEngine.DigitalSignature
{
	public static class PdfSignatureFactory
	{
		public static TPdfSignature NewPdfSignature(ZString signingOption)
		{
			return NewPdfSignature(signingOption, Guid.Empty);
		}

		public static TPdfSignature NewPdfSignature(ZString signingOption, string signerName, TPaperDimensions paperDimensions, ZGuid? branch = null)
		{
			return NewPdfSignature(signingOption, branch == null || branch.Value.IsEmpty ? Guid.Empty : branch.Value.ToGuid(), signerName, paperDimensions);
		}

		public static TPdfSignature NewPdfSignature(ZString signingOption, ZGuid branch)
		{
			return NewPdfSignature(signingOption, branch, (NoResString)"Signer",  new TPaperDimensions(TPaperSize.A4));
		}

		public static string DigitalSignatureField
		{
			get
			{
				return ResString.GetMultilingualString("3a6b44e1-ef2e-4995-931e-e9a57d7c4393",
				  @"{0} (WTG) has provided the feature to generate and sign this PDF document using the data verified by the application users and available in {1} at the time of signing. WTG or any of its employees shall not be held responsible for any discrepancies observed in the data contained in this document.",
					  Core.Constants.CompanyBrandingName, Core.Constants.ProductName);
			}
		}

		public static TPdfSignature NewPdfSignature(ZString signingOption, ZGuid branch, string signerName, TPaperDimensions paperDimensions)
		{
			TPdfSignature result = null;
			switch (signingOption)
			{
				case DocumentsSignBy.PFX:
					{
						if (!DocumentsDataRegistry.Instance.DigitalSignature.GetFallBackValueAtAllLevels(Guid.Empty, branch.IsEmpty ? Guid.Empty : branch.ToGuid(), Guid.Empty).DigitalSignature.IsNullOrEmpty())
						{
							var certificateInfo = DocumentsDataRegistry.Instance.DigitalSignature.GetFallBackValueAtAllLevels(Guid.Empty, branch.IsEmpty ? Guid.Empty : branch.ToGuid(), Guid.Empty);
							var certificate = new X509Certificate2(certificateInfo.DigitalSignature, certificateInfo.CertificatePassword);
							var signer = new CmsSigner(certificate);
							var signerFactory = new TBuiltInSignerFactory(signer);
							result = new TPdfSignature(signerFactory, certificateInfo.SignatureDetailsName.ToString(), DigitalSignatureField, certificateInfo.SignatureDetailsLocation.ToString(), certificateInfo.SignatureDetailsEmail.ToString());
							result.AllowedChanges = TPdfAllowedChanges.None;
						}
						break;
					}
				case PdfSigningOptionCodes.EmudhraV1:
					{
						var generator = new PdfVisibleSignatureGenerator(paperDimensions, signerName, 1, branch);
						result = generator.GenerateTPdfVisibleSignature(new PdfSignerFactory(new EMudhra.V1.EMudhraPdfSigner(GetEMudhraClient(branch))), reason: DigitalSignatureField);
						break;
					}
				case PdfSigningOptionCodes.DigitalSign:
					{
						var generator = new PdfVisibleSignatureGenerator(paperDimensions, signerName, 1);
						result = generator.GenerateTPdfVisibleSignature(new PdfSignerFactory(new DigitalSign.DigitalSignPdfSigner(GetDigitalSignClient(branch))), reason: DigitalSignatureField);
						break;
					}
				case PdfSigningOptionCodes.Placeholder:
					{
						var generator = new PdfVisibleSignatureGenerator(paperDimensions, signerName, 0, branch);
						result = generator.GenerateTPdfVisibleSignature(new PdfSignerFactory(new PlaceholderPdfSigner()), reason: DigitalSignatureField);
						break;
					}
				default:
					{
						throw new ArgumentOutOfRangeException(signingOption);
					}
			}
			return result;
		}

		internal static string GetSignatureDate(ZDateTime date) => date.ToISO8601ShortDateString();

		public static IPdfBatchSigner NewPdfBatchSigner(ZString signingOption, ZGuid branch)
		{
			IPdfBatchSigner result = null;
			switch (signingOption)
			{
				case PdfSigningOptionCodes.EmudhraV1:
				{
					result = new EMudhra.V1.EMudhraBatchSigner(GetEMudhraClient(branch));
					break;
				}
				case PdfSigningOptionCodes.DigitalSign:
				{
					result = new DigitalSign.DigitalSignBatchSigner(GetDigitalSignClient(branch));
					break;
				}
				default:
				{
					throw new ArgumentOutOfRangeException(signingOption);
				}
			}
			return result;
		}

		static EMudhra.V1.EMudhraClient GetEMudhraClient(ZGuid branch)
		{
			var partnerCredentials = DocumentsDataRegistry.Instance.DocumentSigningServicePartnerCredentials.Value;
			var registry = GetDocumentSigningServiceCredentialsConfiguration(branch);
			return new EMudhra.V1.EMudhraClient()
			{
				WebApiUri = GetCloudSigningServiceProviderAPIEndpoint(branch),
				WebApiUrlSource = DocumentsDataRegistry.Instance.CloudSigningServiceProviderAPIEndpoint.GetLocation(),
				AccessKey = registry.AccessKey,
				ClientID = registry.ClientID,
				KeyID = registry.KeyID,
				PartnerID = partnerCredentials.PartnerID,
				PartnerAccessKey = partnerCredentials.PartnerAccessKey,
			};
		}

		static DigitalSign.DigitalSignClient GetDigitalSignClient(ZGuid branch)
		{
			var registry = GetDocumentSigningServiceCredentialsConfiguration(branch);
			var accessToken = GetDocumentSigningServiceAccessToken();
			var endpointUrl = GetCloudSigningServiceProviderAPIEndpoint(branch);

			return new DigitalSign.DigitalSignClient()
			{
				AuthorizerTotpID = registry.AccessKey,
				AuthorizerTotpSecretKey = registry.KeyID,
				AccessToken = accessToken,
				EndpointUrl = endpointUrl
			};
		}

		public static DocumentSigningServiceCredentialsWithProviderConfiguration GetDocumentSigningServiceCredentialsConfiguration(ZGuid branch)
		{
			return DocumentsDataRegistry.Instance.DocumentSigningServiceCredentials.GetFallBackValueAtAllLevels(Guid.Empty, branch.IsEmpty ? Guid.Empty : branch.ToGuid(), Guid.Empty);
		}

		public static string GetDocumentSigningServiceAccessToken()
		{
			return DocumentsDataRegistry.Instance.DocumentSigningServicePartnerCredentialsAccessToken.Value.KeyID;
		}

		public static string GetCloudSigningServiceProviderAPIEndpoint(ZGuid branch)
		{
			return DocumentsDataRegistry.Instance.CloudSigningServiceProviderAPIEndpoint.GetFallBackValueAtAllLevels(Guid.Empty, branch.IsEmpty ? Guid.Empty : branch.ToGuid(), Guid.Empty);
		}

		public class PdfSignerFactory : TPdfSignerFactory
		{
			public PdfSignerFactory(TPdfSigner signer) => Signer = signer;

			public override TPdfSigner CreateSigner() => Signer;

#if DEBUG
			public
#endif
			readonly TPdfSigner Signer;
		}
	}
}
