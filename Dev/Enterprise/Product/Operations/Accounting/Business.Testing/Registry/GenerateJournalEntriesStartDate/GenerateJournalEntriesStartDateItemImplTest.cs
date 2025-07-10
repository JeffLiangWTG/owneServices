using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(GenerateJournalEntriesStartDateItemImpl))]
	class GenerateJournalEntriesStartDateItemImplTest : DateTimeRegistryItemTest
	{
		[TestDate(2023, 3, 28, 12, 12, 12)]
		public void TestSetValueCore()
		{
			var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			var year = ZDateTime.Today.Year;

			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(year, companyPK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

			var registryItem = GetNewRegistryItem();
			registryItem.SetValue(companyPK, Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1));

			var cdcRegistryItem = AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate;
			var value = cdcRegistryItem.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
			AssertEquals("Value is set to today", ZDateTime.UtcNow.ToSmallDateTime().ToDateTime(), value);

			var stmData = new StmData.Loader(new BusinessObjectFactory()).LoadTop1(cdcRegistryItem.Name, companyPK, Guid.Empty);
			AssertEquals("Only one log", 1, stmData.Logs.DatabaseCount);
		}

		[TestDate(2023, 01, 01)]
		public void TestNudgeGLQAfterSetValue()
		{
			var year = ZDateTime.Today.Year;
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(year, GlbCompany.CurrentCompany.PK.ToGuid(), AccountingPeriodTestHelper.CalendarType.CalendarYear);

			var isNugde = false;
			var mock = new Mock<IServiceTaskNudger>();
			mock.Setup(m => m.NudgeServiceTask(It.IsAny<string>(), It.IsAny<TimeSpan?>())).Callback(() => { isNugde = true; });

			var registry = AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate;

			using (ObjectFactory.Substitute(mock.Object))
			{
				registry.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1));

				registry.OnAllValuesSaved();

				mock.Verify(x => x.NudgeServiceTask("GLQ", It.IsAny<TimeSpan?>()), Times.Once());
				Assert("GLQ should be nudged", isNugde);
			}
		}

		protected override StronglyTypedRegistryItem<DateTime, DateTime> GetNewRegistryItem()
		{
			return new GenerateJournalEntriesStartDateItemImpl("GenerateJournalEntriesStartDateItemImpl", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.Company, RegistryOptions.Default);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator.SetControlAccountsForGenerateJournalEntriesStartDate();
		}

		TestObjectCreator TestObjectCreator => fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(new BusinessObjectFactory()));
		TestObjectCreator fTestObjectCreator;
	}
}
