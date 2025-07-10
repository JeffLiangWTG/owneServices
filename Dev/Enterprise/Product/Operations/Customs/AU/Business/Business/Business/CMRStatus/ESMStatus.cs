using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ESMStatus : CMRStatus, IDocumentStatus, IDocumentStatusConditions, ILineDocumentStatusAndDocumentStatusConditions
	{
		public ESMStatus(ForwardingConsol consol)
		{
			this.consol = consol;
		}

		#region IDocumentStatus Members

		public CMRDocumentStatus DocumentStatus
		{
			get
			{
				return GetDocumentStatus(GetLastCUSRESOfType(consol.Messages, CMRMessage.CMRMessageTypes.ESM));
			}
		}

		#endregion

		#region IDocumentStatusConditions Members

		public CMRDocumentStatusConditions DocumentStatusConditions
		{
			get
			{
				return GetDocumentStatusConditions(GetLastCUSRESOfType(consol.Messages, CMRMessage.CMRMessageTypes.ESM));
			}
		}

		#endregion

		#region ILineDocumentStatusAndDocumentStatusConditions Members

		public DocumentStatusAndDocumentStatusConditions[] Line
		{
			get
			{
				return GetLineDocumentStatusAndDocumentStatusConditions(consol.Messages, CMRMessage.CMRMessageTypes.ESM);
			}
		}

		#endregion

		#region Implementation

		protected ForwardingConsol consol;

		#endregion
	}
}
