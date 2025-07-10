using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.Module
{
	public class CusClassificationController : Customs.Module.SingleTariffClassificationController
	{
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(CusClassification); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CusClassificationForm((CusClassification)businessEntity);
		}
	}
}
