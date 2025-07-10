using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.NCTS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.NCTS.Module
{
	public sealed class DEGuaranteesController : Customs.Module.GuaranteesController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new DEGuaranteeForm((CusGuaranteeHeader)businessEntity);
		}

		public override Type TypeOfTopLevelBusinessObject => typeof(CusGuaranteeHeader);
	}
}
