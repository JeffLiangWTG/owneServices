using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class GenericFreightJobWrapperLoader : GenericWrapperLoader
	{
		public override DocumentWrapper[] GetWrappers(BusinessObject businessObjectToWrap, BusinessObjectFactory factory)
		{
			return FreightWrapper.New(businessObjectToWrap, factory);
		}

		public override DocumentWrapper[] GetWrappers(BusinessObject parentBizObjToWrap, BusinessObject childBizObjToWrap, BusinessObjectFactory factory)
		{
			return FreightWrapper.New(parentBizObjToWrap, childBizObjToWrap, factory);
		}

		public override Type GetWrapperType()
		{
			return typeof(FreightWrapper);
		}
	}
}
