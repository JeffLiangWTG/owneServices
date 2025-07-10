using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.JP.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Module
{
	public class CusClassificationController : Customs.Module.SingleTariffClassificationController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(CusClassification);

		protected override IZForm GetForm(IBusiness businessEntity) => new CusClassificationForm((CusClassification)businessEntity);
	}
}
