using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class BMControlCustomisationUsagesCollection : ActiveBusinessObjectCollection<BMControlCustomisationLink>
	{
		public BMControlCustomisationUsagesCollection(BMControlCustomisation parent)
			: base(parent.Factory, parent, new ZQuery(), BMControlCustomisationLinkSchema.FML_FM_ControlCustomisation)
		{
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}
