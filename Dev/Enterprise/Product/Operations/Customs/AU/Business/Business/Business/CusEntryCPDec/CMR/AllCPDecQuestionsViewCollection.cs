using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AllCPDecQuestionsViewCollection : SubsetBusinessObjectCollection<CMRCusEntryCPDec>
	{
		public AllCPDecQuestionsViewCollection(AllEntryLineCPDecQuestion completeCollection)
			: base(completeCollection)
		{
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			CMRCusEntryCPDec question = (CMRCusEntryCPDec)element;
			bool result = true;
			if (CollectionToFilter.EntryHeader.CPDecQuestionViewType == CPDecQuestionViewTypeList.Codes.Answered)
			{
				result = question.IsAnswered;
			}
			else if (CollectionToFilter.EntryHeader.CPDecQuestionViewType == CPDecQuestionViewTypeList.Codes.Unanswered)
			{
				result = !question.IsAnswered;
			}
			return result;
		}

		new AllEntryLineCPDecQuestion CollectionToFilter
		{
			get { return (AllEntryLineCPDecQuestion)base.CollectionToFilter; }
		}
	}
}
