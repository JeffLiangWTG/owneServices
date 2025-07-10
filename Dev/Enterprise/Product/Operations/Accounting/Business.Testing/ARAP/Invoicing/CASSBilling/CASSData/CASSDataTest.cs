using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class CASSDataTest : NonPersistentBusinessObjectTestCase
	{
		protected virtual void AssertCommonReadonlyProperties()
		{
			Assert("Record Type", CASSDataForTest.RecordTypeInfo.ReadOnly);
		}

		protected CASSData CASSDataForTest
		{
			get { return dataForTest ?? (dataForTest = GetCASSData()); }
		}
		CASSData dataForTest;

		protected abstract CASSData GetCASSData();

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetCASSData();
		}
	}
}
