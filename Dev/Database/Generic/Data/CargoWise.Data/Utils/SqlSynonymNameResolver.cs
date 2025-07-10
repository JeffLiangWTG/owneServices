#if DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace CargoWise.Data;

public static class SqlSynonymNameResolver
{
	public static IReadOnlyCollection<string> GetReferencedDatabasesFromSynonyms(string sql)
	{
		if (string.IsNullOrEmpty(sql))
		{
			return [];
		}

		var databaseNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		var matches = SynonymPattern.Matches(sql);
		foreach (Match match in matches)
		{
			var synonymName = match.Groups[1].Value;
			if (SynonymDictionary.TryGetValue(synonymName, out var dbName))
			{
				databaseNames.Add(dbName);
			}
		}

		return databaseNames;
	}

	public static bool IsInitialized => _synonymDictionary.IsValueCreated;

	public static void Initialize()
	{
		if (!IsInitialized)
		{
			_ = SynonymDictionary;
			_ = SynonymPattern;
		}
	}

	static Regex BuildSynonymRegex()
	{
		var synonymPatterns = SynonymDictionary.Keys
			.Select(Regex.Escape)
			.ToArray();

		var pattern = $@"\b({string.Join("|", synonymPatterns)})\b";
		return new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
	}

	static IReadOnlyDictionary<string, string> SynonymDictionary => _synonymDictionary.Value;
	static Regex SynonymPattern => _synonymPattern.Value;

	static Dictionary<string, string> InitializeSynonymsDictionary()
	{
		const string query = @"
				SELECT name, PARSENAME(base_object_name, 3)
				FROM sys.synonyms";

		var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		using var extraConnection = Db.NewExtraRestrictedReaderConnection(Db.ServerName, Db.DatabaseName);
		using var command = extraConnection.Command(query);
		using var reader = command.ExecuteReader();

		while (reader.Read())
		{
			if (!reader.IsDBNull(0) && !reader.IsDBNull(1))
			{
				var name = reader.GetString(0);
				var databaseName = reader.GetString(1);

				result.Add(name, databaseName);
			}
		}

		return result;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "Thread safety is guaranteed through Lazy<T> initialization with LazyThreadSafetyMode.ExecutionAndPublication")]
	static readonly Lazy<IReadOnlyDictionary<string, string>> _synonymDictionary = new(InitializeSynonymsDictionary, LazyThreadSafetyMode.ExecutionAndPublication);

	static readonly Lazy<Regex> _synonymPattern = new(BuildSynonymRegex, LazyThreadSafetyMode.ExecutionAndPublication);
}

#endif
