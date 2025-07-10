using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusEntryLineWarehouseItemWrapper : WarehouseItemWrapper
	{
		public CusEntryLineWarehouseItemWrapper(CusEntryLine entryLine)
		{
			this.entryLine = entryLine;
		}

		public ZString AHECCCode
		{
			get
			{
				return entryLine.TariffNumber;
			}
		}

		public ZDecimal NetQuantity
		{
			get
			{
				return entryLine.Quantity;
			}
		}

		public ZString NetQuantityUnit
		{
			get
			{
				return entryLine.UnitOfQuantity;
			}
		}

		public ZString GoodsDescription
		{
			get
			{
				return entryLine.CL_Description;
			}
		}

		protected CusEntryLine entryLine;
	}
}
