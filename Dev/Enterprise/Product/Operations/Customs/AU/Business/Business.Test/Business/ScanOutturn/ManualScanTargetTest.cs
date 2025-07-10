using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ManualScanTargetTest : TransactionedTestCase
	{
		public void TestBarcodeRegexMatch()
		{
			var factory = new BusinessObjectFactory();

			var registry = OrganisationsDataRegistry.Instance.OrgBarcodeMask.Value;
			var item = registry.AddNew();
			item.Org = Env.CurrentCompany.OrganisationPK;

			factory.Load<IOrgHeader>(Env.CurrentCompany.OrganisationPK).OH_IsPackDepot = true;
			factory.Save();

			item.Priority = 1;
			item.Mask = "[a-zA-Z]+";
			OrganisationsDataRegistry.Instance.OrgBarcodeMask.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registry);
			var manager = new AirScanForOutturnManager(new ScanCusMAWB(factory.New<CusMAWB>()));
			var target = manager.CreateManualScanTarget("123BARCODE456");
			AssertEquals("Mask should of been applied", "BARCODE", target.Barcode);
		}
	}
}
