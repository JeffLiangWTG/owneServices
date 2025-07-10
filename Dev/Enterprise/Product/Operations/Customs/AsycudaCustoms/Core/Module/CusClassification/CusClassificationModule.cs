using CargoWise.EntityFramework;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.AsycudaCustoms.Module
{
	[CodeAlive("Module dynamically hooked up for AsycudaCustoms countries.")]
	public class CusClassificationModule : Customs.Module.SingleTariffClassificationModule
	{
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new CusClassificationFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl()
		{
			return new CusClassificationFilterControl(GridCollection, (CusClassificationFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection() => new Customs.Business.BaseClassificationCollection<CusClassification>(Factory);
	}
}
