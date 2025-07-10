using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Module
{
	public class CusClassificationController : Customs.Module.SingleTariffClassificationController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(CusClassification);

		protected override IZForm GetForm(IBusiness businessEntity) => new CusClassificationForm((CusClassification)businessEntity);
	}
}
