using System;
using System.Collections.Generic;
using System.Reflection;
using Enterprise.Services.OperationalActions.Business.AST;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Services.OperationalActions.Business
{
	public static class FilterTools
	{
		public static IFilterExpression AddRequirement(IFilterExpression expression, FilterRequirement requirement)
		{
			if (expression == null)
			{
				throw new ArgumentNullException(nameof(expression));
			}

			if (requirement == null)
			{
				throw new ArgumentNullException(nameof(requirement));
			}

			IFilterOperand constraint = new FilterConstraintRef(requirement.ConstraintName);
			List<IFilterOperand> values = new List<IFilterOperand>();
			IFilterExpression result;

			foreach (string value in requirement)
			{
				values.Add(new FilterLiteral(value));
			}

			if (values.Count == 1)
			{
				result = new FilterCheckEquality(constraint, values[0]);
			}
			else
			{
				result = new FilterIn(constraint, values.ToArray());
			}

			if (!expression.IsEmpty)
			{
				result = new FilterAnd(result, expression);
			}

			return result;
		}

		public static IFilterExpression[] SplitByInfoChain(IFilterExpression expression, PropertyInfo[] infoChain)
		{
			if (expression == null)
			{
				throw new ArgumentNullException(nameof(expression));
			}

			if (infoChain == null)
			{
				throw new ArgumentNullException(nameof(infoChain));
			}

			if (infoChain.Length == 0)
			{
				throw new ArgumentException("infoChain cannot be empty");
			}
			else if (infoChain.Length == 1)
			{
				return new IFilterExpression[] { expression };
			}
			else
			{
				IFilterExpression[] expressions = new IFilterExpression[infoChain.Length];
				List<IFilterExpression> split = SplitByAnd(expression);

				foreach (IFilterExpression subExp in split)
				{
					string prefix = subExp.GetLongestCommonPrefix();

					int prefixi = 0;
					int stepi = 0;
					int index = 0;
					int count = 0;

					string step = infoChain[0].Name;

					do
					{
						if (stepi == step.Length)
						{
							if (prefixi < prefix.Length && prefix[prefixi] != '.' && prefix[prefixi] != '+')
							{
								break;
							}

							count = prefixi;
							stepi = 0;
							prefixi++;
							index++;

							if (index == infoChain.Length)
							{
								break;
							}

							step = infoChain[index].Name;
						}
						else if (prefixi < prefix.Length && prefix[prefixi] == step[stepi])
						{
							prefixi++;
							stepi++;
						}
						else
						{
							break;
						}
					} while (true);

					IFilterExpression existing = expressions[index];
					IFilterExpression toAdd;

					if (count == 0)
					{
						toAdd = subExp;
					}
					else
					{
						toAdd = subExp.RemoveConstraintPrefix(prefix.Substring(0, count));
					}

					if (existing == null)
					{
						expressions[index] = toAdd;
					}
					else
					{
						expressions[index] = new FilterAnd(existing, toAdd);
					}
				}

				for (int i = 0; i < expressions.Length; i++)
				{
					if (expressions[i] == null)
					{
						expressions[i] = new FilterEmpty();
					}
				}

				return expressions;
			}
		}

		#region Implementation

		static List<IFilterExpression> SplitByAnd(IFilterExpression expression)
		{
			if (expression == null)
			{
				throw new ArgumentNullException(nameof(expression));
			}

			List<IFilterExpression> result = new List<IFilterExpression>();

			if (!expression.IsEmpty)
			{
				Stack<IFilterExpression> expressions = new Stack<IFilterExpression>();
				expressions.Push(expression);

				while (expressions.Count > 0)
				{
					IFilterExpression thisExpression = expressions.Pop();

					if (thisExpression.Precidence == FilterPrecedence.And)
					{
						IFilterExpression[] subExpressions = thisExpression.GetSubExpressions();

						for (int i = subExpressions.Length - 1; i >= 0; i--)
						{
							expressions.Push(subExpressions[i]);
						}
					}
					else
					{
						result.Add(thisExpression);
					}
				}
			}

			return result;
		}

		#endregion
	}
}
