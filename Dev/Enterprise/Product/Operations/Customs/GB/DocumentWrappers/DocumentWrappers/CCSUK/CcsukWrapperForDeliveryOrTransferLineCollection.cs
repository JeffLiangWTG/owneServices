using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.GB.DocumentWrappers.Ccsuk
{
	public class CcsukWrapperForDeliveryOrTransferLineCollection : DocBaseWrapperCollection<CcsukWrapperForDeliveryOrTransferLine>
	{
		public CcsukWrapperForDeliveryOrTransferLineCollection(ICcsukCusAwb awb, BusinessObjectFactory factory)
			: base(factory)
		{
			foreach (CusOutTurn outTurn in awb.OutTurns)
			{
				if (outTurn.IsBeingReleasedNow)
				{
					Add(CcsukWrapperForDeliveryOrTransferLine.New(outTurn, factory));
				}
			}
		}
	}
}
