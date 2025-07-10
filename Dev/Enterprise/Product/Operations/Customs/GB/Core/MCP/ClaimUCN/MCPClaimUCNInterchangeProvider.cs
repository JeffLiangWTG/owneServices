using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.Chief;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.MCP.ClaimUCN
{
	public class MCPClaimUCNInterchangeProvider : GbInterchangeProvider
	{
		public MCPClaimUCNInterchangeProvider(NonDependentEDIMessageCollection messages)
			: base(messages) { }

		public override string ApplicationCode
		{
			get
			{
				return ApplicationCodeList.Codes.GbMcpClaimUcn;
			}
		}

		protected override string InstructionHowToSetInterchangeSenderID
		{
			get { return "See Registry -> Customs -> Country or Region Specific -> United Kingdom -> Service Providers -> MCP -> ISL Webservice Credentials"; }
		}

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			var claimUCNRequestMessage = messages.Cast<EDIMessage>().Single();
			SetInterchangeValuesForTransmit(interchange, messages, claimUCNRequestMessage.EM_MessageType, Constants.EDIInterchange.GBCustoms, claimUCNRequestMessage.EM_MessageOwner);
		}

		protected override ZString GetFooterText(EDIInterchange interchange, NonDependentEDIMessageCollection messages)
		{
			return ZString.Empty;
		}
	}
}
