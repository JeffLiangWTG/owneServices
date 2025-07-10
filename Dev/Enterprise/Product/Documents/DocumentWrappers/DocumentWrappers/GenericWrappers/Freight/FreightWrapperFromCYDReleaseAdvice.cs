using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Yard.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	class FreightWrapperFromCYDReleaseAdvice : FreightWrapper
	{
		public FreightWrapperFromCYDReleaseAdvice(CYDReleaseAdvice releaseAdviceBO, BusinessObjectFactory factory)
			: base(releaseAdviceBO, factory)
		{
			this.releaseAdviceBO = releaseAdviceBO;
		}

		readonly CYDReleaseAdvice releaseAdviceBO;

		protected override ZString GetJobNumber()
		{
			return releaseAdviceBO.JobNumber;
		}
	}
}
