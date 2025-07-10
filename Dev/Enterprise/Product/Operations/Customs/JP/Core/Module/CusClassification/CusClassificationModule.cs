using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Module
{
	public class CusClassificationModule : Customs.Module.SingleTariffClassificationModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CusClassificationFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new CusClassificationFilterControl(GridCollection, (CusClassificationFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new BaseClassificationCollection<CusClassification>(Factory, Core.Constants.CountryCodes.Japan);
	}
}
