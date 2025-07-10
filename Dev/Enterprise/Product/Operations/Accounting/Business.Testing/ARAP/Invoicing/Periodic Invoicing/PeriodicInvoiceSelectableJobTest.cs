using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Periodic_Invoicing.Testing
{
	[TestedType(typeof(PeriodicInvoiceSelectableJob))]
	internal sealed class PeriodicInvoiceSelectableJobTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCorrectSetupOfWrappingProperties()
		{
			var nonWrappingPropertyNames = new[] { "Lookups", "Parent", "Item", "Validation" };
			var readOnlyWrappingProperties =
				from p in typeof(PeriodicInvoiceSelectableJob).GetProperties()
				where
					!nonWrappingPropertyNames.Contains(p.Name) &&                   // Not one of our known "non wrapping" names
					p.PropertyType != typeof(ZPropertyInfo) &&                      // Not a property info
					typeof(Job).GetProperty(p.Name) != null &&                      // Is on the job class
					typeof(NonPersistentBusinessObject).GetProperty(p.Name) == null // Is something we have created on top of the standard BO properties							
				select p;

			AssertEquals(@"This is the expected number of properties taken from Job class. 
							If you have added another one, create a test case in 'TestJobWrappingProperties' then adjust this number.", 30, readOnlyWrappingProperties.Count());

			foreach (var wrappingProperty in readOnlyWrappingProperties)
			{
				Assert("Job wrapping property '" + wrappingProperty.Name + "' must be readonly.", !wrappingProperty.CanWrite);
			}
		}

		[TestDate(2016, 10, 20)]
		public void TestMatchesFilterWithDBOnlyQuery()
		{
			var jobReasonCollection = new JobProfitLossReasonCodeCollection();
			var code = jobReasonCollection.AddNew();
			code.Code = "TST";

			var jobReasonParam = new JobProfitLossRequiringReasonParameters();
			jobReasonParam.LossThreshold = 0.05;
			jobReasonParam.ProfitThreshold = 0.05;
			var jobStatus = jobReasonParam.JobStatusCollection.AddNew();
			jobStatus.Code = JobHeaderStatus.JobInvoiced.Code;

			var changeStatusCollection = new CodeDescriptionBoolDisallowNewCollection();
			var changeStatus = changeStatusCollection.AddNew();
			changeStatus.Code = JobHeaderStatus.Working.Code;
			changeStatus.Bool = false;

			Factory.Save();

			PreparePeriodicInvoiceTestData();
			Factory.Save();

			AssertEquals(ZString.Empty, ParentJob.JH_ProfitLossReasonCode);

			using (AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				jobReasonCollection))
			using (AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				jobReasonParam))
			using (AccountingConfigurationRegistry.Instance.SetJobStatusToInvoicedWhenFirstARInvoicePosted.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				changeStatusCollection))
			{
				var periodicInvoiceJob = new PeriodicInvoiceSelectableJob(Factory, new DummyObserver(), ParentJob);
				AssertEquals(ParentJob, ((IWrapPersistentBizO)periodicInvoiceJob).Parent);
				var dbOnlyQuery = new ZDBOnlyQuery(typeof(JobHeader));
				dbOnlyQuery.AddToFilter(JobHeaderSchema.JH_ProfitLossReasonCode, ZString.Empty);
				Assert(periodicInvoiceJob.MatchesFilter(dbOnlyQuery));
				dbOnlyQuery.AddToFilter(JobHeaderSchema.JH_ProfitLossReasonCode, "AAA");
				Assert(!periodicInvoiceJob.MatchesFilter(dbOnlyQuery));
			}
		}

		[TestDate(2016, 10, 20)]
		public void TestValidateReasonCodeWhenReasonCodeMissing()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2016);

			var jobReasonCollection = new JobProfitLossReasonCodeCollection();
			var code = jobReasonCollection.AddNew();
			code.Code = "TST";

			var jobReasonParam = new JobProfitLossRequiringReasonParameters();
			jobReasonParam.LossThreshold = 0.05;
			jobReasonParam.ProfitThreshold = 0.05;
			var jobStatus = jobReasonParam.JobStatusCollection.AddNew();
			jobStatus.Code = JobHeaderStatus.JobInvoiced.Code;

			var changeStatusCollection = new CodeDescriptionBoolDisallowNewCollection();
			var changeStatus = changeStatusCollection.AddNew();
			changeStatus.Code = JobHeaderStatus.Working.Code;
			changeStatus.Bool = false;

			Factory.Save();

			PreparePeriodicInvoiceTestData();
			Factory.Save();

			AssertEquals(ZString.Empty, ParentJob.JH_ProfitLossReasonCode);

			using (AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				jobReasonCollection))
			using (AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				jobReasonParam))
			using (AccountingConfigurationRegistry.Instance.SetJobStatusToInvoicedWhenFirstARInvoicePosted.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				changeStatusCollection))
			{
				var periodicInvoiceJob = new PeriodicInvoiceSelectableJob(Factory, new DummyObserver(), ParentJob);
				periodicInvoiceJob.IncludeInThePeriodicInvoice = true;
				periodicInvoiceJob.RunPreSaveValidation();
				var errors = periodicInvoiceJob.GetErrors();

				AssertEquals(1, errors.Count());
				AssertContains("The Profit/Loss threshold settings require Profit/Loss reason to be set on this job before posting any AR invoices. Posting is prohibited and Invoice preview not available until Profit / Loss reason has been entered.", errors.First().Message);

				periodicInvoiceJob.IncludeInThePeriodicInvoice = false;
				periodicInvoiceJob.RunPreSaveValidation();

				AssertNoErrors("Should have no errors when not included.", periodicInvoiceJob);
			}
		}

		[TestDate(2016, 10, 20)]
		public void TestValidateReasonCodeWhenRegistryDoNotRequireReasonCode()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2016);

			var jobReasonCollection = new JobProfitLossReasonCodeCollection();
			var code = jobReasonCollection.AddNew();
			code.Code = "TST";

			var changeStatusCollection = new CodeDescriptionBoolDisallowNewCollection();
			var changeStatus = changeStatusCollection.AddNew();
			changeStatus.Code = JobHeaderStatus.Working.Code;
			changeStatus.Bool = false;

			Factory.Save();

			PreparePeriodicInvoiceTestData();
			Factory.Save();

			AssertEquals(ZString.Empty, ParentJob.JH_ProfitLossReasonCode);

			using (AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				jobReasonCollection))
			using (AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				new JobProfitLossRequiringReasonParameters()))
			using (AccountingConfigurationRegistry.Instance.SetJobStatusToInvoicedWhenFirstARInvoicePosted.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				changeStatusCollection))
			{
				var periodicInvoiceJob = new PeriodicInvoiceSelectableJob(Factory, new DummyObserver(), ParentJob);
				periodicInvoiceJob.IncludeInThePeriodicInvoice = true;
				periodicInvoiceJob.RunPreSaveValidation();
				var errors = periodicInvoiceJob.GetErrors();

				AssertEquals("Should report no error when Job P&L does not require reason code.", 0, errors.Count());
			}
		}

		[TestDate(2016, 10, 20)]
		public void TestValidateReasonCodeWhenJobStatusNotChangeToINV()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2016);

			var jobReasonCollection = new JobProfitLossReasonCodeCollection();
			var code = jobReasonCollection.AddNew();
			code.Code = "TST";

			var jobReasonParam = new JobProfitLossRequiringReasonParameters();
			jobReasonParam.LossThreshold = 0.05;
			jobReasonParam.ProfitThreshold = 0.05;
			var jobStatus = jobReasonParam.JobStatusCollection.AddNew();
			jobStatus.Code = JobHeaderStatus.JobInvoiced.Code;

			Factory.Save();

			PreparePeriodicInvoiceTestData();
			Factory.Save();

			AssertEquals(ZString.Empty, ParentJob.JH_ProfitLossReasonCode);

			using (AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				jobReasonCollection))
			using (AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				jobReasonParam))
			using (AccountingConfigurationRegistry.Instance.SetJobStatusToInvoicedWhenFirstARInvoicePosted.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				new CodeDescriptionBoolDisallowNewCollection()))
			{
				var periodicInvoiceJob = new PeriodicInvoiceSelectableJob(Factory, new DummyObserver(), ParentJob);
				periodicInvoiceJob.IncludeInThePeriodicInvoice = true;
				periodicInvoiceJob.RunPreSaveValidation();
				var errors = periodicInvoiceJob.GetErrors();

				AssertEquals("Should report no error when Job status does not change to INV after post.", 0, errors.Count());
			}
		}

		[TestDate(2016, 10, 20)]
		public void TestValidateReasonCodeWhenNoRegistryReasonCodeSettings()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2016);

			var changeStatusCollection = new CodeDescriptionBoolDisallowNewCollection();
			var changeStatus = changeStatusCollection.AddNew();
			changeStatus.Code = JobHeaderStatus.Working.Code;
			changeStatus.Bool = false;

			Factory.Save();

			PreparePeriodicInvoiceTestData();
			Factory.Save();

			AssertEquals(ZString.Empty, ParentJob.JH_ProfitLossReasonCode);

			using (AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				new JobProfitLossReasonCodeCollection()))
			using (AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				new JobProfitLossRequiringReasonParameters()))
			using (AccountingConfigurationRegistry.Instance.SetJobStatusToInvoicedWhenFirstARInvoicePosted.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				changeStatusCollection))
			{
				var periodicInvoiceJob = new PeriodicInvoiceSelectableJob(Factory, new DummyObserver(), ParentJob);
				periodicInvoiceJob.IncludeInThePeriodicInvoice = true;
				periodicInvoiceJob.RunPreSaveValidation();
				var errors = periodicInvoiceJob.GetErrors();

				AssertEquals("Should report no error when there is no reason settings for job P&L.", 0, errors.Count());
			}
		}

		[TestDate(2016, 10, 20)]
		public void TestValidateReasonCodeWithProperReason()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2016);

			var jobReasonCollection = new JobProfitLossReasonCodeCollection();
			var code = jobReasonCollection.AddNew();
			code.Code = "TST";

			var jobReasonParam = new JobProfitLossRequiringReasonParameters();
			jobReasonParam.LossThreshold = 0.05;
			jobReasonParam.ProfitThreshold = 0.05;
			var jobStatus = jobReasonParam.JobStatusCollection.AddNew();
			jobStatus.Code = JobHeaderStatus.JobInvoiced.Code;

			var changeStatusCollection = new CodeDescriptionBoolDisallowNewCollection();
			var changeStatus = changeStatusCollection.AddNew();
			changeStatus.Code = JobHeaderStatus.Working.Code;
			changeStatus.Bool = false;

			Factory.Save();

			PreparePeriodicInvoiceTestData();
			ParentJob.JH_ProfitLossReasonCode = "TST";
			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				jobReasonCollection))
			using (AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				jobReasonParam))
			using (AccountingConfigurationRegistry.Instance.SetJobStatusToInvoicedWhenFirstARInvoicePosted.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				changeStatusCollection))
			{
				var periodicInvoiceJob = new PeriodicInvoiceSelectableJob(Factory, new DummyObserver(), ParentJob);
				periodicInvoiceJob.IncludeInThePeriodicInvoice = true;
				periodicInvoiceJob.RunPreSaveValidation();
				var errors = periodicInvoiceJob.GetErrors();

				AssertEquals("Should report no errors when job has proper reason code.", 0, errors.Count());
			}
		}

		public void TestTaxBranch()
		{
			var currentBranchPK = GlbBranch.CurrentBranch.PK;
			PreparePeriodicInvoiceTestData();
			Factory.Save();

			ParentJob.JH_GB_TaxBranch = currentBranchPK;

			var periodicInvoiceJob = new PeriodicInvoiceSelectableJob(Factory, new DummyObserver(), ParentJob);
			AssertNotNull(periodicInvoiceJob.JH_GB_TaxBranchInfo);
			AssertEquals("TaxBranch", currentBranchPK, periodicInvoiceJob.JH_GB_TaxBranch);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PeriodicInvoiceSelectableJob(Factory, new DummyObserver(), Factory.NewJobWithValidTestDataForTesting<Job>());
		}

		void PreparePeriodicInvoiceTestData()
		{
			var type = TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;

			var shipment = TestObjectCreator.CreateShipment("S1");
			ParentJob = TestObjectCreator.CreateJob(shipment);
			ParentJob.LocalChargesPK = TestObjectCreator.ABIGAS.PK;
			ParentJob.JH_Status = JobHeaderStatus.Working.Code;

			var charge = ParentJob.Charges.AddNew();
			charge.FillWithValidTestData();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_OSSellAmt = 100m;
			charge.JR_LocalSellAmt = 100m;
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
		}

		Job ParentJob;

		TestObjectCreator testObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}

		#endregion
	}
}
