using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSNilAmendmentDeclarationEDIMessage : CDSAmendDeclarationEDIMessage
	{
		public CDSNilAmendmentDeclarationEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CDSEDIMessageTypeList.Codes.NilAmendment;
		}
	}
}
