using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import.esitoServizi;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

sealed class ProspectusDownloadMessage : ResponseMessage<Risposta, CargoWise.Customs.IT.MessageDefinitions.DocumentManagementService.downloadProspettoContabileSintesi.DownloadProspetto>
{
	public ProspectusDownloadMessage(ZString message, ZString messageType) : base(message)
	{
		this.messageType = Argument.NotNullOrEmpty(messageType, nameof(messageType));
	}

	readonly ZString messageType;

	const string MiscellaneousCustomsDocument = "MCD";

	protected override string GetMessageStatus()
		=> ResponseBody?.Esito?.Codice;

	protected override IXmlOverridesCreationFactory GetXmlOverridesCreationFactory()
		=> null;

	public ZString MRN => Data?.Output?.DatiDichiarazione?.Mrn;

	public ZString DocumentType => MiscellaneousCustomsDocument;

	public ZString FileName => $"{messageType}_{MRN}.pdf";

	public byte[] ContentData => Data?.Output?.Allegato?.Contenuto;

	public bool IsFileContentFilled => !MRN.IsEmpty && ContentData.Length > 0;
}
