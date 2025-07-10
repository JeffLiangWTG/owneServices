using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public class AutoWebRefServiceLevelFilterBusinessObject : FilterBusinessObject
	{
		#region Schema

		public abstract class Schema
		{
			public const string TableName = "WebRefServiceLevelFilterBusinessObject";
			public const string PK = "PK";

			public const string Code = "Code";
			public const string Description = "Description";
		}

		#endregion

		public AutoWebRefServiceLevelFilterBusinessObject(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region PK

		public override string PK_ColumnName
		{
			get { return Schema.PK; }
		}

		#endregion

		#region Properties

		#region Code

		public virtual ZString Code
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(CodeInfo).ToString().TrimEnd(' ', '\t')); }
			set
			{
				CheckMaximumLength(CodeInfo, value);
				SetPropertyValue(CodeInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateCode();
				}
			}
		}

		public virtual void ValidateCode()
		{
			CodeInfo.ClearAllNotifications();
		}

		public ZPropertyInfo CodeInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.Code); }
		}

		#endregion

		#region Description

		public virtual ZString Description
		{
			get { return new ZString(((IBusinessObjectInternals)this).GetValueFromRowSafely(DescriptionInfo).ToString().TrimEnd(' ', '\t')); }
			set
			{
				CheckMaximumLength(DescriptionInfo, value);
				SetPropertyValue(DescriptionInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateDescription();
				}
			}
		}

		public virtual void ValidateDescription()
		{
			DescriptionInfo.ClearAllNotifications();
		}

		public ZPropertyInfo DescriptionInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.Description); }
		}

		#endregion

		#endregion

		#region BusinessObject Overrides

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			((IBusinessObjectInternals)this).Row[Schema.Code] = "";
			((IBusinessObjectInternals)this).Row[Schema.Description] = "";
		}

		#endregion

		#region Run Pre-Save Validation

		protected override void RunPreSaveValidationCore()
		{
			ValidateCode();
			ValidateDescription();

			base.RunPreSaveValidationCore();
		}

		#endregion

		#endregion

		#region Filters

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = new ZQuery();

				query.DefaultJoinCondition = JoinCondition.And;
				query.AddToFilter(RefServiceLevelSchema.RS_IsActive, ZBool.True);
				query.AddToFilter(SearchPanelFilterFilter);

				return query;
			}
		}

		protected virtual ZQuery SearchPanelFilterFilter
		{
			get
			{
				ZQuery query = new ZQuery();

				query.DefaultJoinCondition = JoinCondition.Or;

				AddIfNotEmpty(query, RefServiceLevelSchema.RS_Code, SQLComparisonOperator.StartsWith, Code);
				AddIfNotEmpty(query, RefServiceLevelSchema.RS_Description, SQLComparisonOperator.Contains, Description);

				return query;
			}
		}

		#endregion
	}
}
