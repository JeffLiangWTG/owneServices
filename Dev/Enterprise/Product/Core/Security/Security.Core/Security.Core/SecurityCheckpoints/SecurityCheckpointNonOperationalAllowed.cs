using System;
using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security
{
	public sealed class SecurityCheckpointNonOperationalAllowed : SecurityCheckpoint, ISupportAllowWithConstraint
	{
		internal SecurityCheckpointNonOperationalAllowed(string code, MultilingualString displayText, SecurityCheckpoint parent, IZSecurity security)
			: base(code, displayText, parent, security)
		{
		}

		internal SecurityCheckpointNonOperationalAllowed(string code, MultilingualString displayText, SecurityCheckpoint parent, IZSecurity security, Guid itemGuid)
			: base(code, displayText, parent, security, itemGuid)
		{
		}

		internal SecurityCheckpointNonOperationalAllowed(string code, MultilingualString displayText, SecurityCheckpoint parent, IZSecurity security, bool hasConstraint)
			: base(code, displayText, parent, security)
		{
			this.hasConstraint = hasConstraint;
		}

		public override bool IsAllowed
		{
			get
			{
				var staff = User.LoadUserFromStaffPK(Security.UserPK);
				return (staff != null && !staff.IsOperational) || base.IsAllowed;
			}
		}

		public override bool Visible => base.Visible && !hasConstraint;

		bool ISupportAllowWithConstraint.IsAllowedWithConstraint => !hasConstraint && IsAllowed;

		readonly bool hasConstraint;
	}
}
