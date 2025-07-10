using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DocumentVisualizer.Integration
{
	public interface ICustomMessageAmendmentSupporter
	{
		object GetMessageAmendmentReason();
		bool PopulateMessageAmendmentReason(IDataObject dataObject, object reasonForSending);
	}
}
