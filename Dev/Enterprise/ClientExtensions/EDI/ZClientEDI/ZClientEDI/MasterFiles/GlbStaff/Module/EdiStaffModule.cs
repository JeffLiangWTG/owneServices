using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EdiStaffModule : GlbStaffModule
	{
		protected override IFilterControl GetNewFilterControl()
		{
			return new EdiGlbStaffFilterControl(GridCollection, (EdiGlbStaffFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new EdiGlbStaffFilterBusinessObject();
	}
}
