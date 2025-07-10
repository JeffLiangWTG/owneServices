using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DummyHeaderAttachee : DummyBusinessObject, ICPQAHeaderAttachee
	{
		public DummyHeaderAttachee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ICPQAHeaderAttachee Members

		public LodgementQuestionKeys LodgementQuestionKeyExposed;
		public LodgementQuestionKeys LodgementQuestionKey
		{
			get
			{
				return LodgementQuestionKeyExposed;
			}
		}

		public bool IsStatusPostLodgeExposed;
		public bool IsStatusPostLodge
		{
			get { return IsStatusPostLodgeExposed; }
		}

		#endregion

		#region ICPQAAttachee Members

		public SchemaGuidColumn FKColumnInCusEntryCPDecTableExposed;
		public SchemaGuidColumn FKColumnInCusEntryCPDecTable
		{
			get
			{
				return FKColumnInCusEntryCPDecTableExposed;
			}
		}

		public CMRCusEntryCPDecCollection QuestionsExposed;
		public CMRCusEntryCPDecCollection Questions
		{
			get
			{
				return QuestionsExposed;
			}
		}

		public ZDateTime SelectionDateExposed;
		public ZDateTime SelectionDate
		{
			get
			{
				return SelectionDateExposed;
			}
		}

		#endregion
	}
}
