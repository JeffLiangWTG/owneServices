using System;
using System.Text.RegularExpressions;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class eHubGatewayRegistryItem : StringRegistryItem
	{
		public eHubGatewayRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, string defaultValue, bool isProduction)
			: base(new eHubGatewayImpl(name, category, caption, hint, storage, options, defaultValue, isProduction))
		{
		}

		class eHubGatewayImpl : RegistryItemImpl
		{
			public eHubGatewayImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, string defaultValue, bool isProduction)
				: base(name, category, caption, hint, new eHubGatewayRegistryDataType(isProduction), storage, options, defaultValue)
			{
			}
		}
	}

	public class eHubGatewayRegistryDataType : StringRegistryDataType
	{
		public eHubGatewayRegistryDataType(bool isProduction)
			: base()
		{
			this.isProduction = isProduction;
		}

		readonly bool isProduction;

		protected override void ValidateCore(IRegistryItem registryItem, string proposedValue, Guid companyPK, Guid branchPK,
			Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			var ipAddressRegex = new Regex(@"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");
			var hostnameRegex = new Regex(@"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9])\.)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-]*[A-Za-z0-9])$");
			if (!ipAddressRegex.IsMatch(proposedValue) && !hostnameRegex.IsMatch(proposedValue))
			{
				throw new RegistryValidationException(Res.GetString("30da4676-5e80-453e-8153-112ac4790625", "It should be entered as the server name: {0} in {1} (In Example: {2}).", "{SERVER_NAME}", "http://{SERVER_NAME}/Extra/", "www.google.com"));
			}

			if (isProduction && string.Equals(proposedValue, eHubMessagingRegistry.eHubGateway.TestServerName, StringComparison.InvariantCultureIgnoreCase))
			{
				throw new RegistryValidationException(Res.GetString("71dcf105-f9ae-4e55-ada5-467984eab6fb", "To exchange messages with the test eHub gateway, please set 'Send Interchanges To The eHub Test Gateway' to true and use the THI service task for receiving messages."));
			}
		}
	}
}
