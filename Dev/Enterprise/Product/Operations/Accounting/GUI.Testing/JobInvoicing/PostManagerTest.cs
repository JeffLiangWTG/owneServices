using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class PostManagerTest : TestCase
	{
		[CargoWise.Data.Testing.UseSnapshotProtection]
		public void TestWhenPostChargeAreDeletedInAnotherFactoryNotTriggeringCV()
		{
			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);

			new AccountingPeriodTestHelper(factory).PostPeriodsForEntireYear(ZDateTime.Today.Year);
			new AccountingPeriodTestHelper(factory).PostPeriodsForEntireYear(ZDateTime.Today.Year + 1);

			var valuesForTest = new RevenueRecognitionCollection();
			var setting = valuesForTest.AddNew();
			setting.JobType = RevenueRecognitionLookups.JobTypeAdditionalCodes.All;
			setting.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			setting.Mode = Core.Constants.TransportModes.All;
			setting.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.PostDateOfFirstARTransaction;

			var oldValue = AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.Value;

			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new BackDateInvoicesConfiguration());

			try
			{
				var shipment = testObjectCreator.CreateShipment("TEST");

				var job = testObjectCreator.CreateJob(shipment, false);
				job.LocalChargesPK = testObjectCreator.ABIGAS.PK;
				job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();

				var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "desc", testObjectCreator.AUD, 100m, testObjectCreator.AALSHI, testObjectCreator.AUD, 100m, testObjectCreator.ABIGAS);
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

				testObjectCreator.ABIGAS.CompanyData.OB_ARClientNumber = "TEST";

				factory.Save();

				var wrapper = new PostManagerGUIWrapperTest.TestPostManagerGUIWrapperForTestConcurrency(JobInvoicingPostingOption.Revenue, factory, job, charge.PK);
				wrapper.Post();

				if (ExceptionReporterTestListener.Instance.Count > 0)
				{
					AssertNotContains("Should not fail any critical validation.", "REV transaction line that does not have a related job charge.",
					ExceptionReporterTestListener.Instance[0].Message);
				}

				AssertEquals("Should trigger the Concurrency Exception", "Please try to post these charges again. Posting has failed. One or more of the charges you are attempting to post has been modified by another user. The other users changes have been merged with yours.",
					UnitTestUserNotification.Instance.LastMessage.Text);
				ErrorReporter.Clear();
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, oldValue);
			}
		}
	}
}
