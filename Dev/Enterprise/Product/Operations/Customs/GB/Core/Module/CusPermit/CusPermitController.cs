using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.Module
{
	public class CusPermitController : EU.Module.CusPermitController
	{
		public CusPermitController()
		{
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CusPermitForm((CusPermitHeader)businessEntity);
		}
	}
}
