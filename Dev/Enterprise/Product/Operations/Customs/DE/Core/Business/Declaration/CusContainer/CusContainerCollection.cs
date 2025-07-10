using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class CusContainerCollection : BaseCusContainerCollection<CusContainer>
	{
		public CusContainerCollection(JobDeclaration jobDeclaration, BusinessObjectFactory factory)
			: base(jobDeclaration, factory)
		{
		}

		public CusContainerCollection(JobDeclaration jobDeclaration, ZQuery filter)
			: base(jobDeclaration, filter)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (Master.IsAir)
			{
				((CusContainer)child).CO_FCL_LCL_AIR = Core.Constants.ContainerModes.ULD;
			}
		}
	}
}
