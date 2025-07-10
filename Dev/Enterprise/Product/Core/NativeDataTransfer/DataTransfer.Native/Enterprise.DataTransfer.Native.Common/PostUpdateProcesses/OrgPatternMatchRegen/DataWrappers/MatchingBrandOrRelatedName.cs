using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Common.PostUpdateProcesses.OrgPatternMatchRegen
{
	[TableName(OrgBrandOrRelatedNameSchema.Constants.TableName)]
	class MatchingBrandOrRelatedName : Wrapper, IMatchingBrandOrRelatedName
	{
		public ZString P1_RelatedName
		{
			get { return GetValue(OrgBrandOrRelatedNameSchema.P1_RelatedName); }
		}

		public ZString P1_RelatedNameOriginal
		{
			get { return GetOriginalValue(OrgBrandOrRelatedNameSchema.P1_RelatedName); }
		}

		protected override SchemaPKColumn PKSchemaColumn
		{
			get { return OrgBrandOrRelatedNameSchema.PK; }
		}
	}
}
