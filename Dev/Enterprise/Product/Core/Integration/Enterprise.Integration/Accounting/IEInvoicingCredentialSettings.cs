using CargoWise.Types;

namespace Enterprise.Integration.Accounting
{
	/// <summary>
	/// Represents country specific settings for Electronic Invoicing Credentials
	/// </summary>
	/// <remarks>
	/// This is a closed set with three options (as defined on extension methods):
	/// 1. Certificate based credentials (zero or more x.509 certs).
	/// 2. Password based credentials (fixed number of key value pairs).
	/// 3. No credentials.
	/// </remarks>
	public interface IEInvoicingCredentialSettings
	{
		/// <summary>
		/// Value for GP_PasswordType, used for legacy implementations of eInvoicing.
		/// Please use Electronic Invoice Messaging (EIM) for new implementations.
		/// </summary>
		ZString PasswordType { get; }

		/// <summary>
		/// When false, company level credentials cannot be edited.
		/// </summary>
		bool IsCompanyCredentialsRequired { get; }

		/// <summary>
		/// When false, branch level credentials cannot be edited.
		/// </summary>
		bool IsBranchCredentialsRequired { get; }

		/// <summary>
		/// When true, branch level credentials registration is enabled.
		/// </summary>
		bool IsBranchRegistrationRequired { get; }
	}
}
