using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ReportWriter.Testing
{
	[TestedType(typeof(DataSourceCollection))]
	sealed class DataSourceCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DataSourceCollection>
	{
		#region Implementation

		protected override DataSourceCollection GetCollectionToTest()
		{
			return Coll;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DataSource(BizObj);
		}

		ReportBizObj BizObj
		{
			get { return bizObj ?? (bizObj = new ReportBizObj(Factory)); }
		}
		ReportBizObj bizObj;

		DataSourceCollection Coll
		{
			get { return fColl ?? (fColl = new DataSourceCollection(BizObj)); }
		}
		DataSourceCollection fColl;

		#endregion
	}
}
