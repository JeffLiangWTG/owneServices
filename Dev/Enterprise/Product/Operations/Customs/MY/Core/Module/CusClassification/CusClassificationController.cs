using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.MY.Business;
using Enterprise.Customs.MY.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MY.Module
{
	public class CusClassificationController : Customs.Module.SingleTariffClassificationController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(CusClassification);

		protected override IZForm GetForm(IBusiness businessEntity) => new CusClassificationForm((CusClassification)businessEntity);
	}
}
