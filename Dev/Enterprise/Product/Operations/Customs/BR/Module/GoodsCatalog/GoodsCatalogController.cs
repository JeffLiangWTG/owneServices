using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.Module
{
	public class GoodsCatalogController : Customs.Module.GoodsCatalogController
	{
		public override Type TypeOfTopLevelBusinessObject => typeof(CusGoodsCatalog);

		protected override IZForm GetForm(IBusiness businessEntity) => new CusGoodsCatalogForm((CusGoodsCatalog)businessEntity);
	}
}
