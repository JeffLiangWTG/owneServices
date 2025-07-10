using System.Collections.Generic;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing
{
	#region E-Invoicing Credential Settings

	public abstract class GlobalEInvoicingCredentialSettings : IEInvoicingCredentialSettings
	{
		protected virtual ZString PasswordType => PasswordTypesList.Codes.EIM;

		protected virtual bool IsCompanyCredentialsRequired => false;

		protected virtual bool IsBranchCredentialsRequired => false;

		protected virtual bool IsBranchRegistrationRequired => false;

		#region IEInvoicingCredentialSettings

		ZString IEInvoicingCredentialSettings.PasswordType => PasswordType;

		bool IEInvoicingCredentialSettings.IsCompanyCredentialsRequired => IsCompanyCredentialsRequired;

		bool IEInvoicingCredentialSettings.IsBranchCredentialsRequired => IsBranchCredentialsRequired;

		bool IEInvoicingCredentialSettings.IsBranchRegistrationRequired => IsBranchRegistrationRequired;

		#endregion

	}

	/// <summary>
	/// Represents no / null credentials.
	/// </summary>
	public sealed class GlobalEInvoicingNoCredentialSettings : GlobalEInvoicingCredentialSettings
	{
	}

	#endregion

	#region E-Invoicing Certificate Settings

	public abstract class GlobalEInvoicingCertificateCredentialSettings : GlobalEInvoicingCredentialSettings, IEInvoicingCertificateCredentialSettings
	{
		protected virtual int ExpiryWarningDays => 90;

		protected virtual string[] HiddenGridColumns => System.Array.Empty<string>();

		#region IEInvoicingCertificateCredentialSettings

		int IEInvoicingCertificateCredentialSettings.ExpiryWarningDays => ExpiryWarningDays;

		#endregion

		#region IEInvoicingCredentialControlSettings

		string[] IGridControlSettings.HiddenColumns => HiddenGridColumns;

		#endregion
	}

	#endregion

	#region E-Invoicing Password Settings

	[CodeAlive("Will be used for password based credentials")]
	public abstract class GlobalEInvoicingPasswordCredentialSettings : GlobalEInvoicingCredentialSettings, IEInvoicingPasswordCredentialSettings
	{
		public IReadOnlyCollection<IEInvoicingPasswordCredentialDefinition> PasswordDefinitions => new List<IEInvoicingPasswordCredentialDefinition>().AsReadOnly();
	}

	[CodeAlive("Will be used for password based credentials")]
	public class EInvoicingPasswordCredentialDefinition : IEInvoicingPasswordCredentialDefinition
	{
		public int DisplayOrder { get; set; }

		public string UniqueKey { get; set; }

		public IMultilingualString UsernameLabel { get; set; }

		public IMultilingualString PasswordLabel { get; set; }

		public IEInvoicingPasswordCredentialDefinition AsInterface() => this;
	}

	#endregion
}
