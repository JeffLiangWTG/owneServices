using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.PAVE.MENT.Business
{
	[ModuleID(ModuleId.MENTAgedScoreExtraction)]
	public class MENTAgedScoreExtractionCollection : ActiveBusinessObjectCollection<MENTAgedScoreExtraction>
	{
		public MENTAgedScoreExtractionCollection(MENTAgedScoreQuery query)
			: base(query.Factory, query, new ZQuery(), MENTAgedScoreExtractionSchema.MEX_MAQ)
		{
			Argument.NotNull(query, nameof(query));
			this.agedScoreQuery = query;
		}

		public MENTAgedScoreExtractionCollection(BusinessObjectFactory factory)
			: this(factory, new ZQuery())
		{
		}

		public MENTAgedScoreExtractionCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		readonly MENTAgedScoreQuery agedScoreQuery;

		public MENTAgedScoreQuery AgedScoreQuery
		{
			get { return agedScoreQuery; }
		}
	}
}
