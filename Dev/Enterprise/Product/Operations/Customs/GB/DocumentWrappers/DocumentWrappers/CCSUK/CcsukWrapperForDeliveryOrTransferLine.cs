using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.GB.DocumentWrappers.Ccsuk
{
	public class CcsukWrapperForDeliveryOrTransferLine : DocBaseWrapper
	{
		public static CcsukWrapperForDeliveryOrTransferLine New(CusOutTurn outTurn, BusinessObjectFactory factory)
		{
			return new CcsukWrapperForDeliveryOrTransferLine(outTurn, factory);
		}

		CcsukWrapperForDeliveryOrTransferLine(CusOutTurn outTurn, BusinessObjectFactory factory)
			: base(outTurn, factory)
		{
			this.outTurn = outTurn;
		}

		public ZString WarehouseLocation
		{
			get { return outTurn.ShedStorageLocationForRraDocument; }
		}

		public ZInt PackagesOutturned
		{
			get { return outTurn.C5_PackagesOutturned; }
		}

		public ZString MarksAndNumbers
		{
			get { return outTurn.C5_MarksAndNumbers; }
		}

		readonly CusOutTurn outTurn;
	}
}
