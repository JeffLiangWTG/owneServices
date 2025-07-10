using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Amazon.ACMPCA;
using CargoWise.Common;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.Client.EDI.Registry.Business;
using WTG.AWSCertificateIntegration;

namespace Enterprise.Client.EDI.ServiceTasks.EDICertProcessing
{
	public class AwsCertManagementService
	{
		public AwsCertManagementService()
		{
		}

		internal AwsCertManagementService(Dictionary<string, (AwsPcaManager AWSPcaManager, string ARN)> awsCATupleDictionary)
		{
			this.awsCATupleDictionary = awsCATupleDictionary;
		}

		public X509Certificate2 IssueCertificate(EdiIdentityCertificate certificateRequest)
		{
			if (!certificateRequest.ICE_CertificateData.IsEmpty)
			{
				return new X509Certificate2(certificateRequest.ICE_CertificateData);
			}

			var certificate = IssueCertificate(certificateRequest.ICE_CARoot, certificateRequest.ICE_CertificateSigningRequest);
			return certificate;
		}

		public X509Certificate2 IssueCertificate(string ca, string csr, int validDays = 365)
		{
			var awsTuple = GetAwsPacManagerAndARN(ca);
			var certificate = DoAwsOperationWithRetry(() => awsTuple.AWSPcaManager.IssueCertificate(awsTuple.ARN, csr, validDays), "Issue Certificate Error");

			return certificate;
		}

		public bool RevokeCertificate(EdiIdentityCertificate certificateRequest)
		{
			if (certificateRequest.ICE_IsCertificateRevoked)
			{
				return true;
			}

			var awsTuple = GetAwsPacManagerAndARN(certificateRequest.ICE_CARoot);

			var x509Certificate2 = new X509Certificate2(certificateRequest.ICE_CertificateData);

			var certificateResponse = DoAwsOperationWithRetry(() => awsTuple.AWSPcaManager.RevokeCertificate(awsTuple.ARN, x509Certificate2.SerialNumber, RevocationReason.KEY_COMPROMISE), "Revoke Certificate Error");

			return certificateResponse.HttpStatusCode == HttpStatusCode.OK;
		}

		T DoAwsOperationWithRetry<T>(Func<T> awsOperation, string errorMessage)
		{
			var retryCount = 3;
			AwsCertManagementServiceException exception = null;
			while (--retryCount >= 0)
			{
				try
				{
					return awsOperation.Invoke();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					if (exception == null)
					{
						exception = new AwsCertManagementServiceException(errorMessage, ex);
					}
					else
					{
						exception.InnerExceptions.Add(ex);
					}

					Thread.Sleep(1000);
				}
			}

			throw exception;
		}

		(AwsPcaManager AWSPcaManager, string ARN) GetAwsPacManagerAndARN(string ca)
		{
			if (!awsCATupleDictionary.ContainsKey(ca))
			{
				var awsPrivateCA = EDIDataRegistry.Instance.AWSPrivateCAListManager.Value.Cast<AWSPrivateCA>().FirstOrDefault(x => x.IssuingCA == ca && x.IsEnabled)
					?? throw new AwsCertManagementServiceException($"Unable to find the corresponding CA: {ca}");
				var awsTuple = (new AwsPcaManager(awsPrivateCA.AccessKey, awsPrivateCA.SecretKey), awsPrivateCA.Arn);
				awsCATupleDictionary.Add(ca, awsTuple);
				return awsTuple;
			}

			return awsCATupleDictionary.GetValueSafe(ca);
		}

		readonly Dictionary<string, (AwsPcaManager AWSPcaManager, string ARN)> awsCATupleDictionary = new Dictionary<string, (AwsPcaManager AWSPcaManager, string ARN)>();
	}
}
