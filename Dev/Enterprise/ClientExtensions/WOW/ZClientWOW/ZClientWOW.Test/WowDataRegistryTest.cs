using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WowDataRegistry))]
	public class WowDataRegistryTest : RegistryItemSetTestCaseWithFactory<WowDataRegistry>
	{
		public void TestVisibleItems()
		{
			AssertVisible(ItemSet.UnmatchedDataItemsAccountRaw);
			AssertVisible(ItemSet.EnableOrderNumberFountainRaw);
			AssertVisible(ItemSet.DeclarationImporterRaw);
			AssertVisible(ItemSet.FilterEmailImportFilesBySpecificSubject, true);
			AssertVisible(ItemSet.ContainerCustomsDeclLastCreatedRaw, true);
			AssertVisible(ItemSet.DeclarationInvoiceExportDirectoryRaw);
			AssertVisible(ItemSet.DeclarationInvoiceExportLastRunRaw);
			AssertVisible(ItemSet.DailyTaskExecuteTimeRaw);
			AssertVisible(ItemSet.MidnightTaskExecuteTimeRaw);
		}

		public void TestDeclarationImporter()
		{
			AssertEquals("Declaration Importer", Guid.Empty, ItemSet.DeclarationImporter);
			Guid importerPK = Guid.NewGuid();
			ItemSet.DeclarationImporter = importerPK;
			AssertEquals("Declaration Importer", importerPK, ItemSet.DeclarationImporter);
		}

		public void TestGetDeclarationImporter()
		{
			AssertEquals("Declaration Importer", Guid.Empty, ItemSet.GetDeclarationImporter(GlbBranch.CurrentBranch));
			Guid importerPK = Guid.NewGuid();
			ItemSet.SetDeclarationImporter(GlbBranch.CurrentBranch, importerPK);
			AssertEquals("Declaration Importer", importerPK, ItemSet.GetDeclarationImporter(GlbBranch.CurrentBranch));
		}

		public void TestIsUnmatchedDataItemsAccountValid()
		{
			ItemSet.UnmatchedDataItemsAccount = ZGuid.Empty;
			AssertEquals("IsUnmatchedDataItemsAccountValid with empty account", false, ItemSet.IsUnmatchedDataItemsAccountValid(Factory));
			ItemSet.UnmatchedDataItemsAccount = ZGuid.NewZGuid();
			AssertEquals("IsUnmatchedDataItemsAccountValid with invalid account", false, ItemSet.IsUnmatchedDataItemsAccountValid(Factory));
			ZGuid someOrgPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			ItemSet.UnmatchedDataItemsAccount = someOrgPK;
			AssertEquals("IsUnmatchedDataItemsAccountValid with ok account", true, ItemSet.IsUnmatchedDataItemsAccountValid(Factory));
		}

		public void TestUnmatchedDataItemsAccount()
		{
			ItemSet.UnmatchedDataItemsAccount = ZGuid.Empty;
			AssertEquals("UnmatchedDataItemsAccount empty", ZGuid.Empty, ItemSet.UnmatchedDataItemsAccount);
			ZGuid someOrgPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			ItemSet.UnmatchedDataItemsAccount = someOrgPK;
			AssertEquals("UnmatchedDataItemsAccount populated", someOrgPK, ItemSet.UnmatchedDataItemsAccount);
		}

		public void TestFilterEmailImportFilesBySpecificSubject()
		{
			AssertEquals("Should be a hidden registry item for developers only", true, ItemSet.FilterEmailImportFilesBySpecificSubject.HasOption(RegistryOptions.IsOnlyForDevelopers));
			AssertEquals("Default should be true", true, ItemSet.FilterEmailImportFilesBySpecificSubject.Value);
			ItemSet.FilterEmailImportFilesBySpecificSubject.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("FilterEmailImportFilesBySpecificSubject", true, ItemSet.FilterEmailImportFilesBySpecificSubject.Value);
			ItemSet.FilterEmailImportFilesBySpecificSubject.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("FilterEmailImportFilesBySpecificSubject", false, ItemSet.FilterEmailImportFilesBySpecificSubject.Value);
		}

		public void TestContainerCustomsDeclLastCreated()
		{
			ZDateTime testDate = ZDateTime.Now.AddDays(10).Date;
			ItemSet.ContainerCustomsDeclLastCreated = testDate;
			AssertEquals("ContainerCustomsDeclLastCreated", testDate, ItemSet.ContainerCustomsDeclLastCreated.Date);
		}

		public void TestEnableOrderNumberFountain()
		{
			ItemSet.EnableOrderNumberFountain = true;
			AssertEquals("EnableOrderNumberFountain", ZBool.True, ItemSet.EnableOrderNumberFountain);
			ItemSet.EnableOrderNumberFountain = false;
			AssertEquals("EnableOrderNumberFountain", ZBool.False, ItemSet.EnableOrderNumberFountain);
		}

		public void TestDailyTaskExecuteTime()
		{
			AssertEquals(new TimeSpan(18, 0, 0), ItemSet.DailyTaskExecuteTime);
			ItemSet.DailyTaskExecuteTime = new TimeSpan(19, 33, 11);
			AssertEquals(new TimeSpan(19, 33, 0), ItemSet.DailyTaskExecuteTime);
			ItemSet.DailyTaskExecuteTimeRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "blabla");
			AssertExceptionThrown(typeof(ArgumentException), delegate()
			{
				TimeSpan time = ItemSet.DailyTaskExecuteTime;
			});
		}

		public void TestMidnightTaskExecuteTime()
		{
			AssertEquals(new TimeSpan(0, 0, 0), ItemSet.MidnightTaskExecuteTime);
			ItemSet.MidnightTaskExecuteTime = new TimeSpan(19, 45, 11);
			AssertEquals(new TimeSpan(19, 45, 0), ItemSet.MidnightTaskExecuteTime);
			ItemSet.MidnightTaskExecuteTimeRaw.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "blabla");
			AssertExceptionThrown(typeof(ArgumentException), delegate()
			{
				TimeSpan time = ItemSet.MidnightTaskExecuteTime;
			});
		}

		#region ServiceTasks
		#region DeclarationInvoice
		public void TestDeclarationInvoiceExportDirectory()
		{
			AssertEquals("DeclarationInvoiceExportDirectory", Env.TempPath, ItemSet.DeclarationInvoiceExportDirectoryRaw.DefaultValue);
			var tempPath = TempForTest.TempPath;
			ItemSet.DeclarationInvoiceExportDirectory = tempPath;
			AssertEquals("DeclarationInvoiceExportDirectory", tempPath, ItemSet.DeclarationInvoiceExportDirectory);
		}

		public void TestDeclarationInvoiceExportLastRun()
		{
			AssertEquals("ContainerCustomsDeclLastCreated", DateTime.MinValue, ItemSet.DeclarationInvoiceExportLastRunRaw.DefaultValue);
			ZDateTime testDate = ZDateTime.Now.AddDays(-1).Date;
			ItemSet.DeclarationInvoiceExportLastRun = testDate;
			AssertEquals("ContainerCustomsDeclLastCreated", testDate, ItemSet.DeclarationInvoiceExportLastRun.Date);
		}

		#endregion
		#endregion
		public void TestDisplayOption()
		{
			Assert(!ItemSet.CASSKIRKDisplayOption);
			ItemSet.CASSKIRKDisplayOptionRaw.SetValue(Guid.Empty, Guid.NewGuid(), Guid.Empty, true);
			Assert(!ItemSet.CASSKIRKDisplayOption);
			ItemSet.CASSKIRKDisplayOptionRaw.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			Assert(ItemSet.CASSKIRKDisplayOption);
		}

		public void TestGetDisplayOption()
		{
			Assert(!ItemSet.GetCASSKIRKDisplayOption(GlbBranch.CurrentBranch));
			ItemSet.CASSKIRKDisplayOptionRaw.SetValue(Guid.Empty, Guid.NewGuid(), Guid.Empty, true);
			Assert(!ItemSet.GetCASSKIRKDisplayOption(GlbBranch.CurrentBranch));
			ItemSet.CASSKIRKDisplayOptionRaw.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true);
			Assert(ItemSet.GetCASSKIRKDisplayOption(GlbBranch.CurrentBranch));
		}
	}
}
