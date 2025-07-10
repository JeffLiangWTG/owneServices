using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class GroupedErrorList
	{
		readonly Dictionary<string, IList<string>> groupedErrors = new Dictionary<string, IList<string>>();

		public void Add(string group, params string[] errors)
		{
			if (errors != null && errors.Length > 0)
			{
				if (!groupedErrors.ContainsKey(group))
				{
					groupedErrors.Add(group, new List<string>());
				}

				foreach (var error in errors)
				{
					groupedErrors[group].Add(error);
				}
			}
		}

		public void Assert(string message)
		{
			Assertion.AssertGroupedErrorList(message, groupedErrors);
		}
	}
}
