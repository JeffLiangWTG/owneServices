using System;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class DaysAndAmountOverdueModuleFilter : ModuleNumberFilter
	{
		public DaysAndAmountOverdueModuleFilter(ZString description)
		  : base(description, vw_OrgCollectionCallSchema.CC_OH_FullName)
		{
			// The SchemaColumn passed to the base are never used as we build our query ourselves.
			// But there is no base constructor that accepts just what we need.
		}

		const String AndString = "AND";
		const String OrString = "OR";

		#region Properties

		#region AmountOverdue

		ZDecimal amountOverdue;
		public ZDecimal AmountOverdue
		{
			get { return amountOverdue; }
			set
			{
				if (AmountOverdue != value)
				{
					InvalidateCachedQuery();
				}

				SetNonPersistentPropertyValue(AmountOverdueInfo, ref amountOverdue, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateAmountOverdue();
				}
			}
		}

		public ZPropertyInfo AmountOverdueInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(nameof(AmountOverdue)); }
		}

		#endregion

		#region DaysOverdue

		public ZInt DaysOverdue
		{
			get { return ZInt.ParseSafe(base.Property, 0); }
			set
			{
				base.Property = value.ToString();
			}
		}

		public ZPropertyInfo DaysOverdueInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return base.PropertyInfo; }
		}

		#endregion

		#region AndOrDecider

		ZString andOrDecider;
		[List("AndOrDeciderList")]
		[MaxLength(3)]
		public ZString AndOrDecider
		{
			get { return andOrDecider; }
			set
			{
				if (AndOrDecider != value)
				{
					InvalidateCachedQuery();
				}

				CheckMaximumLength(AndOrDeciderInfo, value);
				SetNonPersistentPropertyValue(AndOrDeciderInfo, ref andOrDecider, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateAndOrDecider();
				}
			}
		}

		public ZPropertyInfo AndOrDeciderInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(AndOrDecider));
			}
		}

		#endregion

		#region TransactionsJoinCondition

		protected JoinCondition TransactionsJoinCondition
		{
			get { return AndOrDecider == AndString ? JoinCondition.And : JoinCondition.Or; }
		}

		#endregion

		#endregion

		#region Validation

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new DaysAndAmountOverdueModuleFilterValidation(this);
		}

		public new DaysAndAmountOverdueModuleFilterValidation Validation
		{
			get { return (DaysAndAmountOverdueModuleFilterValidation)base.Validation; }
		}

		#endregion

		#region Empty

		protected override bool IsEmptyCore => base.IsEmptyCore && AmountOverdue.IsEmpty;

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AndOrDecider = AndString;
		}

		#endregion

		#region Clear

		protected override void ClearCore()
		{
			base.ClearCore();
			AmountOverdue = ZDecimal.Zero;
			AndOrDecider = AndString;
		}

		#endregion

		#region GetQuery

		protected override ZQuery GetQuery()
		{
			ZQuery query = new ZQuery();
			if (!DaysOverdue.IsEmpty)
			{
				ZDateTime startDate = ZDateTime.Now.AddDays(-DaysOverdue);
				query.AddToFilter(TransactionsJoinCondition, vw_OrgCollectionCallSchema.CC_OldestDueDate, SQLComparisonOperator.LessThanOrEqualTo, startDate);
			}
			if (!AmountOverdue.IsEmpty)
			{
				query.AddToFilter(TransactionsJoinCondition, vw_OrgCollectionCallSchema.CC_TotalOverdueAmount, SQLComparisonOperator.GreaterThanOrEqualTo, AmountOverdue);
			}
			return query;
		}

		#endregion

		#region XML Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("AmountOverdue", AmountOverdue.ToString());
			writer.WriteElementString("AndOrDecider", AndOrDecider);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			AmountOverdue = ZDecimal.ParseSafe(reader.ReadElementString("AmountOverdue"), 0.00);
			AndOrDecider = reader.ReadElementString("AndOrDecider");
		}
		#endregion

		#region LookUp

		CodeDescriptionPairList fAndOrDeciderList;
		public CodeDescriptionPairList AndOrDeciderList
		{
			get
			{
				if (fAndOrDeciderList == null)
				{
					fAndOrDeciderList = new CodeDescriptionPairList();
					fAndOrDeciderList.AddPair(AndString, Res.GetString("Accounting|DaysAndAmountOverdueModuleFilter|AndString", "Use AND Operator"));
					fAndOrDeciderList.AddPair(OrString, Res.GetString("Accounting|DaysAndAmountOverdueModuleFilter|OrString", "Use OR Operator"));
				}
				return fAndOrDeciderList;
			}
		}

		#endregion
	}
}
