using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.AE.Business;

public class AEEDIMessage : Messaging.Business.EDIMessage
{
	public AEEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{ }

	[ThreadSafe]
	public new static readonly EDIMessageTypeDecider TypeDecider = new EDIMessageTypeDecider();

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EM_ApplicationCode = ApplicationCodeList.Codes.UAECustoms;
	}

	protected override string GetMessageReferenceNumber() => new ReferenceNumberGenerator(Factory).GenerateMessageReferenceNumber(EM_ApplicationCode, EM_MessageType);

	protected override string MessageNumberPlaceHolderOverride => AEConstants.Messaging.Placeholders.MessageNumber;

	protected override string GetSendersReference()
	{
		return EM_LinkedObject is IMessageAttachee messageAttachee ? messageAttachee.DocumentIdentifier : string.Empty;
	}
}
