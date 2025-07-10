using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ConsolRevenue;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting.Testing
{
	[TestedType(typeof(ConsolRevenueApportionForm))]
	public partial class ConsolRevenueApportionFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ConsolRevenueApportionForm(new ConsolRevenueMaster(Factory.New<ForwardingConsol>(), Factory));
		}

		public void TestBusinessContextIsAdded()
		{
			using (var consolRevenueApportionForm = new ConsolRevenueApportionForm(new ConsolRevenueMaster(Factory.New<ForwardingConsol>(), Factory)))
			{
				AssertEquals("RevenueToShipment context should have been added", true, this.Factory.HasContext(BusinessContext.ApportionRevenueToShipment));
			}
		}
		public void TestTaxBranchControls()
		{
			AssertTaxBranchControls(true);
			AssertTaxBranchControls(false);

			void AssertTaxBranchControls(bool enableTaxBranchReporting)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableTaxBranchReporting))
				{
					using (var form = (ConsolRevenueApportionForm)GetFormToBashCore())
					{
						form.Show();

						AssertEquals(!enableTaxBranchReporting, form.ConsolRevenuesControl.ApportionedChargesGrid.GetColumnStyle(JobChargeSchema.JR_GB_CostTaxBranch.Name).IsUnavailable);
						AssertEquals(!enableTaxBranchReporting, form.ConsolRevenuesControl.ApportionedChargesGrid.GetColumnStyle(JobChargeSchema.JR_GB_SellTaxBranch.Name).IsUnavailable);
					}
				}
			}
		}

		public void TestSupplyTypeControls()
		{
			AssertSupplyTypeControls(true);
			AssertSupplyTypeControls(false);

			void AssertSupplyTypeControls(bool enableSupplyTypeClassificationCodes)
			{
				using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableSupplyTypeClassificationCodes))
				{
					using (var form = (ConsolRevenueApportionForm)GetFormToBashCore())
					{
						form.Show();

						AssertEquals(!enableSupplyTypeClassificationCodes, form.ConsolRevenuesControl.RevenueGrid.GetColumnStyle(ConsolRevenue.Schema.SellSupplyType).IsUnavailable);
						AssertEquals(!enableSupplyTypeClassificationCodes, form.ConsolRevenuesControl.ApportionedChargesGrid.GetColumnStyle(Charge.Schema.JR_CostSupplyType).IsUnavailable);
						AssertEquals(!enableSupplyTypeClassificationCodes, form.ConsolRevenuesControl.ApportionedChargesGrid.GetColumnStyle(Charge.Schema.JR_SellSupplyType).IsUnavailable);
					}
				}
			}
		}

		public void TestSaveButtonValidatesRevenueMasterBeforeSaving()
		{
			AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			ConsolRevenueMaster revenueMaster = new ConsolRevenueMaster(consol, Factory);
			ConsolRevenue revenue = revenueMaster.Revenues.AddNew();
			revenue.ChargeCode = TestObjectCreator.CC1.PK;

			revenueMaster.RunPreSaveValidation();
			AssertEquals("Should have errors", true, revenueMaster.HasErrors);

			using (ConsolRevenueApportionForm testForm = new ConsolRevenueApportionForm(revenueMaster))
			{
				testForm.Show();
				Application.DoEvents();
				testForm.ApportionButton.PerformClick();
				AssertEquals("Validation Error Message Should Be Shown", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			revenueMaster.ReleaseMutexes();
		}

		public void TestSaveButtonAsksToReOpenClosedJobBeforeSaving()
		{
			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "USCHI", "C0001");
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			var job1 = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
			var job2 = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
			job1.JH_GE = TestObjectCreator.FESDepartment.PK;
			job2.JH_GE = TestObjectCreator.FESDepartment.PK;

			Factory.Save();

			AssertNotNull("shipment1 must have a Job", shipment1.Job);
			AssertNotNull("shipment2 must have a Job", shipment2.Job);
			shipment2.Job.JH_Status = JobHeaderStatus.Closed.Code;

			ConsolRevenueMaster revenueMaster = new ConsolRevenueMaster(consol, Factory);
			ConsolRevenue revenue = revenueMaster.Revenues.AddNew();

			revenue.ChargeCode = TestObjectCreator.RevenueChargeCode.PK;
			revenue.SellAmount = 200M;
			revenue.ApportionmentMethod = AllocationMethod.Shipment;

			revenueMaster.RunPreSaveValidation();
			AssertEquals("Should have no errors", false, revenueMaster.HasErrors);

			using (ConsolRevenueApportionForm testForm = new ConsolRevenueApportionForm(revenueMaster))
			{
				testForm.Show();
				Application.DoEvents();
				UnitTestUserNotification.Instance.AddOKAnswer();
				testForm.ApportionButton.PerformClick();
				AssertEquals("Request to re-open Job should be shown", "Closed Job(s) :" + shipment2.JobNumber + "\r\nYou are about to reopen these closed jobs. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Job must be re-opened", JobHeaderStatus.Working.Code, shipment2.Job.JH_Status);
			}
		}

		public void TestSaveButtonAsksToReOpenClosedJobBeforeSaving_JobClosed_JobWorkInFactory()
		{
			var consol = TestObjectCreator.CreateConsol("AUSYD", "USCHI", "C0001");
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();

			var job1 = new Job.Loader(shipment1).TryCreateWithoutMutexForTestOnly();
			var job2 = new Job.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
			job1.JH_GE = TestObjectCreator.FESDepartment.PK;
			job2.JH_GE = TestObjectCreator.FESDepartment.PK;

			Factory.Save();

			AssertNotNull("shipment1 must have a Job", shipment1.Job);
			AssertNotNull("shipment2 must have a Job", shipment2.Job);

			var revenueMaster = new ConsolRevenueMaster(consol, Factory);
			var revenue = revenueMaster.Revenues.AddNew();

			revenue.ChargeCode = TestObjectCreator.RevenueChargeCode.PK;
			revenue.SellAmount = 200M;
			revenue.ApportionmentMethod = AllocationMethod.Shipment;

			revenueMaster.RunPreSaveValidation();
			AssertEquals("Should have no errors", false, revenueMaster.HasErrors);

			using (var testForm = new ConsolRevenueApportionForm(revenueMaster))
			{
				testForm.Show();

				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				var jobLoadByNewFactory = newFactory.Load<Job>(shipment2.Job.PK);
				jobLoadByNewFactory.JH_Status = JobHeaderStatus.Closed.Code;
				newFactory.Save();

				Application.DoEvents();
				UnitTestUserNotification.Instance.AddOKAnswer();
				testForm.ApportionButton.PerformClick();
				AssertEquals("Request to re-open Job should be shown", "Closed Job(s) :" + shipment2.JobNumber + "\r\nYou are about to reopen these closed jobs. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Job must be re-opened", JobHeaderStatus.Working.Code, shipment2.Job.JH_Status);
			}
		}

		public void TestSaveButtonIsSavingCorrectly()
		{
			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUABC", "KRPKM", "C00001");
			ForwardingShipment shipment1 = TestObjectCreator.CreateShipment("S00001", consol);

			shipment1.JS_ActualWeight = 2;
			Job job1 = TestObjectCreator.CreateJob(shipment1);
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var master = new ConsolRevenueMaster(consol, Factory);
			ConsolRevenue rev = new ConsolRevenue(master);

			rev.ChargeCode = TestObjectCreator.CC1.PK;
			rev.ApportionmentMethod = AllocationMethod.Shipment;
			rev.SellAmount = 523512M;

			AssertEquals(0, job1.Charges.Count);
			using (ConsolRevenueApportionForm testForm = new ConsolRevenueApportionForm(master))
			{
				testForm.Show();
				Application.DoEvents();
				testForm.AcceptButton.PerformClick();
			}
			Factory.Save();
			AssertEquals(1, job1.Charges.Count);
			AssertEquals(523512M, ((IReceivablesPostingCharge)job1.Charges[0]).LocalSellAmount);
			AssertEquals(TestObjectCreator.CC1.PK, job1.Charges[0].ChargeCode.PK);
			Assert("Charge should be in the database", job1.Charges[0].IsInDatabase);
		}

		public void TestConsolRevenueApportionFormWithMutexException()
		{
			var creator = new TestObjectCreator(Factory);

			var consol = creator.CreateConsol("AUABC", "KRABC", "C00001");
			var shipment = creator.CreateShipment("S00001", consol);
			shipment.JS_ActualWeight = 100m;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			Factory.Save();

			//user opens the billing tab
			var factory2 = new BusinessObjectFactory();
			var shipment2 = factory2.Load<ForwardingShipment>(shipment.PK);
			var loader = new JobHeader.Loader(factory2, shipment2);
			var job = loader.TryLoadOrCreateWithMutex();
			AssertNotNull(job);

			var consolRevMaster = new ConsolRevenueMaster(consol, new BusinessObjectFactory());
			using (var consolRevenueApportionForm = new ConsolRevenueApportionForm(consolRevMaster))
			{
				consolRevenueApportionForm.Show();
			}

			AssertEquals(@"You have created the job S00001 on another form, but haven't saved it yet.
Please close or save other forms that use job S00001 to continue.", UnitTestUserNotification.Instance.LastMessage.Text);

			consolRevMaster.ReleaseMutexes();
			job.Dispose();
		}

		public void TestGovtChargeCodeColumnsVisibility()
		{
			foreach (var enableGovtChargeCode in new[] { true, false })
			{
				using (AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, enableGovtChargeCode))
				{
					using (ConsolRevenueApportionForm form = (ConsolRevenueApportionForm)GetFormToBashCore())
					{
						form.Show();
						Application.DoEvents();

						AssertEquals(!enableGovtChargeCode, form.ConsolRevenuesControl.RevenueGrid.GetColumnStyle(ConsolRevenue.Schema.CostGovtChargeCode).IsUnavailable);
						AssertEquals(!enableGovtChargeCode, form.ConsolRevenuesControl.RevenueGrid.GetColumnStyle(ConsolRevenue.Schema.SellGovtChargeCode).IsUnavailable);
						AssertEquals(!enableGovtChargeCode, form.ConsolRevenuesControl.ApportionedChargesGrid.GetColumnStyle(JobChargeSchema.JR_CostGovtChargeCode.Name).IsUnavailable);
						AssertEquals(!enableGovtChargeCode, form.ConsolRevenuesControl.ApportionedChargesGrid.GetColumnStyle(JobChargeSchema.JR_SellGovtChargeCode.Name).IsUnavailable);
					}
				}
			}
		}

		public void TestSaveDialogShouldNotBeShownWhenMutexHasDisposed()
		{
			TestSaveDialogBeShownDependOnMutex(true, null);
		}

		public void TestSaveDialogShouldBeShownWhenMutexExisting()
		{
			var expectMessage = @"This record has been modified.
Would you like to save the changes?";
			TestSaveDialogBeShownDependOnMutex(false, expectMessage);
		}

		void TestSaveDialogBeShownDependOnMutex(bool isMutexDisposed, string lastMessage)
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0000010");
			var shipments = new List<IJobInvoicingPlugIn>() { shipment1 };

			var costSupporter = new Mock<IGenericJobCostSupporter>();
			costSupporter.Setup(x => x.ShipmentsList).Returns(shipments.ToArray());

			var consol = new Mock<IJobCostingPlugIn>();
			consol.Setup(x => x.CostSupporter).Returns(costSupporter.Object);
			var consolRevenueMaster = new ConsolRevenueMaster(consol.Object, Factory);
			if (isMutexDisposed)
			{
				Factory.SetContext(BusinessContext.ShouldSkipConsolRevenueApportionFormClosing);
			}

			using (var form = new ConsolRevenueApportionForm(consolRevenueMaster))
			{
				form.Show();
				Application.DoEvents();
				form.Close();
				AssertEquals(lastMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		TestObjectCreator testObjectCreator;
		protected TestObjectCreator TestObjectCreator
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
	}
}
