using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Common.PostUpdateProcesses.OrgPatternMatchRegen
{
	[TableName(OrgCusCodeSchema.Constants.TableName)]
	class MatchingCusCode : Wrapper, IMatchingCusCode
	{
		public ZString OK_CodeType
		{
			get { return GetValue(OrgCusCodeSchema.OK_CodeType); }
		}

		public bool OK_CodeTypeHasChanges
		{
			get { return OK_CodeType != OK_CodeTypeOriginalValue; }
		}

		public ZString OK_CodeTypeOriginalValue
		{
			get { return GetOriginalValue(OrgCusCodeSchema.OK_CodeType); }
		}

		public ZString OK_CustomsRegNo
		{
			get { return GetValue(OrgCusCodeSchema.OK_CustomsRegNo); }
		}

		public bool OK_CustomsRegNoHasChanges
		{
			get { return OK_CustomsRegNo != OK_CustomsRegNoOriginalValue; }
		}

		public ZString OK_CustomsRegNoOriginalValue
		{
			get { return GetOriginalValue(OrgCusCodeSchema.OK_CustomsRegNo); }
		}

		public ZGuid OK_OA_PremisesAddress
		{
			get { return GetValue(OrgCusCodeSchema.OK_OA_PremisesAddress); }
		}

		public ZString OK_RN_NKCodeCountry
		{
			get { return GetValue(OrgCusCodeSchema.OK_RN_NKCodeCountry); }
		}

		protected override SchemaPKColumn PKSchemaColumn
		{
			get { return OrgCusCodeSchema.PK; }
		}
	}
}
