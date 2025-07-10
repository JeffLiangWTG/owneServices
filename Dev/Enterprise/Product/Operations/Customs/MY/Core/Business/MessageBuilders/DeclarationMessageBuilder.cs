using CargoWise.Types;
using Enterprise.Messaging.Business.MessageBuilders;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.MY.Business
{
	public class DeclarationMessageBuilder : IMessageBuilder
	{
		public DeclarationMessageBuilder(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "When declaration is used to generate a message, remove this suppression (See WI00641329)")]
		readonly JobDeclaration declaration;

		#region IMessageBuilder Members

		public IMessageBuilderResult PopulateMessages()
		{
			// Write your message implementation here
			return null;
		}

		public ZString[] GeneratedMessageStrings
		{
			// Return your message here
			get { return System.Array.Empty<ZString>(); }
		}

		#endregion
	}
}
