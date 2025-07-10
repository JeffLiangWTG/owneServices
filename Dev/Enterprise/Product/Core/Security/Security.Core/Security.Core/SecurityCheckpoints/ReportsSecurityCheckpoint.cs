using Enterprise.Core.Environment;
using ResString = Enterprise.Security.Core.ResString;

namespace Enterprise.Security
{
	public class ReportsSecurityCheckpoint : SecurityCheckpoint
	{
		public ReportsSecurityCheckpoint(string code, SecurityCheckpoint parent, IZSecurity security)
			: base(code, ResString.GetMultilingualString("b5a55c3b-1b48-45c6-ae7a-dd4120b268eb", "Reports"), parent, security)
		{
		}
	}
}