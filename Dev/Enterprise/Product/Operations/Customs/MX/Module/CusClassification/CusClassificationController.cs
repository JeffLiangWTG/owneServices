using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.MX.Business;
using Enterprise.Customs.MX.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.Module
{
	public class CusClassificationController : Customs.Module.SingleTariffClassificationController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(CusClassification);

		protected override IZForm GetForm(IBusiness businessEntity) => new CusClassificationForm((CusClassification)businessEntity);
	}
}
