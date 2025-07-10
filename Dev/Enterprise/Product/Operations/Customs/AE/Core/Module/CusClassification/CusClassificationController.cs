using System;

using CargoWise.EntityFramework;
using Enterprise.Customs.AE.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AE.Module;

public class CusClassificationController : Customs.Module.SingleTariffClassificationController
{
	public override Type TypeOfTopLevelBusinessObject
	{
		get { return typeof(CusClassification); }
	}

	protected override IZForm GetForm(IBusiness businessEntity)
	{
		return new BaseClassificationForm((CusClassification)businessEntity);
	}
}
