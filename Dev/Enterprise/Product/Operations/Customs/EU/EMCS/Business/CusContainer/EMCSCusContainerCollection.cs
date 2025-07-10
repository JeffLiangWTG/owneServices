using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSCusContainerCollection : BaseCusContainerCollection<EMCSCusContainer>
	{
		public EMCSCusContainerCollection(EMCSJobDeclaration master)
			: base(master, master.Factory)
		{
			this.EnableMaxCountValidation(99, Res.GetString("c2e5cf1e-2d57-4f07-b368-38e33d854b34", "There are too many Transports. Maximum of 99."), false);
		}
	}
}
