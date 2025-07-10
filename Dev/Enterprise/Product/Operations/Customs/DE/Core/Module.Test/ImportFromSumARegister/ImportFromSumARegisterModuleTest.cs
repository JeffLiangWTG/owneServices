using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(ImportFromSumARegisterModule))]
	sealed class ImportFromSumARegisterModuleTest : ZModuleBasherTest
	{
		public void TestModuleType()
		{
			AssertType<ImportFromSumARegisterModule>(filterModule);
		}

		public void TestModuleID()
		{
			AssertEquals(filterModule.ID, ModuleIDs.Customs.EU.DE.ImportFromSumARegister);
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

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.DESumARegister, filterModule.SecurityCheckpoint);
		}

		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.Core, filterModule.LicenceCheckPoint);
		}

		public void TestGetNewFilterControl()
		{
			using (var filterControl = filterModule.GetNewFilterControlForGrid())
			{
				AssertType<ImportFromSumARegisterFilterStripControl>(filterControl);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			AssertType<ImportFromSumARegisterFilterStripBusinessObject>(filterModule.FilterBusinessObject);
		}

		[TestDate(2022, 07, 28)]
		public void TestCollectionFilters()
		{
			var collection = filterModule.GridCollection as ActiveBusinessObjectCollection<CusTempStorageRegLine>;
			var filterString = collection.AdditionalFilter.GetAsWhereClause(true);
			AssertNotContains("SRH_CustomsOffice", EFTA.TemporaryStorageRegister.Business.AutoCusTempStorageRegHeader.Schema.SRH_CustomsOffice, filterString, true);
		}

		public void TestCollectionFilters_JE_CustomsOfficeIsNotEmpty()
		{
			jobDeclaration.JE_CustomsOffice = "DE003202";
			using (var form = new ZForm(jobDeclaration))
			{
				filterModule.SetFormsModalTo(form);
				using (var moduleForm = filterModule.ShowPopup())
				{
					var collection = filterModule.GridCollection as ActiveBusinessObjectCollection<CusTempStorageRegLine>;
					var filterString = collection.CompleteFilter.GetAsWhereClause(true);
					AssertContains("SRH_CustomsOffice", $"{EFTA.TemporaryStorageRegister.Business.AutoCusTempStorageRegHeader.Schema.SRH_CustomsOffice} = 'DE003202'", filterString, true);
				}
			}
		}

		public void TestCollectionFilters_NCTS_DepartureCustomsOfficeCodeIsNotEmpty()
		{
			var cusInBondHeader = Factory.New<CusInBondHeaderForTest>();
			using (var form = new ZForm(cusInBondHeader))
			{
				filterModule.SetFormsModalTo(form);
				using (var moduleForm = filterModule.ShowPopup())
				{
					var collection = filterModule.GridCollection as ActiveBusinessObjectCollection<CusTempStorageRegLine>;
					var filterString = collection.CompleteFilter.GetAsWhereClause(true);
					AssertContains("SRH_CustomsOffice", $"{EFTA.TemporaryStorageRegister.Business.AutoCusTempStorageRegHeader.Schema.SRH_CustomsOffice} = 'DE003203'", filterString, true);
				}
			}
		}

		public void TestShowPopup()
		{
			using (var form = new ZForm(jobDeclaration))
			{
				filterModule.SetFormsModalTo(form);
				using (var moduleForm = (ImportFromSumARegisterModuleForm)filterModule.ShowPopup())
				{
					CombineAssertions(() =>
					{
						AssertEquals("Text", "Import from SumA Register", moduleForm.Text);
						var embeddedControl = filterModule.EmbeddedControl;
						AssertEquals("EmbeddedControl is within FilterControlPanel", true, moduleForm.FindSingle<ZPanel>("FilterControlPanel").Controls.Contains(embeddedControl));
						AssertNull("DataSource", moduleForm.DataSource);
					});
				}
			}
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			base.AddTestObjects(collection);

			var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();

			var line1 = header.CusTempStorageRegLines.AddNew();
			line1.SRL_LineNumber = 1;
			var line2 = header.CusTempStorageRegLines.AddNew();
			line2.SRL_LineNumber = 2;

			Factory.Save();
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.DE.ImportFromSumARegister;

		protected override string CountryCode => Core.Constants.CountryCodes.Germany;

		protected override bool HasController() => false;

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			filterModule = ZModuleFactory.Instance.Create(ModuleIDs.Customs.EU.DE.ImportFromSumARegister) as ZFilterModule;
		}

		protected override void TearDown()
		{
			base.TearDown();
			filterModule.Dispose();
		}
		ZFilterModule filterModule;
		JobDeclaration jobDeclaration;

		sealed class CusInBondHeaderForTest : DummyBusinessObject, IDepartureCustomsOfficeCodeProvider
		{
			public CusInBondHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZString DepartureCustomsOfficeCode => "DE003203";
		}
	}
}
