using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public static class GlbGroupExtensions
	{
		public static GlbStaff FindManager(this GlbGroup group)
		{
			GlbStaff result = null;
			foreach (GlbStaff staff in group.Staff)
			{
				if (staff.CurrentGroupLink.GK_MembershipType == Enterprise.ZArchitecture.Core.MembershipTypeList.Codes.MGR)
				{
					result = staff;
					break;
				}
			}

			return result;
		}
	}
}

