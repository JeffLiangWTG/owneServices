using System.Collections.Generic;

namespace Enterprise.Dat.Implementation.Preconditions
{
	sealed class SqlServerDependentPrecondition : DependentPrecondition
	{
		protected override IEnumerable<IPrecondition> DependentPreconditions
		{
			get
			{
				var restarter = new SqlServerServiceRestarter();
				yield return new SqlServerNameChecker(restarter);
				yield return new SqlServiceAccountChecker(restarter);
				yield return new DBLoginChecker();
				yield return new SqlServerChecker();
				yield return new SqlServerMemoryChecker();
				yield return new SqlServerVersionChecker();
				yield return new SQLEditionChecker();
				yield return new DBCollationChecker();
				yield return new SqlServerXpCmdShellChecker();
				yield return new SqlServerAssertionFailureChecker(restarter);
				yield return restarter;
			}
		}
	}
}
