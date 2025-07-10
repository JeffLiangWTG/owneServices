using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ReportWriter.Testing
{
	[TestedType(typeof(AreaCollection))]
	sealed class AreaCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AreaCollection>
	{
		#region Implementation

		protected override AreaCollection GetCollectionToTest()
		{
			return Coll;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new Area(BizObj);
		}

		ReportBizObj BizObj
		{
			get { return bizObj ?? (bizObj = new ReportBizObj(Factory)); }
		}
		ReportBizObj bizObj;

		AreaCollection Coll
		{
			get { return fColl ?? (fColl = new AreaCollection(BizObj)); }
		}
		AreaCollection fColl;

		#endregion
	}
}
