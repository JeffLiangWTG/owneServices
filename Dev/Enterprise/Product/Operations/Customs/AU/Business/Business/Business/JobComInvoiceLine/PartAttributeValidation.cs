using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class PartAttributeValidation : MasterFiles.Business.PartAttributeValidation
	{
		protected override void CheckVinCore(OrgHeader org, OrgSupplierPart part, ZPropertyInfo info, int attributeNumber)
		{
			base.CheckVinCore(org, part, info, attributeNumber);
			var invoiceLine = (JobComInvoiceLine)info.BizObj;
			if (invoiceLine != null)
			{
				var declaration = invoiceLine.Declaration;
				if (declaration != null && declaration.JE_MessageType == JobMessageTypeList.Codes.Import)
				{
					var addInfo = invoiceLine.AddInfo;
					if (!info.Value.IsEmpty && !addInfo.ZA_VID.IsEmpty)
					{
						info.AddMessageError(Res.GetString("61b0b835-e278-40ba-9079-162b7d92323b", "Only the VIN in Additional Info will be sent in the message."));
					}
				}
			}
		}
	}
}
