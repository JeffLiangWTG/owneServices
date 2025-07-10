using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Yard.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class CYDReceiveAdviceLineWrapper : GenericWrapper
	{
		public CYDReceiveAdviceLineWrapper(CYDReceiveAdviceLine receiveAdviceLineBO, BusinessObjectFactory factory)
			: base(receiveAdviceLineBO, factory)
		{
			this.receiveAdviceLineBO = receiveAdviceLineBO ?? factory.GetNull<CYDReceiveAdviceLine>();
		}
		readonly CYDReceiveAdviceLine receiveAdviceLineBO;

		public ZDate? OffHireDate => receiveAdviceLineBO?.YRL_OffHireDate;

		public ZDate? PreviousOnHireDate => receiveAdviceLineBO?.YRL_PreviousOnHireDate;
	}
}
