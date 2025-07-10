using System;
using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	internal class DataViewBusinessObjectMapping_DebuggerTypeProxy
	{
		public DataViewBusinessObjectMapping_DebuggerTypeProxy(DataViewBusinessObjectMapping mapping)
		{
			if (mapping == null)
			{
				throw new ArgumentNullException(nameof(mapping));
			}
			this.mapping = mapping;
		}

		public string[] Mappings
		{
			get
			{
				List<string> result = new List<string>();
				for (int i = mapping.StartIndex; i <= mapping.EndIndex; i++)
				{
					result.Add(i + "->" + mapping[i]);
				}
				return result.ToArray();
			}
		}

		readonly DataViewBusinessObjectMapping mapping;
	}
}
