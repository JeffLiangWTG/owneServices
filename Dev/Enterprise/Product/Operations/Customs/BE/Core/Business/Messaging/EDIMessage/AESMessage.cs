using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.Business;

[WTG.StaticAnalysis.Annotation.CodeAlive("Will be used for future AES WI's")]
public class AESMessage : BEMessage
{
	public AESMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EM_MessageType = MessageVersionRegistry.AESDomainCode;
	}

	protected override string MessageNumberPlaceHolderOverride => MessageNumberPlaceHolderHtml;
}
