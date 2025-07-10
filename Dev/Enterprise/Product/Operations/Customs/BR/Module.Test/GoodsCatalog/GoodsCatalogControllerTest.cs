using System;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Module.Testing
{
	[TestedType(typeof(GoodsCatalogController))]
	class GoodsCatalogControllerTest : Customs.Module.Testing.GoodsCatalogControllerTest
	{
		public override Type ControllerToBashType => typeof(GoodsCatalogController);

		protected override Customs.Module.GoodsCatalogController GetNewGoodsCatalogController() => new GoodsCatalogController();

		protected override Type ExpectedBusinessObjectype => typeof(CusGoodsCatalog);

		protected override Type ExpectedFormType => typeof(CusGoodsCatalogForm);
	}
}
