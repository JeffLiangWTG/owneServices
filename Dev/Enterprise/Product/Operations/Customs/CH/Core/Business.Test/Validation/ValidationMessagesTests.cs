using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

class ValidationMessagesTests : TestCaseWithFactory
{
	public void TestPlausiNamingConvention()
	{
		var result = new SortedDictionary<string, IList<string>>();

		Regex memberNamePattern = new Regex(@"^Message([A-Z]{1,2}[0-9]{1,4}[a-z|_]*[0-9]*)+$");
		Regex messageValuePattern = new Regex(@"\([A-Z]{1,2}[0-9]{1,4}.*\)\.$");

		var members = GetMembers(typeof(ValidationMessages.Plausi));

		foreach (var member in members)
		{
			AddMessageToListIfNotMatching(result, memberNamePattern, member.Name, member.Name, $"Property Name '{member.Name}' is not matching the pattern (e.g. MessageR268a or MessageR268_2).");
			AddMessageToListIfNotMatching(result, messageValuePattern, member.Name, member.Value, $"Property Value of property '{member.Name}' is not matching the pattern. It should contain the message number within brackets at the end of the sentence (e.g. ' ... (R268a).').");
		}

		AssertGroupedErrorList($"The following {nameof(ValidationMessages.Plausi)} properties have to be changed:", result);
	}

	void AddMessageToListIfNotMatching(SortedDictionary<string, IList<string>> result, Regex pattern, string memberName, string input, string assertionMessage)
	{
		if (!pattern.IsMatch(input))
		{
			if (result.ContainsKey(memberName))
			{
				result[memberName].Add(assertionMessage);
			}
			else
			{
				result.Add(memberName, new List<string> { assertionMessage });
			}
		}
	}

	static IEnumerable<(string Name, string Value)> GetMembers(Type obj)
	{
		foreach (var property in obj.GetProperties(BindingFlags.Public | BindingFlags.Static)
				  .Where(p => p.PropertyType == typeof(string)))
		{
			yield return (property.Name, (string)property.GetValue(null, null));
		}

		foreach (var method in obj.GetMethods(BindingFlags.Public | BindingFlags.Static)
				  .Where(m => m.ReflectedType == typeof(string)))
		{
			yield return (method.Name, (string)method.Invoke(null, method.GetParameters().Select(p => p.DefaultValue).ToArray()));
		}
	}
}
