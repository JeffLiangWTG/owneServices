using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DummyHolder : DummyBusinessObject, ICPQAAttacheeHolder
	{
		public DummyHolder(BusinessObjectFactory factory, DataRow row)
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
