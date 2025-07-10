using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using static Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.CH.Business;

public class EvvRequestSendingObject : IMessageSendingObject
{
	public EvvRequestSendingObject(IEDIMessageCollectionOwner messageOwner, ZString mrn, ZInt mrnVersion, ZString messageSubType)
	{
		MessageOwner = Argument.NotNull(messageOwner, nameof(messageOwner));
		MessageSubTypeForEDIMessage = messageSubType;
		Mrn = mrn;
		MrnVersion = mrnVersion;
	}

	public IEDIMessageCollectionOwner MessageOwner { get; }

	public ZString MessageSubTypeForEDIMessage { get; }

	public BusinessObjectFactory Factory => MessageOwner.MessageOwner.Factory;

	public ZString ApplicationCode => ApplicationCodes.CHCustomsEdec;

	public ZString MessageTypeForEDIMessage => MessageTypeCodeList.Codes.EVV;

	public ZString GetApplicationReference() => $"{Mrn}.{MrnVersion}";

	public ZGuid GetCredentialPK() => GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany)?.GlbExternalPassword?.PK ?? ZGuid.Empty;

	public ZString ToMessageString() => MessageBuilderFactory.NewMessageBuilder(this).GenerateXmlMessage().GetSerializedString();

	public ZString Mrn;

	public ZInt MrnVersion;

	public string DocumentType => MessageSubTypeForEDIMessage.ToString() switch
	{
		MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties => EvvDocumentType.Codes.TaxationDecisionCustomsDuties,
		MessageSubTypeCodeList.Codes.TaxationDecisionVat => EvvDocumentType.Codes.TaxationDecisionVAT,
		MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties => EvvDocumentType.Codes.RefundCustomsDuties,
		MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat => EvvDocumentType.Codes.RefundVAT,
		_ => string.Empty
	};
}
