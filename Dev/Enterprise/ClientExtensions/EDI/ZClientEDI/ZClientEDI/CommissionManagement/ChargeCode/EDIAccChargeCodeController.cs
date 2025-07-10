using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.CommissionManagement.GUI
{
	public class EDIAccChargeCodeController : AccChargeCodeController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new EDIAccChargeCodeForm((AccChargeCode)businessEntity);
		}
	}
}
