using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IWareHouse
	{
		ZString TypeCode { get; }
		ZString ID { get; }
	}

	class WarehouseWrapper : IWareHouse
	{
		WarehouseWrapper(ZString typeCode, ZString id)
		{
			this.typeCode = typeCode;
			this.id = id;
		}

		public static WarehouseWrapper New(ZString typeCode, ZString id)
		{
			return new WarehouseWrapper(typeCode, id);
		}

		ZString IWareHouse.TypeCode => typeCode;

		ZString IWareHouse.ID => id;

		readonly ZString typeCode;
		readonly ZString id;

		/*
		<Warehouse>
			<ID>1234567GB</ID>
			<TypeCode>U</TypeCode>
		</Warehouse>
		 */
	}
}
