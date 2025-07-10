using System;
using System.Collections;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	static class DataMemberTypeProvider
	{
		internal static Type GetFinalComponentTypeFromDataMember(Type dataSourceType, string propertyPath)
		{
			Argument.NotNull(dataSourceType, "dataSourceType");
			Argument.NotNull(propertyPath, "propertyPath");

			var propertyPathArray = propertyPath.Split('.', '+');
			return GetFinalPropertyComponentType(dataSourceType, propertyPathArray);
		}

		static Type GetFinalPropertyComponentType(Type dataSourceType, string[] propertyPath)
		{
			Argument.NotNull(dataSourceType, "dataSourceType");
			Argument.NotNull(propertyPath, "propertyPath");

			var elementType = TryGetElementType(dataSourceType);
			var result = elementType;
			if (propertyPath.Length > 1)
			{
				var propertyName = propertyPath[0];
				var property = ZCustomTypeDescriptor.GetProperties(elementType)[propertyName];
				if (property == null && elementType != dataSourceType)
				{
					property = ZCustomTypeDescriptor.GetProperties(dataSourceType)[propertyName];
				}
				var nextDataSourceType = property == null ? dataSourceType : property.PropertyType;
				var nextPropertyPath = new string[propertyPath.Length - 1];
				Array.Copy(propertyPath, 1, nextPropertyPath, 0, nextPropertyPath.Length);
				result = GetFinalPropertyComponentType(nextDataSourceType, nextPropertyPath);
			}
			return result;
		}

		static Type TryGetElementType(Type dataSourceType)
		{
			var result = dataSourceType;
			if (typeof(IList).IsAssignableFrom(dataSourceType))
			{
				result = ListUtil.GetListElementType(dataSourceType) ?? dataSourceType;
			}
			return result;
		}
	}
}
