
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.SystemMerge.Business
{
	public class OrgHeaderForDataTransfer : AutoOrgHeader
	{
		public OrgHeaderForDataTransfer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
