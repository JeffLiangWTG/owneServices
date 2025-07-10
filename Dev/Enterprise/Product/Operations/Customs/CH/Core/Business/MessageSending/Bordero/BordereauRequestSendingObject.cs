using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.Business;

public sealed class BordereauRequestSendingObject : NonPersistentBusinessObject, IMessageSendingObject
{
	public ZString ApplicationCode => ApplicationCodeList.Codes.CHCustomsEdec;

	public ZString MessageTypeForEDIMessage => MessageTypeCodeList.Codes.BOR;

	public ZString MessageSubTypeForEDIMessage => MessageSubTypeCodeList.Codes.BordereauResponse;

	public ZString ToMessageString() => MessageBuilderFactory.NewMessageBuilder(this).GenerateXmlMessage().GetSerializedString();

	public ZString GetApplicationReference() => ZString.Empty;

	public ZGuid GetCredentialPK() => GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany)?.GlbExternalPassword?.PK ?? ZGuid.Empty;

	public ZString BordereauNumber { get; set; }

	public ZString ProcessingCenterNumber { get; set; }

	public ZDate CreationDate { get; set; }
}
