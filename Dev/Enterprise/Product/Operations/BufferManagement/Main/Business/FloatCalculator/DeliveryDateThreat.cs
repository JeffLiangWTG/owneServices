using CargoWise.PAVE.Common.Interfaces;

namespace Enterprise.BufferManagement.Business
{
	public class DeliveryDateThreat
	{
		public DeliveryDateThreat(ILinkEntity responsibleEntity, decimal floatConsumption)
		{
			ResponsibleEntity = responsibleEntity;
			FloatConsumption = floatConsumption;
		}

		public ILinkEntity ResponsibleEntity { get; private set; }
		public decimal FloatConsumption { get; private set; }
	}
}
