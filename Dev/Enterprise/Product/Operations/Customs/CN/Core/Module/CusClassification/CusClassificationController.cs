using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.CN.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.Module
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("This will be used by CN Customs")]
	public class CusClassificationController : Customs.Module.SingleTariffClassificationController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(CusClassification);

		protected override IZForm GetForm(IBusiness businessEntity) => new CusClassificationForm((CusClassification)businessEntity);
	}
}
