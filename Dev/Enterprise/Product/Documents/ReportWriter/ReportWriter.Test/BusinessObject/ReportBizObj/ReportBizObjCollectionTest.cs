using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ReportWriter.Testing
{
	[TestedType(typeof(ReportBizObjCollection))]
	sealed class ReportBizObjCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ReportBizObjCollection>
	{
		#region implementation

		protected override ReportBizObjCollection GetCollectionToTest()
		{
			return Coll;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ReportBizObj(Factory);
		}

		MainBizObj BizObj
		{
			get { return bizObj ?? (bizObj = new MainBizObj(Factory)); }
		}
		MainBizObj bizObj;

		ReportBizObjCollection Coll
		{
			get { return fColl ?? (fColl = new ReportBizObjCollection(BizObj)); }
		}
		ReportBizObjCollection fColl;

		#endregion
	}
}
