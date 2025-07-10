using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Module
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

