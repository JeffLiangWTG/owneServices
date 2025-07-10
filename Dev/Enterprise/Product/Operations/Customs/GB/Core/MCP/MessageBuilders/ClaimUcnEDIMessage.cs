using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.MCP.MessageBuilders
{
	public class ClaimUcnEDIMessage : EDIMessage
	{
		public ClaimUcnEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
			MessageNumberStrategy = new GbMessageNumberStrategy(Factory, ApplicationCodeList.Codes.GbMcpClaimUcn);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodeList.Codes.GbMcpClaimUcn;
			EM_MessageType = Constants.EDIMessageTypes.UCN;
		}
	}
}
