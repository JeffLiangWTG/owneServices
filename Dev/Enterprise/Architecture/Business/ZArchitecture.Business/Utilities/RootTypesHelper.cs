using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business
{
	public static class RootTypesHelper
	{
		public static Type[] GetDataFieldsOnlyRootTypes(Type[] rootTypes)
		{
			var result = new List<Type>();

			if (rootTypes.Any())
			{
				var workflowProviderType = rootTypes[0];
				result.Add(workflowProviderType);

				if (typeof(IAllowAdditionalRootType).IsAssignableFrom(workflowProviderType))
				{
					result.AddRange(rootTypes.Skip(1).Where(x => x.GetCustomAttribute(typeof(AdditionalRootTypeAttribute), true) != null));
				}
			}

			return result.ToArray();
		}
	}
}
