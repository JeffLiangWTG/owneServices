using System.Collections.Generic;

namespace Enterprise.Integration.Accounting
{
	public interface IEInvoicingPasswordCredentialSettings : IEInvoicingCredentialSettings
	{
		/// <summary>
		/// One or more pairs of username + password definitions.
		/// </summary>
		IReadOnlyCollection<IEInvoicingPasswordCredentialDefinition> PasswordDefinitions { get; }
	}
}
