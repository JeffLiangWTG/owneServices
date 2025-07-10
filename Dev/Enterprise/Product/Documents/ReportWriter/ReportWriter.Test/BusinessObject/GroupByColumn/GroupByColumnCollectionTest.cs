using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ReportWriter.Testing
{
	[TestedType(typeof(GroupByColumnCollection))]
	sealed class GroupByColumnCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GroupByColumnCollection>
	{
		#region Implementation

		protected override GroupByColumnCollection GetCollectionToTest()
		{
			return Coll;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GroupByColumn(Factory);
		}

		ReportBizObj BizObj
		{
			get { return bizObj ?? (bizObj = new ReportBizObj(Factory)); }
		}
		ReportBizObj bizObj;

		Area Area
		{
			get { return area ?? (area = new Area(BizObj)); }
		}
		Area area;

		GroupByColumnCollection Coll
		{
			get { return fColl ?? (fColl = new GroupByColumnCollection(Area)); }
		}
		GroupByColumnCollection fColl;

		#endregion
	}
}
