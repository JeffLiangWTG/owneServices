using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5;
public class CC044CDocumentWrapper : DocumentWrapper
{
	public CC044CDocumentWrapper(CusSupportingInfo supportingInfo) : base(supportingInfo)
	{
	}

	public new static CC044CDocumentWrapper New(CusSupportingInfo supportingInfo) => supportingInfo == null ? null : new CC044CDocumentWrapper(supportingInfo);

	public override string Type => StatusIsNew ? supportingInfo.CSI_Code : null;

	public override string ReferenceNumber => StatusIsNew ? supportingInfo.CSI_ReferenceNumber : null;

	protected bool StatusIsNew => supportingInfo.CSI_Status == EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
}
