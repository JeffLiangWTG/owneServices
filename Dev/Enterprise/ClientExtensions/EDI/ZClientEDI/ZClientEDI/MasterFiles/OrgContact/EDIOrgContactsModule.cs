using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EDIOrgContactsModule : OrgContactsModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EDIOrgContactsFilterBusinessObject();
		}
	}
}
