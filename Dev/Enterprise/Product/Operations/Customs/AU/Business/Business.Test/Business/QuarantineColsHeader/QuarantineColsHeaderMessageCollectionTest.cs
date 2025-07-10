using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(QuarantineColsHeaderMessageCollection))]
	sealed class QuarantineColsHeaderMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultFilterEM_ApplicationCode()
		{
			AssertContains("EM_ApplicationCode = 'COL'", Collection.CompleteFilter.LiteralTextSqlFormatted);
		}

		public void TestAddMessage() => CombineAssertions(() =>
		{
			var message = Factory.New<EDIMessage>();
			message.ClearHasChanges();
			Collection.Add(message);
			AssertEquals("EM_LinkUniqueID", colsHeader.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable", colsHeader.TableName, message.EM_LinkTable);
			AssertEquals("HasChanges = true because EM_LinkUniqueID is updated", true, message.HasChanges);
		});

		public void TestAddMessageWithOriginalLinkUniqueID()
		{
			var docPivot = colsHeader.EDocPivotCollection.AddNew();
			var message = Factory.New<EDIMessage>();
			message.EM_LinkUniqueID = docPivot.PK;
			message.EM_LinkTable = docPivot.TableName;
			message.ClearHasChanges();
			Collection.Add(message);
			AssertEquals("EM_LinkUniqueID", docPivot.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable", docPivot.TableName, message.EM_LinkTable);
			AssertEquals("HasChanges", false, message.HasChanges);
		}

		protected override BusinessObjectCollection GetCollectionToTest() => new QuarantineColsHeaderMessageCollection(colsHeader);

		protected override void SetUp()
		{
			base.SetUp();
			colsHeader = Factory.New<QuarantineColsHeader>();
		}
		QuarantineColsHeader colsHeader;
	}
}
