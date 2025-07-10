using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using static Enterprise.Integration.Customs.BE;

namespace Enterprise.Customs.BE.Business;

public class BEMessage : EDIMessage, IEDIMessage
{
	public BEMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EM_ApplicationCode = ApplicationCodes.BECustoms;
	}

	protected override string SendersReferencePlaceHolderOverride => EDIMessage.SendersReferencePlaceHolderHtml;

	protected override string GetSendersReference() => EM_MessageNum;

	protected override string GetMessageReferenceNumber() => EM_MessageNum.IsEmpty ? Env.NumberFountains.EDIFACTNumberFountain("M", "ENT", EM_ApplicationCode).GetNextFormatted(Factory) : EM_MessageNum;

	protected override ZBool ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride() => true;
}
