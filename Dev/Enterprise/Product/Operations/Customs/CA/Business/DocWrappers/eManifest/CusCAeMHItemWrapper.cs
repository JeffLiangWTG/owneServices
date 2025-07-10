using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	class CusCAeMHItemWrapper : NonPersistentBusinessObject, IDocumentWrapper
	{
		public CusCAeMHItemWrapper(CusCAeMHItem item, ZShort index)
			: base(item.Factory)
		{
			this.item = item;
			this.index = index;
		}
		readonly CusCAeMHItem item;
		readonly ZShort index;

		public ZDecimal Quantity => item.BX_Quantity;
		public ZString QuantityUQName => item.BX_QuantityUQDescription;
		public ZString HSCode => item.BX_HSCode;

		public ZShort Index => this.index;

		public ZString GoodsDescription => item.BX_Description;

		public ZString MarksAndNumbers => item.BX_Marks;

		public ZString DGCodes => item.UNDGs.FirstOrDefault()?.UNDGSubstance?.DG_UNNO ?? ZString.Empty;
	}
}
