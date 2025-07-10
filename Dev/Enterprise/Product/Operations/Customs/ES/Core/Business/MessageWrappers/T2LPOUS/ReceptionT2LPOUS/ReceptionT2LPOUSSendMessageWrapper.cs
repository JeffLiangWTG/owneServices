using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class ReceptionT2LPOUSSendMessageWrapper : T2LPOUSRequestAndReceptionSendMessageWrapper, IReceptionT2LMessageDataProvider
{
	public ReceptionT2LPOUSSendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
	{
	}

	public IT2LPOUSReceptionProofOperationInformationForT2LT2LF ProofOperationInformationForT2LT2LF => proofOperationInformationForT2LT2LF ?? (proofOperationInformationForT2LT2LF = new T2LPOUSReceptionProofOperationInformationForT2LT2LFWrapper(entryHeader));
	T2LPOUSReceptionProofOperationInformationForT2LT2LFWrapper proofOperationInformationForT2LT2LF;

	public ZString TipoAltaIndirecta => entryInstruction.ZG_IndirectType;

	public IReadOnlyCollection<IAnnexDocCommon> EnvioDocumentos
	{
		get
		{
			if (envioDocumentos == null)
			{
				var envioDocumentosList = new List<AnnexDocCommonWrapper>();
				var eDocPivotList = entryHeader.GetAllSendableEDocPivots();

				eDocPivotList.Where(x => x != null).ForEach(x => envioDocumentosList.Add(new AnnexDocCommonWrapper(x.Document, x.CSD_Description)));

				envioDocumentos = envioDocumentosList.AsReadOnly();
			}
			return envioDocumentos;
		}
	}
	IReadOnlyCollection<AnnexDocCommonWrapper> envioDocumentos;

	protected override OrgAddress OrgAddressForPersonReqPres => declaration.ImporterDocumentaryAddress?.Address;
}
