using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.MCP.MessageBuilders;

namespace Enterprise.Customs.GB.MCP.MessageSenders
{
	public class UCNMessageSender
	{
		public UCNMessageSender(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		public ZString SendMessage()
		{
			var builder = new UCNMessageBuilder(declaration);
			var message = builder.Build();
			return message.ResultText;
		}

		readonly JobDeclaration declaration;
	}
}
