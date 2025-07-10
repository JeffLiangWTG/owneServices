using System.Collections;
using System.Diagnostics;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.Business
{
	internal class QueryBuilder
	{
		Query ProcessFilter(Filter root)
		{
			Query opnd = ProcessNode(root.Condition, null);
			Query qyInput = ProcessNode(root.Input, null);

			return new FilterQuery(qyInput, opnd);
		}

		Query ProcessOperand(Operand root)
		{
			return new OperandQuery(root.OperandValue, root.ReturnType);
		}

		Query ProcessFunction(Function root, Query qyInput)
		{
			Query qy = null;

			switch (root.TypeOfFunction)
			{
				case Function.FunctionType.FuncPosition:
					qy = new MethodOperand(null, root.TypeOfFunction);
					return qy;

				case Function.FunctionType.FuncCount:
					qy = ProcessNode((AstNode)(root.ArgumentList[0]), null);
					if (qy is AttributeQuery)
					{
						return new MethodOperand(qy, Function.FunctionType.FuncCount);
					}
					break;

				case Function.FunctionType.FuncLocalName:
				case Function.FunctionType.FuncNameSpaceUri:
				case Function.FunctionType.FuncName:
					if (root.ArgumentList != null && root.ArgumentList.Count > 0)
					{
						return new MethodOperand(ProcessNode((AstNode)(root.ArgumentList[0]), null), root.TypeOfFunction);
					}
					return new MethodOperand(null, root.TypeOfFunction);

				case Function.FunctionType.FuncString:
				case Function.FunctionType.FuncConcat:
				case Function.FunctionType.FuncStartsWith:
				case Function.FunctionType.FuncContains:
				case Function.FunctionType.FuncSubstringBefore:
				case Function.FunctionType.FuncSubstringAfter:
				case Function.FunctionType.FuncSubstring:
				case Function.FunctionType.FuncStringLength:
				case Function.FunctionType.FuncNormalize:
				case Function.FunctionType.FuncTranslate:
					ArrayList argList = null;
					if (root.ArgumentList != null)
					{
						int count = 0;
						argList = new ArrayList();
						while (count < root.ArgumentList.Count)
						{
							argList.Add(ProcessNode((AstNode)root.ArgumentList[count++], null));
						}
					}
					return new StringFunctions(argList, root.TypeOfFunction);

				case Function.FunctionType.FuncNumber:
				case Function.FunctionType.FuncFloor:
				case Function.FunctionType.FuncCeiling:
				case Function.FunctionType.FuncRound:
					if (root.ArgumentList != null)
					{
						return new NumberFunctions(ProcessNode((AstNode)root.ArgumentList[0], null),
													 root.TypeOfFunction);
					}
					else
					{
						return new NumberFunctions(null);
					}

				case Function.FunctionType.FuncTrue:
				case Function.FunctionType.FuncFalse:
					return new BooleanFunctions(null, root.TypeOfFunction);

				case Function.FunctionType.FuncNot:
				case Function.FunctionType.FuncLang:
				case Function.FunctionType.FuncBoolean:
					return new BooleanFunctions(ProcessNode((AstNode)root.ArgumentList[0], null),
												root.TypeOfFunction);

				//case FT.FuncID:
				//case FT.FuncLast:

				default:
					throw new XPathReaderException("The XPath query is not supported.");
			}

			return null;
		}

		Query ProcessOperator(Operator root, Query qyInput)
		{
			Query ret = null;

			switch (root.OperatorType)
			{
				case Operator.Op.OR:
					ret = new OrExpr(ProcessNode(root.Operand1, null), ProcessNode(root.Operand2, null));
					return ret;

				case Operator.Op.AND:
					ret = new AndExpr(ProcessNode(root.Operand1, null), ProcessNode(root.Operand2, null));
					return ret;
			}

			switch (root.ReturnType)
			{
				case XPathResultType.Number:
					ret = new NumericExpr(root.OperatorType, ProcessNode(root.Operand1, null), ProcessNode(root.Operand2, null));
					return ret;

				case XPathResultType.Boolean:
					ret = new LogicalExpr(root.OperatorType, ProcessNode(root.Operand1, null), ProcessNode(root.Operand2, null));
					return ret;
			}

			return ret;
		}

		Query ProcessAxis(Axis root, Query qyInput)
		{
			Query result = null;

			switch (root.TypeOfAxis)
			{
				case Axis.AxisType.Attribute:
					result = new AttributeQuery(qyInput, root.Name, root.Prefix, root.Type);
					break;

				case Axis.AxisType.Self:
					result = new XPathSelfQuery(qyInput, root.Name, root.Prefix, root.Type);
					break;

				case Axis.AxisType.Child:
					result = new ChildQuery(qyInput, root.Name, root.Prefix, root.Type);
					break;

				case Axis.AxisType.Descendant:
				case Axis.AxisType.DescendantOrSelf:
					result = new DescendantQuery(qyInput, root.Name, root.Prefix, root.Type);
					break;

				default:
					throw new XPathReaderException("xpath is not supported!");
			}

			return result;
		}

		Query ProcessNode(AstNode root, Query qyInput)
		{
			Query result = null;

			if (root == null)
			{
				return null;
			}

			switch (root.TypeOfAst)
			{
				case AstNode.QueryType.Axis:
					Axis axis = (Axis)root;
					result = ProcessAxis(axis, ProcessNode(axis.Input, qyInput));
					break;

				case AstNode.QueryType.Operator:
					result = ProcessOperator((Operator)root, null);
					break;

				case AstNode.QueryType.Filter:
					result = ProcessFilter((Filter)root);
					break;

				case AstNode.QueryType.ConstantOperand:
					result = ProcessOperand((Operand)root);
					break;

				case AstNode.QueryType.Function:
					result = ProcessFunction((Function)root, qyInput);
					break;

				case AstNode.QueryType.Root:
					result = new AbsoluteQuery();
					break;

				case AstNode.QueryType.Group:
					result = new GroupQuery(ProcessNode(((Group)root).GroupNode, qyInput));
					break;
				default:
					Debug.Assert(false, (NoResString)"Unknown QueryType encountered!!");
					break;
			}
			return result;
		}

		public void Build(string xpath, ArrayList compiledXPath, int depth)
		{
			Query query;
			AstNode root = XPathParser.ParseXPathExpresion(xpath);
			Stack stack = new Stack();

			query = ProcessNode(root, null);

			while (query != null)
			{
				if (query is BaseAxisQuery)
				{
					stack.Push(query);
					query = ((BaseAxisQuery)query).QueryInput;
				}
				else
				{
					// these queries are not supported
					// for example, the primary exprission not in the predicate.
					throw new XPathReaderException("XPath query is not supported!");
				}
			}

			query = (Query)stack.Peek();

			if (query is AbsoluteQuery)
			{ //AbsoluteQuery at root means nothing. Throw it away.
				stack.Pop();
			}

			// reverse the query
			// compute the query depth table
			while (stack.Count > 0)
			{
				compiledXPath.Add(stack.Pop());
				BaseAxisQuery currentQuery = (BaseAxisQuery)compiledXPath[compiledXPath.Count - 1];

				FilterQuery filterQuery = null;

				if (currentQuery is FilterQuery)
				{
					filterQuery = (FilterQuery)currentQuery;
					currentQuery = ((FilterQuery)currentQuery).Axis;
				}

				if (currentQuery is ChildQuery || currentQuery is AttributeQuery || currentQuery is DescendantQuery)
				{
					++depth;
				}
				else if (currentQuery is AbsoluteQuery)
				{
					depth = 0;
				}

				currentQuery.Depth = depth;

				if (filterQuery != null)
				{
					filterQuery.Depth = depth;
				}
			}

			//
			// matchIndex always point to the next query to match.
			// We use the matchIndex to retriev the query depth info,
			// without this added Null query, we need to check the
			// condition all the time in the Expression Advance method.
			//
			compiledXPath.Add(new NullQuery());
		}
	}
}
