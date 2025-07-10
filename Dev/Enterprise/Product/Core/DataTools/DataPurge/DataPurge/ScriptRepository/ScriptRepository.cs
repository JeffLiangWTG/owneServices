using System.Collections.Generic;

namespace Enterprise.DataPurge
{
	public abstract class ScriptRepository
	{
		public IEnumerable<string> GetPurgeScripts()
		{
			foreach (string script in PurgeScripts)
			{
				yield return script;
			}
		}

		abstract protected string[] PurgeScripts { get; }
	}
}