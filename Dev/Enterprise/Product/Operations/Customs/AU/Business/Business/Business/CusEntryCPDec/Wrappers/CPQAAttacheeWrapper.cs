
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CPQAAttacheeWrapper
	{
		public CPQAAttacheeWrapper(ICPQAAttachee attachee)
		{
			this.attachee = attachee;
		}

		public SchemaGuidColumn FKColumnInCusEntryCPDecTable
		{
			get { return CusEntryCPDecSchema.ON_ParentID; }
		}

		[ChildEditable(true)]
		public CMRCusEntryCPDecCollection Questions
		{
			get
			{
				if (fQuestions == null)
				{
					fQuestions = new CMRCusEntryCPDecCollection(attachee);
					fQuestions.Load();
					attachee.RegisterEditableChildObject(fQuestions);
				}
				return fQuestions;
			}
		}
		CMRCusEntryCPDecCollection fQuestions;

		public ZDateTime SelectionDate
		{
			get { return ZDateTime.Today; }
		}

		readonly ICPQAAttachee attachee;
	}
}
