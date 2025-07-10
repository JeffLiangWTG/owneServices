using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security
{
	public class SecurityCheckPointWithConstraint : SecurityCheckpoint, ISupportAllowWithConstraint
	{
		public SecurityCheckPointWithConstraint(string code, MultilingualString displayText, ISecurityCheckpoint parent, IZSecurity security, bool hasConstraint)
			: base(code, displayText, parent, security)
		{
			this.hasConstraint = hasConstraint;
		}

		public override bool Visible => base.Visible && !hasConstraint;

		bool ISupportAllowWithConstraint.IsAllowedWithConstraint => !hasConstraint && IsAllowed;

		readonly bool hasConstraint;
	}
}
