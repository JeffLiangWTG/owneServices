using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	public abstract class CASSHOTFileDataRowTest : TestCase
	{
		public virtual void TestPublicFields()
		{
			DataRowForTest.SetField(CASSHOTFileDataRow.Schema.RecordType, "AWB");
			AssertEquals("AWB", DataRowForTest.RecordType);
		}

		protected CASSHOTFileDataRow DataRowForTest
		{
			get { return dataRowForTest ?? (dataRowForTest = GetCASSHOTFileDataRowForTest()); }
		}
		CASSHOTFileDataRow dataRowForTest;

		protected abstract CASSHOTFileDataRow GetCASSHOTFileDataRowForTest();
	}
}
