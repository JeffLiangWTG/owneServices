using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.OperationalActions;
using Enterprise.Customs.FR.Messaging.Interfaces.COD;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.COD.Testing
{
	public class ArticleWrapperTest : TestCaseWithFactory
	{
		public void TestEntryNumber()
		{
			AssertEquals("EntryNumber1", wrapper.EntryNumber);
		}

		public void TestDirection()
		{
			AssertEquals(EU.Business.MessageTypeList.Codes.Import, wrapper.Direction);
		}

		public void TestItemNumber()
		{
			AssertEquals("1", wrapper.ItemNumber);
		}

		public void TestDocuments()
		{
			AssertEquals(documents, wrapper.Documents);
		}

		public void TestApur()
		{
			AssertNull(wrapper.Apur);

			var headerApplicator = new FrCreditCODApplicator(Factory);
			var itemApplicator = headerApplicator.FrCreditCODItemApplicators.AddNew();
			wrapper = new ArticleWrapper(itemApplicator, documents);
			AssertNotNull(wrapper.Apur);
			AssertType<ApurWrapper>(wrapper.Apur);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.EntryNumber = "EntryNumber1";
			entryLine = entryHeader.AllEntryLines.AddNew() as CusEntryLine;
			entryLine.CL_LineNumber = 1;

			documents = new List<IDocAapurer>();
			wrapper = new ArticleWrapper(entryLine, documents);
		}

		CusEntryLine entryLine;
		List<IDocAapurer> documents;
		ArticleWrapper wrapper;
	}
}
