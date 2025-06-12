using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests
{
	public class WithProxyGatewayServiceAttribute : WithGatewayServiceAttribute
	{
		public WithProxyGatewayServiceAttribute()
			: base(true)
		{
		}

		const string applicationName = "ProxyGatewayService";

		public static new WithProxyGatewayServiceAttribute Current => (WithProxyGatewayServiceAttribute)TestContext.CurrentContext.Test.Properties.Get(applicationName);

		protected override string ApplicationName => applicationName;

		protected override void InitializeWebConfig(XDocument doc)
		{
			base.InitializeWebConfig(doc);

			doc.XPathSelectElement("//connectionStrings//add[@name='CargoWise.eHub.DataAccess.Sql.eHubTransactions']")
				.SetAttributeValue("connectionString", "Data Source=localhost;Initial Catalog=eHubTransactionsProxy;App=eHub Gateway;User Id=eHubGateway;Password=3hubgatewayappp001");
			doc.XPathSelectElement("//connectionStrings//add[@name='eHubTransactionsContext']")
				.SetAttributeValue("connectionString", "Data Source=localhost;Initial Catalog=eHubTransactionsProxy;App=eHub Gateway;User Id=eHubGateway;Password=3hubgatewayappp001");
		}
	}
}
