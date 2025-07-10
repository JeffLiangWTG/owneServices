using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IAgent
	{
		IOrganisation Agent { get; }
		ZString FunctionCode { get; }
	}

	public class AgentWrapper : IAgent
	{
		AgentWrapper(IOrganisation agent, ZString functionCode)
		{
			this.agent = agent;
			this.functionCode = functionCode;
		}

		public static AgentWrapper New(IOrganisation agent, ZString functionCode)
		{
			return new AgentWrapper(agent, functionCode);
		}

		IOrganisation IAgent.Agent => agent;

		ZString IAgent.FunctionCode => functionCode;

		readonly IOrganisation agent;
		readonly ZString functionCode;
	}
}
