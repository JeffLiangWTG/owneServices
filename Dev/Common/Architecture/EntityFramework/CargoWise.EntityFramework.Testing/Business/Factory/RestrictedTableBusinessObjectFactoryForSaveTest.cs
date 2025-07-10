using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed public class RestrictedTableBusinessObjectFactoryForSaveTest : TestCaseWithFactory
	{
		public void TestDontReportInvalidBusinessObjectLoadedWithNoChanges()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			var factory = new RestrictedTableBusinessObjectFactoryForSaveImpl([GlbStaffSchema.Constants.TableName]);
			factory.Load<DummyBusinessObject>(dummy.PK);
			factory.Save();

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}

		public void TestDontReportValidBusinessObjectSaved()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			var factory = new RestrictedTableBusinessObjectFactoryForSaveImpl([DummyBizoSchema.Constants.TableName]);
			var dummyLoad = factory.Load<DummyBusinessObject>(dummy.PK);
			dummyLoad.Z0_Code = "TEST";
			factory.Save();

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}

		public void TestReportInvalidBusinessObjectChangesSaved()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			var factory = new RestrictedTableBusinessObjectFactoryForSaveImpl([GlbStaffSchema.Constants.TableName]);
			var dummyLoad = factory.Load<DummyBusinessObject>(dummy.PK);
			dummyLoad.Z0_Code = "TEST";
			factory.Save();

			AssertContains(
@$"Found BusinessObject from invalid table to be saved in Test Save factory
Tables: [DummyBizo]", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestReportMultipleInvalidBusinessObjectChangesSaved()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			var factory = new RestrictedTableBusinessObjectFactoryForSaveImpl([GlbStaffSchema.Constants.TableName]);
			var dummyLoad = factory.Load<DummyBusinessObject>(dummy.PK);
			dummyLoad.Z0_Code = "TEST";
			var dummy2 = factory.NewWithValidTestData<DummyDependantBusinessObject>();
			dummy2.ZD1_Code = "TEST";

			factory.Save();

			AssertContains(
@$"Found BusinessObject from invalid table to be saved in Test Save factory
Tables: [DummyBizo;DummyDependentBizo]", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestAdditionalInformationIncludedInReport()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			var factory = new RestrictedTableBusinessObjectFactoryForSaveImpl([GlbStaffSchema.Constants.TableName]);
			factory.AdditionalInformation = "Test Information";
			var dummyLoad = factory.Load<DummyBusinessObject>(dummy.PK);
			dummyLoad.Z0_Code = "TEST";
			factory.Save();

			AssertContains(
@$"Found BusinessObject from invalid table to be saved in Test Save factory
Tables: [DummyBizo]
Test Information", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}

	class RestrictedTableBusinessObjectFactoryForSaveImpl : RestrictedTableBusinessObjectFactoryForSave
	{
		readonly string[] allowedTablesForTest;

		public RestrictedTableBusinessObjectFactoryForSaveImpl(string[] allowedTablesForTest)
		{
			this.allowedTablesForTest = allowedTablesForTest;
			NameForDebugging = "Test Save";
		}

		protected override ICollection<string> GetAllowedTablesToSave()
		{
			return allowedTablesForTest;
		}
	}
}
