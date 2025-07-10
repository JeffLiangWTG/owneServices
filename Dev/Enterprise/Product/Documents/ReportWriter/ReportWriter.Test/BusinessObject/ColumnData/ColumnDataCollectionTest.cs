using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ReportWriter.Testing
{
	[TestedType(typeof(ColumnDataCollection))]
	sealed class ColumnDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ColumnDataCollection>
	{
		#region Implementation

		protected override ColumnDataCollection GetCollectionToTest()
		{
			return Coll;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ColumnData(BizObj);
		}

		ReportBizObj BizObj
		{
			get { return bizObj ?? (bizObj = new ReportBizObj(Factory)); }
		}
		ReportBizObj bizObj;

		ColumnDataCollection Coll
		{
			get { return fColl ?? (fColl = new ColumnDataCollection(BizObj)); }
		}
		ColumnDataCollection fColl;

		#endregion
	}
}
