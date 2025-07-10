using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSFECAmendmentDeclarationEDIMessage : CDSAmendDeclarationEDIMessage
	{
		public CDSFECAmendmentDeclarationEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CDSEDIMessageTypeList.Codes.FecChallenge;
		}
	}
}
