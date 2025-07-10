using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ReportWriter.Testing
{
	[TestedType(typeof(DocumentHeaderRowCollection))]
	sealed class DocumentHeaderRowCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentHeaderRowCollection>
	{
		#region Implementation

		protected override DocumentHeaderRowCollection GetCollectionToTest()
		{
			return Coll;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DocumentHeaderRow(BizObj);
		}

		ReportBizObj BizObj
		{
			get { return bizObj ?? (bizObj = new ReportBizObj(Factory)); }
		}
		ReportBizObj bizObj;

		DocumentHeaderRowCollection Coll
		{
			get { return fColl ?? (fColl = new DocumentHeaderRowCollection(BizObj)); }
		}
		DocumentHeaderRowCollection fColl;

		#endregion
	}
}
