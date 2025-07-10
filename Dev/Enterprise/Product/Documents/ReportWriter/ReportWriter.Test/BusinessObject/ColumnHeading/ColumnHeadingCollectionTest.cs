using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ReportWriter.Testing
{
	[TestedType(typeof(ColumnHeadingCollection))]
	sealed class ColumnHeadingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ColumnHeadingCollection>
	{
		#region Implementation

		protected override ColumnHeadingCollection GetCollectionToTest()
		{
			return Coll;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ColumnHeading(BizObj);
		}

		ReportBizObj BizObj
		{
			get { return bizObj ?? (bizObj = new ReportBizObj(Factory)); }
		}
		ReportBizObj bizObj;

		ColumnHeadingCollection Coll
		{
			get { return fColl ?? (fColl = new ColumnHeadingCollection(BizObj)); }
		}
		ColumnHeadingCollection fColl;

		#endregion
	}
}
