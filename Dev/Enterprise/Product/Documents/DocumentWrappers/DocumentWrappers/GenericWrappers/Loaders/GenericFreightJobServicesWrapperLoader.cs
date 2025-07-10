using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class GenericFreightJobServicesWrapperLoader : GenericWrapperLoader
	{
		public override DocumentWrapper[] GetWrappers(BusinessObject businessObjectToWrap, BusinessObjectFactory factory)
		{
			return new DocumentWrapper[] { ServiceWrapper.New(businessObjectToWrap, factory) };
		}

		public override DocumentWrapper[] GetWrappers(BusinessObject parentBizObjToWrap, BusinessObject childBizObjToWrap, BusinessObjectFactory factory)
		{
			return new DocumentWrapper[] { ServiceWrapper.New(childBizObjToWrap, factory) };
		}

		public override Type GetWrapperType()
		{
			return typeof(ServiceWrapper);
		}
	}
}
