using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalCopy.Business.Testing
{
	[TestedType(typeof(StmUniversalCopy))]
	class StmUniversalCopyTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTaskIsDeletedWhenCopyJobIsDeleted()
		{
			var copyTemplate = Factory.New<UniversalCopyTemplate>();
			copyTemplate.S9_ModuleID = "XXX_UC";

			var dummy1 = Factory.New<DummyBusinessObject>();
			var copyJob1 = Factory.New<StmUniversalCopy>();
			copyJob1.SUC_S9_CopyTemplate = copyTemplate.PK;
			copyJob1.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
			copyJob1.SUC_CopyObjectId = dummy1.PK;
			var task1 = Factory.New<StmUniversalCopyScheduleTask>();
			task1.S5_ParentID = copyJob1.PK;

			var dummy2 = Factory.New<DummyBusinessObject>();
			var copyJob2 = Factory.New<StmUniversalCopy>();
			copyJob2.SUC_S9_CopyTemplate = copyTemplate.PK;
			copyJob2.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
			copyJob2.SUC_CopyObjectId = dummy2.PK;
			var task2 = Factory.New<StmUniversalCopyScheduleTask>();
			task2.S5_ParentID = copyJob2.PK;

			Factory.Save();

			AssertEquals(false, copyJob1.IsDeleted);
			AssertEquals(false, task1.IsDeleted);

			AssertEquals(false, copyJob2.IsDeleted);
			AssertEquals(false, task2.IsDeleted);

			copyJob1.Delete();
			Factory.Save();

			AssertEquals(true, copyJob1.IsDeleted);
			AssertEquals(true, task1.IsDeleted);

			AssertEquals(false, copyJob2.IsDeleted);
			AssertEquals(false, task2.IsDeleted);
		}

		public void TestCopyObjectDescription_WhenCopyObjectIsNull()
		{
			var copyTemplate = Factory.New<UniversalCopyTemplate>();
			copyTemplate.S9_ModuleID = "XXX_UC";

			var copyJob = Factory.New<StmUniversalCopy>();
			copyJob.SUC_S9_CopyTemplate = copyTemplate.PK;
			copyJob.SUC_CopyObjectTableCode = DummyBizoSchema.Constants.Prefix;
			copyJob.SUC_CopyObjectId = Guid.Empty;
			Factory.Save();

			AssertEquals(ZString.Empty, copyJob.CopyObjectDescription);
		}
	}
}
