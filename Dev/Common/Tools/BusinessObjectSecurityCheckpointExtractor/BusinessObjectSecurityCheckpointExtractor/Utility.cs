using System.Text.Json;
using System.Text.Json.Serialization;

namespace BusinessObjectSecurityCheckpointExtractor;
public static class Utility
{
	public static string DictionaryToString(IDictionary<string, BusinessObjectCheckpoint> dict)
	{
		var options = new JsonSerializerOptions
		{
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			WriteIndented = true,
		};

		return JsonSerializer.Serialize(dict, options);
	}
	public static void WriteDictionaryToFile(IDictionary<string, BusinessObjectCheckpoint> dict, string filename)
	{
		File.WriteAllText(filename, DictionaryToString(dict));
	}

	public static Dictionary<string, BusinessObjectCheckpoint> CleanDictionary(Dictionary<string, BusinessObjectCheckpoint> dict)
	{
		foreach (var item in dict.Values)
		{
			item.CleanEmptyStrings();
		}
		return dict.Where(kvp => !kvp.Value.IsEmpty()).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
	}
}
