using Enterprise.Core.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security
{
	public class SecurityCheckPointWithCustomVisibility : SecurityCheckpoint
	{
		public SecurityCheckPointWithCustomVisibility(string code, MultilingualString displayText, ISecurityCheckpoint parent, IZSecurity security, bool visible)
			: base(code, displayText, parent, security)
		{
			shouldBeDisplayed = visible;
		}

		public override bool Visible
		{
			get
			{
				return base.Visible && shouldBeDisplayed;
			}
		}
		readonly bool shouldBeDisplayed;
	}
}
