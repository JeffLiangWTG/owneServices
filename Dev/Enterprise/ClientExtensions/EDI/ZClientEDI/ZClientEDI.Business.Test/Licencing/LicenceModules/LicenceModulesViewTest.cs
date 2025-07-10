using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	[TestedType(typeof(LicenceModulesView))]
	public class LicenceModulesViewTest : BusinessObjectCollectionViewTestCase<LicenceModulesView>
	{
		public void TestMode()
		{
			LicenceModulesView view = GetCollectionToTest();
			view.Mode = LicenceModulesView.FilterMode.HideUnlicenced;
			AssertEquals((int)LicenceModulesView.FilterMode.HideUnlicenced, EDIDataRegistry.Instance.LicenceModuleViewMode);

			LicenceModulesDependentCollection modules = (LicenceModulesDependentCollection)view.CollectionToFilter;
			AddModules(modules);
			view.Rebuild();
			AssertEquals("nothing licenced", 0, view.Count);
			modules.FindByCode(LegacyLicence.Codes.Core).LM_Calc_IsEnabled = true;
			view.Rebuild();
			AssertEquals("one licenced", 1, view.Count);
			AssertEquals(LegacyLicence.Codes.Core, view[0].LM_GroupModuleCode);

			view.Mode = LicenceModulesView.FilterMode.ShowAll;

			EDIDataRegistry.Instance.LicenceModuleViewMode = (int)LicenceModulesView.FilterMode.ShowAll;
		}

		public void TestHideUnlicenced()
		{
			LicenceModulesView view = GetCollectionToTest();
			Assert(!view.HideUnlicenced);
			view.HideUnlicenced = true;
			Assert(view.HideUnlicenced);
		}

		void AddModules(LicenceModulesDependentCollection modules)
		{
			var module = modules.AddNew();
			module.LM_GroupModuleCode = LegacyLicence.Codes.Core;
			module.LM_LicenceType = LicenceTypes.Codes.NON;

			module = modules.AddNew();
			module.LM_GroupModuleCode = Env.Licence.FaxEngine.Name;
			module.LM_LicenceType = LicenceTypes.Codes.NON;

			module = modules.AddNew();
			module.LM_GroupModuleCode = Env.Licence.WarehouseManagerOperationsAnd3PL.Name;
			module.LM_LicenceType = LicenceTypes.Codes.NON;

			module = modules.AddNew();
			module.LM_GroupModuleCode = Env.Licence.Booking.Name;
			module.LM_LicenceType = LicenceTypes.Codes.NON;
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(LicenceModulesView);
		}

		protected override LicenceModulesView GetCollectionToTest()
		{
			EDIOrgHeader testHeader = HeaderForTest;
			testHeader.CreateAndLoadLicenceForOrg();
			LicenceDatabase database = testHeader.LicCompany.LicDatabases.AddNew();
			LicenceModulesDependentCollection modules = new LicenceModulesDependentCollection(testHeader.LicCompany.GetHeader(database), Factory);
			return new LicenceModulesView(modules);
		}

		public EDIOrgHeader HeaderForTest
		{
			get
			{
				EDIOrgHeader headerForTest = Factory.NewWithValidTestData<EDIOrgHeader>();
				headerForTest.OH_Code = "TGBLOG";
				headerForTest.OH_RL_NKClosestPort = "AUBNE";

				OrgAddress newAddress = headerForTest.Addresses.AddNew();
				newAddress.OA_Address1 = "123 Test Address";

				return headerForTest;
			}
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.NewWithValidTestData<LicenceModules>();
		}
	}
}
