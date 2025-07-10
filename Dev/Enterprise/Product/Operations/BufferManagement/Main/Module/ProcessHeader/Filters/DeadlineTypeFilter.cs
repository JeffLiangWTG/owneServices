using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	delegate ZQuery DeadlineTypeQuery(ZString deadlineType, ZBool isEffective, ZBool isImmediate);

	public class DeadlineTypeFilter : ModuleTextBaseFilter
	{
		public DeadlineTypeFilter()
			: base(ProcessHeader.ModuleFilterConstants.DeadlineType, (DeadlineTypeQuery)GetDeadlineTypeQuery, new DeadlineTypeFilterTypeList())
		{
		}

		public static class Schema
		{
			public const string DeadlineType = "DeadlineType";
			public const string IsEffective = "IsEffective";
			public const string IsImmediate = "IsImmediate";
		}

		#region Properties

		ZString deadlineType;
		ZBool isEffective = true, isImmediate = false;

		public ZString DeadlineType
		{
			get => deadlineType;
			set => SetNonPersistentPropertyValue(DeadlineTypeInfo, ref deadlineType, value);
		}

		public ZBool IsEffective
		{
			get => isEffective;
			set
			{
				if (isEffective != value)
				{
					isEffective = value;
					IsImmediate = !value;
					IsEffectiveInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZBool IsImmediate
		{
			get => isImmediate;
			set
			{
				if (isImmediate != value)
				{
					isImmediate = value;
					IsEffective = !value;
					IsImmediateInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo DeadlineTypeInfo => GetZPropertyInfo(Schema.DeadlineType);
		public ZPropertyInfo IsEffectiveInfo => GetZPropertyInfo(Schema.IsEffective);
		public ZPropertyInfo IsImmediateInfo => GetZPropertyInfo(Schema.IsImmediate);

		#endregion

		#region Query Implementation

		static ZQuery GetDeadlineTypeQuery(ZString deadlineType, ZBool isEffective, ZBool isImmediate)
		{
			var target = deadlineType == DeadlineTypeFilterTypeList.Codes.None ? ZString.Empty : deadlineType;

			if (isImmediate)
			{
				return new ZQuery(ProcessHeaderSchema.FH_DeadlineType, target);
			}

			var processHeaderQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
			processHeaderQuery.AddToFilter(ProcessHeaderSchema.FH_DeadlineType, SQLComparisonOperator.Equal, target);
			if (target.IsEmpty)
			{
				processHeaderQuery.AddToFilter(ProcessHeaderSchema.FH_FH_ParentHeader, null);
			}

			var jobSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
			jobSubQuery.AddToFilter(ProcessHeaderSchema.FH_DeadlineType, SQLComparisonOperator.Equal, target);

			var workflowInheritsFromParentQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.PK);
			workflowInheritsFromParentQuery.AddToFilter(ProcessHeaderSchema.FH_DeadlineType, ZString.Empty);
			workflowInheritsFromParentQuery.AddSubQuery(ProcessHeaderSchema.FH_FH_ParentHeader, jobSubQuery, JoinCondition.And);
			workflowInheritsFromParentQuery.AddAsUnionQuery(processHeaderQuery);

			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			query.AddSubQuery(workflowInheritsFromParentQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region XML Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString(Schema.DeadlineType, DeadlineType);
			writer.WriteElementString(Schema.IsEffective, IsEffective.ToString());
			writer.WriteElementString(Schema.IsImmediate, IsImmediate.ToString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			DeadlineType = reader.ReadElementString(Schema.DeadlineType);
			IsEffective = new ZBool(reader.ReadElementString(nameof(IsEffective)));
			IsImmediate = new ZBool(reader.ReadElementString(nameof(IsImmediate)));
		}

		#endregion

		#region Implementation

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return null;
		}

		protected override object[] QueryDelegateParameters => new object[] { DeadlineType, IsEffective, IsImmediate };

		protected override bool IsEmptyCore => DeadlineType.IsEmpty;

		#endregion

		#region Validation

		protected override ModuleFilterValidation GetNewValidation() => new DeadlineTypeFilterValidation(this);

		class DeadlineTypeFilterValidation : ModuleTextFilterValidation
		{
			public DeadlineTypeFilterValidation(DeadlineTypeFilter parent)
				: base(parent)
			{
				Parent = parent;
			}

			protected readonly new DeadlineTypeFilter Parent;

			public void ValidateDeadlineType()
			{
				ValidateCalculatedProperty(Parent.DeadlineTypeInfo);
			}

			protected virtual void CheckDeadlineType()
			{
				MandatoryValidation.CheckEntered(Parent.DeadlineTypeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.DeadlineTypeInfo, (ICodeDescriptionPairList)Parent.List);
			}

			public override void ValidateAll()
			{
				ValidateDeadlineType();
				base.ValidateAll();
			}
		}

		#endregion
	}
}
