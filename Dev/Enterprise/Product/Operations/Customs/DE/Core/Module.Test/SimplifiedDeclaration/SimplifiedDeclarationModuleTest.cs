using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(SimplifiedDeclarationModule))]
	class SimplifiedDeclarationModuleTest : ZModuleBasherTest
	{
		public void TestModuleType()
		{
			AssertType<SimplifiedDeclarationModule>(filterModule);
		}

		public void TestModuleID()
		{
			AssertEquals(filterModule.ID, ModuleIDs.Customs.EU.DE.SimplifiedDeclaration);
		}

		public void TestAllowView()
		{
			AssertEquals(false, filterModule.AllowView);
		}

		public void TestAllowEdit()
		{
			AssertEquals(false, filterModule.AllowEdit);
		}

		public void TestHasActions()
		{
			AssertEquals(false, filterModule.HasActions);
		}

		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.Core, filterModule.LicenceCheckPoint);
		}

		public void TestGetNewFilterControl()
		{
			using (var filterControl = filterModule.GetNewFilterControlForGrid())
			{
				AssertType<SimplifiedDeclarationFilterStripControl>(filterControl);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			AssertType<SimplifiedDeclarationFilterStripBusinessObject>(filterModule.FilterBusinessObject);
		}

		public void TestCollectionFilters()
		{
			using (var form = new ZForm(reconDeclaration))
			{
				filterModule.SetFormsModalTo(form);
				using (var moduleForm = filterModule.ShowPopup())
				{
					CombineAssertions(() =>
					{
						AssertType<ActiveBusinessObjectCollection<CusReconEntry>>(filterModule.GridCollection);
						var filter = (filterModule.GridCollection as ActiveBusinessObjectCollection<CusReconEntry>).AdditionalFilter;
						var filterString = filter.GetAsWhereClause(true);
						AssertContains("Not linked", $"{CusReconEntry.Schema.CRE_CRD} is null", filterString, true);
						AssertContains("Recon Declaration Branch", $"{CusReconEntry.Schema.CRE_GB_Branch} = CONVERT('{reconDeclaration.CRD_GB_Branch}', 'System.Guid')", filterString, true);
					});
				}
			}
		}

		public void TestPopUp()
		{
			using (var form = new ZForm(reconDeclaration))
			{
				filterModule.SetFormsModalTo(form);
				using (var moduleForm = filterModule.ShowPopup())
				{
					CombineAssertions(() =>
					{
						AssertType<SimplifiedDeclarationModuleForm>("Form", moduleForm);
						AssertNull("DataSource", (moduleForm as ZForm).DataSource);
					});
				}
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.DE.SimplifiedDeclaration;

		protected override string CountryCode => Core.Constants.CountryCodes.Germany;

		protected override bool HasController() => false;

		protected override void SetUp()
		{
			base.SetUp();
			reconDeclaration = Factory.NewWithValidTestData<CusReconDeclaration>();
			filterModule = ZModuleFactory.Instance.Create(ModuleIDs.Customs.EU.DE.SimplifiedDeclaration) as ZFilterModule;
		}

		protected override void TearDown()
		{
			base.TearDown();
			filterModule.Dispose();
		}
		ZFilterModule filterModule;
		CusReconDeclaration reconDeclaration;
	}
}
