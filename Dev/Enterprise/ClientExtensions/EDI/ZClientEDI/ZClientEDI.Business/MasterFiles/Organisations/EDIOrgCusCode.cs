using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIOrgCusCode : OrgCusCode
	{
		public EDIOrgCusCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override OrgCusCodeLookups GetNewLookups()
		{
			return new EDIOrgCusCodeLookups(this);
		}
	}
}

