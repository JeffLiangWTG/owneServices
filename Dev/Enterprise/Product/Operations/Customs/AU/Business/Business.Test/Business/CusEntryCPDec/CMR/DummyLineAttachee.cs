using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DummyLineAttachee : DummyBusinessObject, ICPQALineAttachee
	{
		public DummyLineAttachee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ICPQALineAttachee Members

		public CPQuestionKeys CPQuestionKeyExposed;
		public CPQuestionKeys CPQuestionKey
		{
			get { return CPQuestionKeyExposed; }
		}

		public ZString TableCodeExposed;
		public ZString TableCode
		{
			get { return TableCodeExposed; }
		}

		ZBool ICPQALineAttachee.IsRiskCalculatedFromTariff
		{
			get { return true; }
		}

		public bool IsRiskHistorySupportedExposed;
		ZBool ICPQALineAttachee.IsRiskHistorySupported
		{
			get { return IsRiskHistorySupportedExposed; }
		}

		#endregion

		#region ICPQAAttachee Members

		public SchemaGuidColumn FKColumnInCusEntryCPDecTableExposed = CusEntryCPDecSchema.ON_ParentID;
		public SchemaGuidColumn FKColumnInCusEntryCPDecTable
		{
			get { return FKColumnInCusEntryCPDecTableExposed; }
		}

		public CMRCusEntryCPDecCollection QuestionsExposed;
		public CMRCusEntryCPDecCollection Questions
		{
			get { return QuestionsExposed; }
		}

		public ZDateTime SelectionDateExposed = ZDateTime.Empty;
		public ZDateTime SelectionDate
		{
			get { return SelectionDateExposed; }
		}

		public ICPQALineAttachee[] SourcesToDefaultExposed = System.Array.Empty<ICPQALineAttachee>();
		public ICPQALineAttachee[] SourcesToDefault
		{
			get { return SourcesToDefaultExposed; }
		}

		public LineDefaultQuestions DefaultUniqueQuestionsExposed;
		public LineDefaultQuestions DefaultUniqueQuestions
		{
			get
			{
				if (DefaultUniqueQuestionsExposed == null)
				{
					DefaultUniqueQuestionsExposed = new LineDefaultQuestions();
				}
				return DefaultUniqueQuestionsExposed;
			}
		}

		#endregion
	}
}
