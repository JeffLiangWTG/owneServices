using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.Module
{
	public class CusClassificationModule : Customs.Module.SingleTariffClassificationModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CusClassificationFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new CusClassificationFilterControl(GridCollection, (CusClassificationFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new BaseClassificationCollection<CusClassification>(Factory, Core.Constants.CountryCodes.Brazil);
	}
}
