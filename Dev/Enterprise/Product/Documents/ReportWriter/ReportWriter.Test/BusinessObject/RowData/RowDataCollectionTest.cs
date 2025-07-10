using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ReportWriter.Testing
{
	[TestedType(typeof(RowDataCollection))]
	sealed class RowDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<RowDataCollection>
	{
		#region Implementation

		protected override RowDataCollection GetCollectionToTest()
		{
			return Coll;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RowData(BizObj);
		}

		ReportBizObj BizObj
		{
			get { return bizObj ?? (bizObj = new ReportBizObj(Factory)); }
		}
		ReportBizObj bizObj;

		RowDataCollection Coll
		{
			get { return fColl ?? (fColl = new RowDataCollection(BizObj)); }
		}
		RowDataCollection fColl;

		#endregion
	}
}
