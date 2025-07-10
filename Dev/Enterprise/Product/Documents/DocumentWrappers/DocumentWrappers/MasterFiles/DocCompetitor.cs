using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocCompetitor : DocumentWrapper
	{
		DocCompetitor(OrgCompetitor competitor, BusinessObjectFactory factoryToWrap)
			: base(competitor, factoryToWrap)
		{
		}

		public static DocCompetitor New(OrgCompetitor competitor, BusinessObjectFactory factoryToWrap)
		{
			return (competitor != null) ? new DocCompetitor(competitor, factoryToWrap) : null;
		}

		new OrgCompetitor WrappedObject => (OrgCompetitor)base.WrappedObject;

		public ZString CompetitorName => WrappedObject != null ? WrappedObject.Competitor.OH_FullName : ZString.Empty;

		public ZString CompetitorCode => WrappedObject != null ? WrappedObject.Competitor.OH_Code : ZString.Empty;
	}
}
