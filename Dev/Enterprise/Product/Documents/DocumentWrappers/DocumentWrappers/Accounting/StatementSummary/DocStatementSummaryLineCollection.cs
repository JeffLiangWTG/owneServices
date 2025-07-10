using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentWrappers
{
	public class DocStatementSummaryLineCollection : NonPersistentBusinessObjectCollection<DocStatementSummaryLine>
	{
		public DocStatementSummaryLineCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocStatementSummaryLine GetLineFromOrgCode(ZString orgCode)
		{
			DocStatementSummaryLine result = null;
			foreach (DocStatementSummaryLine line in this)
			{
				if (line.OrganisationCode == orgCode)
				{
					result = line;
					break;
				}
			}
			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new System.NotImplementedException();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}

