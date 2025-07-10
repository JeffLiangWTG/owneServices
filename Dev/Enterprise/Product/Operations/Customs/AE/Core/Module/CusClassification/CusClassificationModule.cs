
using CargoWise.EntityFramework;
using Enterprise.Customs.AE.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.Module;

public class CusClassificationModule : Customs.Module.SingleTariffClassificationModule
{
	protected override FilterBusinessObject GetNewFilterBusinessObject()
	{
		return new CusClassificationFilterBusinessObject();
	}

	protected override IFilterControl GetNewFilterControl()
	{
		return new CusClassificationFilterControl(GridCollection, (CusClassificationFilterBusinessObject)FilterBusinessObject);
	}

	protected override IBusinessObjectCollection GetNewGridCollection()
	{
		return new Customs.Business.BaseClassificationCollection<CusClassification>(Factory);
	}
}
