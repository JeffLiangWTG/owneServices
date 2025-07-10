using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Xml.XPath;
using Enterprise.ZArchitecture.Core;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.eHubMessaging.Business
{
	internal class XPathParser
	{
		readonly XPathScanner scanner;

		XPathParser(XPathScanner scanner)
		{
			this.scanner = scanner;
		}

		public static AstNode ParseXPathExpresion(string xpathExpresion)
		{
			XPathScanner scanner = new XPathScanner(xpathExpresion);
			XPathParser parser = new XPathParser(scanner);
			AstNode result = parser.ParseExpresion(null);
			if (scanner.Kind != XPathScanner.LexKind.Eof)
			{
				throw new XPathException(String.Format("'{0}' has an invalid token.", scanner.SourceText));
			}
			return result;
		}

		public static AstNode ParseXPathPattern(string xpathPattern)
		{
			XPathScanner scanner = new XPathScanner(xpathPattern);
			XPathParser parser = new XPathParser(scanner);
			AstNode result = parser.ParsePattern(null);
			if (scanner.Kind != XPathScanner.LexKind.Eof)
			{
				throw new XPathException(string.Format("'{0}' has an invalid token.", scanner.SourceText));
			}
			return result;
		}

		// --------------- Expresion Parsing ----------------------

		AstNode ParseExpresion(AstNode qyInput)
		{
			return ParseOrExpr(qyInput);
		}

		//>> OrExpr ::= ( OrExpr 'or' )? AndExpr 
		AstNode ParseOrExpr(AstNode qyInput)
		{
			AstNode opnd = ParseAndExpr(qyInput);

			do
			{
				if (!TestOp((NoResString)"or"))
				{
					return opnd;
				}
				NextLex();
				opnd = new Operator(Operator.Op.OR, opnd, ParseAndExpr(qyInput));
			} while (true);
		}

		//>> AndExpr ::= ( AndExpr 'and' )? EqualityExpr 
		AstNode ParseAndExpr(AstNode qyInput)
		{
			AstNode opnd = ParseEqualityExpr(qyInput);

			do
			{
				if (!TestOp((NoResString)"and"))
				{
					return opnd;
				}
				NextLex();
				opnd = new Operator(Operator.Op.AND, opnd, ParseEqualityExpr(qyInput));
			} while (true);
		}

		//>> EqualityOp ::= '=' | '!='
		//>> EqualityExpr    ::= ( EqualityExpr EqualityOp )? RelationalExpr
		AstNode ParseEqualityExpr(AstNode qyInput)
		{
			AstNode opnd = ParseRelationalExpr(qyInput);

			do
			{
				Operator.Op op = (
					this.scanner.Kind == XPathScanner.LexKind.Eq ? Operator.Op.EQ :
					this.scanner.Kind == XPathScanner.LexKind.Ne ? Operator.Op.NE :
					/*default :*/                                  Operator.Op.INVALID
				);
				if (op == Operator.Op.INVALID)
				{
					return opnd;
				}
				NextLex();
				opnd = new Operator(op, opnd, ParseRelationalExpr(qyInput));
			} while (true);
		}

		//>> RelationalOp ::= '<' | '>' | '<=' | '>='
		//>> RelationalExpr    ::= ( RelationalExpr RelationalOp )? AdditiveExpr  
		AstNode ParseRelationalExpr(AstNode qyInput)
		{
			AstNode opnd = ParseAdditiveExpr(qyInput);

			do
			{
				Operator.Op op = (
					this.scanner.Kind == XPathScanner.LexKind.Lt ? Operator.Op.LT :
					this.scanner.Kind == XPathScanner.LexKind.Le ? Operator.Op.LE :
					this.scanner.Kind == XPathScanner.LexKind.Gt ? Operator.Op.GT :
					this.scanner.Kind == XPathScanner.LexKind.Ge ? Operator.Op.GE :
					/*default :*/                                  Operator.Op.INVALID
				);
				if (op == Operator.Op.INVALID)
				{
					return opnd;
				}
				NextLex();
				opnd = new Operator(op, opnd, ParseAdditiveExpr(qyInput));
			} while (true);
		}

		//>> AdditiveOp   ::= '+' | '-'
		//>> AdditiveExpr ::= ( AdditiveExpr AdditiveOp )? MultiplicativeExpr
		AstNode ParseAdditiveExpr(AstNode qyInput)
		{
			AstNode opnd = ParseMultiplicativeExpr(qyInput);

			do
			{
				Operator.Op op = (
					this.scanner.Kind == XPathScanner.LexKind.Plus ? Operator.Op.PLUS :
					this.scanner.Kind == XPathScanner.LexKind.Minus ? Operator.Op.MINUS :
					/*default :*/                                     Operator.Op.INVALID
				);
				if (op == Operator.Op.INVALID)
				{
					return opnd;
				}
				NextLex();
				opnd = new Operator(op, opnd, ParseMultiplicativeExpr(qyInput));
			} while (true);
		}

		//>> MultiplicativeOp   ::= '*' | 'div' | 'mod'
		//>> MultiplicativeExpr ::= ( MultiplicativeExpr MultiplicativeOp )? UnaryExpr
		AstNode ParseMultiplicativeExpr(AstNode qyInput)
		{
			AstNode opnd = ParseUnaryExpr(qyInput);

			do
			{
				Operator.Op op = (
					this.scanner.Kind == XPathScanner.LexKind.Star ? Operator.Op.MUL :
					TestOp((NoResString)"div") ? Operator.Op.DIV :
					TestOp((NoResString)"mod") ? Operator.Op.MOD :
					/*default :*/                                     Operator.Op.INVALID
				);
				if (op == Operator.Op.INVALID)
				{
					return opnd;
				}
				NextLex();
				opnd = new Operator(op, opnd, ParseUnaryExpr(qyInput));
			} while (true);
		}

		//>> UnaryExpr    ::= UnionExpr | '-' UnaryExpr
		AstNode ParseUnaryExpr(AstNode qyInput)
		{
			if (this.scanner.Kind == XPathScanner.LexKind.Minus)
			{
				NextLex();
				return new Operator(Operator.Op.NEGATE, ParseUnaryExpr(qyInput), null);
			}
			else
			{
				return ParseUnionExpr(qyInput);
			}
		}

		//>> UnionExpr ::= ( UnionExpr '|' )? PathExpr  
		AstNode ParseUnionExpr(AstNode qyInput)
		{
			AstNode opnd = ParsePathExpr(qyInput);

			do
			{
				if (this.scanner.Kind != XPathScanner.LexKind.Union)
				{
					return opnd;
				}
				NextLex();
				AstNode opnd2 = ParsePathExpr(qyInput);
				CheckNodeSet(opnd.ReturnType);
				CheckNodeSet(opnd2.ReturnType);
				opnd = new Operator(Operator.Op.UNION, opnd, opnd2);
			} while (true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		static bool IsNodeType(XPathScanner scaner)
		{
			return (
				scaner.Prefix.Length == 0 && (
					scaner.Name == "node" ||
					scaner.Name == "text" ||
					scaner.Name == "processing-instruction" ||
					scaner.Name == "comment"
				)
			);
		}

		//>> PathOp   ::= '/' | '//'
		//>> PathExpr ::= LocationPath | 
		//>>              FilterExpr ( PathOp  RelativeLocationPath )?
		AstNode ParsePathExpr(AstNode qyInput)
		{
			AstNode opnd;
			if (IsPrimaryExpr(this.scanner))
			{ // in this moment we shoud distinct LocationPas vs FilterExpr (which starts from is PrimaryExpr)
				opnd = ParseFilterExpr(qyInput);
				if (this.scanner.Kind == XPathScanner.LexKind.Slash)
				{
					NextLex();
					opnd = ParseRelativeLocationPath(opnd);
				}
				else if (this.scanner.Kind == XPathScanner.LexKind.SlashSlash)
				{
					NextLex();
					opnd = ParseRelativeLocationPath(new Axis(Axis.AxisType.DescendantOrSelf, opnd));
				}
			}
			else
			{
				opnd = ParseLocationPath(null);
			}

			return opnd;
		}

		//>> FilterExpr ::= PrimaryExpr | FilterExpr Predicate 
		AstNode ParseFilterExpr(AstNode qyInput)
		{
			AstNode opnd = ParsePrimaryExpr(qyInput);
			while (this.scanner.Kind == XPathScanner.LexKind.LBracket)
			{
				// opnd must be a query
				opnd = new Filter(opnd, ParsePredicate(opnd));
			}
			return opnd;
		}

		//>> Predicate ::= '[' Expr ']'
		AstNode ParsePredicate(AstNode qyInput)
		{
			AstNode opnd;

			// we have predicates. Check that input type is NodeSet
			CheckNodeSet(qyInput.ReturnType);

			PassToken(XPathScanner.LexKind.LBracket);
			opnd = ParseExpresion(qyInput);
			PassToken(XPathScanner.LexKind.RBracket);

			return opnd;
		}

		//>> LocationPath ::= RelativeLocationPath | AbsoluteLocationPath
		AstNode ParseLocationPath(AstNode qyInput)
		{
			if (this.scanner.Kind == XPathScanner.LexKind.Slash)
			{
				NextLex();
				AstNode opnd = new Root();

				if (IsStep(this.scanner.Kind))
				{
					opnd = ParseRelativeLocationPath(opnd);
				}
				return opnd;
			}
			else if (this.scanner.Kind == XPathScanner.LexKind.SlashSlash)
			{
				NextLex();
				return ParseRelativeLocationPath(new Axis(Axis.AxisType.DescendantOrSelf, new Root()));
			}
			else
			{
				return ParseRelativeLocationPath(qyInput);
			}
		} // ParseLocationPath

		//>> PathOp   ::= '/' | '//'
		//>> RelativeLocationPath ::= ( RelativeLocationPath PathOp )? Step 
		AstNode ParseRelativeLocationPath(AstNode qyInput)
		{
			AstNode opnd = ParseStep(qyInput);
			if (XPathScanner.LexKind.SlashSlash == this.scanner.Kind)
			{
				NextLex();
				opnd = ParseRelativeLocationPath(new Axis(Axis.AxisType.DescendantOrSelf, opnd));
			}
			else if (XPathScanner.LexKind.Slash == this.scanner.Kind)
			{
				NextLex();
				opnd = ParseRelativeLocationPath(opnd);
			}
			return opnd;
		}

		static bool IsStep(XPathScanner.LexKind lexKind)
		{
			return (
				lexKind == XPathScanner.LexKind.Dot ||
				lexKind == XPathScanner.LexKind.DotDot ||
				lexKind == XPathScanner.LexKind.At ||
				lexKind == XPathScanner.LexKind.Axe ||
				lexKind == XPathScanner.LexKind.Star ||
				lexKind == XPathScanner.LexKind.Name          // NodeTest is also Name
			);
		}

		//>> Step ::= '.' | '..' | ( AxisName '::' | '@' )? NodeTest Predicate*
		AstNode ParseStep(AstNode qyInput)
		{
			AstNode opnd;
			if (XPathScanner.LexKind.Dot == this.scanner.Kind)
			{         //>> '.'
				NextLex();
				opnd = new Axis(Axis.AxisType.Self, qyInput);
			}
			else if (XPathScanner.LexKind.DotDot == this.scanner.Kind)
			{ //>> '..'
				NextLex();
				opnd = new Axis(Axis.AxisType.Parent, qyInput);
			}
			else
			{                                                          //>> ( AxisName '::' | '@' )? NodeTest Predicate*
				Axis.AxisType axisType = Axis.AxisType.Child;
				switch (this.scanner.Kind)
				{
					case XPathScanner.LexKind.At:                               //>> '@'
						axisType = Axis.AxisType.Attribute;
						NextLex();
						break;
					case XPathScanner.LexKind.Axe:                              //>> AxisName '::'
						axisType = GetAxis(this.scanner);
						NextLex();
						break;
				}
				XPathNodeType nodeType = (
					axisType == Axis.AxisType.Attribute ? XPathNodeType.Attribute :
					//                    axisType == Axis.AxisType.Namespace ? XPathNodeType.Namespace : // No Idea why it's this way but othervise Axes doesn't work
					/* default: */                        XPathNodeType.Element
				);

				opnd = ParseNodeTest(qyInput, axisType, nodeType);

				while (XPathScanner.LexKind.LBracket == this.scanner.Kind)
				{
					opnd = new Filter(opnd, ParsePredicate(opnd));
				}
			}
			return opnd;
		}

		//>> NodeTest ::= NameTest | 'comment ()' | 'text ()' | 'node ()' | 'processing-instruction ('  Literal ? ')'
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		AstNode ParseNodeTest(AstNode qyInput, Axis.AxisType axisType, XPathNodeType nodeType)
		{
			string nodeName, nodePrefix;

			switch (this.scanner.Kind)
			{
				case XPathScanner.LexKind.Name:
					if (this.scanner.CanBeFunction && IsNodeType(this.scanner))
					{
						nodePrefix = string.Empty;
						nodeName = string.Empty;
						nodeType = (
							this.scanner.Name == "comment" ? XPathNodeType.Comment :
							this.scanner.Name == "text" ? XPathNodeType.Text :
							this.scanner.Name == "node" ? XPathNodeType.All :
							this.scanner.Name == "processing-instruction" ? XPathNodeType.ProcessingInstruction :
							/* default: */ XPathNodeType.Root
						);
						Debug.Assert(nodeType != XPathNodeType.Root);
						NextLex();

						PassToken(XPathScanner.LexKind.LParens);

						if (nodeType == XPathNodeType.ProcessingInstruction)
						{
							if (this.scanner.Kind != XPathScanner.LexKind.RParens)
							{ //>> 'processing-instruction (' Literal ')'
								CheckToken(XPathScanner.LexKind.String);
								nodeName = this.scanner.StringValue;
								NextLex();
							}
						}

						PassToken(XPathScanner.LexKind.RParens);
					}
					else
					{
						nodePrefix = this.scanner.Prefix;
						nodeName = this.scanner.Name;
						NextLex();
						if (nodeName == "*")
						{
							nodeName = string.Empty;
						}
					}
					break;
				case XPathScanner.LexKind.Star:
					nodePrefix = string.Empty;
					nodeName = string.Empty;
					NextLex();
					break;
				default:
					throw new XPathException(String.Format("Expression {0} must evaluate to a node-set.", this.scanner.SourceText));
			}
			return new Axis(axisType, qyInput, nodePrefix, nodeName, nodeType);
		}

		static bool IsPrimaryExpr(XPathScanner scanner)
		{
			return (
				scanner.Kind == XPathScanner.LexKind.String ||
				scanner.Kind == XPathScanner.LexKind.Number ||
				scanner.Kind == XPathScanner.LexKind.Dollar ||
				scanner.Kind == XPathScanner.LexKind.LParens ||
				scanner.Kind == XPathScanner.LexKind.Name && scanner.CanBeFunction && !IsNodeType(scanner)
			);
		}

		//>> PrimaryExpr ::= Literal | Number | VariableReference | '(' Expr ')' | FunctionCall
		AstNode ParsePrimaryExpr(AstNode qyInput)
		{
			Debug.Assert(IsPrimaryExpr(this.scanner));
			AstNode opnd = null;
			switch (this.scanner.Kind)
			{
				case XPathScanner.LexKind.String:
					opnd = new Operand(this.scanner.StringValue);
					NextLex();
					break;
				case XPathScanner.LexKind.Number:
					opnd = new Operand(this.scanner.NumberValue);
					NextLex();
					break;
				case XPathScanner.LexKind.Dollar:
					NextLex();
					CheckToken(XPathScanner.LexKind.Name);
					opnd = new Variable(this.scanner.Name, this.scanner.Prefix);
					NextLex();
					break;
				case XPathScanner.LexKind.LParens:
					NextLex();
					opnd = ParseExpresion(qyInput);
					if (opnd.TypeOfAst != AstNode.QueryType.ConstantOperand)
					{
						opnd = new Group(opnd);
					}
					PassToken(XPathScanner.LexKind.RParens);
					break;
				case XPathScanner.LexKind.Name:
					if (this.scanner.CanBeFunction && !IsNodeType(this.scanner))
					{
						opnd = ParseMethod(null);
					}
					break;
			}
			Debug.Assert(opnd != null, (NoResString)"IsPrimaryExpr() was true. We should recognize this lex.");
			return opnd;
		}

		AstNode ParseMethod(AstNode qyInput)
		{
			ArrayList argList = new ArrayList();
			string name = this.scanner.Name;
			string prefix = this.scanner.Prefix;
			PassToken(XPathScanner.LexKind.Name);
			PassToken(XPathScanner.LexKind.LParens);
			if (this.scanner.Kind != XPathScanner.LexKind.RParens)
			{
				do
				{
					argList.Add(ParseExpresion(qyInput));
					if (this.scanner.Kind == XPathScanner.LexKind.RParens)
					{
						break;
					}
					PassToken(XPathScanner.LexKind.Comma);
				} while (true);
			}
			PassToken(XPathScanner.LexKind.RParens);
			if (string.IsNullOrEmpty(prefix))
			{
				ParamInfo pi;
				if (FunctionTable.TryGetValue(name, out pi))
				{
					int argCount = argList.Count;
					if (argCount < pi.Minargs)
					{
						throw new XPathException(String.Format("Function '{0}' in '{1}' has invalid number of arguments.", name, this.scanner.SourceText));
					}
					if (pi.FType == Function.FunctionType.FuncConcat)
					{
						for (int i = 0; i < argCount; i++)
						{
							AstNode arg = (AstNode)argList[i];
							if (arg.ReturnType != XPathResultType.String)
							{
								arg = new Function(Function.FunctionType.FuncString, arg);
							}
							argList[i] = arg;
						}
					}
					else
					{
						if (pi.Maxargs < argCount)
						{
							throw new XPathException(String.Format("Function '{0}' in '{1}' has invalid number of arguments.", name, this.scanner.SourceText));
						}
						if (pi.ArgTypes.Length < argCount)
						{
							argCount = pi.ArgTypes.Length;    // argument we have the type specified (can be < pi.Minargs)
						}
						for (int i = 0; i < argCount; i++)
						{
							AstNode arg = (AstNode)argList[i];
							if (
								pi.ArgTypes[i] != XPathResultType.Any &&
								pi.ArgTypes[i] != arg.ReturnType
							)
							{
								switch (pi.ArgTypes[i])
								{
									case XPathResultType.NodeSet:
										if (!(arg is Variable) && !(arg is Function && arg.ReturnType == XPathResultType.Error))
										{
											throw new XPathException(String.Format("Function '{0}' in '{1}' has invalid number of arguments.", name, this.scanner.SourceText));
										}
										break;
									case XPathResultType.String:
										arg = new Function(Function.FunctionType.FuncString, arg);
										break;
									case XPathResultType.Number:
										arg = new Function(Function.FunctionType.FuncNumber, arg);
										break;
									case XPathResultType.Boolean:
										arg = new Function(Function.FunctionType.FuncBoolean, arg);
										break;
								}
								argList[i] = arg;
							}
						}
					}
					return new Function(pi.FType, argList);
				}
			}
			return new Function(prefix, name, argList);
		}

		// --------------- Pattern Parsing ----------------------

		//>> Pattern ::= ( Pattern '|' )? LocationPathPattern
		AstNode ParsePattern(AstNode qyInput)
		{
			AstNode opnd = ParseLocationPathPattern(qyInput);

			do
			{
				if (this.scanner.Kind != XPathScanner.LexKind.Union)
				{
					return opnd;
				}
				NextLex();
				opnd = new Operator(Operator.Op.UNION, opnd, ParseLocationPathPattern(qyInput));
			} while (true);
		}

		//>> LocationPathPattern ::= '/' | RelativePathPattern | '//' RelativePathPattern  |  '/' RelativePathPattern
		//>>                       | IdKeyPattern (('/' | '//') RelativePathPattern)?  
		AstNode ParseLocationPathPattern(AstNode qyInput)
		{
			AstNode opnd = null;
			switch (this.scanner.Kind)
			{
				case XPathScanner.LexKind.Slash:
					NextLex();
					opnd = new Root();
					if (this.scanner.Kind == XPathScanner.LexKind.Eof || this.scanner.Kind == XPathScanner.LexKind.Union)
					{
						return opnd;
					}
					break;
				case XPathScanner.LexKind.SlashSlash:
					NextLex();
					opnd = new Axis(Axis.AxisType.DescendantOrSelf, new Root());
					break;
				case XPathScanner.LexKind.Name:
					if (this.scanner.CanBeFunction)
					{
						opnd = ParseIdKeyPattern(qyInput);
						if (opnd != null)
						{
							switch (this.scanner.Kind)
							{
								case XPathScanner.LexKind.Slash:
									NextLex();
									break;
								case XPathScanner.LexKind.SlashSlash:
									NextLex();
									opnd = new Axis(Axis.AxisType.DescendantOrSelf, opnd);
									break;
								default:
									return opnd;
							}
						}
					}
					break;
			}
			return ParseRelativePathPattern(opnd);
		}

		//>> IdKeyPattern ::= 'id' '(' Literal ')' | 'key' '(' Literal ',' Literal ')'  
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Microsoft XPath Reader")]
		AstNode ParseIdKeyPattern(AstNode qyInput)
		{
			Debug.Assert(this.scanner.CanBeFunction);
			ArrayList argList = new ArrayList();
			if (this.scanner.Prefix.Length == 0)
			{
				if (this.scanner.Name == "id")
				{
					var pi = FunctionTable["id"];
					NextLex();
					PassToken(XPathScanner.LexKind.LParens);
					CheckToken(XPathScanner.LexKind.String);
					argList.Add(new Operand(this.scanner.StringValue));
					NextLex();
					PassToken(XPathScanner.LexKind.RParens);
					return new Function(pi.FType, argList);
				}
				if (this.scanner.Name == "key")
				{
					NextLex();
					PassToken(XPathScanner.LexKind.LParens);
					CheckToken(XPathScanner.LexKind.String);
					argList.Add(new Operand(this.scanner.StringValue));
					NextLex();
					PassToken(XPathScanner.LexKind.Comma);
					CheckToken(XPathScanner.LexKind.String);
					argList.Add(new Operand(this.scanner.StringValue));
					NextLex();
					PassToken(XPathScanner.LexKind.RParens);
					return new Function("", (NoResString)"key", argList);
				}
			}
			return null;
		}

		//>> PathOp   ::= '/' | '//'
		//>> RelativePathPattern ::= ( RelativePathPattern PathOp )? StepPattern
		AstNode ParseRelativePathPattern(AstNode qyInput)
		{
			AstNode opnd = ParseStepPattern(qyInput);
			if (XPathScanner.LexKind.SlashSlash == this.scanner.Kind)
			{
				NextLex();
				opnd = ParseRelativePathPattern(new Axis(Axis.AxisType.DescendantOrSelf, opnd));
			}
			else if (XPathScanner.LexKind.Slash == this.scanner.Kind)
			{
				NextLex();
				opnd = ParseRelativePathPattern(opnd);
			}
			return opnd;
		}

		//>> StepPattern    ::=    ChildOrAttributeAxisSpecifier NodeTest Predicate*   
		//>> ChildOrAttributeAxisSpecifier    ::=    @ ? | ('child' | 'attribute') '::' 
		AstNode ParseStepPattern(AstNode qyInput)
		{
			AstNode opnd;
			Axis.AxisType axisType = Axis.AxisType.Child;
			switch (this.scanner.Kind)
			{
				case XPathScanner.LexKind.At:                               //>> '@'
					axisType = Axis.AxisType.Attribute;
					NextLex();
					break;
				case XPathScanner.LexKind.Axe:                              //>> AxisName '::'
					axisType = GetAxis(this.scanner);
					if (axisType != Axis.AxisType.Child && axisType != Axis.AxisType.Attribute)
					{
						throw new XPathException(String.Format("'{0}' has an invalid token.", scanner.SourceText));
					}
					NextLex();
					break;
			}
			XPathNodeType nodeType = (
				axisType == Axis.AxisType.Attribute ? XPathNodeType.Attribute :
				/* default: */                        XPathNodeType.Element
			);

			opnd = ParseNodeTest(qyInput, axisType, nodeType);

			while (XPathScanner.LexKind.LBracket == this.scanner.Kind)
			{
				opnd = new Filter(opnd, ParsePredicate(opnd));
			}
			return opnd;
		}

		// --------------- Helper methods ----------------------

		void CheckToken(XPathScanner.LexKind t)
		{
			if (this.scanner.Kind != t)
			{
				throw new XPathException(String.Format("'{0}' has an invalid token.", this.scanner.SourceText));
			}
		}

		void PassToken(XPathScanner.LexKind t)
		{
			CheckToken(t);
			NextLex();
		}

		void NextLex()
		{
			this.scanner.NextLex();
		}

		bool TestOp(string op)
		{
			return (
				this.scanner.Kind == XPathScanner.LexKind.Name &&
				this.scanner.Prefix.Length == 0 &&
				this.scanner.Name.Equals(op)
			);
		}

		void CheckNodeSet(XPathResultType t)
		{
			if (t != XPathResultType.NodeSet && t != XPathResultType.Error)
			{
				throw new XPathException(String.Format("Expression {0} must evaluate to a node-set", this.scanner.SourceText));
			}
		}

		// ----------------------------------------------------------------
		static readonly ImmutableArray<XPathResultType> Temparray1 = ImmutableArray.Create(XPathResultType.Error);
		static readonly ImmutableArray<XPathResultType> Temparray2 = ImmutableArray.Create(XPathResultType.NodeSet);
		static readonly ImmutableArray<XPathResultType> Temparray3 = ImmutableArray.Create(XPathResultType.Any);
		static readonly ImmutableArray<XPathResultType> Temparray4 = ImmutableArray.Create(XPathResultType.String);
		static readonly ImmutableArray<XPathResultType> Temparray5 = ImmutableArray.Create(XPathResultType.String, XPathResultType.String);
		static readonly ImmutableArray<XPathResultType> Temparray6 = ImmutableArray.Create(XPathResultType.String, XPathResultType.Number, XPathResultType.Number);
		static readonly ImmutableArray<XPathResultType> Temparray7 = ImmutableArray.Create(XPathResultType.String, XPathResultType.String, XPathResultType.String);
		static readonly ImmutableArray<XPathResultType> Temparray8 = ImmutableArray.Create(XPathResultType.Boolean);
		static readonly ImmutableArray<XPathResultType> Temparray9 = ImmutableArray.Create(XPathResultType.Number);

		[Immutable]
		class ParamInfo
		{
			readonly Function.FunctionType fType;
			readonly int minargs;
			readonly int maxargs;
			readonly ImmutableArray<XPathResultType> argTypes;

			public Function.FunctionType FType
			{
				get { return fType; }
			}

			public int Minargs
			{
				get { return minargs; }
			}

			public int Maxargs
			{
				get { return maxargs; }
			}

			public ImmutableArray<XPathResultType> ArgTypes
			{
				get { return argTypes; }
			}

			internal ParamInfo(Function.FunctionType ftype, int minargs, int maxargs, ImmutableArray<XPathResultType> argTypes)
			{
				fType = ftype;
				this.minargs = minargs;
				this.maxargs = maxargs;
				this.argTypes = argTypes;
			}
		} //ParamInfo

		static readonly ImmutableDictionary<string, ParamInfo> FunctionTable = new Dictionary<string, ParamInfo>(27)
		{
			{ (NoResString)"last", new ParamInfo(Function.FunctionType.FuncLast, 0, 0, Temparray1) },
			{ (NoResString)"position", new ParamInfo(Function.FunctionType.FuncPosition, 0, 0, Temparray1) },
			{ (NoResString)"name", new ParamInfo(Function.FunctionType.FuncName, 0, 1, Temparray2) },
			{ "namespace-uri", new ParamInfo(Function.FunctionType.FuncNameSpaceUri, 0, 1, Temparray2) },
			{ "local-name", new ParamInfo(Function.FunctionType.FuncLocalName, 0, 1, Temparray2) },
			{ (NoResString)"count", new ParamInfo(Function.FunctionType.FuncCount, 1, 1, Temparray2) },
			{ (NoResString)"id", new ParamInfo(Function.FunctionType.FuncID, 1, 1, Temparray3) },
			{ (NoResString)"string", new ParamInfo(Function.FunctionType.FuncString, 0, 1, Temparray3) },
			{ (NoResString)"concat", new ParamInfo(Function.FunctionType.FuncConcat, 2, 100, Temparray4) },
			{ "starts-with", new ParamInfo(Function.FunctionType.FuncStartsWith, 2, 2, Temparray5) },
			{ (NoResString)"contains", new ParamInfo(Function.FunctionType.FuncContains, 2, 2, Temparray5) },
			{ "substring-before", new ParamInfo(Function.FunctionType.FuncSubstringBefore, 2, 2, Temparray5) },
			{ "substring-after", new ParamInfo(Function.FunctionType.FuncSubstringAfter, 2, 2, Temparray5) },
			{ (NoResString)"substring", new ParamInfo(Function.FunctionType.FuncSubstring, 2, 3, Temparray6) },
			{ "string-length", new ParamInfo(Function.FunctionType.FuncStringLength, 0, 1, Temparray4) },
			{ "normalize-space", new ParamInfo(Function.FunctionType.FuncNormalize, 0, 1, Temparray4) },
			{ (NoResString)"translate", new ParamInfo(Function.FunctionType.FuncTranslate, 3, 3, Temparray7) },
			{ (NoResString)"boolean", new ParamInfo(Function.FunctionType.FuncBoolean, 1, 1, Temparray3) },
			{ (NoResString)"not", new ParamInfo(Function.FunctionType.FuncNot, 1, 1, Temparray8) },
			{ (NoResString)"true", new ParamInfo(Function.FunctionType.FuncTrue, 0, 0, Temparray8) },
			{ (NoResString)"false", new ParamInfo(Function.FunctionType.FuncFalse, 0, 0, Temparray8) },
			{ (NoResString)"lang", new ParamInfo(Function.FunctionType.FuncLang, 1, 1, Temparray4) },
			{ (NoResString)"number", new ParamInfo(Function.FunctionType.FuncNumber, 0, 1, Temparray3) },
			{ (NoResString)"sum", new ParamInfo(Function.FunctionType.FuncSum, 1, 1, Temparray2) },
			{ (NoResString)"floor", new ParamInfo(Function.FunctionType.FuncFloor, 1, 1, Temparray9) },
			{ (NoResString)"ceiling", new ParamInfo(Function.FunctionType.FuncCeiling, 1, 1, Temparray9) },
			{ (NoResString)"round", new ParamInfo(Function.FunctionType.FuncRound, 1, 1, Temparray9) },
		}.ToImmutableDictionary();

		static readonly ImmutableDictionary<string, Axis.AxisType> AxesTable = new Dictionary<string, Axis.AxisType>(13)
		{
			{ (NoResString)"ancestor", Axis.AxisType.Ancestor },
			{ "ancestor-or-self", Axis.AxisType.AncestorOrSelf },
			{ (NoResString)"attribute", Axis.AxisType.Attribute },
			{ (NoResString)"child", Axis.AxisType.Child },
			{ (NoResString)"descendant", Axis.AxisType.Descendant },
			{ "descendant-or-self", Axis.AxisType.DescendantOrSelf },
			{ (NoResString)"following", Axis.AxisType.Following },
			{ "following-sibling", Axis.AxisType.FollowingSibling },
			{ (NoResString)"namespace", Axis.AxisType.Namespace },
			{ (NoResString)"parent", Axis.AxisType.Parent },
			{ (NoResString)"preceding", Axis.AxisType.Preceding },
			{ "preceding-sibling", Axis.AxisType.PrecedingSibling },
			{ (NoResString)"self", Axis.AxisType.Self },
		}.ToImmutableDictionary();

		Axis.AxisType GetAxis(XPathScanner scaner)
		{
			Debug.Assert(scaner.Kind == XPathScanner.LexKind.Axe);
			Axis.AxisType axis;
			if (!AxesTable.TryGetValue(scaner.Name, out axis))
			{
				throw new XPathException(string.Format("'{0}' has an invalid token.", scanner.SourceText));
			}
			return axis;
		}
	}
}
