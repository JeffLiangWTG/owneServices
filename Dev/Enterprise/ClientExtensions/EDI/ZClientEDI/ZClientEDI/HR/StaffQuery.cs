using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.HR
{
	public static class StaffQuery
	{
		public static GenRegCertAccredMaintList[] LoadCertByNumber(BusinessObjectFactory factory, string type, IEnumerable<string> numbers)
		{
			var certQuery = new ZQuery(GenRegCertAccredMaintListSchema.XZ_Type, type);
			certQuery.AddToFilter(GenRegCertAccredMaintListSchema.XZ_ParentTableCode, GlbStaffSchema.Constants.Prefix);
			certQuery.AddToFilter(GenRegCertAccredMaintListSchema.XZ_RefNumber, numbers);
			return factory.Load<GenRegCertAccredMaintList>(certQuery);
		}

		public static EDIGlbStaff[] LoadStaffByPks(BusinessObjectFactory factory, IEnumerable<ZGuid> pks)
		{
			var staffQuery = new ZDBOnlyQuery(typeof(EDIGlbStaff));
			staffQuery.AddToFilter(GlbStaffSchema.PK, pks);
			staffQuery.AddToFilter(GlbStaffSchema.GS_IsActive, true);
			return factory.Load<EDIGlbStaff>(staffQuery);
		}
	}
}
