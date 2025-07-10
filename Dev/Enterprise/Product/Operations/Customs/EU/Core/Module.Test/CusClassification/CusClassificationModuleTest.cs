using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(CusClassificationModule))]
	sealed class CusClassificationModuleTest : Customs.Module.Testing.SingleTariffClassificationModuleAbstractTest<CusClassificationModule>
	{
		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (var module = new CusClassificationModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				AssertType<CusClassificationFilterControl>(filterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetCreateImportFromCSVForm()
		{
			using (var module = new CusClassificationModuleForTest())
			using (var csv = module.GetCsvForm())
			{
				AssertType<GUI.ImportClassificationsFromCSVForm>(csv);
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new CusClassificationModuleForTest())
			{
				AssertType<Customs.Business.BaseClassificationCollection<CusClassification>>(module.NewGridCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new CusClassificationModuleForTest())
			{
				AssertType<CusClassificationFilterBusinessObject>(module.NewFilterBusinessObject);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.CusClassificationModule;

		sealed class CusClassificationModuleForTest : CusClassificationModule
		{
			public CusClassificationModuleForTest()
			{
			}

			public KForm GetCsvForm() => CreateImportFromCSVForm();

			public IFilterControl NewFilterControl => GetNewFilterControl();

			public IBusinessObjectCollection NewGridCollection => GetNewGridCollection();

			public FilterBusinessObject NewFilterBusinessObject => GetNewFilterBusinessObject();
		}
	}
}
