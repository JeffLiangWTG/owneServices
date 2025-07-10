using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Checkpoints;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using WTG.SecurityRights;

namespace Security.Core.Testing
{
	sealed class CargoWiseCheckpointsTest : TestCase
	{
		public void TestAllOSMGCheckpointsAreSameWithSecurityCore()
		{
			var security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);

			var cargoWiseCheckpoints = new CargoWiseCheckpoints();

			var osmgCheckpoints = typeof(CargoWiseCheckpoints)
				.GetProperties()
				.Where(p => p.GetCustomAttribute<SharedCheckpointAttribute>() != null)
				.Select(p => p.GetValue(cargoWiseCheckpoints))
				.Cast<ICheckpoint>();

			foreach(var checkpoint in osmgCheckpoints)
			{
				var checkpointName = checkpoint.LookupKey.Right;
				var cw1Checkpoint = security.FindCheckPoint(checkpointName);
				AssertNotNull($"checkpoint {checkpointName}", cw1Checkpoint);
				AssertEquals(GetPathToRootBySecurityCheckpoint(cw1Checkpoint), GetPathToRoot(checkpoint));
			}

			string GetPathToRoot(ICheckpoint checkpoint)
			{
				var builder = new StringBuilder();
				var x = checkpoint;
				do
				{
					builder.Append(x.LookupKey.Right);
					builder.Append(" < ");
					x = x.Parent;
				} while (x != null);
				return builder.ToString();
			}

			string GetPathToRootBySecurityCheckpoint(SecurityCheckpoint checkpoint)
			{
				var builder = new StringBuilder();
				var x = checkpoint;
				do
				{
					builder.Append(x.Code);
					builder.Append(" < ");
					x = x.Parent;
				} while (x != null);
				return builder.ToString();
			}
		}

		public void TestAllOSMGCheckpointsHaveBeenDefinedInCargoWiseCheckpoints()
		{
			var security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			var checkpoints = new CargoWiseCheckpoints();
			var allOsmgCheckpointInSecurity = security.AllLoadedCheckPoints.Where(x => x.Code.EndsWith("IgnoreOSMG")).Select(x => x.Code).ToList();
			var set = new HashSet<string>(allOsmgCheckpointInSecurity);
			var osmgCheckpointsInCargoWiseCheckpoint = typeof(CargoWiseCheckpoints)
					.GetProperties()
					.Where(p => p.GetCustomAttribute<SharedCheckpointAttribute>() != null)
					.Select(p => p.GetValue(checkpoints))
					.Cast<ICheckpoint>()
					.Select(x => x.LookupKey.Right);

			foreach (var checkpoint in osmgCheckpointsInCargoWiseCheckpoint)
			{
				set.Remove(checkpoint);
			}

			AssertEquals(0, set.Count);
		}
	}
}
