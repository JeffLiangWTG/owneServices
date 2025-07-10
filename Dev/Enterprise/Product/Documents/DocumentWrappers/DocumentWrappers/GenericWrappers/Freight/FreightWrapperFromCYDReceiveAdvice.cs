using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Yard.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class FreightWrapperFromCYDReceiveAdvice : FreightWrapper
	{
		public FreightWrapperFromCYDReceiveAdvice(CYDReceiveAdvice receiveAdviceBO, BusinessObjectFactory factory)
			: base(receiveAdviceBO, factory)
		{
			this.receiveAdviceBO = receiveAdviceBO;
		}
		readonly CYDReceiveAdvice receiveAdviceBO;

		protected override ZString GetJobNumber()
		{
			return receiveAdviceBO.JobNumber;
		}
	}
}
