using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Customs.GB.Business.MessageBuilders.DocumentSending;

namespace Enterprise.Customs.GB.H7.Business
{
	public class CDSH7SupportingDocUniversalEventBuilder(ISupportingDocumentMessageDataProvider dataWrapper) : GBSupportingDocUniversalEventBuilder(dataWrapper)
	{
		protected override ZString GetCredentialKey(ISupportingDocumentMessageDataProvider dataWrapper)
		{
			var bill = dataWrapper.BusinessObject as AsycudaBill;
			return bill.Header.CredentialsKey;
		}
	}
}
