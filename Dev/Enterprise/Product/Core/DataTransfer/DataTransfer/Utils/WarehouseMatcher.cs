using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Business
{
	public class WarehouseMatcher : Matcher<ZGuid>
	{
		public WarehouseMatcher(BusinessObjectFactory factory, ZGuid mappingOrgPK, ZString value)
			: base()
		{
			this.factory = factory;
			this.mappingOrgPK = mappingOrgPK;
			this.value = value;
		}

		protected override MatchDelegate[] matchDelegates
		{
			get { return new MatchDelegate[] { ByNaturalKeyPattern }; }
		}

		MatchResult ByNaturalKeyPattern()
		{
			var filter = new ZQuery();
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_OH, mappingOrgPK);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Constants.OrgPatternMatchOverrideRelationships.Warehouse);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, value);

			var match = factory.LoadTop1<OrgPatternMatchOverride>(filter);
			return (match != null && !match.OO_LocalGuid.IsEmpty) ? new MatchResult(true, match.OO_LocalGuid) : new MatchResult();
		}

		readonly BusinessObjectFactory factory;
		readonly ZGuid mappingOrgPK;
		readonly ZString value;
	}
}
