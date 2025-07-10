using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class DummyCPQAAttachee : DummyBusinessObject, ICPQAAttachee
	{
		public DummyCPQAAttachee(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
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

		public ZDateTime SelectionDateExposed;
		public ZDateTime SelectionDate
		{
			get { return SelectionDateExposed; }
		}

		#endregion
	}
}
