using System;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DebtorBalanceExport.Testing
{
	sealed class DebtorBalanceExporterTest : TestCaseWithFactory
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1074:DoNotHardcodeDeveloperName", Justification = "Testing")]
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2009, 3, 3, 13, 35, 9)]
		public void TestProcess()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var dataExporter = new DebtorBalanceExporter(Factory);

			var exportedFile = Path.Combine(ExportDirectory, "DebtorBalances_20090303133509.xml");
			AssertEquals("File doesn't exist", true, !File.Exists(exportedFile));
			dataExporter.Process(new NotificationBuffer());

			AssertEquals("Export File is generated", true, File.Exists(exportedFile));
			var expectedOutput = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\DebtorBalance\Testing\DebtorBalances_20090303133509.xml"))
				.Replace("<LoginName> ulysses</LoginName>", @"<LoginName>" + GlbStaff.CurrentUser.GS_LoginName + @"</LoginName>"); // ulysses removal from test file
			this.AssertXMLEqualsByDiff("Export File", expectedOutput, File.ReadAllText(exportedFile));
		}

		public void TestExportWithErrors()
		{
			DebtorBalanceExporterForTest dataExporter = new DebtorBalanceExporterForTest(Factory);
			NotificationBuffer buffer = new NotificationBuffer();
			dataExporter.Process(buffer);
			ZString expectedErrorMesg = DebtorBalanceExporter.GetCompanyLevelErrorMesg(GlbCompany.CurrentCompany.GC_Code, "test exception");
			AssertEquals("buffer has errors", true, buffer.HasErrors);
			AssertEquals("buffer error message", true, buffer.AsString.Contains(expectedErrorMesg));
		}

		public void TestExportDirectoryNotExist()
		{
			SystemDataRegistry.Instance.DebtorOutstandingBalancesExportDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "oooo");

			DebtorBalanceExporter dataExporter = new DebtorBalanceExporter(Factory);
			NotificationBuffer buffer = new NotificationBuffer();
			dataExporter.Process(buffer);

			ZString expectedError = DebtorBalanceExporter.GetRegistryNotSet(GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("buffer has errors", true, buffer.HasErrors);
			AssertEquals("buffer error message", true, buffer.AsString.Contains(expectedError));
		}

		class DebtorBalanceExporterForTest : DebtorBalanceExporter
		{
			public DebtorBalanceExporterForTest(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override void ExportDataToXml(INotifications notifications, ZString tempFileName, DebtorBalanceRecordForExport[] debtorBalArray)
			{
				throw new Exception("test exception");
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			ExportDirectory = Path.Combine(Env.TempPath, "DebtorBalance");
			ObjectCreator.CreateTestPeriods(ZDateTime.Today);
			SetupTestDirectory();
			TransactionExportTestDataHelper helper = new TransactionExportTestDataHelper(Factory);
			helper.WIP.RelatedJobCharge.JR_OH_SellAccount = helper.Header.PK;
			helper.WIP.AL_OH = helper.Header.PK;
			var invoiceWithNoDebtor = helper.ObjectCreator.CreateARInvoice<ARInvoice>("", helper.ObjectCreator.AUD, 1.0m, objectCreator.AALSHI);
			invoiceWithNoDebtor.AH_OH = ZGuid.Empty;
			Factory.Save();
		}

		TestObjectCreator ObjectCreator
		{
			get { return objectCreator ?? (objectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator objectCreator;

		ZString ExportDirectory;

		void SetupTestDirectory()
		{
			if (!Directory.Exists(ExportDirectory))
			{
				Directory.CreateDirectory(ExportDirectory);
			}

			SystemDataRegistry.Instance.DebtorOutstandingBalancesExportDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ExportDirectory);
		}

		void DeleteDirectory()
		{
			try
			{ Directory.Delete(ExportDirectory, true); }
			catch (UnauthorizedAccessException) { }
			catch (IOException) { }
		}

		protected override void TearDown()
		{
			base.TearDown();
			DeleteDirectory();
		}
	}
}
