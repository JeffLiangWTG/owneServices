using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AQISUserControlTest : TestCaseWithFactory
	{
		public void TestPremisesIdColumnModuleID()
		{
			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var control = new AQISUserControl())
			{
				var premisesIdColumn = control.aQISPremisesIdProcessingTypeGrid.GetColumnStyle(nameof(AQISPremisesIdAndProcessingType.PremisesId)) as ZCodeFindBoxColumnStyleInfo;
				AssertEquals("ModuleID - Registry is off", Enterprise.ZArchitecture.Modules.ModuleIDs.Premises, premisesIdColumn.ModuleID);
			}

			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var control = new AQISUserControl())
			{
				var premisesIdColumn = control.aQISPremisesIdProcessingTypeGrid.GetColumnStyle(nameof(AQISPremisesIdAndProcessingType.PremisesId)) as ZCodeFindBoxColumnStyleInfo;
				AssertEquals("ModuleID - Registry is on", Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList, premisesIdColumn.ModuleID);
			}
		}

		[ExpectNoExceptions]
		public void TestInstantiation()
		{
			using (var form = new ZForm(Factory.NewWithValidTestData<AUOrgSupplierPart>()))
			using (var control = new AQISUserControl())
			{
				form.Controls.Add(control);
				form.Show();
			}
		}
	}
}
