using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Module.Testing;

[TestedType(typeof(ImportFromTemporaryStorageRegisterModule))]
sealed class ImportFromTemporaryStorageRegisterModuleTest : ZModuleBasherTest
{
	public void TestModuleType() => AssertType<ImportFromTemporaryStorageRegisterModule>(filterModule);

	public void TestModuleID() => AssertEquals(filterModule.ID, ModuleIDs.Customs.ImportFromTemporaryStorageRegister);

	public void TestAllowView() => AssertEquals(expected: false, filterModule.AllowView);

	public void TestAllowEdit() => AssertEquals(expected: false, filterModule.AllowEdit);

	public void TestHasActions() => AssertEquals(expected: false, filterModule.HasActions);

	public void TestSecurityCheckpoint() => AssertEquals(Env.Security.EFTATemporaryStorageRegister, filterModule.SecurityCheckpoint);

	public void TestLicenceCheckPoint() => AssertEquals(Env.Licence.Core, filterModule.LicenceCheckPoint);

	[RequiresSTA]
	public void TestGetNewFilterControl()
	{
		using (var filterControl = filterModule.GetNewFilterControlForGrid())
		{
			AssertType<ImportFromTemporaryStorageRegisterFilterStripControl>(filterControl);
		}
	}

	public void TestGetNewFilterBusinessObject() => AssertType<ImportFromTemporaryStorageRegisterFilterStripBusinessObject>(filterModule.FilterBusinessObject);

	[TestDate(2022, 07, 28)]
	public void TestCollectionFilters()
	{
		var collection = filterModule.GridCollection as ActiveBusinessObjectCollection<CusTempStorageRegLine>;
		var filterString = collection.AdditionalFilter.GetAsWhereClause(true);
		AssertNotContains("SRH_CustomsOffice", AutoCusTempStorageRegHeader.Schema.SRH_CustomsOffice, filterString, ignoreCase: true);
	}

	[RequiresSTA]
	public void TestCollectionFilters_CustomsOfficePopulated()
	{
		using (var form = new ZForm(supporter))
		{
			filterModule.SetFormsModalTo(form);
			using (var moduleForm = filterModule.ShowPopup())
			{
				var collection = filterModule.GridCollection as ActiveBusinessObjectCollection<CusTempStorageRegLine>;
				var filterString = collection.CompleteFilter.GetAsWhereClause(true);
				AssertContains("SRH_CustomsOffice", $"{AutoCusTempStorageRegHeader.Schema.SRH_CustomsOffice} = 'DE003203'", filterString, ignoreCase: true);
			}
		}
	}

	[RequiresSTA]
	public void TestShowPopup()
	{
		using (var form = new ZForm(supporter))
		{
			filterModule.SetFormsModalTo(form);
			using (var moduleForm = (ImportFromTemporaryStorageRegisterModuleForm)filterModule.ShowPopup())
			{
				CombineAssertions(() =>
				{
					AssertEquals("Text", "Import from Temporary Storage Register", moduleForm.Text);
					var embeddedControl = filterModule.EmbeddedControl;
					AssertEquals("EmbeddedControl is within FilterControlPanel", expected: true, moduleForm.FindSingle<ZPanel>("FilterControlPanel").Controls.Contains(embeddedControl));
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

	protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.ImportFromTemporaryStorageRegister;

	protected override bool HasController() => false;

	protected override void SetUp()
	{
		base.SetUp();
		supporter = Factory.New<ImportFromTemporaryStorageRegisterSupporter>();
		filterModule = ZModuleFactory.Instance.Create(ModuleIDs.Customs.ImportFromTemporaryStorageRegister) as ZFilterModule;
	}
	ZFilterModule filterModule;
	ImportFromTemporaryStorageRegisterSupporter supporter;

	protected override void TearDown()
	{
		base.TearDown();
		filterModule.Dispose();
	}

	sealed class ImportFromTemporaryStorageRegisterSupporter : DummyBusinessObject, ICanImportFromTemporaryStorageRegister
	{
		public ImportFromTemporaryStorageRegisterSupporter(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public string TemporaryStorageApplicationCode => "SUM";

		public string DepartureCustomsOfficeCode => "DE003203";
	}
}
