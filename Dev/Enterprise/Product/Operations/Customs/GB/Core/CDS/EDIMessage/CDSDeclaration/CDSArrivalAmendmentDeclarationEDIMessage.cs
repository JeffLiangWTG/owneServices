using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSArrivalAmendmentDeclarationEDIMessage : CDSAmendDeclarationEDIMessage
	{
		public CDSArrivalAmendmentDeclarationEDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CDSEDIMessageTypeList.Codes.ArrivalNotification;
		}
	}
}
