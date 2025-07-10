using System.Collections.Generic;
using Enterprise.Accounting.ElectronicMessaging.Common.GlobalElectronicInvoice;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Credentials
{
	/// <summary>
	/// Loads credentials for a GEI message.
	/// Zero credentials may be returned, if none are available. Or many, if required.
	/// Data is stored in GlbExternalPassword.
	/// Sensitive data (eg: passwords, certificates) should be encrypted using GlbExternalPassword.GetEncryptedPassphraseForEHub(). These should be tagged as Encrypted (and EncryptedSpecified).
	/// Any Key may be used, but constants in CredentialKeys are recommended for consistency. Keys must be unique on a GEI Message.
	/// </summary>
	public interface ICredentialsLoader
	{
		IEnumerable<GlobalElectronicInvoicingHeaderElectronicInvoiceBatchRequestCredential> LoadForGEIRequest(GlbBranch branch, TransactionBatch batch, ICountryEInvoicingObjectFactory countryFactory);
	}

	public static class CredentialKeys
	{
		#region SuppressResourceStringsCheckRegion

		// Note that xT has a 39 character limitation on message attributes.
		// These are deliberately short.
		// Used in GEI XML message; not on GUI.
		public const string Certificate = "Cert";
		public const string CertificatePassword = "CertPass";

		public const string Username = "Username";
		public const string Password = "Password";
		public const string Secret = "Secret";

		#endregion
	}
}
