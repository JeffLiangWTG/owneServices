#region Test
#if DEBUG

using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Common.GUI.Import
{
	[TestedType(typeof(ImportStatus))]
	public class ImportStatusTest : NonPersistentBusinessObjectTestCase
	{
		public void TestReset()
		{
			status.ErrorRecords++;
			status.ImportedRecords++;
			status.Reset();
			AssertEquals(0, status.ImportedRecords);
			AssertEquals(0, status.ErrorRecords);
		}

		protected override void SetUp()
		{
			base.SetUp();
			status = new ImportStatus();
		}
		ImportStatus status;
	}
}


#endif
#endregion
