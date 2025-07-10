using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.Core.Testing
{
	sealed class EInvoicingPivotActionTypeTests : TestCase
	{
		public void TestAllActionsAccountedForInCommandAndQueryLists()
		{
			var allActionTypes = typeof(Constants.EInvoicingPivotActionType)
									.GetFields(BindingFlags.Public | BindingFlags.Static)
									.Where(fieldInfo => fieldInfo.IsLiteral && !fieldInfo.IsInitOnly)
									.Select(fieldInfo => (string)fieldInfo.GetValue(null));
			var categorisedActionTypes = Constants.EInvoicingPivotActionType.CommandActionTypes
											.Concat(Constants.EInvoicingPivotActionType.QueryActionTypes);

			AssertContainsExactElementsInAnyOrder("All pivot actions must be categorised as a command or query (see CRQS); add new action types to an appropriate collection.", allActionTypes, categorisedActionTypes);
		}
	}
}
