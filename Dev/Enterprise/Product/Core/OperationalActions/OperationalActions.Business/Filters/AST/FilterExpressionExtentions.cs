using System;
using System.Collections.Generic;

namespace Enterprise.Services.OperationalActions.Business
{
	public static class FilterExpressionExtentions
	{
		/// <summary>
		/// Collects a list of all the distinct constraints used in the expression.
		/// </summary>
		/// <param name="expression">The expression to extract the constraints from.</param>
		/// <returns>The list of distinct constraints.</returns>
		public static IList<string> GetConstraints(this IFilterExpression expression)
		{
			List<string> constraints = new List<string>();
			expression.CollectConstraints(constraints);
			return constraints;
		}

		/// <summary>
		/// Finds the longest common prefix for all referenced constraints.
		/// </summary>
		/// <param name="expression">The expression to extract the prefix from.</param>
		/// <returns>The longest common prefix for all referenced constraints.</returns>
		public static string GetLongestCommonPrefix(this IFilterExpression expression)
		{
			var constraints = expression.GetConstraints();
			string prefix;

			if (constraints.Count > 0)
			{
				prefix = constraints[0];
				int len = prefix.Length;

				for (int j = 1; j < constraints.Count; j++)
				{
					len = Math.Min(constraints[j].Length, len);
				}

				for (int i = 0; i < len; i++)
				{
					char c = prefix[i];
					if (c == '+')
					{
						c = '.';
					}

					for (int j = 1; j < constraints.Count; j++)
					{
						char other = constraints[j][i];
						if (other == '+')
						{
							other = '.';
						}

						if (other != c)
						{
							len = i;
							break;
						}
					}
				}

				if (len > 0)
				{
					len = prefix.LastIndexOfAny(new char[] { '.', '+' }, len - 1);
					if (len > 0)
					{
						prefix = prefix.Substring(0, len);
					}
					else
					{
						prefix = "";
					}
				}
				else
				{
					prefix = "";
				}
			}
			else
			{
				prefix = "";
			}

			return prefix;
		}
	}
}
