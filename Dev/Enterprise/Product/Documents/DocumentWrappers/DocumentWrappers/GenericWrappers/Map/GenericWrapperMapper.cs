using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.Mapping;

namespace Enterprise.DocumentWrappers.GenericWrappers.Map
{
	public class GenericWrapperMapper : Mapper
	{
		GenericWrapperMapper(Type wrapperTypeToMap, bool includeChildrenAndRelatedObjects, bool includeIBODocDataProviders)
			: base(wrapperTypeToMap, includeChildrenAndRelatedObjects, includeIBODocDataProviders, typeof(GenericWrapper), typeof(GenericWrapperCollection), new List<Type>(), null, null)
		{
		}

		public static ZString GetMapAsText(Type wrapperTypeToMap, bool includeChildrenAndRelatedObjects, bool includeIBODocDataProviders)
		{
			List<MapTable> tables = GetMapAsTableList(wrapperTypeToMap, includeChildrenAndRelatedObjects, includeIBODocDataProviders);
			ZStringBuilder result = new ZStringBuilder();
			foreach (MapTable table in tables)
			{
				result.Append(table.ToString());
			}
			return result.ToStringWithDelimiterBetweenAppends("\r\n\r\n\r\n");
		}

		public static List<MapTable> GetMapAsTableList(Type wrapperTypeToMap, bool includeChildrenAndRelatedObjects, bool includeIBODocDataProviders)
		{
			GenericWrapperMapper mapper = new GenericWrapperMapper(wrapperTypeToMap, includeChildrenAndRelatedObjects, includeIBODocDataProviders);
			return mapper.GetMapAsTableList();
		}
	}
}
