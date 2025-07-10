using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.CryptoUtilities;
using Enterprise.StabilityChecker;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CertificateChecker
	{
		public CertificateChecker(BusinessObjectFactory factory) : base()
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		public StabilityResultLevel CheckCompanyKey(out string message, bool allowNull)
		{
			StabilityResultLevel result = StabilityResultLevel.Healthy;
			message = string.Empty;

			try
			{
				using (var certificateManager = new CertificateManager(factory))
				{
					if (certificateManager.CompanyCertificate != null)
					{
						if (certificateManager.CompanyCertificate.ValidToDate < ZDateTime.Today)
						{
							message = string.Format("CMR Company Key File (Type 3 Certificate) has expired, expiry date is {0}",
								certificateManager.CompanyCertificate.ValidToDate.ToShortDateString());
							result = StabilityResultLevel.Critical;
						}
						else if (!certificateManager.IsType3Certificate(certificateManager.CompanyCertificate.IssuerName))
						{
							message = string.Format("CMR Company Key File (Type 3 Certificate) Issuer Name is invalid, should be {0} but is '{1}'",
								string.Join(" or ", certificateManager.ValidType3CertificateIssuerNames),
								certificateManager.CompanyCertificate.IssuerName);
							result = StabilityResultLevel.Critical;
						}
						else if (certificateManager.CompanyCertificate.ValidFromDate > ZDateTime.Today)
						{
							message = string.Format("CMR Company Key File (Type 3 Certificate) is not current yet, start date is {0}",
								certificateManager.CompanyCertificate.ValidFromDate.ToShortDateString());
							result = StabilityResultLevel.Warning;
						}
						else if (certificateManager.CompanyCertificate.ValidToDate.AddMonths(-1) < ZDateTime.Today)
						{
							message = string.Format("CMR Company Key File (Type 3 Certificate) will soon expire, expiry date is {0}",
								certificateManager.CompanyCertificate.ValidToDate.ToShortDateString());
							result = StabilityResultLevel.Warning;
						}
					}
					else if (!allowNull)
					{
						message = "No CMR Company Key File (Type 3 Certificate) is loaded in the registry";
						result = StabilityResultLevel.Critical;
					}
				}
			}
			catch (CryptoUtilitiesException e)
			{
				message = string.Format("An invalid CMR Company Key File (Type 3 Certificate) is loaded in the registry, error: {0}", e.Message);
				result = StabilityResultLevel.Critical;
			}

			return result;
		}

		public StabilityResultLevel CheckCustomsKey(out string message, bool allowNull)
		{
			StabilityResultLevel result = StabilityResultLevel.Healthy;
			message = string.Empty;

			using (var certificateManager = new CertificateManager(factory))
			{
				if (certificateManager.CustomsCertificate != null)
				{
					if (certificateManager.CustomsCertificate.ValidToDate < ZDateTime.Today)
					{
						message = string.Format("Australian Customs Key File (Certificate) has expired, expiry date is {0}",
							certificateManager.CustomsCertificate.ValidToDate.ToShortDateString());
						result = StabilityResultLevel.Critical;
					}
					else if (certificateManager.CustomsCertificate.IssuerName != "DigiCert Gatekeeper Device Issuing CA")
					{
						message = string.Format("Australian Customs Key File (Certificate) Issuer Name is invalid, should be 'DigiCert Gatekeeper Device Issuing CA' but is '{0}'",
							certificateManager.CustomsCertificate.IssuerName);
						result = StabilityResultLevel.Critical;
					}
					else if (certificateManager.CustomsCertificate.ValidFromDate > ZDateTime.Today)
					{
						message = string.Format("Australian Customs Key File (Certificate) is not current yet, start date is {0}",
							certificateManager.CustomsCertificate.ValidFromDate.ToShortDateString());
						result = StabilityResultLevel.Warning;
					}
					else if (certificateManager.CustomsCertificate.ValidToDate.AddMonths(-1) < ZDateTime.Today)
					{
						message = string.Format(CustomsKeyExpiryWarning, certificateManager.CustomsCertificate.ValidToDate.ToShortDateString());
						result = StabilityResultLevel.Warning;
					}
				}
				else if (!allowNull)
				{
					message = "No Australian Customs Key File (Certificate) is loaded in the registry";
					result = StabilityResultLevel.Critical;
				}
			}

			return result;
		}

		const string CustomsKeyExpiryWarning = @"The Australian Customs Key File (Certificate) will expire soon, the expiry date is {0}. 
Your system will be automatically updated at the time that Customs rolls over to the new certificate. This is an automated process, there is nothing you need to do.
Your customs messaging and functions will continue without incident after the expiry date of {0}.";
	}
}
