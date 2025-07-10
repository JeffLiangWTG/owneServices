using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec.Evv;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class EdecEvvRequestDataProvider : IEdecEvvRequest
{
	public EdecEvvRequestDataProvider(EvvRequestSendingObject sendingObject)
	{
		this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
	}
	readonly EvvRequestSendingObject sendingObject;

	public string RequestorTraderIdentificationNumber => GlbCompany.CurrentCompany.GC_CustomsRegistrationNo;

	public string RequestorCorrelationID => null;

	public string CustomsDeclarationNumber => sendingObject.Mrn;

	public int? CustomsDeclarationVersion => sendingObject.MrnVersion == 0 ? null : sendingObject.MrnVersion;

	public string DocumentType => sendingObject.DocumentType;
}
