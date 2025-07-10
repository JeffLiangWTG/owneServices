using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageDefinitions.Declaration.Import.esitoServizi;
using CargoWise.Types;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.IT.Business;

sealed class EadTadResponseMessage : ResponseMessage<Risposta, CargoWise.Customs.IT.MessageDefinitions.DocumentManagementService.richiestaDaeDat.RichiestaDaeDat>
{
	public EadTadResponseMessage(ZString message) : base(message)
	{
	}

	protected override string GetMessageStatus()
		=> ResponseBody?.Esito?.Codice;

	protected override IXmlOverridesCreationFactory GetXmlOverridesCreationFactory()
		=> null;

	public ZString MRN => Data?.Output?.Dichiarazione?.Mrn;

	public ZString DocumentType => RefDocTypes.ClearanceAdvice;

	public ZString FileName => Data?.Output?.DaeDat?.TipoPdf == CargoWise.Customs.IT.MessageDefinitions.DocumentManagementService.richiestaDaeDat.DaeDatTipoPdf.Dae
		? $"EAD_{MRN}.pdf" : $"TAD_{MRN}.pdf";

	public byte[] ContentData => Data?.Output?.DaeDat?.Contenuto;

	public bool IsFileContentFilled => !MRN.IsEmpty && ContentData.Length > 0;
}
