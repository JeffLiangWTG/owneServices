using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.SecurityCheckpoints
{
	public class UserAdminReportsSecurityCheckpoint : ReportsSecurityCheckpoint
	{
		public UserAdminReportsSecurityCheckpoint(string code, SecurityCheckpoint parent, IZSecurity security)
			: base(code, parent, security)
		{
		}

		public override bool IsAllowed
		{
			get
			{
				var staff = User.LoadUserFromStaffPK(Security.UserPK);
				return (staff != null && !staff.IsOperational) || base.IsAllowed;
			}
		}
	}
}
