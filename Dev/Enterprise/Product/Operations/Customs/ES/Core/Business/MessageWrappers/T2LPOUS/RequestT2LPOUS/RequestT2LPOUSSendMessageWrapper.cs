using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class RequestT2LPOUSSendMessageWrapper : T2LPOUSRequestAndReceptionSendMessageWrapper, IRequestT2LMessageDataProvider
{
	public RequestT2LPOUSSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
	{
	}

	public IT2LPOUSRequestProofOperationInformationForT2LT2LF ProofOperationInformationForT2LT2LF => proofOperationInformationForT2LT2LF ?? (proofOperationInformationForT2LT2LF = new T2LPOUSRequestProofOperationInformationForT2LT2LFWrapper(entryHeader));
	T2LPOUSRequestProofOperationInformationForT2LT2LFWrapper proofOperationInformationForT2LT2LF;

	protected override OrgAddress OrgAddressForPersonReqPres => declaration.SupplierDocumentaryAddress?.Address;
}
