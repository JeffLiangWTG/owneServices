using Enterprise.Customs.GB.Registry;

namespace Enterprise.Customs.GB.CNS.WebServices.CnsPrints
{
	public partial class AcknowledgeEdifactPrintsResponse : ICspResultOfAcknowledgement
	{
		int ICspResultOfAcknowledgement.messageCode
		{
			get
			{
				return int.Parse(this.messageCode);
			}
			set
			{
				this.messageCode = value.ToString();
			}
		}
		// messageText alrady matches
	}

	public partial class GetAvailableEdifactPrintsResponse : ICspDownloadResult
	{
		string ICspDownloadResult.errorText
		{   // CNS do not return this
			get { return null; } // null means 'ok'
		}

		int ICspDownloadResult.batchId
		{   // just need to convert from CNS's decimal (tsk) to int
			get
			{
				return (int)this.batchId;
			}
			set
			{
				this.batchId = value;
			}
		}

		string[] ICspDownloadResult.MessagesArray
		{
			get
			{
				return this.messages; // w00t - no conversion needed
			}
		}
	}
}
