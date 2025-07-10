using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ConsolCosting.Testing
{
	class JobConsolCostParentLinkDeleteCheckerTest : TestCaseWithFactory
	{
		public void TestDeleteDetails_ShouldDisallowDeletion_WhenConsolCostExistsInMemoryAndParentInMemory()
		{
			var expectedExceptionMessage = "dummyConsolNumber cannot be deleted. A Consol Cost has been created for this.";

			var consolCostParent = Factory.New<DummyConsolCostParent>();
			var consolCost = CreateConsolCostWithParent(consolCostParent);

			var exception = AssertExceptionThrown<CannotDeleteException>(() => consolCostParent.Delete());

			AssertContains("Exception Message", expectedExceptionMessage, exception.Message);
			Assert("Delete ConsolCostParent", !consolCostParent.IsDeleted);
		}

		public void TestDeleteDetails_ShouldDisallowDeletion_WhenConsolCostExistsInMemoryAndParentInDatabase()
		{
			var expectedExceptionMessage = "dummyConsolNumber cannot be deleted. A Consol Cost has been created for this.";

			var consolCostParent = Factory.New<DummyConsolCostParent>();
			Factory.Save();

			var consolCost = CreateConsolCostWithParent(consolCostParent);

			var exception = AssertExceptionThrown<CannotDeleteException>(() => consolCostParent.Delete());

			AssertContains("Exception Message", expectedExceptionMessage, exception.Message);
			Assert("Delete ConsolCostParent", !consolCostParent.IsDeleted);
		}

		public void TestDeleteDetails_ShouldDisallowDeletion_WhenConsolCostExistsButNotLoaded()
		{
			var expectedExceptionMessage = "dummyConsolNumber cannot be deleted. A Consol Cost has been created for this.";

			var consolCostParent = Factory.New<DummyConsolCostParent>();

			var secondFactory = new BusinessObjectFactory();
			var consolCost = secondFactory.NewWithValidTestData<JobConsolCost>();
			using (consolCost.ReportSettingParentSuspender.GetSuspender())
			{
				consolCost.E6_ParentID = consolCostParent.PK;
				consolCost.E6_ParentTableCode = consolCostParent.TablePrefix;
			}
			secondFactory.Save();

			var exception = AssertExceptionThrown<CannotDeleteException>(() => consolCostParent.Delete());

			AssertContains("Exception Message", expectedExceptionMessage, exception.Message);
			Assert("Delete ConsolCostParent", !consolCostParent.IsDeleted);
		}

		public void TestDeleteDetails_ShouldDisallowDeletion_WhenConsolCostExistsInDatabaseAndParentInDatabase()
		{
			var expectedExceptionMessage = "dummyConsolNumber cannot be deleted. A Consol Cost has been created for this.";

			var consolCostParent = Factory.New<DummyConsolCostParent>();
			var consolCost = CreateConsolCostWithParent(consolCostParent);
			Factory.Save();

			var exception = AssertExceptionThrown<CannotDeleteException>(() => consolCostParent.Delete());

			AssertContains("Exception Message", expectedExceptionMessage, exception.Message);
			Assert("Delete ConsolCostParent", !consolCostParent.IsDeleted);
		}

		public void TestDeleteDetails_ShouldAllowDeletion_WhenConsolCostDoesNotExist()
		{
			var expectedExceptionMessage = "dummyConsolNumber cannot be deleted. A Consol Cost has been created for this.";

			var consolCostParent = Factory.New<DummyConsolCostParent>();
			var consolCost = CreateConsolCostWithParent(consolCostParent);

			var exception = AssertExceptionThrown<CannotDeleteException>(() => consolCostParent.Delete());

			AssertContains("Precondition: Exception Message", expectedExceptionMessage, exception.Message);
			Assert("Precondition: Delete ConsolCostParent", !consolCostParent.IsDeleted);

			consolCost.Delete();

			AssertNoExceptionThrown(() => consolCostParent.Delete());
			Assert("Delete ConsolCostParent", consolCostParent.IsDeleted);
		}

		public void TestDeleteDetails_ShouldAllowDeletion_WhenBusinessObjectIsNotJobCosting()
		{
			var consolCostParent = Factory.New<DummyBusinessObject>();

			AssertNoExceptionThrown(() => consolCostParent.Delete());

			Assert("Delete ConsolCostParent", consolCostParent.IsDeleted);
		}

		public void TestDeleteDetails_ShouldDisallowDeletion_WhenConsolCostParentHasOverridedPK()
		{
			var expectedExceptionMessage = "dummyConsolNumber cannot be deleted. A Consol Cost has been created for this.";
			var consolCostParentWithOverridedPK = Factory.New<DummyConsolCostParentWithOverridedPK>();

			var consolCostParentOveridedPK = ((IJobCostingPlugIn)(consolCostParentWithOverridedPK as IGenericJobCostPlugInBase)).PK;
			AssertEquals("Precondition: Overrided BusinessObject PK", ZGuid.Empty, consolCostParentOveridedPK);
			AssertNotEquals("Precondition: Base BusinessObject PK", consolCostParentOveridedPK, consolCostParentWithOverridedPK.PK);

			CreateConsolCostWithParent(consolCostParentWithOverridedPK);

			var exception = AssertExceptionThrown<CannotDeleteException>(() => consolCostParentWithOverridedPK.Delete());
			AssertContains("Exception Message", expectedExceptionMessage, exception.Message);
			Assert("Delete consolCostParent", !consolCostParentWithOverridedPK.IsDeleted);
		}

		JobConsolCost CreateConsolCostWithParent(DummyConsolCostParent parent)
		{
			var consolCost = Factory.NewWithValidTestData<JobConsolCost>();
			using (consolCost.ReportSettingParentSuspender.GetSuspender())
			{
				consolCost.E6_ParentID = parent.PK;
				consolCost.E6_ParentTableCode = parent.TablePrefix;
			}

			return consolCost;
		}

		class DummyConsolCostParentWithOverridedPK : DummyConsolCostParent, IJobCostingPlugIn
		{
			public DummyConsolCostParentWithOverridedPK(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			ZGuid IJobCostingPlugIn.PK => ZGuid.Empty;
		}

		class DummyConsolCostParent : DummyBusinessObject, IJobCostingPlugIn
		{
			public DummyConsolCostParent(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			ZString IJobCostingPlugIn.JK_UniqueConsignRef => "dummyConsolNumber";

			RefUNLOCO IJobCostingPlugIn.LoadPort => null;

			RefUNLOCO IJobCostingPlugIn.DischargePort => null;

			JobProfitLossCollection IJobCostingPlugIn.ProfitLossContainer => null;

			decimal IJobCostingPlugIn.ConsolExchangeRate => 0m;

			RefCurrency IJobCostingPlugIn.ConsolCurrency => null;

			bool IJobCostingPlugIn.IsMasterCollect => false;

			OrgHeader IJobCostingPlugIn.ReceivingAgent => null;

			OrgHeader IJobCostingPlugIn.ReceivingAgentAPInvoicingParty => null;

			OrgHeader IJobCostingPlugIn.ReceivingAgentARInvoicingParty => null;

			OrgHeader IJobCostingPlugIn.SendingAgent => null;

			OrgHeader IJobCostingPlugIn.SendingAgentAPInvoicingParty => null;

			OrgHeader IJobCostingPlugIn.SendingAgentARInvoicingParty => null;

			ZString IJobCostingPlugIn.ContainerMode => ZString.Empty;

			ZString IJobCostingPlugIn.ConsolType => ZString.Empty;

			ZString IJobCostingPlugIn.Direction => ZString.Empty;

			ZString IJobCostingPlugIn.Module => throw new NotImplementedException();

			CodeDescriptionPairList IJobCostingPlugIn.PrepaidCollectList => null;

			decimal IJobCostingPlugIn.ExchangeRateForCurrency(RefCurrency currency, ZGuid currentJobConsolCostPK) => 0m;

			void IJobCostingPlugIn.AddNewToLogs(Event @event, ZString reference) => throw new NotImplementedException();

			ZString IJobCostingPlugIn.GetPrepaidCollect(IJobInvoicingPlugIn apportionableJob) => ZString.Empty;

			ZString IJobCostingPlugIn.TransportMode => throw new NotImplementedException();

			IGenericJobCostSupporter IGenericJobCostPlugIn.CostSupporter => throw new NotImplementedException();
		}
	}
}
