using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AllEntryLineCPDecQuestion))]
	sealed class AllEntryLineCPDecQuestionTest : BusinessObjectCollectionTestCase
	{
		public void TestLoadingCPDecsAndMergedLinesAreIndepedent()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entry.MergedLines.AddNew();
			entryLine1.Questions.AddNew();

			var entryLine2 = entry.MergedLines.AddNew();
			entryLine2.CL_CustomsPostedStatus = Customs.Business.EntryLineStatusList.Codes.DeletePending;
			entryLine2.Questions.AddNew();

			var entryLine3 = entry.MergedLines.AddNew();
			entryLine3.Questions.AddNew();

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var mock = factory2.LoadMoq<CusEntryHeader>(entry.PK);
			mock.Protected().Setup<Customs.Business.IAllCusEntryLineCollection<Customs.Business.CusEntryLine>>("GetAllEntryLinesCollection").Returns(new EntryLinesCollectionForTest(mock.Object));

			AssertEquals("there should be two entryLines. Accessing this causes MergedLines.Load() which touches AllCPDecQuestions after one element is added", 2, mock.Object.MergedLines.Count);
			AssertEquals("There should be two questions", 2, mock.Object.AllCPDecQuestions.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			return new AllEntryLineCPDecQuestion(entryHeader);
		}

		sealed class EntryLinesCollectionForTest : Customs.Business.AllCusEntryLineCollection<CusEntryLine>
		{
			public EntryLinesCollectionForTest(CusEntryHeader entryHeader)
				: base(entryHeader)
			{
				this.entryHeader = entryHeader;
			}
			readonly CusEntryHeader entryHeader;

			protected override void SetCollectionRelationships(BusinessObject dependent)
			{
				base.SetCollectionRelationships(dependent);
				_ = entryHeader.AllCPDecQuestions.Count;
			}
		}
	}
}
