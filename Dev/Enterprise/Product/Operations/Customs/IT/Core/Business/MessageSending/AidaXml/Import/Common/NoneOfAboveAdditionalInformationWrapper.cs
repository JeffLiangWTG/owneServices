using CargoWise.Customs.IT.MessageContracts;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

public class NoneOfAboveAdditionalInformationWrapper : IAdditionalInformation
{
	public NoneOfAboveAdditionalInformationWrapper()
	{
	}

	string IAdditionalInformation.Code => null;

	string IAdditionalInformation.Description => NoneOfAboveDescription;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Italian text, not a code smell")]
	const string NoneOfAboveDescription = "nessuna delle precedenti";  
}
