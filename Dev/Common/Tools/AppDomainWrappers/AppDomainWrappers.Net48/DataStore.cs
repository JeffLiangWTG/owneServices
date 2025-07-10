using System.Collections.Generic;

public class DataStore
{
	readonly Dictionary<string, object> data = new Dictionary<string, object>();

	public object GetData(string key)
	{
		if (data.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public void SetData(string key, object value)
	{
		data[key] = value;
	}
}
