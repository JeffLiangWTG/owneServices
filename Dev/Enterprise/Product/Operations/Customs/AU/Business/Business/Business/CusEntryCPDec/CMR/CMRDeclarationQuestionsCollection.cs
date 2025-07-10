
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	//public struct CachedQuestionKey
	//{
	//  public CachedQuestionKey(ZGuid ParentPK, ZInt QuestionID, ZDateTime StartDate)
	//  {
	//    this.ParentPK = ParentPK;
	//    this.QuestionID = QuestionID;
	//    this.StartDate = StartDate;
	//  }

	//  public ZGuid ParentPK;
	//  public ZInt QuestionID;
	//  public ZDateTime StartDate;
	//}

	public class CMRDeclarationQuestionsCollection : DependentBusinessObjectCollection<CMRCusEntryCPDec, JobDeclaration>
	{
		public CMRDeclarationQuestionsCollection(JobDeclaration parent)
			: base(parent)
		{
			this.Parent = parent;
		}
		protected readonly JobDeclaration Parent;

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		//public bool AreAllCPDecQuestionsAnswered
		//{
		//  get
		//  {
		//    if (CachedAreCPQuestionsAnswered == null)
		//    {
		//      CachedAreCPQuestionsAnswered = new CachedProperty<bool>(Factory, delegate
		//      {
		//        bool Result = true;
		//        foreach (CMRCusEntryCPDec Question in this)
		//        {
		//          if (!Question.IsValidationSuspended)
		//          {
		//            Question.Validation.ValidateON_AnswerCode();
		//          }
		//          if (Question.IsOptionalQuestion)
		//          {
		//            Result &= !Question.HasNotifications;
		//          }
		//          else
		//          {
		//            Result &= Question.IsAnswered && !Question.HasNotifications;
		//          }
		//          if (!Result) break;
		//        }
		//        return Result;
		//      });
		//    }
		//    return CachedAreCPQuestionsAnswered.Value;
		//  }
		//}
		//CachedProperty<bool> CachedAreCPQuestionsAnswered;

		//public bool IsAnyCPQuestionAnswered
		//{
		//  get
		//  {
		//    if (CachedIsAnyCPQuestionAnswered == null)
		//    {
		//      CachedIsAnyCPQuestionAnswered = new CachedProperty<bool>(Factory, GetIsAnyCPQuestionAnswered);
		//    }
		//    return CachedIsAnyCPQuestionAnswered.Value;
		//  }
		//}
		//CachedProperty<bool> CachedIsAnyCPQuestionAnswered;

		//bool GetIsAnyCPQuestionAnswered()
		//{
		//  bool Result = false;
		//  foreach (CMRCusEntryCPDec Question in this)
		//  {
		//    if (Question.IsAnswered)
		//    {
		//      Result = true;
		//      break;
		//    }
		//  }
		//  return Result;
		//}

		public CMRCusEntryCPDec GetQuestionWithID(ZInt questionID)
		{
			foreach (CMRCusEntryCPDec cPDec in this)
			{
				if (cPDec.ON_CPDecNum == questionID)
				{
					return cPDec;
				}
			}
			return null;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(JoinCondition.And, CusEntryCPDecSchema.ON_CH, SQLComparisonOperator.Equal, null);
			return result;
		}
	}
}
