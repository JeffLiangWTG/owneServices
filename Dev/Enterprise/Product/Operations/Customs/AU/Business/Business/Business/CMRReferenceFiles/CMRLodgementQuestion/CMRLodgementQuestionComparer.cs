using System.Collections;

namespace Enterprise.Customs.AU.Declaration.Business
{
	internal sealed class CMRLodgementQuestionComparer : IComparer
	{
		public int Compare(object x, object y)
		{
			var q1 = (CMRLodgementQuestion)x;
			var q2 = (CMRLodgementQuestion)y;
			int result;
			if (q1.CQ_LodgementQuestionEndDate.IsEmpty)
			{
				result = 1;
			}
			else if (q2.CQ_LodgementQuestionEndDate.IsEmpty)
			{
				result = -1;
			}
			else
			{
				result = q1.CQ_LodgementQuestionEndDate.CompareTo(q2.CQ_LodgementQuestionEndDate);
			}
			return result;
		}
	}
}
