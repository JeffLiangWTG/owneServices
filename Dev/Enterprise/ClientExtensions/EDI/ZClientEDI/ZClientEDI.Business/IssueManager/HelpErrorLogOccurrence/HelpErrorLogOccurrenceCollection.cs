using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IssueManager.Business
{
	public class HelpErrorLogOccurrenceCollection : DependentBusinessObjectCollection<HelpErrorLogOccurrence, EdiHelpErrorLog>
	{
		public HelpErrorLogOccurrenceCollection(EdiHelpErrorLog master)
			: base(master)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			return new ZQuery() { MaximumRows = 10000, OrderBy = HelpErrorLogOccurrenceSchema.Constants.HO_ExceptionDateTime + OrderByClause.Descending };
		}

		public override string ToString()
		{
			return Count.ToString(CultureInfo.InvariantCulture) + " Occurrence" + (Count == 1 ? "" : "s");
		}
	}
}

