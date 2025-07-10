using System;
using System.Collections.Generic;
using System.Text;
using Enterprise.Services.OperationalActions.Business.AST;

namespace Enterprise.Services.OperationalActions.Business
{
	/// <summary>
	/// This class is used to parse the expression string and build the abstract syntax tree.
	/// </summary>
	public static class FilterParser
	{
		// Grammer:
		// <Expression>	::= <OrExp> <End> | <End>
		// <OrExp>		::= <AndExp> | <AndExp> "||" <OrExp>
		// <AndExp>		::= <Comparison> | <Comparison> "&&" <AndExp>
		// <Comparison> ::= <Label> <Operator> <Value> | "(" <OrExp> ")" | <Label> "in" "(" <ValueList> ")"
		// <Operator>	::= "==" | "!="
		// <ValueList>	::= <Value> | <Value> "," <ValueList>
		// <End>		::= ? The end of the expression string ?
		// <Label>		::= ? A string matching the pattern ^[a-zA-Z_][a-zA-Z0-9_.+]*$ ?
		// <Value>		::= ? A string matching the pattern ^["]([^"]|["]{2})*["]$ ?
		//
		// Whitespace *between* tokens is ignored but may still be included as part of a <Value> token.

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Expression Keyword")]
		const string Keyword_In = "in";

		public static IFilterExpression Parse(string expressionString)
		{
			using (IEnumerator<Token> tokens = Tokeniser(expressionString))
			{
				tokens.MoveNext();
				return Parse_Expression(tokens);
			}
		}
		static IFilterExpression Parse_Expression(IEnumerator<Token> tokens)
		{
			if (tokens.Current.Type == TokenType.End)
			{
				tokens.MoveNext();
				return new FilterEmpty();
			}
			else
			{
				IFilterExpression expression = Parse_OrExp(tokens);
				ReadToken(tokens, TokenType.End);
				return expression;
			}
		}
		static IFilterExpression Parse_OrExp(IEnumerator<Token> tokens)
		{
			IFilterExpression left = Parse_AndExp(tokens);

			if (tokens.Current.Type == TokenType.Or)
			{
				tokens.MoveNext();
				IFilterExpression right = Parse_OrExp(tokens);

				return new FilterOr(left, right);
			}
			else
			{
				return left;
			}
		}
		static IFilterExpression Parse_AndExp(IEnumerator<Token> tokens)
		{
			IFilterExpression left = Parse_Comparison(tokens);

			if (tokens.Current.Type == TokenType.And)
			{
				tokens.MoveNext();
				IFilterExpression right = Parse_AndExp(tokens);

				return new FilterAnd(left, right);
			}
			else
			{
				return left;
			}
		}
		static IFilterExpression Parse_Comparison(IEnumerator<Token> tokens)
		{
			if (tokens.Current.Type == TokenType.OpenParen)
			{
				tokens.MoveNext();
				IFilterExpression innerExp = Parse_OrExp(tokens);
				ReadToken(tokens, TokenType.CloseParen);

				return innerExp;
			}
			else
			{
				IFilterOperand name = Parse_Label(tokens);

				if (tokens.Current.Type == TokenType.In)
				{
					tokens.MoveNext();

					ReadToken(tokens, TokenType.OpenParen);
					IFilterOperand[] values = Parse_ValueList(tokens);
					ReadToken(tokens, TokenType.CloseParen);

					return new FilterIn(name, values);
				}
				else
				{
					IFilterOperand value;

					switch (tokens.Current.Type)
					{
						case TokenType.Equality:
							tokens.MoveNext();
							value = Parse_Value(tokens);
							return new FilterCheckEquality(name, value);

						case TokenType.Inequality:
							tokens.MoveNext();
							value = Parse_Value(tokens);
							return new FilterCheckInequality(name, value);

						case TokenType.Greater:
							tokens.MoveNext();
							value = Parse_Value(tokens);
							return new FilterCheckGreaterThan(name, value);

						case TokenType.GreaterEqual:
							tokens.MoveNext();
							value = Parse_Value(tokens);
							return new FilterCheckGreaterThanOrEqual(name, value);

						case TokenType.Less:
							tokens.MoveNext();
							value = Parse_Value(tokens);
							return new FilterCheckLessThan(name, value);

						case TokenType.LessEqual:
							tokens.MoveNext();
							value = Parse_Value(tokens);
							return new FilterCheckLessThanOrEqual(name, value);

						default:
							ThrowParseException(tokens);
							throw new InvalidOperationException("ThrowParseException should have thrown an exception");
					}
				}
			}
		}
		static IFilterOperand[] Parse_ValueList(IEnumerator<Token> tokens)
		{
			List<IFilterOperand> values = new List<IFilterOperand>();

			values.Add(Parse_Value(tokens));

			while (tokens.Current.Type == TokenType.Comma)
			{
				tokens.MoveNext();
				values.Add(Parse_Value(tokens));
			}

			return values.ToArray();
		}
		static IFilterOperand Parse_Label(IEnumerator<Token> tokens)
		{
			return new FilterConstraintRef(ReadToken(tokens, TokenType.Label).Text);
		}
		static IFilterOperand Parse_Value(IEnumerator<Token> tokens)
		{
			return new FilterLiteral(ReadToken(tokens, TokenType.Value).Text);
		}

		static Token ReadToken(IEnumerator<Token> tokens, TokenType type)
		{
			if (tokens.Current.Type == type)
			{
				Token result = tokens.Current;
				tokens.MoveNext();
				return result;
			}
			else
			{
				ThrowParseException(tokens);
				throw new InvalidOperationException("ThrowParseException should have thrown an exception");
			}
		}
		static void ThrowParseException(IEnumerator<Token> tokens)
		{
			if (tokens.Current.Type == TokenType.Error)
			{
				throw new ParseException(tokens.Current.Text);
			}
			else if (tokens.Current.Type == TokenType.End)
			{
				throw new ParseException("unexpected end of expression");
			}
			else
			{
				throw new ParseException(string.Format("unexpected token at position {0} ('{1}')", tokens.Current.Index, tokens.Current.Text));
			}
		}

		static IEnumerator<Token> Tokeniser(string expression)
		{
			int startOfError = -1;

			for (int i = 0; i < expression.Length; i++)
			{
				bool flushError = false;
				int startOfToken = i;
				Token tok;
				if (char.IsWhiteSpace(expression, i))
				{
					flushError = true;
					tok = null;
				}
				else if (expression[i] == '(')
				{
					tok = new Token(startOfToken, TokenType.OpenParen, "(");
				}
				else if (expression[i] == ')')
				{
					tok = new Token(startOfToken, TokenType.CloseParen, ")");
				}
				else if (expression[i] == ',')
				{
					tok = new Token(startOfToken, TokenType.Comma, ",");
				}
				else if (expression[i] == '=' && i + 1 < expression.Length && expression[i + 1] == '=')
				{
					i++;
					tok = new Token(startOfToken, TokenType.Equality, "==");
				}
				else if (expression[i] == '!' && i + 1 < expression.Length && expression[i + 1] == '=')
				{
					i++;
					tok = new Token(startOfToken, TokenType.Inequality, "!=");
				}
				else if (expression[i] == '>' && i + 1 < expression.Length && expression[i + 1] == '=')
				{
					i++;
					tok = new Token(startOfToken, TokenType.GreaterEqual, ">=");
				}
				else if (expression[i] == '>' && i + 1 < expression.Length && (expression[i + 1] == '"' || expression[i + 1] == ' '))
				{
					tok = new Token(startOfToken, TokenType.Greater, ">");
				}
				else if (expression[i] == '<' && i + 1 < expression.Length && expression[i + 1] == '=')
				{
					i++;
					tok = new Token(startOfToken, TokenType.LessEqual, "<=");
				}
				else if (expression[i] == '<' && i + 1 < expression.Length && (expression[i + 1] == '"' || expression[i + 1] == ' '))
				{
					tok = new Token(startOfToken, TokenType.Less, "<");
				}
				else if (expression[i] == '&' && i + 1 < expression.Length && expression[i + 1] == '&')
				{
					i++;
					tok = new Token(startOfToken, TokenType.And, "&&");
				}
				else if (expression[i] == '|' && i + 1 < expression.Length && expression[i + 1] == '|')
				{
					i++;
					tok = new Token(startOfToken, TokenType.Or, "||");
				}
				else if (char.IsLetter(expression, i) || expression[i] == '_')
				{
					while (i + 1 < expression.Length && (char.IsLetterOrDigit(expression, i + 1) || expression[i + 1] == '_'))
					{
						i++;

						if (i + 2 < expression.Length && char.IsLetter(expression, i + 2) && (expression[i + 1] == '.' || expression[i + 1] == '+'))
						{
							i += 2;
						}
					}

					string label = expression.Substring(startOfToken, i - startOfToken + 1);

					if (label == Keyword_In)
					{
						tok = new Token(startOfToken, TokenType.In, Keyword_In);
					}
					else
					{
						tok = new Token(startOfToken, TokenType.Label, label);
					}
				}
				else if (expression[i] == '"')
				{
					StringBuilder builder = new StringBuilder();
					tok = null;
					i++;

					while (i < expression.Length)
					{
						if (expression[i] != '"')
						{
							builder.Append(expression[i]);
						}
						else if (i + 1 < expression.Length && expression[i + 1] == '"')
						{
							i++;
							builder.Append('"');
						}
						else
						{
							tok = new Token(startOfToken, TokenType.Value, builder.ToString());
							break;
						}

						i++;
					}

					if (tok == null)
					{
						string text = expression.Substring(startOfToken, i - startOfToken);
						tok = new Token(startOfToken, TokenType.Error, Res.GetString("OperationalActionsFilter|Parser|UnterminatedValue", "unterminated value at position {0} ('{1}')", startOfToken, text));
					}
				}
				else
				{
					if (startOfError < 0)
					{
						startOfError = i;
					}

					tok = null;
				}

				if (tok != null || flushError)
				{
					if (startOfError >= 0)
					{
						string text = expression.Substring(startOfError, startOfToken - startOfError);
						yield return new Token(startOfError, TokenType.Error, Res.GetString("OperationalActionsFilter|Parser|UnrecognisedToken", "unrecognized token at position {0} ('{1}')", startOfError, text));
						startOfError = -1;
					}

					if (tok != null)
					{
						yield return tok;
					}
				}
			}

			if (startOfError >= 0)
			{
				string text = expression.Substring(startOfError, expression.Length - startOfError);
				yield return new Token(startOfError, TokenType.Error, Res.GetString("OperationalActionsFilter|Parser|UnrecognisedToken", "unrecognized token at position {0} ('{1}')", startOfError, text));
				startOfError = -1;
			}

			yield return new Token(expression.Length, TokenType.End, "");
		}

		#region TokenType

		enum TokenType
		{
			End,
			Error,

			Label,
			Value,

			Equality,
			Inequality,
			Greater,
			GreaterEqual,
			Less,
			LessEqual,

			And,
			Or,

			OpenParen,
			CloseParen,

			In,
			Comma,
		}

		#endregion

		#region Token

		sealed class Token
		{
			public Token(int index, TokenType type, string text)
			{
				this.Index = index;
				this.Type = type;
				this.Text = text;
			}

			public readonly int Index;
			public readonly TokenType Type;
			public readonly string Text;
		}

		#endregion
	}
}
