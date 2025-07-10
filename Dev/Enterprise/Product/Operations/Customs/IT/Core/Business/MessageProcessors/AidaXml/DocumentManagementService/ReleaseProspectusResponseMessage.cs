using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import.esitoServizi;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IT.Business;

sealed class ReleaseProspectusResponseMessage : ResponseMessage<Risposta, CargoWise.Customs.IT.MessageDefinitions.DocumentManagementService.richiesta_prospetto_svincolo.RichiestaProspettoSvincolo>
{
	public ReleaseProspectusResponseMessage(ZString message) : base(message)
	{
	}

	protected override string GetMessageStatus()
		=> ResponseBody?.Esito?.Codice;

	protected override IXmlOverridesCreationFactory GetXmlOverridesCreationFactory()
		=> null;

	public ZString MRN => Data?.Output?.Dichiarazione?.Mrn;

	public ZString DocumentType => RefDocTypes.ClearanceAdvice;

	public ZString FileName => $"SVI_{MRN}.pdf";

	public byte[] ContentData => Data?.Output?.ProspettoSvincolo?.Contenuto;

	public bool IsFileContentFilled => !MRN.IsEmpty && ContentData.Length > 0;
}
