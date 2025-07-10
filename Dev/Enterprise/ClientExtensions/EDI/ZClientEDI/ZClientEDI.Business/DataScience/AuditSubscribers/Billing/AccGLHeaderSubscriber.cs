using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Billing
{
	public sealed class AccGLHeaderSubscriber : DataScienceSubscriberToKafkaBase<KafkaRegistryConfigs.Billing>
	{
		public override string Code => SubscriberCodes.AccGLHeaderCode;
		public override int DataSchemaVersion => 1;
		public override ITableSchema Table => AccGLHeaderSchema.Instance;
		public override IEnumerable<SchemaColumn> BizObjColumns { get; } = new SchemaColumn[]
		{
			AccGLHeaderSchema.PK,
			AccGLHeaderSchema.AG_AccountGroup,
			AccGLHeaderSchema.AG_AccountNum,
			AccGLHeaderSchema.AG_AccountType,
			AccGLHeaderSchema.AG_AG_AlternateNum,
			AccGLHeaderSchema.AG_AG_ConsolidationNum,
			AccGLHeaderSchema.AG_AG_HeaderDependsOnTotal,
			AccGLHeaderSchema.AG_AG_PercentNum,
			AccGLHeaderSchema.AG_CashFlowType,
			AccGLHeaderSchema.AG_Column,
			AccGLHeaderSchema.AG_ControlAccount,
			AccGLHeaderSchema.AG_DebitCredit,
			AccGLHeaderSchema.AG_Description,
			AccGLHeaderSchema.AG_DisallowDirectPosting,
			AccGLHeaderSchema.AG_IsActive,
			AccGLHeaderSchema.AG_IsGlobal,
			AccGLHeaderSchema.AG_Notes,
			AccGLHeaderSchema.AG_PrintSequence,
			AccGLHeaderSchema.AG_StatisticalUnits,
			AccGLHeaderSchema.AG_SystemCreateTimeUtc,
			AccGLHeaderSchema.AG_SystemCreateUser,
			AccGLHeaderSchema.AG_SystemLastEditTimeUtc,
			AccGLHeaderSchema.AG_SystemLastEditUser,
			AccGLHeaderSchema.AG_TotalLevel,
		};
	}
}
