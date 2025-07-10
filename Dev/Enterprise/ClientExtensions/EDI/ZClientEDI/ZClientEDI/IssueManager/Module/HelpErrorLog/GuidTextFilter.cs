using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IssueManager.Module
{
	public class GuidTextFilter : ModuleTextFilter
	{
		public GuidTextFilter(ZString description, GetGuidQueryWithOperator queryDelegate)
			: base(description, queryDelegate)
		{
			PropertyValidation = Validate;
		}

		public GuidTextFilter(ZString description, SchemaGuidColumn column)
			: this(description, CreateQueryDelegate(column))
		{
		}

		static GetGuidQueryWithOperator CreateQueryDelegate(SchemaGuidColumn column)
		{
			return (comparisonOperator, value) => { return CreateQuery(column, comparisonOperator, value); };
		}

		static ZQuery CreateQuery(SchemaGuidColumn column, SQLComparisonOperator comparisonOperator, object value)
		{
			if (comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				return new ZQuery(column, SQLComparisonOperator.NotEqual, null);
			}
			else if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				return new ZQuery(column, SQLComparisonOperator.Equal, null);
			}
			else
			{
				return new ZQuery(column, comparisonOperator, value);
			}
		}

		public override bool HasComparisonOperator
		{
			get { return true; }
		}

		void Validate(ZPropertyInfo info)
		{
			var text = (ZString)info.Value;
			if (!text.IsEmpty)
			{
				ZGuid result;
				if (!ZGuid.TryParse(text, out result))
				{
					info.AddError("Please enter a valid GUID");
				}
			}
		}

		protected override object[] QueryDelegateParameters
		{
			get
			{
				return new object[] { SqlComparisonOperator, PropertyForZQuery };
			}
		}

		object PropertyForZQuery
		{
			get
			{
				ZString text = Property;
				ZGuid result;
				if (!ZGuid.TryParse(text, out result))
				{
					result = ZGuid.Empty;
				}
				return result;
			}
		}

		public override IReadOnlyList<string> AllowedComparisonOperators
		{
			get
			{
				return new[]
					{
						ModuleTextFilter.ComparisonConstants.Exact,
						ModuleTextFilter.ComparisonConstants.NotEqual,
						ModuleTextFilter.ComparisonConstants.IsBlank,
						ModuleTextFilter.ComparisonConstants.IsNotBlank
					};
			}
		}
	}
}
