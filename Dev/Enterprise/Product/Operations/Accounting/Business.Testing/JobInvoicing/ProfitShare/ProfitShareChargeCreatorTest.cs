using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	public class ProfitShareChargeCreatorTest : TestCaseWithFactory
	{
		public void TestLogCreated()
		{
			var creator = new DummyProfitShareChargeCreator();
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();

			var existingValue = AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value;
			AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);

			ErrorReporter.Clear();
			Assert(!creator.CreateCharges());
			ErrorReporter.Clear();

			AssertNull(dummy.GetLogs().MostRecentLog);

			AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, existingValue);
			Assert(creator.CreateCharges());
			AssertNull(dummy.GetLogs().MostRecentLog);

			creator.ParentObjectForTest = dummy;
			creator.ChargesUpdatedOrCreatedForTest = false;
			Assert(!creator.CreateCharges());
			AssertNull(dummy.GetLogs().MostRecentLog);

			creator.ChargesUpdatedOrCreatedForTest = true;
			Assert(creator.CreateCharges());
			AssertNotNull(dummy.GetLogs().MostRecentLog);
			AssertEquals(Events.ProfitShareCalculated.Code, dummy.GetLogs().MostRecentLog.SL_SE_NKEvent);
			AssertEquals("Profit share charges created: 1", dummy.GetLogs().MostRecentLog.SL_Reference);
		}

		public void TestCreateChargesResult()
		{
			var creator = new DummyProfitShareChargeCreatorForSaveTest();
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();

			var existingValue = AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value;
			AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, existingValue);

			creator.ParentObjectForTest = dummy;
			creator.ChargesUpdatedOrCreatedForTest = false;
			creator.ShouldSaveCharges_ForTest = true;
			AssertEquals("AreChargesUpdatedOrCreated && ShouldSaveCharges", false, creator.CreateCharges());
			AssertEquals("AreChargesUpdatedOrCreated && ShouldSaveCharges", false, creator.CreateCharges(true));
			AssertEquals("AreChargesUpdatedOrCreated", false, creator.CreateCharges(false));

			creator.ChargesUpdatedOrCreatedForTest = true;
			creator.ShouldSaveCharges_ForTest = true;
			AssertEquals("AreChargesUpdatedOrCreated && ShouldSaveCharges", true, creator.CreateCharges());
			AssertEquals("AreChargesUpdatedOrCreated && ShouldSaveCharges", true, creator.CreateCharges(true));
			AssertEquals("AreChargesUpdatedOrCreated", true, creator.CreateCharges(false));

			creator.ChargesUpdatedOrCreatedForTest = true;
			creator.ShouldSaveCharges_ForTest = false;
			AssertEquals("AreChargesUpdatedOrCreated && ShouldSaveCharges", false, creator.CreateCharges());
			AssertEquals("AreChargesUpdatedOrCreated && ShouldSaveCharges", false, creator.CreateCharges(true));
			AssertEquals("AreChargesUpdatedOrCreated", true, creator.CreateCharges(false));

			creator.ChargesUpdatedOrCreatedForTest = false;
			creator.ShouldSaveCharges_ForTest = false;
			AssertEquals("AreChargesUpdatedOrCreated && ShouldSaveCharges", false, creator.CreateCharges());
			AssertEquals("AreChargesUpdatedOrCreated && ShouldSaveCharges", false, creator.CreateCharges(true));
			AssertEquals("AreChargesUpdatedOrCreated", false, creator.CreateCharges(false));
		}

		public void TestSetContext()
		{
			var creator = new DummyProfitShareChargeCreatorForSaveTest();
			var dummy = Factory.New<DummyEnterpriseBusinessObject>();

			var existingValue = AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value;
			AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Guid.Empty);

			creator.ParentObjectForTest = dummy;
			creator.ChargesUpdatedOrCreatedForTest = true;
			creator.ShouldSaveCharges_ForTest = true;
			creator.CreateCharges();
			AssertEquals("Should not set BusinessContext when ProfitShareChargeCodePK is empty", false, dummy.Factory.HasContext(BusinessContext.CreatingProfitShareCharge));

			creator.ChargesUpdatedOrCreatedForTest = false;
			AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, existingValue);
			creator.CreateCharges();
			AssertEquals("Should not set BusinessContext when ChargesUpdatedOrCreatedForTest is false", false, dummy.Factory.HasContext(BusinessContext.CreatingProfitShareCharge));

			creator.ChargesUpdatedOrCreatedForTest = true;
			creator.CreateCharges();
			AssertEquals("Should set BusinessContext", true, dummy.Factory.HasContext(BusinessContext.CreatingProfitShareCharge));
		}

		#region Implementation

		class DummyProfitShareChargeCreator : ProfitShareChargeCreator
		{
			protected override bool CreateChargesCore()
			{
				CreatedChargesCount = 1;
				return ChargesUpdatedOrCreatedForTest;
			}

			public bool ChargesUpdatedOrCreatedForTest = true;

			protected override BusinessObject ParentObject
			{
				get { return ParentObjectForTest; }
			}

			public BusinessObject ParentObjectForTest;
		}

		class DummyProfitShareChargeCreatorForSaveTest : DummyProfitShareChargeCreator
		{
			protected override ZBool SaveCharges()
			{
				return ShouldSaveCharges_ForTest;
			}

			public bool ShouldSaveCharges_ForTest;
		}

		#endregion
	}
}
