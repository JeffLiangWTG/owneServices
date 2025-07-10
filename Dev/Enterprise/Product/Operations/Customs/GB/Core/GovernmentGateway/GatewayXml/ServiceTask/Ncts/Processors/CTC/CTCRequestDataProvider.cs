using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayXml.ServiceTask.Ncts
{
	public class CTCRequestDataProvider : GBCustomsRequestDataProviderBase
	{
		public CTCRequestDataProvider(NctsHeader header, EDIMessage outboundMessage)
		{
			this.nctsHeader = Argument.NotNull(header, nameof(header));
			this.outboundMessage = Argument.NotNull(outboundMessage, nameof(outboundMessage));
		}

		public ZString ServiceReference => nctsHeader.GetServiceReference(GovernmentGatewayExtensions.GetMessageTypeBasedOnApplicationCode(outboundMessage));

		protected override ZString GatewayCore => "GovernmentGateway";

		protected override ZString JobNumberCore => nctsHeader.BH_JobReference;

		protected override ZString GetCredentialsKey()
		{
			var eoriBadge = ".NOVALIDTOKENFOUND";
			var token = nctsHeader.GetValidAccessToken();

			if (token != null)
			{
				eoriBadge = FormattableString.Invariant($"{token.EORI}.{token.Badge}");
			}

			var enterpriseCode = GBExtensions.GetEnterpriseCode();
			return FormattableString.Invariant($"{enterpriseCode}.{eoriBadge}");
		}

		protected override CredentialsSetting GetCredentialsSetting() => null;

		readonly NctsHeader nctsHeader;
		readonly EDIMessage outboundMessage;
	}
}
