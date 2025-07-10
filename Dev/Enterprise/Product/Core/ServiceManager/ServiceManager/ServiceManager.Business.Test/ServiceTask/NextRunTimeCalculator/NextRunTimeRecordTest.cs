using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(NextRunTimeRecord))]
	class NextRunTimeRecordTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2024, 1, 1, 1, 1, 1)]
		public void TestNextRunTime()
		{
			// Act
			var record = new NextRunTimeRecord { NextRunTime = ZDateTime.UtcNow };

			// Assert
			AssertEquals(TestDateAttribute.Date, record.NextRunTime);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewNextRunTimeRecord();
		}

		public static NextRunTimeRecord GetNewNextRunTimeRecord()
		{
			return new NextRunTimeRecord();
		}

		#endregion
	}
}
