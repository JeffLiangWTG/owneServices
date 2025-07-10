using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocConfirmDivotCollection : DocumentWrapperCollection<DocConfirmDivot>
	{
		public DocConfirmDivotCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocConfirmDivotCollection(CommonPickupDeliveryConfirm confirm)
			: base(confirm.Factory)
		{
			foreach (CommonConfirmDivot divot in confirm.Divots)
			{
				Add(DocConfirmDivot.New(divot, Factory));
			}
		}
	}
}
