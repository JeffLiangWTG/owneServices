using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Business
{
	public class OrgForeignCodeMatcher
	{
		public OrgForeignCodeMatcher(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		public OrgHeader Match(ZString foreignOrgCode, OrgHeader mappingOrg)
		{
			OrgHeader result = null;
			if (!foreignOrgCode.IsEmpty && mappingOrg != null)
			{
				result = OrgHeader.LoadFromForeignCode(factory, foreignOrgCode, mappingOrg);
			}
			return result;
		}
	}
}
