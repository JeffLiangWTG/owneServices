using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class GenericCommercialInvoiceWrapperLoader : GenericWrapperLoader
	{
		public override DocumentWrapper[] GetWrappers(BusinessObject businessObjectToWrap, BusinessObjectFactory factory)
		{
			return CommercialInvoiceWrapper.New(businessObjectToWrap, factory);
		}

		public override DocumentWrapper[] GetWrappers(BusinessObject parentBizObjToWrap, BusinessObject childBizObjToWrap, BusinessObjectFactory factory)
		{
			return CommercialInvoiceWrapper.New(childBizObjToWrap, factory);
		}

		public override Type GetWrapperType()
		{
			return typeof(CommercialInvoiceWrapper);
		}
	}
}
