using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.SeaCargo.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(SeaCargoOutturnBillsModule))]
	sealed class SeaCargoOutturnBillsModuleTest : CMRModuleTest
	{
		public void TestShowNewForm()
		{
			using (var module = new SeaCargoOutturnBillsModule())
			using (var form = ((IShowNewForm)module).ShowNewForm())
			{
				AssertEquals("Form is actually a HEADER form", typeof(SeaCargoDepotOutturnForm), form.GetType());
			}
		}

		public void TestShowEditForm()
		{
			var header = Factory.New<CusOutturnHeader>();
			var outturn = header.Outturns.AddNew();
			Factory.Save();
			using (var module = new SeaCargoOutturnBillsModule())
			using (var form = (ZForm)((IFilterModuleInternalsForTesting)module).ShowEditForm(outturn))
			{
				AssertEquals("Form is actually a HEADER form", typeof(SeaCargoDepotOutturnForm), form.GetType());
				AssertEquals("Bound object is the header", header.PK, ((BusinessObject)form.BusinessEntity).PK);
				AssertEquals("Form is in edit mode", false, ((BusinessObject)form.BusinessEntity).ReadOnly);
			}
		}

		[ExpectNoExceptions]
		public void TestShowEditFormWithDeletedParent()
		{
			var header = Factory.New<CusOutturnHeader>();
			var outturn = header.Outturns.AddNew();
			Factory.Save();
			var secondFactory = new BusinessObjectFactory();
			secondFactory.RefreshEnabled = false;
			var headerInSecondfactory = secondFactory.Load<CusOutturnHeader>(header.PK);
			headerInSecondfactory.Delete();
			secondFactory.Save();
			using (var module = new SeaCargoOutturnBillsModule())
			using (var form = (ZForm)((IFilterModuleInternalsForTesting)module).ShowEditForm(outturn))
			{
				AssertNull(form);
			}
		}

		public void TestShowViewForm()
		{
			var header = Factory.New<CusOutturnHeader>();
			var outturn = header.Outturns.AddNew();
			Factory.Save();
			using (var module = new SeaCargoOutturnBillsModule())
			using (var form = (ZForm)((IFilterModuleInternalsForTesting)module).ShowViewForm(outturn))
			{
				AssertEquals("Form is actually a HEADER form", typeof(SeaCargoDepotOutturnForm), form.GetType());
				AssertEquals("Bound object is the header", header.PK, ((BusinessObject)form.BusinessEntity).PK);
				AssertEquals("Form is in view mode", true, ((BusinessObject)form.BusinessEntity).ReadOnly);
			}
		}

		public void TestID()
		{
			AssertEquals(ModuleIDs.Customs.AU.SeaCargoOutturnBills, testModule.ID);
		}

		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.SeaCargoDepot, testModule.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.AUCustomsSCAOutturnBills, testModule.SecurityCheckpoint);
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.AU.SeaCargoOutturnBills;

		protected override Type GetTypeOfFilterControl() => typeof(SeaCargoOutturnBillsFilterControl);

		protected override BusinessObject GetNewBusinessObjectForLoadingInCorrectThreadTests() => Factory.NewWithValidTestData<DepotCusOutturn>();

		SeaCargoOutturnBillsModule testModule;
		protected override void SetUp()
		{
			testModule = new SeaCargoOutturnBillsModule();
			base.SetUp();
		}

		protected override void TearDown()
		{
			testModule?.Dispose();
			base.TearDown();
		}
	}
}
