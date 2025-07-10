using System;
using System.Collections.Generic;
using CargoWise.Common;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectNestedTest : TestCaseWithFactory
	{
		public void TestCloneUsingADifferentFactory()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "AAA";
			dummy.Z0_NVarCharMax = "BBB";

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject cloned = (DummyBusinessObject)dummy.Clone(new BusinessObjectCloneArgs(factory2, Array.Empty<string>(), null, true));
			AssertEquals(factory2, cloned.Factory);
		}

		[NUnit.Framework.ExpectException(typeof(ArgumentNullException))]
		public void TestCopyPersistentValuesFromWithoutBO()
		{
			DummyBusinessObject copiedDummy = Factory.New<DummyBusinessObject>();
			copiedDummy.CopyPersistentValuesFrom(null, new BusinessObjectCloneArgs());
		}

		public void TestCopyPersistentValuesFromWithColumnsToExcludeFromCopy()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "AAA";
			dummy.Z0_NVarCharMax = "BBB";

			List<string> exclusionList = new List<string>();
			exclusionList.Add(dummy.Z0_VarCharMaxInfo.Name);
			DummyBusinessObject copiedDummy1 = Factory.New<DummyBusinessObject>();
			copiedDummy1.CopyPersistentValuesFrom(dummy, new BusinessObjectCloneArgs(exclusionList.ToArray()));
			AssertEquals("copiedDummy1.Z0_VarCharMax", "", copiedDummy1.Z0_VarCharMax);
			AssertEquals("copiedDummy1.Z0_NVarCharMax", "BBB", copiedDummy1.Z0_NVarCharMax);

			exclusionList = new List<string>();
			exclusionList.Add(dummy.Z0_NVarCharMaxInfo.Name);
			DummyBusinessObject copiedDummy2 = Factory.New<DummyBusinessObject>();
			copiedDummy2.CopyPersistentValuesFrom(dummy, new BusinessObjectCloneArgs(exclusionList.ToArray()));
			AssertEquals("copiedDummy2.Z0_VarCharMax", "AAA", copiedDummy2.Z0_VarCharMax);
			AssertEquals("copiedDummy2.Z0_NVarCharMax", "", copiedDummy2.Z0_NVarCharMax);

			exclusionList = new List<string>();
			DummyBusinessObject copiedDummy3 = Factory.New<DummyBusinessObject>();
			copiedDummy3.CopyPersistentValuesFrom(dummy, new BusinessObjectCloneArgs(exclusionList.ToArray()));
			AssertEquals("copiedDummy3.Z0_VarCharMax", "AAA", copiedDummy3.Z0_VarCharMax);
			AssertEquals("copiedDummy3.Z0_NVarCharMax", "BBB", copiedDummy3.Z0_NVarCharMax);
		}

		public void TestCopyPersistentValuesFrom_WhenObjectWasDetached()
		{
			DummyBusinessObject sourceDummy = Factory.New<DummyBusinessObject>();
			DummyBusinessObject sourceDummyDeleted = Factory.New<DummyBusinessObject>();
			sourceDummyDeleted.Delete();

			DummyBusinessObject destDummy = Factory.New<DummyBusinessObject>();
			DummyBusinessObject destDummyDeleted = Factory.New<DummyBusinessObject>();
			destDummyDeleted.Delete();

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => destDummy.CopyPersistentValuesFrom(sourceDummy));

				AssertNoExceptionThrown(() => destDummyDeleted.CopyPersistentValuesFrom(sourceDummy));
				AssertContains("CopyPersistentValuesFrom_RowNotInTableException", ErrorReporter.LastKeyReported);
				AssertContains("sourceObject.Row.RowState is", ErrorReporter.LastMessageReported);
				AssertContains("current.Row.RowState is", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();

				AssertNoExceptionThrown(() => destDummy.CopyPersistentValuesFrom(sourceDummyDeleted));
				AssertContains("CopyPersistentValuesFrom_RowNotInTableException", ErrorReporter.LastKeyReported);
				AssertContains("sourceObject.Row.RowState is", ErrorReporter.LastMessageReported);
				AssertContains("current.Row.RowState is", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			});
		}

		public void TestCopyPersistentValuesFromWithBlobFieldsInDatabase()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			byte[] bytes = { 1, 2 };
			dummy.Z0_VarBinaryMax = bytes;
			dummy.Z0_VarCharMax = "Text";

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			DummyBusinessObject loadedDummy = newFactory.Load<DummyBusinessObject>(dummy.GetPKInternal());
			DummyBusinessObject anotherDummy = newFactory.New<DummyBusinessObject>();

			anotherDummy.CopyPersistentValuesFrom(loadedDummy);
			AssertEquals("Z0_VarBinaryMax", bytes, anotherDummy.Z0_VarBinaryMax);
			AssertEquals("Z0_VarCharMax", "Text", anotherDummy.Z0_VarCharMax);
		}

		public void TestCloneInternalWithColumnsToExcludeFromCopy()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "AAA";
			dummy.Z0_NVarCharMax = "BBB";

			List<string> exclusionList = new List<string>();
			exclusionList.Add(dummy.Z0_VarCharMaxInfo.Name);
			DummyBusinessObject copiedDummy1 = (DummyBusinessObject)dummy.CloneInternal(new BusinessObjectCloneArgs(exclusionList.ToArray()));
			AssertEquals("copiedDummy1.Z0_VarCharMax", "", copiedDummy1.Z0_VarCharMax);
			AssertEquals("copiedDummy1.Z0_NVarCharMax", "BBB", copiedDummy1.Z0_NVarCharMax);

			exclusionList = new List<string>();
			exclusionList.Add(dummy.Z0_NVarCharMaxInfo.Name);
			DummyBusinessObject copiedDummy2 = (DummyBusinessObject)dummy.CloneInternal(new BusinessObjectCloneArgs(exclusionList.ToArray()));
			AssertEquals("copiedDummy2.Z0_VarCharMax", "AAA", copiedDummy2.Z0_VarCharMax);
			AssertEquals("copiedDummy2.Z0_NVarCharMax", "", copiedDummy2.Z0_NVarCharMax);

			exclusionList = new List<string>();
			DummyBusinessObject copiedDummy3 = (DummyBusinessObject)dummy.CloneInternal(new BusinessObjectCloneArgs(exclusionList.ToArray()));
			AssertEquals("copiedDummy3.Z0_VarCharMax", "AAA", copiedDummy3.Z0_VarCharMax);
			AssertEquals("copiedDummy3.Z0_NVarCharMax", "BBB", copiedDummy3.Z0_NVarCharMax);
		}

		public void TestCloneInternalGetPropertiesThatShouldNotBeCopied()
		{
			var dummy = Factory.New<DummyWithDependentsAndClusterKeyBusinessObject>();
			var cloneArgs = new BusinessObjectCloneArgs();
			_ = (DummyWithDependentsAndClusterKeyBusinessObject)dummy.CloneInternal(cloneArgs);
			var tablePrefix = DummyWithDependentsAndClusterKeyBusinessObject.Schema.TablePrefix + "_";
			AssertContainsExactElementsInAnyOrder(new[] { DummyWithDependentsAndClusterKeyBusinessObject.Schema.PK, tablePrefix + "SystemCreateTimeUtc", tablePrefix + "SystemCreateUser", tablePrefix + "SystemCreateBranch", tablePrefix + "SystemCreateDepartment", tablePrefix + "IsValid", dummy.ClusterKeyPty.Name }, cloneArgs.GetExcludedColumns());
		}
	}
}
