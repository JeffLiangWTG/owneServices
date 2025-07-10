using CargoWise.Types;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.eServices
{
	#region SuppressResourceStringsCheckRegion

	class UsageSummaryCollectorScript : UsageScript
	{
		public override bool IsMandatoryForMilestones => false;
		protected override string FeatureCode => "USS";
		protected override string FeatureName => "Usage Data To Summarise";
		protected override string RoleName => "eServices";
		protected override string ModuleName => "Usage Data";
		protected override string FunctionName => "Usage Data To Summarise";
		protected override string TransactionCompanyExpression => "gc.GC_Code";
		protected override string CompanyNameExpression => "gc.GC_Name";
		protected override string BranchCodeExpression => "gb.GB_Code";
		protected override string TransactionDateUtcExpression => "@StartDateTimeInclusive";
		protected override string TransactionGuidReference => "cast(cast(BINARY_CHECKSUM(sm.EM_MessageData) as varbinary) as uniqueidentifier)";
		protected override string AdditionalRefs => "sm.EM_MessageData";
		protected override string CreateUserCodeExpression => "sm.EM_SystemCreateUser";
		protected override string ActiveOn => "ALL";
		protected override string FromClause => @"
					SummarisedMessages sm
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = sm.EM_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		protected override string WhereClause => string.Empty;
		protected override string PreparationScript => @"WITH SummarisedMessages AS
(
					SELECT EM_GB, EM_MessageData, EM_SystemCreateUser, Count(*) Records
					FROM dbo.EDIMessage
					WHERE (EM_ApplicationCode = 'USS')
						AND (EM_ReceiveTransmit = 'TRX')
						AND (EM_IsActive = 1)
						AND (EM_Status = 'CAP')
						AND (EM_MessageType = 'USS')
						AND (EM_SystemCreateTimeUtc >= @StartDateTimeInclusive)
						AND (EM_SystemCreateTimeUtc < @EndDateTimeExclusive)
					GROUP BY EM_GB, EM_MessageData, EM_SystemCreateUser
)";
		protected override string TransactionReference01 => ZString.Empty;
		protected override StlDataGrain StlItemGrain => StlDataGrain.Daily;
		protected override string TransactionCountExpression => "sm.Records";
	}

	#endregion // SuppressResourceStringsCheckRegion
}
