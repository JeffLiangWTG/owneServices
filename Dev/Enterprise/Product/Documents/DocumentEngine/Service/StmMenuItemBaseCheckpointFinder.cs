using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine
{
	class StmMenuItemBaseCheckpointFinder
	{
		internal static ISecurityCheckpoint FindSecurityCheckpointByPK(ZGuid stmMenuItemBasePK)
		{
			var securityVector = new SecurityVector();
			securityVector.Initialise(Env.Security);

			ISecurityCheckpoint checkpoint = default;

			foreach (var info in securityVector.Nodes)
			{
				checkpoint = SearchSecurityCheckpointInHierarchy(info, stmMenuItemBasePK);
				if (checkpoint != null)
				{
					break;
				}
			}

			return checkpoint;
		}

		static ISecurityCheckpoint SearchSecurityCheckpointInHierarchy(ISecurityInfo security, ZGuid stmMenuItemBasePK)
		{
			if (security == null)
			{
				return null;
			}

			if (security.Checkpoint?.ItemGuid == stmMenuItemBasePK)
			{
				return security.Checkpoint;
			}

			if (security.Nodes == null)
			{
				return null;
			}

			foreach (var info in security.Nodes)
			{
				var checkPoint = SearchSecurityCheckpointInHierarchy(info, stmMenuItemBasePK);
				if (checkPoint != null)
				{
					return checkPoint;
				}
			}

			return null;
		}
	}
}
