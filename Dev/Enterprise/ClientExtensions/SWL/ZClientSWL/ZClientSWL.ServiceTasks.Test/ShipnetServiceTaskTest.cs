using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Client.SWL.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.SWL.ServiceTasks.Testing
{
	[TestedType(typeof(ShipnetServiceTask))]
	public class ShipnetServiceTaskTest : ServiceTaskTestCase<ShipnetServiceTask>
	{
		public void TestProcessShipnetDataExport()
		{
			var processMock = new Mock<ShipnetServiceTask>();
			processMock.CallBase = true;
			SWLDataRegistry.Instance.IsShipnetEnable.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			var process = processMock.Object;
			AssertNotNull(process);
			process.ServiceLogger = new LoggerForTest();
			process.RunTask();
			processMock.Protected()
				.Verify("ProcessShipnetDataExport", Times.Never());
			processMock.VerifyAll();
			AssertCollectionContains("Has Enabled Error", "Can not run Shipnet Data Exporter. Shipnet is not enabled in the registry.", ((LoggerForTest)process.ServiceLogger).LogEntries);

			processMock.Reset();
			processMock.Protected()
				.Setup("ProcessShipnetDataExport");
			SWLDataRegistry.Instance.IsShipnetEnable.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			process = processMock.Object;
			AssertNotNull(process);
			process.RunTask();
			processMock.VerifyAll();
		}

		[TestDate(2016, 12, 5, 9, 30, 0)]
		public void TestMultipleCompanies()
		{
			SetupCompany("DAU");
			SetupCompany("DTW");
			var processor = new ShipnetServiceTask();
			InitialiseTaskSchedule(processor);
			processor.RunTask();
			AssertContains("Task started", "================== Task started ==================", processor.ServiceLogger.ToString());
			AssertContains("Task ended", "================== Task ended ==================", processor.ServiceLogger.ToString());
			AssertContains("Has DAU company", "Processing DAU company...", processor.ServiceLogger.ToString());
			AssertContains("Has current batch interval", "Current Batch Interval UTC: '05-Dec-16 09:29:00' - '05-Dec-16 09:30:00'", processor.ServiceLogger.ToString());
			AssertContains("Has DTW company", "Processing DTW company...", processor.ServiceLogger.ToString());
			AssertContains("Has current batch interval", "Current Batch Interval UTC: '05-Dec-16 09:29:00' - '05-Dec-16 09:30:00'", processor.ServiceLogger.ToString());
		}

		void SetupCompany(String companyCode)
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = companyCode;
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			SWLDataRegistry.Instance.IsShipnetEnable.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			SWLDataRegistry.Instance.ShipnetBackupDirectoryItem.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, Temp.TempPath);
			SWLDataRegistry.Instance.ShipnetHighWaterMarkItem.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddMinutes(-1).ToDateTime());
			OrgHeader shipnetOrg = Factory.NewWithValidTestData<OrgHeader>();
			ShipnetSetupBusinessObject shipnetSetup = new ShipnetSetupBusinessObject(shipnetOrg.Factory);
			shipnetSetup.CompanyDataPK = shipnetOrg.PK;
			shipnetSetup.IsShipnetCarrier = true;
			shipnetSetup.DebtorControlCode = "DEBTORCODE";
			shipnetSetup.CreditorControlCode = "CREDITORCODE";
			var objectCreator = new TestObjectCreator(Factory);
			ShipnetChargeGroup group = shipnetSetup.ChargeGroups.AddNew();
			group.ChargeGroupCode = "GROUPCODE1";
			group.ChargeGroupDescription = "GROUP1 DESCRIPTION";
			group.Charges.AddNew(objectCreator.CC1.PK);
			group = shipnetSetup.ChargeGroups.AddNew();
			group.ChargeGroupCode = "GROUPCODE2";
			group.ChargeGroupDescription = "GROUP2 DESCRIPTION";
			group.Charges.AddNew(objectCreator.CC3.PK);
			SWLDataRegistry.Instance.ShipnetSetupRaw.SetValue(company.PK.ToGuid(), Guid.Empty, shipnetOrg.CompanyData.PK.ToGuid(), shipnetSetup);
			Factory.Save();
		}

		public void TestPurge()
		{
			string tempBackupDirectory = Temp.TempPath;
			try
			{
				var oldFile = Temp.GetTempFileName(tempBackupDirectory);
				using (File.Create(oldFile))
				{
				}

				var fileInfo = new FileInfo(oldFile);
				fileInfo.CreationTime = DateTime.Now.AddDays(-31);
				using (File.Create(Temp.GetTempFileName(Temp.GetNewTempSubdirectory())))
				{
				}

				using (File.Create(Temp.GetTempFileName(Temp.GetNewTempSubdirectory())))
				{
				}

				var processor = new ShipnetServiceTask();
				InitialiseTaskSchedule(processor);
				processor.RunTask();
				AssertEquals(3, Directory.GetFiles(tempBackupDirectory, "*", SearchOption.AllDirectories).Length);
				SWLDataRegistry.Instance.ShipnetBackupDirectoryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tempBackupDirectory);
				processor.RunTask();
				AssertEquals(2, Directory.GetFiles(tempBackupDirectory, "*", SearchOption.AllDirectories).Length);
			}
			finally
			{
				TempDirectory.DeleteDirectory(tempBackupDirectory);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
