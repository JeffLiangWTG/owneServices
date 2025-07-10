using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class EDI010
	{
		public void Method(ZQuery query)
		{
			//EDI010:PredefinedNoteType Description Rule
			query.AddToFilter(StmNoteSchema.ST_Description, PredefinedNoteTypes.Instance.MarksAndNumbers.Description);
		}
	}
}
