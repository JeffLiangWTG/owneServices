using System;
using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security
{
	public class ControllerOrExplicitAccessOnlyCheckpoint : SecurityCheckpoint
	{
		internal ControllerOrExplicitAccessOnlyCheckpoint(string code, MultilingualString displayText, SecurityCheckpoint parent, IZSecurity security) : base(code, displayText, parent, security)
		{
		}

		internal ControllerOrExplicitAccessOnlyCheckpoint(string code, MultilingualString displayText, SecurityCheckpoint parent, IZSecurity security, Guid itemGuid) : base(code, displayText, parent, security, itemGuid)
		{
		}

		public override bool IsAllowed
		{
			get
			{
				return IsController || IsStaffAllowed == SecurityState.Granted || IsGroupAllowed == SecurityState.Granted;
			}

#if DEBUG
			set
			{
				base.IsAllowed = value;
			}
#endif
		}

		bool IsController => EnvProxy.Instance.CurrentUser.IsController;

		protected internal override SecurityState IsStaffAllowed
		{
			get { return Security.IsStaffAllowed(this); }
		}

		protected internal override SecurityState IsGroupAllowed
		{
			get { return Security.IsGroupExplicitlyAllowed(this) ? SecurityState.Granted : SecurityState.Denied; }
		}

		public override bool Visible => IsController;
	}
}
