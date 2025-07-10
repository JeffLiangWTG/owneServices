using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Warehouse.Yard.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class CYDReleaseAdviceLineWrapper : GenericWrapper
	{
		public CYDReleaseAdviceLineWrapper(CYDReleaseAdviceLine releaseAdviceLineBO, BusinessObjectFactory factory)
			: base(releaseAdviceLineBO, factory)
		{
			this.releaseAdviceLineBO = releaseAdviceLineBO ?? factory.GetNull<CYDReleaseAdviceLine>();
		}
		readonly CYDReleaseAdviceLine releaseAdviceLineBO;

		public ZDate? OffHireDate => releaseAdviceLineBO?.YEL_OffHireDate;

		public ZDate? OnHireDate => releaseAdviceLineBO?.YEL_OnHireDate;
	}
}
