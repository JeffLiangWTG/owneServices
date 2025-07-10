using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class GenericFreightJobFromShipmentWrapperLoader : GenericWrapperLoader
	{
		public override DocumentWrapper[] GetWrappers(BusinessObject businessObjectToWrap, BusinessObjectFactory factory)
		{
			return FreightWrapperFromShipment.New(businessObjectToWrap, factory);
		}

		public override DocumentWrapper[] GetWrappers(BusinessObject parentBizObjToWrap, BusinessObject childBizObjToWrap, BusinessObjectFactory factory)
		{
			return FreightWrapperFromShipment.New(parentBizObjToWrap, childBizObjToWrap, factory);
		}

		public override Type GetWrapperType()
		{
			return typeof(FreightWrapperFromShipment);
		}
	}
}
