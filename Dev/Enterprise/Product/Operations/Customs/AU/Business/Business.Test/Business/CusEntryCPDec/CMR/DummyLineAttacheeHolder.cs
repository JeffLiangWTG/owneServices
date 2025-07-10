using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DummyLineAttacheeHolder : DummyLineAttachee, ISelfHoldingLineAttachee
	{
		public DummyLineAttacheeHolder(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ICPQAAttacheeHolder Members

		public ICPQAHeaderAttachee[] HeadersExposed = System.Array.Empty<ICPQAHeaderAttachee>();
		public ICPQAHeaderAttachee[] Headers
		{
			get { return HeadersExposed; }
		}

		public ICPQALineAttachee[] LinesExposed = System.Array.Empty<ICPQALineAttachee>();
		public ICPQALineAttachee[] Lines
		{
			get { return LinesExposed; }
		}

		public CachedAnsweredQuestions CachedQuestionsExposed;
		public CachedAnsweredQuestions CachedQuestions
		{
			get
			{
				if (CachedQuestionsExposed == null)
				{
					CachedQuestionsExposed = new CachedAnsweredQuestions();
				}
				return CachedQuestionsExposed;
			}
		}

		#endregion
	}
}
