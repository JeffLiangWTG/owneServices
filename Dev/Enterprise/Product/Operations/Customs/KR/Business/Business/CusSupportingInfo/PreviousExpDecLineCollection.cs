using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.KR.Business
{
	public class PreviousExpDecLineCollection : CusSupportingInfoCollection<PreviousExpDecLine>
	{
		public PreviousExpDecLineCollection(ILineOrProduct parent)
			: base((BusinessObject)parent, CusSupportingInfoTypeList.Codes.PreviousExpDecLine)
		{
		}

		internal void AddNewIfRequired(PreviousExpDecLine previousExpDecLine)
		{
			var item = this.Cast<PreviousExpDecLine>().FirstOrDefault(x => x.HasSameKey(previousExpDecLine));
			if (item == null)
			{
				item = AddNew();
				item.CSI_ReferenceNumber = previousExpDecLine.CSI_ReferenceNumber;
				item.CSI_ReferenceNumber2 = previousExpDecLine.CSI_ReferenceNumber2;
				item.CSI_ItemNumber = previousExpDecLine.CSI_ItemNumber;
			}
			if (item.CSI_UnitOfQuantity.IsEmpty)
			{
				item.CSI_UnitOfQuantity = previousExpDecLine.CSI_UnitOfQuantity;
			}
			item.CSI_Quantity += previousExpDecLine.CSI_Quantity;
		}
	}
}
