//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiTrustedMessagingConfigValidation
//
//    This class should be used for overriding validation in AutoEdiTrustedMessagingConfigValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System;
using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.TrustedMessaging.Business
{
	public class EdiTrustedMessagingConfigValidation : AutoEdiTrustedMessagingConfigValidation
	{
		public EdiTrustedMessagingConfigValidation(AutoEdiTrustedMessagingConfig parent) : base(parent)
		{
		}

		protected override void CheckETM_Product()
		{
			base.CheckETM_Product();

			if (!Parent.IsInDatabase || Parent.ETM_ProductInfo.HasChanges)
			{
				MandatoryValidation.CheckEntered(Parent.ETM_ProductInfo);
				ListValidation.ErrorIfInvalidCode(Parent.ETM_ProductInfo);
			}
		}

		protected override void CheckETM_CertificateType()
		{
			base.CheckETM_CertificateType();

			MandatoryValidation.CheckEntered(Parent.ETM_CertificateTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.ETM_CertificateTypeInfo);

			if (Parent.ETM_CertificateType != CertificateTypeList.Codes.TrustedSystemCertificate)
			{
				if (!Parent.ETM_Product.IsEmpty && (!Parent.IsInDatabase || Parent.ETM_ProductInfo.HasChanges || Parent.ETM_CertificateTypeInfo.HasChanges))
				{
					var query = new ZQuery(EdiTrustedMessagingConfigSchema.ETM_Product, Parent.ETM_Product);
					query.AddToFilter(EdiTrustedMessagingConfigSchema.ETM_CertificateType, Parent.ETM_CertificateType);
					query.AddToFilter(EdiTrustedMessagingConfigSchema.PK, SQLComparisonOperator.NotEqual, Parent.PK);

					if (Parent.Factory.ExistsInDatabase(EdiTrustedMessagingConfigSchema.Constants.TableName, query))
					{
						Parent.ETM_CertificateTypeInfo.AddError("Another configuration with the same Product/System exists. There can only be one configuration valid at a time.");
					}
				}
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCertificateData();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		void ValidateCertificateData()
		{
			Parent.ClearRowNotifications();
			if (Parent.ETM_CertificateType != CertificateTypeList.Codes.TrustedSystemCertificate)
			{
				if (Parent.ETM_CertificateData.IsEmpty)
				{
					Parent.AddRowError("Please enter a Certificate.");
				}
				else
				{
					try
					{
						var cert = ((EdiTrustedMessagingConfig)Parent).GetCertificate();
						if (cert == null)
						{
							Parent.AddRowError("Invalid Certificate.");
						}
						else if (Parent.ETM_CertificateType == CertificateTypeList.Codes.CentralSystemCertificate && cert.GetRSAPrivateKey() == null)
						{
							Parent.AddRowError("Certificate does not contain a private key.");
						}
						else if (Parent.ETM_CertificateType == CertificateTypeList.Codes.PreDeploymentCertificate && cert.GetRSAPublicKey() == null)
						{
							Parent.AddRowError("Certificate does not contain a public key.");
						}
					}
					catch (Exception)
					{
						Parent.AddRowError("Invalid Certificate / Invalid Certificate Password.");
					}
				}
			}
		}
	}
}
