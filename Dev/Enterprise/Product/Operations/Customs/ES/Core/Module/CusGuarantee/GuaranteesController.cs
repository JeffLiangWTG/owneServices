using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.GUI;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Module
{
	public class GuaranteesController : Customs.Module.GuaranteesController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(CusGuaranteeHeader);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new GuaranteeForm((CusGuaranteeHeader)businessEntity, new GuaranteeTransactionFilterStripBusinessObject());
		}
	}
}
