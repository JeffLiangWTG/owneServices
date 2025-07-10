using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class FilteredSuggestedRelevantDiagnosticCriteriaCollection : FilteredRelevantDiagnosticCriteriaCollection
	{
		public FilteredSuggestedRelevantDiagnosticCriteriaCollection(TriageAssistBusinessObject parent) : base(parent.SuggestedCriteriaCollection)
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var item = (RelevantDiagnosticCriteria)element;
			if (Parent.ShowFocusedSuggestedCriteriaOnly && !item.IsFocused)
			{
				return false;
			}

			if (Parent.ShouldSearchSuggestedList)
			{
				if (Parent.SearchTermTokens.Any())
				{
					return (Parent.ShouldSearchKeywords && CheckMatch(item.DiagnosticCriteria.IMD_Keywords))
						|| (Parent.ShouldSearchDescription && CheckMatch(item.DiagnosticCriteria.IMD_Description));
				}
			}

			return true;
		}

		bool CheckMatch(ZString content)
		{
			switch (Parent.SearchTermOperator)
			{
				case TriageAssistBusinessObject.ComparisonConstants.Exact:
					{
						return content.Contains(Parent.SearchTerm.Trim('"'), System.StringComparison.OrdinalIgnoreCase);
					}
				case TriageAssistBusinessObject.ComparisonConstants.ContainsAll:
					{
						if (Parent.SearchTermEndsWithSpace)
						{
							return Parent.SearchTermTokens.All(x => CheckWholeWordMatch(content, x));
						}
						else
						{
							return Parent.SearchTermTokens.Take(Parent.SearchTermTokens.Count() - 1)
								.All(x => CheckWholeWordMatch(content, x)) && CheckStartsWith(content, Parent.SearchTermTokens.Last());
						}
					}
				default: //AnyMatch
					{
						if (Parent.SearchTermEndsWithSpace)
						{
							return Parent.SearchTermTokens.Any(x => CheckWholeWordMatch(content, x));
						}
						else
						{
							return Parent.SearchTermTokens.Take(Parent.SearchTermTokens.Count() - 1)
								.Any(x => CheckWholeWordMatch(content, x)) || CheckStartsWith(content, Parent.SearchTermTokens.Last());
						}
					}
			}

			bool CheckWholeWordMatch(string input, string keyword)
				=> Regex.IsMatch(content, $@"\b{Regex.Escape(keyword)}\b", RegexOptions.IgnoreCase);
			bool CheckStartsWith(string input, string keyword)
				=> Regex.IsMatch(content, $@"\b{Regex.Escape(keyword)}", RegexOptions.IgnoreCase);
		}
	}
}
