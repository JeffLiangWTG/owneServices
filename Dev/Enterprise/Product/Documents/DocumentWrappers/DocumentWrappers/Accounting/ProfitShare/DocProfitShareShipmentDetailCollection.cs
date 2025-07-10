
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocProfitShareShipmentDetailCollection : DocumentWrapperCollection
	{
		public DocProfitShareShipmentDetailCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocProfitShareShipmentDetailCollection(ProfitShareShipmentDetailCollection shipmentDetails, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (ProfitShareShipmentDetail shipmentDetail in shipmentDetails)
			{
				Add(DocProfitShareShipmentDetail.New(shipmentDetail, factory));
			}
		}

		public new DocProfitShareShipmentDetail this[int index]
		{
			get { return (DocProfitShareShipmentDetail)base[index]; }
		}
	}
}
