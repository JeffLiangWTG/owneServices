using System;

using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security
{
	sealed class SecurityCheckpointWebUserAllowed : SecurityCheckpoint
	{
		internal SecurityCheckpointWebUserAllowed(string code, MultilingualString displayText, SecurityCheckpoint parent, IZSecurity security)
			: base(code, displayText, parent, security)
		{
		}

		internal SecurityCheckpointWebUserAllowed(string code, MultilingualString displayText, SecurityCheckpoint parent, IZSecurity security, Guid itemGuid)
			: base(code, displayText, parent, security, itemGuid)
		{
		}

		public override bool IsAllowed
		{
			get
			{
				var staff = User.LoadUserFromStaffPK(Security.UserPK);
				return (staff != null && staff.IsWebUser) || base.IsAllowed;
			}
		}
	}
}