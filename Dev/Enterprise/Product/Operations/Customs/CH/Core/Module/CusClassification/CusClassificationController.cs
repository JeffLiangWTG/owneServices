using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.Module;

public class CusClassificationController : Customs.Module.SingleTariffClassificationController
{
	public override Type TypeOfTopLevelBusinessObject => typeof(CusClassification);

	protected override IZForm GetForm(IBusiness businessEntity) => new CusClassificationForm((CusClassification)businessEntity);
}
