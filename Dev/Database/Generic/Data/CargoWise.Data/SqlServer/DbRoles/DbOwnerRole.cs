using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data
{
	[CodeAlive("For user login role")]
	public class DbOwnerRole : DbRole
	{
		public override string Name => DbRoleTypes.DbOwnerRole;
	}
}
