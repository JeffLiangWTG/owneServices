using CargoWise.Types;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.eServices
{
	#region SuppressResourceStringsCheckRegion

	class UsageCollectorScript : UsageScript
	{
		public override bool IsMandatoryForMilestones => false;
		protected override string FeatureCode => "USG";
		protected override string RoleName => "eServices";
		protected override string ModuleName => "Usage Data";
		protected override string FunctionName => "Usage Data Messages";
		protected override string FeatureName => "Usage Data Messages";
		protected override string TransactionCompanyExpression => "gc.GC_Code";
		protected override string CompanyNameExpression => "gc.GC_Name";
		protected override string BranchCodeExpression => "gb.GB_Code";
		protected override string TransactionDateUtcExpression => "em.EM_SystemCreateTimeUtc";
		protected override string TransactionGuidReference => "em.EM_PK";
		protected override string AdditionalRefs => "em.EM_MessageData";
		protected override string CreateUserCodeExpression => "em.EM_SystemCreateUser";
		protected override string ActiveOn => "ALL";
		protected override string FromClause => @"
					EDIMessage em
					INNER JOIN dbo.GlbBranch gb ON gb.GB_PK = em.EM_GB
					INNER JOIN dbo.GlbCompany gc ON gc.GC_PK = gb.GB_GC";
		protected override string WhereClause => @"
					(
						(em.EM_ApplicationCode = 'USG')
						AND (em.EM_ReceiveTransmit = 'TRX')
						AND (em.EM_IsActive = 1)
						AND (em.EM_Status = 'CAP')
						AND (em.EM_MessageType = 'USG')
					)";
		protected override string TransactionReference01 => ZString.Empty;
	}

	#endregion // SuppressResourceStringsCheckRegion
}
