using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocCMRCusEntryCPDec : DocumentWrapper
	{
		DocCMRCusEntryCPDec(CMRCusEntryCPDec cMRCusEntryCPDec, BusinessObjectFactory factory)
			: base(cMRCusEntryCPDec, factory)
		{
		}

		public static DocCMRCusEntryCPDec New(CMRCusEntryCPDec cMRCusEntryCPDec, BusinessObjectFactory factory)
		{
			return (cMRCusEntryCPDec == null) ? null : new DocCMRCusEntryCPDec(cMRCusEntryCPDec, factory);
		}

		public static class Schema
		{
			public const string Answer = "Answer";
			public const string Question = "Question";
			public const string QuestionNo = "QuestionNo";
			public const string QuestionNum = "QuestionNum";
		}

		public override string ToString()
		{
			return Question;
		}

		public ZString Question
		{
			get { return CMRCusEntryCPDec.Question; }
		}

		public ZString Answer
		{
			get { return CMRCusEntryCPDec.ON_AnswerCode; }
		}

		public ZString QuestionNo
		{
			get { return CMRCusEntryCPDec.QuestionID; }
		}

		public ZInt QuestionNum
		{
			get { return CMRCusEntryCPDec.ON_CPDecNum; }
		}

		#region Implementation

		CMRCusEntryCPDec CMRCusEntryCPDec
		{
			get { return (CMRCusEntryCPDec)WrappedObject; }
		}
		#endregion
	}
}
