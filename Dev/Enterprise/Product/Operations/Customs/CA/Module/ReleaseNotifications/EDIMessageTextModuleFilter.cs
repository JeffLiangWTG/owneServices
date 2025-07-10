using System.Collections;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CA.Module
{
	class EDIMessageTextModuleFilter : ModuleTextFilter
	{
		/// <summary>
		/// Filter to match value in EDIFACT message text.
		/// </summary>
		/// <param name="description">Filter description.</param>
		/// <param name="segmentPattern">SQL 'Like' pattern which should:
		///		- start with EDIFACT segmnet
		///		- contain 1 arg replacement to insert filter value
		///		- end with single quote
		///		  e.g. "LOC+22+____:129::{0}'"</param>
		/// <param name="valueExactLength">Specify, if filter value has constant lenght.</param>
		public EDIMessageTextModuleFilter(ZString description, string segmentPattern, int valueExactLength = 0)
			: this(description, new[] { segmentPattern }, valueExactLength)
		{
		}

		public EDIMessageTextModuleFilter(ZString description, IEnumerable<string> segmentPatterns, int valueExactLength = 0)
			: base(description, (c, v) => EDIMessageQueryHelper.GetSegmentQuery(c, v, segmentPatterns, valueExactLength))
		{
		}

		public EDIMessageTextModuleFilter(ZString description, string segmentPattern, IList list, int valueExactLength = 0)
			: this(description, new[] { segmentPattern }, list, valueExactLength)
		{
		}

		public EDIMessageTextModuleFilter(ZString description, IEnumerable<string> segmentPatterns, IList list, int valueExactLength = 0)
			: base(description, (c, v) => EDIMessageQueryHelper.GetSegmentQuery(c, v, segmentPatterns, valueExactLength), list)
		{
		}

		public override IReadOnlyList<string> AllowedComparisonOperators
		{
			get
			{
				return new[]
								 {
									 string.Empty,
									 ComparisonConstants.Exact,
									 ComparisonConstants.StartsWith,
									 ComparisonConstants.Contains,
									 ComparisonConstants.NotEqual,
									 ComparisonConstants.NotStartsWith,
									 ComparisonConstants.NotContain
								 };
			}
		}

		public override bool IsExpensiveQuery
		{
			get { return false; } //It's actually allways expensive because it's always Contains comparison operator but user cannot do anything so annoying popup should not be shown
		}
	}
}
