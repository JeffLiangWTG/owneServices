using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ChequeModule))]
	sealed class ChequeModuleTest : ZModuleBasherTest
	{
		#region Fields

		TestObjectCreator testObjectCreator;

		#endregion

		public void TestAllowedActions()
		{
			using var module = (ChequeModule)ZModuleFactory.Instance.Create(ModuleID);

			Assert(!module.AllowView);
			Assert(!module.AllowEdit);
			Assert(!module.AllowNew);
			Assert(!module.AllowDelete);
		}

		public void TestGetNewGridCollection()
		{
			using var testModule = new ChequeModule();
			testObjectCreator.CreateChequesAndSave();

			var cheques = (BusinessObjectCollection)testModule.GetNewBusinessObjectCollection();
			cheques.Load();

			AssertEquals(2, cheques.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			testObjectCreator = new TestObjectCreator(Factory);
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Cheque;

		protected override void AddTestObjects(IBusinessObjectCollection collection)
			=> collection.Add(Factory.NewWithValidTestData(typeof(AccReceivedCheque)));

		protected override BusinessObject[] GetBusinessObjectsToGetControllersFor()
			=> new BusinessObject[] { Factory.NewWithValidTestData<AccReceivedCheque>() };
	}
}
