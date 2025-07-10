using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business.ImportSiscomex.Testing
{
	class DeclarationPackingProviderTest : TestCaseWithFactory
	{
		public void TestDeclarationPackingProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			var pack1 = declaration.Packages.AddNew();
			pack1.CW_PackType = Core.Constants.PkgUnit.Unit;
			pack1.CW_PackQty = 10;

			var pack2 = declaration.Packages.AddNew();
			pack2.CW_PackType = Core.Constants.PkgUnit.Unit;
			pack2.CW_PackQty = 20;

			List<BasePackage> packages = new List<BasePackage>();
			packages.Add(pack1);
			packages.Add(pack2);

			var packing = new DeclarationPackingProvider(packages);

			AssertEquals("PackingTypeCode should be", Core.Constants.PkgUnit.Unit, packing.PackingTypeCode);
			AssertEquals("PackingQty  should be", 30, packing.PackingQty);
		}
	}
}


