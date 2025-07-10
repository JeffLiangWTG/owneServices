using System;
using System.Collections;
using System.Collections.Immutable;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.eHubMessaging.Business
{
	class Function : AstNode
	{
		internal enum FunctionType
		{
			FuncLast = 0,
			FuncPosition,
			FuncCount,
			FuncLocalName,
			FuncNameSpaceUri,
			FuncName,
			FuncString,
			FuncBoolean,
			FuncNumber,
			FuncTrue,
			FuncFalse,
			FuncNot,
			FuncID,
			FuncConcat,
			FuncStartsWith,
			FuncContains,
			FuncSubstringBefore,
			FuncSubstringAfter,
			FuncSubstring,
			FuncStringLength,
			FuncNormalize,
			FuncTranslate,
			FuncLang,
			FuncSum,
			FuncFloor,
			FuncCeiling,
			FuncRound,
			FuncUserDefined,
			Error
		}

		readonly FunctionType _functionType = FunctionType.Error;
		readonly ArrayList _argumentList;

		static readonly ImmutableArray<string> Str = ImmutableArray.Create(
			(NoResString)"last()",
			(NoResString)"position()",
			(NoResString)"count()",
			(NoResString)"localname()",
			(NoResString)"namespaceuri()",
			(NoResString)"name()",
			(NoResString)"string()",
			(NoResString)"boolean()",
			(NoResString)"number()",
			(NoResString)"true()",
			(NoResString)"false()",
			(NoResString)"not()",
			(NoResString)"id()",
			(NoResString)"concat()",
			(NoResString)"starts-with()",
			(NoResString)"contains()",
			(NoResString)"substring-before()",
			(NoResString)"substring-after()",
			"substring()",
			(NoResString)"string-length()",
			(NoResString)"normalize-space()",
			(NoResString)"translate()",
			(NoResString)"lang()",
			(NoResString)"sum()",
			(NoResString)"floor()",
			(NoResString)"celing()",
			(NoResString)"round()"
		);

		readonly String _Name;
		readonly String _Prefix;

		internal Function(FunctionType ftype, ArrayList argumentList)
		{
			_functionType = ftype;
			_argumentList = new ArrayList(argumentList);
		}

		internal Function(String prefix, String name, ArrayList argumentList)
		{
			_functionType = FunctionType.FuncUserDefined;
			_Prefix = prefix;
			_Name = name;
			_argumentList = new ArrayList(argumentList);
		}

		internal Function(FunctionType ftype)
		{
			_functionType = ftype;
		}

		internal Function(FunctionType ftype, AstNode arg)
		{
			_functionType = ftype;
			_argumentList = new ArrayList();
			_argumentList.Add(arg);
		}

		internal override QueryType TypeOfAst
		{
			get { return QueryType.Function; }
		}

		internal override XPathResultType ReturnType
		{
			get
			{
				switch (_functionType)
				{
					case FunctionType.FuncLast: return XPathResultType.Number;
					case FunctionType.FuncPosition: return XPathResultType.Number;
					case FunctionType.FuncCount: return XPathResultType.Number;
					case FunctionType.FuncID: return XPathResultType.NodeSet;
					case FunctionType.FuncLocalName: return XPathResultType.String;
					case FunctionType.FuncNameSpaceUri: return XPathResultType.String;
					case FunctionType.FuncName: return XPathResultType.String;
					case FunctionType.FuncString: return XPathResultType.String;
					case FunctionType.FuncBoolean: return XPathResultType.Boolean;
					case FunctionType.FuncNumber: return XPathResultType.Number;
					case FunctionType.FuncTrue: return XPathResultType.Boolean;
					case FunctionType.FuncFalse: return XPathResultType.Boolean;
					case FunctionType.FuncNot: return XPathResultType.Boolean;
					case FunctionType.FuncConcat: return XPathResultType.String;
					case FunctionType.FuncStartsWith: return XPathResultType.Boolean;
					case FunctionType.FuncContains: return XPathResultType.Boolean;
					case FunctionType.FuncSubstringBefore: return XPathResultType.String;
					case FunctionType.FuncSubstringAfter: return XPathResultType.String;
					case FunctionType.FuncSubstring: return XPathResultType.String;
					case FunctionType.FuncStringLength: return XPathResultType.Number;
					case FunctionType.FuncNormalize: return XPathResultType.String;
					case FunctionType.FuncTranslate: return XPathResultType.String;
					case FunctionType.FuncLang: return XPathResultType.Boolean;
					case FunctionType.FuncSum: return XPathResultType.Number;
					case FunctionType.FuncFloor: return XPathResultType.Number;
					case FunctionType.FuncCeiling: return XPathResultType.Number;
					case FunctionType.FuncRound: return XPathResultType.Number;
					case FunctionType.FuncUserDefined: return XPathResultType.Error;
				}
				return XPathResultType.Error;
			}
		}

		internal FunctionType TypeOfFunction
		{
			get { return _functionType; }
		}

		internal ArrayList ArgumentList
		{
			get { return _argumentList; }
		}

		internal String Prefix
		{
			get { return _Prefix; }
		}

		internal string Name
		{
			get { return _functionType == FunctionType.FuncUserDefined ? _Name : Str[(int)_functionType]; }
		}
	}
}
