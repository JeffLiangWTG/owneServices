using System;
using System.Security.Cryptography;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[RegistryEditor("Enterprise.DocumentEngine.GUI.Registry.DigitalSignatureRegistryItemEditor, Enterprise.DocumentEngine.GUI")]
	public class DigitalSignatureRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DigitalSignatureRegistry>
	{
		protected override void ValidateBeforeRegistryFormSaveCore(IRegistryItem registryItem, DigitalSignatureRegistry proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateBeforeRegistryFormSaveCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			var invalidCertificateErrorMessage = string.Empty;
			try
			{
				invalidCertificateErrorMessage = proposedValue.DigitalSignature.Length == 0
					? ResString.GetMultilingualString("5bdbc532-d4a4-45af-a88d-9c7c7e6ae259", "The certificate is empty. Please select a valid certificate.")
					: CertificateValidationHelper.GetInvalidCertificateChainErrors(proposedValue.DigitalSignature, proposedValue.CertificatePassword);

				if (!string.IsNullOrEmpty(invalidCertificateErrorMessage))
				{
					throw new CryptographicException();
				}
			}
			catch (Exception ex) when (ex is CryptographicException || ex is ArgumentException)
			{
				string errorMessage;

				if (ex.HResult == InvalidPasswordErrorCode)
				{
					errorMessage = ResString.GetMultilingualString("c12afa99-d6b6-4733-9ae6-2b4f3c812baf", "The password provided for the certificate '{0}' is incorrect.", proposedValue.CertificateFileName);
				}
				else if (!string.IsNullOrEmpty(invalidCertificateErrorMessage))
				{
					errorMessage = ResString.GetMultilingualString("38044733-1A6E-477E-869D-D2EE3EEFA5D7", "The certificate '{0}' could not be read because of the following reason(s):\r\n{1}", proposedValue.CertificateFileName, invalidCertificateErrorMessage);
				}
				else
				{
					errorMessage = ResString.GetMultilingualString("bd342a3d-6724-432f-9916-efb913bd35e4", "The certificate '{0}' could not be read. Please make sure it is valid.", proposedValue.CertificateFileName);
				}

				throw new RegistryValidationException(errorMessage);
			}
		}

		const int InvalidPasswordErrorCode = unchecked((int)0x80070056);
	}
}
