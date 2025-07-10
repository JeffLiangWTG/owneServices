using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ReportWriter.Testing
{
	[TestedType(typeof(DocumentHeaderColumnCollection))]
	sealed class DocumentHeaderColumnCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentHeaderColumnCollection>
	{
		#region Implementation

		protected override DocumentHeaderColumnCollection GetCollectionToTest()
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

		DocumentHeaderRow Row
		{
			get { return row ?? (row = new DocumentHeaderRow(BizObj)); }
		}
		DocumentHeaderRow row;

		DocumentHeaderColumnCollection Coll
		{
			get { return fColl ?? (fColl = new DocumentHeaderColumnCollection(Row)); }
		}
		DocumentHeaderColumnCollection fColl;

		#endregion
	}
}
