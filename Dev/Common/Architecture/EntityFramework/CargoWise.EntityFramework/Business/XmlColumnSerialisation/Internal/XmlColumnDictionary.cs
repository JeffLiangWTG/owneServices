using System.Collections.Generic;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	class XmlColumnDictionary
	{
		internal XmlColumnDictionary()
		{
		}

		internal T GetValue<T>(string propertyName)
			where T : IZType
		{
			return Values.ContainsKey(propertyName) ? (T)Values[propertyName] : default(T);
		}

		internal void SetValue(IZType value, string propertyName)
		{
			Values[propertyName] = value;
		}

		#region Implementation

#if DEBUG
		internal
#endif
 Dictionary<string, IZType> Values
		{
			get { return values ?? (values = new Dictionary<string, IZType>()); }
		}

		Dictionary<string, IZType> values;

		#endregion
	}
}
