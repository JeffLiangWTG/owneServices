using System;
using System.Collections;
using System.Reflection;

namespace Enterprise.ZArchitecture.Modules
{
	public static class ModuleIDLoader
	{
		public static Array GetModuleIDs(Type moduleIDsType, Type moduleIDType)
		{
			ArrayList result = new ArrayList();
			ArrayList typesToCheck = new ArrayList();
			int currentPosition = 0;
			typesToCheck.Add(moduleIDsType);
			while (currentPosition < typesToCheck.Count)
			{
				Type typeToCheck = (Type)typesToCheck[currentPosition];
				foreach (FieldInfo info in typeToCheck.GetFields(BindingFlags.Static | BindingFlags.Public))
				{
					result.Add(info.GetValue(null));
				}
				typesToCheck.AddRange(typeToCheck.GetNestedTypes());
				currentPosition++;
			}

			return result.ToArray(moduleIDType);
		}
	}
}
