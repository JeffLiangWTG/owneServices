using System;
using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Security.Core.ResString;

namespace Enterprise.Security
{
	sealed class LocalAdminCheckpoint : SecurityCheckpoint
	{
		internal static MultilingualString LocalAdminHumanReadableNameConstant { get { return ResString.GetMultilingualString("9675b1b5-9269-4cb9-bdb1-63de31fa3853", "Local Administrator"); } }
		internal static MultilingualString GroupOwnerHumanReadableNameConstant { get { return ResString.GetMultilingualString("89f73924-1725-4eb6-86ef-804359cf256e", "Group Owner"); } }

		internal LocalAdminCheckpoint(string code, IZSecurity security, Guid itemGuid)
			: base(code, null, null, security, itemGuid)
		{
		}

		public override MultilingualString HumanReadableName
		{
			get { return Code.Contains("GroupOwner") ? GroupOwnerHumanReadableNameConstant : LocalAdminHumanReadableNameConstant; }
		}

		protected override internal SecurityState IsStaffAllowed
		{
			get { return IsThisNonOperational ? SecurityState.Granted : Security.IsStaffAllowed(this); }
		}

		protected override internal SecurityState IsGroupAllowed
		{
			get { return Security.IsGroupExplicitlyAllowed(this) ? SecurityState.Granted : SecurityState.Denied; }
		}

		// Not in separate methods as the unit test needs to set EnvProxy.Instance.CurrentUser.IsController
		bool IsThisNonOperational
		{
			get
			{
				var user = User.LoadUserFromStaffPK(Security.UserPK);
				return user != null && !user.IsOperational;
			}
		}

		public override bool Visible
		{
			get { return false; }
		}
	}
}
