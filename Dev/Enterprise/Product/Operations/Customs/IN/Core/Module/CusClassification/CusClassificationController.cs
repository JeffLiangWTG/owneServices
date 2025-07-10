using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.IN.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.Module;

public class CusClassificationController : Customs.Module.SingleTariffClassificationController
{
	public override Type TypeOfTopLevelBusinessObject => typeof(CusClassification);

	protected override IZForm GetForm(IBusiness businessEntity) => new CusClassificationForm((CusClassification)businessEntity);
}
