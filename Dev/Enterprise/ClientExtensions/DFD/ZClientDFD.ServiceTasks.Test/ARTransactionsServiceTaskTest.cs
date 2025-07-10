using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.DFD.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.DFD.ServiceTasks.Testing
{
	[TestedType(typeof(ARTransactionsServiceTask))]
	public class ARTransactionsServiceTaskTest : ServiceTaskTestCase<ARTransactionsServiceTask>
	{
		[TestDate(2007, 1, 29, 1, 30, 0)]
		public void TestExecute()
		{
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(-1);
			OrgHeader debtor = Factory.New<OrgHeader>();
			debtor.OH_Code = "DEBTORTEST";
			debtor.OH_RL_NKClosestPort = "HKHKG";
			debtor.OH_IsDebtor = true;
			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_OH = debtor.PK;
			invoice.AH_PostDate = currentDate.AddDays(-2).AddHours(2);
			invoice.AH_OSExTaxAmount = 10m;
			invoice.AH_ConsolidatedInvoiceRef = "S0000001";
			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			jobHeader.JH_ParentID = shipment.PK;
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			invoice.AH_JH = jobHeader.PK;
			ARAdjustmentNote adjustmentNote = Factory.New<ARAdjustmentNote>();
			adjustmentNote.AH_OH = debtor.PK;
			adjustmentNote.AH_PostDate = currentDate.AddDays(-2).AddHours(3);
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.AddDays(1);
			TransactionsTypesToExportBusinessObject registryBusinessObject = new TransactionsTypesToExportBusinessObject();
			registryBusinessObject.ARAdjustmentNote = true;
			registryBusinessObject.ARCreditNote = true;
			registryBusinessObject.ARInvoice = true;
			registryBusinessObject.ARJobRelated = true;
			registryBusinessObject.ARNonJobRelated = true;
			DFDDataRegistry.Instance.ARTransactionsTypesToExport.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryBusinessObject);
			ARTransactionsServiceTask processor = new ARTransactionsServiceTask();
			InitialiseTaskSchedule(processor, out _, out var scheduleGovernor);
			scheduleGovernor.SetBranchPk(Env.CurrentBranchPK);
			using (new ZArchitecture.Environment.User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				RunTaskSchedule(processor);
			}

			ZString expectedDirectory = Path.Combine(Env.TempPath, "DFD");
			DirectoryInfo directoryPath = new DirectoryInfo(expectedDirectory);
			try
			{
				processor.RunTask();
				Assert(Directory.Exists(expectedDirectory));
				directoryPath = new DirectoryInfo(Env.TempPath);
				AssertEquals(2, directoryPath.GetFiles().Length);
				AssertEquals("DEBTORTEST-HKHKG-ADJ-00001000-20070127-20070129-01-30.xml", directoryPath.GetFiles()[0].Name);
				AssertEquals("DEBTORTEST-HKHKG-INV-00001000-S0000001-20070127-20070129-01-30.xml", directoryPath.GetFiles()[1].Name);
			}
			finally
			{
				TempDirectory.DeleteDirectory(directoryPath.FullName);
			}

			invoice = Factory.Load<ARInvoice>(invoice.PK);
			AssertEquals("DEX event with reference should be created", 1, invoice.Logs.Find(l => l.SL_SE_NKEvent == ZArchitecture.Business.Events.DataExport.Code && l.SL_Reference == "Transaction has been exported via ZD3 on 29-Jan-07").Count());
		}

		public void TestDates()
		{
			AssertEquals(DFDDataRegistry.Instance.ARTransactionsExportItem.Value.LastRunDateTime, currentDate.AddDays(-2));
			AssertEquals(DFDDataRegistry.Instance.ARTransactionsExportItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).LastRunDateTime, currentDate.AddDays(-2));
			currentSettings.LastRunDateTime = currentDate.AddDays(-3);
			DFDDataRegistry.Instance.ARTransactionsExportItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, currentSettings);
			AssertEquals(DFDDataRegistry.Instance.ARTransactionsExportItem.Value.LastRunDateTime, currentDate.AddDays(-3));
			AssertEquals(DFDDataRegistry.Instance.ARTransactionsExportItem.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty).LastRunDateTime, currentDate.AddDays(-3));
			AssertEquals(DFDDataRegistry.Instance.ARTransactionsExportItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).LastRunDateTime, currentDate.AddDays(-2));
		}

		// No nudging: no queue table.
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		public void TestHasNoQueueProvider()
		{
			var queueProvider = GetHostedServiceQueueProviderInstanceByServiceTaskCode("ZD3");
			AssertNull("No queue table for this service task.", queueProvider);
		}

		AdditionalSettingsRegistryBusinessObject currentSettings;
		ZDateTime currentDate;
		protected override void SetUpCore()
		{
			base.SetUpCore();
			currentDate = new ZDateTime(2007, 1, 29, 1, 30, 0);
			currentSettings = new AdditionalSettingsRegistryBusinessObject(Factory);
			currentSettings.Directory = Env.TempPath;
			currentSettings.Interval = 1;
			currentSettings.LastRunDateTime = currentDate.AddDays(-2);
			currentSettings.NextRunDateTime = currentDate.AddDays(-1);
			currentSettings.GroupPK = Core.Constants.Groups.PostMastersGroupPK;
			currentSettings.ExportFileName = "ARTest";
			DFDDataRegistry.Instance.ARTransactionsExportItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, currentSettings);
		}
	}
}
