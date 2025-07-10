using System;
using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngineCore.DocWrappers
{
	public class EmptyGenericWrapperLoader : GenericWrapperLoader
	{
		public override DocumentWrapper[] GetWrappers(BusinessObject businessObjectToWrap, BusinessObjectFactory factory)
		{
			return null;
		}

		public override DocumentWrapper[] GetWrappers(BusinessObject parentBizObjToWrap, BusinessObject childBizObjToWrap, BusinessObjectFactory factory)
		{
			return null;
		}

		public override Type GetWrapperType()
		{
			return null;
		}
	}
}
