using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.Web.Business
{
	public interface IModuleFilterProvider
	{
		ZQuery FilterSubQuery(Type businessObjectTypeToFilter);
		void SetupDefaults(FilterBusinessObject filterBizO, FilterBusinessObjectDefaults filterBODetauls);
		void ApplyAdditionalLoggedInUserFilter(Type businessObjectTypeToFilter, ZQuery loggedInUserFilter);
	}
}
