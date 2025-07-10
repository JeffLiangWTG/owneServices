using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class EdiCustomerUserAccountEmailWrapper : NonPersistentBusinessObject
	{
		public EdiCustomerUserAccountEmailWrapper()
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", Justification = "We are using URL as string on the rest of the implementation.")]
		public EdiCustomerUserAccountEmailWrapper(EdiCustomerUserAccount account, string url)
		{
			Account = account;
			Url = url;
		}

		public EdiCustomerUserAccount Account { get; }

		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		public string Url { get; }
	}
}
