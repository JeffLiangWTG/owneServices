using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business;

public class Q05OutboundEDIMessage : ICS2OutboundEDIMessage
{
	public Q05OutboundEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EM_MessageType = MessageTypes.Codes.Q05;
	}

	protected override void ReplaceMessageTextPlaceHolders()
	{
		var messageText = EM_MessageText;
		var manifest = EM_LinkedObject as AsycudaManifestHeader;
		var lrn = LRNHelper.GenerateLRN(Factory, manifest.Branch.Company, manifest.Branch,
			manifest.SpecificCircumstanceIndicator);

		EM_ExternalReferenceNumber = lrn;

		if (messageText.IndexOf(LRNPlaceHolderHtml) != -1)
		{
			messageText = messageText.Replace(LRNPlaceHolderHtml, lrn);
		}

		EM_MessageText = messageText;
	}
}
