using System.Data;
using CargoWise.EntityFramework;
using Enterprise.EConversation.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EdiJobConversationMessage : JobConversationMessage
	{
		public EdiJobConversationMessage(BusinessObjectFactory factory, DataRow dataRow)
			: base(factory, dataRow)
		{
		}

		protected override JobConversationMessageValidation GetNewValidation()
		{
			return new EdiJobConversationMessageValidation(this);
		}
	}
}
