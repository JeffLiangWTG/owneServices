using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	public abstract class GenericWrapperLoader
	{
		public abstract DocumentWrapper[] GetWrappers(BusinessObject businessObjectToWrap, BusinessObjectFactory factory);
		public abstract DocumentWrapper[] GetWrappers(BusinessObject parentBizObjToWrap, BusinessObject childBizObjToWrap, BusinessObjectFactory factory);
		public abstract Type GetWrapperType();

		public static GenericWrapperLoader GetFromDataContext(Core.Constants.DataContext contextFromDocument)
		{
			GenericWrapperLoader genericWrapperLoader = null;

			Hashtable dictionary = ObjectFactory.Get("GenericWrapperLoaders") as Hashtable;

			string key = Convert.ToString(contextFromDocument);

			if (dictionary != null && dictionary.ContainsKey(key))
			{
				ObjectHandle handle = (ObjectHandle)dictionary[key];
				genericWrapperLoader = (GenericWrapperLoader)handle.GetObject();
			}

			return genericWrapperLoader ?? new EmptyGenericWrapperLoader();
		}
	}
}
