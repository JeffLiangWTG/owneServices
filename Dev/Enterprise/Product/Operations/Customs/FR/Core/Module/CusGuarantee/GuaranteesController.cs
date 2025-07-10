using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.Module
{
	public class GuaranteesController : Customs.Module.GuaranteesController
	{
		public GuaranteesController() : base()
		{
		}

		public override Type TypeOfTopLevelBusinessObject => typeof(CusGuaranteeHeader);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GuaranteeForm((Customs.Business.BaseCusGuaranteeHeader)businessEntity);
		}
	}
}

