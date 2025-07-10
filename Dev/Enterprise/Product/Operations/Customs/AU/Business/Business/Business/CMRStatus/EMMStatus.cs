using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EMMStatus : CMRStatus, IDocumentStatus, IDocumentStatusConditions, ILineDocumentStatusAndDocumentStatusConditions
	{
		public EMMStatus(ForwardingConsol consol)
		{
			this.consol = consol;
		}

		#region IDocumentStatus Members

		public CMRDocumentStatus DocumentStatus
		{
			get
			{
				return GetDocumentStatus(GetLastCUSRESOfType(consol.Messages, CMRMessage.CMRMessageTypes.EMM));
			}
		}

		#endregion

		#region IDocumentStatusConditions Members

		public CMRDocumentStatusConditions DocumentStatusConditions
		{
			get
			{
				return GetDocumentStatusConditions(GetLastCUSRESOfType(consol.Messages, CMRMessage.CMRMessageTypes.EMM));
			}
		}

		#endregion

		#region ILineDocumentStatusAndDocumentStatusConditions Members

		public DocumentStatusAndDocumentStatusConditions[] Line
		{
			get
			{
				return GetLineDocumentStatusAndDocumentStatusConditions(consol.Messages, CMRMessage.CMRMessageTypes.EMM);
			}
		}

		#endregion

		#region Implementation

		protected ForwardingConsol consol;

		#endregion

	}
}
