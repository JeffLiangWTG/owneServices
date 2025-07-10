using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Module
{
	public class GuaranteesController : Customs.Module.GuaranteesController
	{
		public GuaranteesController() : base()
		{
		}

		public override Type TypeOfTopLevelBusinessObject => typeof(CusGuaranteeHeader);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new Customs.GUI.GuaranteeForm((CusGuaranteeHeader)businessEntity, new GuaranteeTransactionFilterStripBusinessObject());
		}
	}
}
