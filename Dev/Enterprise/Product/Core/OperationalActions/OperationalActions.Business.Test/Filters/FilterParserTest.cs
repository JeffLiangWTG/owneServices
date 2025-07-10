using System;
using System.Text;
using Enterprise.Services.OperationalActions.Business.AST;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class FilterParserTest : TestCase
	{
		public void TestParsingEmpty()
		{
			const string inString = " \r\n\t" + "";
			const string outString = "empty" + "";
			AssertCanParse(inString, outString);
		}

		public void TestParsingComplex()
		{
			const string inString = @"
				(
					(Bob.Name == ""O'Shay"" && Bob.Class != ""Frank"") ||
					(Bob.Name in (""Bob"", ""Mike"") && Bob.Class != ""Frank"" && XX_Wigit in ( ""on""))
				) && Thunk == """"""true""""""
				";
			const string outString = "and(\n" + " or(\n" + "  and(\n" + "   check-eq(Bob.Name, 'O''Shay'),\n" + "   check-neq(Bob.Class, 'Frank')\n" + "  ),\n" + "  and(\n" + "   check-in(Bob.Name, 'Bob', 'Mike'),\n" + "   and(\n" + "    check-neq(Bob.Class, 'Frank'),\n" + "    check-in(XX_Wigit, 'on')\n" + "   )\n" + "  )\n" + " ),\n" + " check-eq(Thunk, '\"true\"')\n" + ")";
			AssertCanParse(inString, outString);
		}

		public void TestParsingAndOrPrecidence()
		{
			const string inString = @"
				name1 == ""value1"" && name2 == ""value2"" || name3 == ""value3"" && name4 == ""value4""
				";
			const string outString = "or(\n" + " and(\n" + "  check-eq(name1, 'value1'),\n" + "  check-eq(name2, 'value2')\n" + " ),\n" + " and(\n" + "  check-eq(name3, 'value3'),\n" + "  check-eq(name4, 'value4')\n" + " )\n" + ")";
			AssertCanParse(inString, outString);
		}

		public void TestParsingInequalities()
		{
			const string inString = @"
				name1 >= ""value1"" && name2 > ""value2"" && name3 < ""value3"" && name4 <= ""value4""
				";
			const string outString = "and(\n" + " check-greater-equal(name1, 'value1'),\n" + " and(\n" + "  check-greater(name2, 'value2'),\n" + "  and(\n" + "   check-less(name3, 'value3'),\n" + "   check-less-equal(name4, 'value4')\n" + "  )\n" + " )\n" + ")\n";
			AssertCanParse(inString, outString);
		}

		public void TestParseFailure()
		{
			AssertError("Country == \"AU\")", "unexpected token at position 15 (')')");
			AssertError("(Country == \"AU\"", "unexpected end of expression");
			AssertError("Country <> \"AU\"", "unrecognized token at position 8 ('<')");
			AssertError("Country == \"Blat\"\"icus", "unterminated value at position 11 ('\"Blat\"\"icus')");
			AssertError("Country == AU", "unexpected token at position 11 ('AU')");
			AssertError("<>", "unrecognized token at position 0 ('<>')");
		}

		#region Implementation
		void AssertCanParse(string inString, string outString)
		{
			IFilterExpression expression = FilterParser.Parse(inString);
			AssertNotNull(string.Format("should be able to parse '{0}'", inString), expression);
			AssertMultilineASCIIEquals(string.Format("should correctly parse '{0}'", inString), outString, DisplayString(expression));
		}

		void AssertError(string inString, string error)
		{
			try
			{
				FilterParser.Parse(inString);
				Fail(string.Format("should fail to parse '{0}'", inString));
			}
			catch (ParseException ex)
			{
				AssertEquals(string.Format("should fail to parse '{0}'", inString), error, ex.Message);
			}
		}

		string DisplayString(IFilterExpression exp)
		{
			StringBuilder builder = new StringBuilder();
			DisplayString(builder, 0, exp);
			return builder.ToString();
		}

		void DisplayString(StringBuilder builder, int indent, IFilterExpression exp)
		{
			if (exp is FilterCheckEquality)
			{
				DisplayString(builder, indent, (FilterCheckEquality)exp);
			}
			else if (exp is FilterCheckInequality)
			{
				DisplayString(builder, indent, (FilterCheckInequality)exp);
			}
			else if (exp is FilterIn)
			{
				DisplayString(builder, indent, (FilterIn)exp);
			}
			else if (exp is FilterAnd)
			{
				DisplayString(builder, indent, (FilterAnd)exp);
			}
			else if (exp is FilterOr)
			{
				DisplayString(builder, indent, (FilterOr)exp);
			}
			else if (exp is FilterEmpty)
			{
				DisplayString(builder, indent, (FilterEmpty)exp);
			}
			else if (exp is FilterCheckGreaterThan)
			{
				DisplayString(builder, indent, (FilterCheckGreaterThan)exp);
			}
			else if (exp is FilterCheckGreaterThanOrEqual)
			{
				DisplayString(builder, indent, (FilterCheckGreaterThanOrEqual)exp);
			}
			else if (exp is FilterCheckLessThan)
			{
				DisplayString(builder, indent, (FilterCheckLessThan)exp);
			}
			else if (exp is FilterCheckLessThanOrEqual)
			{
				DisplayString(builder, indent, (FilterCheckLessThanOrEqual)exp);
			}
			else
			{
				throw new ArgumentException("Invalid filter expression type", nameof(exp));
			}
		}

		void DisplayString(StringBuilder builder, int indent, FilterCheckEquality check)
		{
			builder.Append(' ', indent);
			builder.AppendFormat("check-eq(");
			DisplayString(builder, check.Left);
			builder.Append(", ");
			DisplayString(builder, check.Right);
			builder.Append(")");
		}

		void DisplayString(StringBuilder builder, int indent, FilterCheckInequality check)
		{
			builder.Append(' ', indent);
			builder.AppendFormat("check-neq(");
			DisplayString(builder, check.Left);
			builder.Append(", ");
			DisplayString(builder, check.Right);
			builder.Append(")");
		}

		void DisplayString(StringBuilder builder, int indent, FilterIn checkin)
		{
			builder.Append(' ', indent);
			builder.AppendFormat("check-in(");
			DisplayString(builder, checkin.Left);
			foreach (IFilterOperand operand in checkin.Right)
			{
				builder.Append(", ");
				DisplayString(builder, operand);
			}

			builder.Append(")");
		}

		void DisplayString(StringBuilder builder, int indent, FilterAnd and)
		{
			builder.Append(' ', indent);
			builder.Append("and(\n");
			DisplayString(builder, indent + 1, and.Left);
			builder.Append(",\n");
			DisplayString(builder, indent + 1, and.Right);
			builder.Append('\n');
			builder.Append(' ', indent);
			builder.Append(")");
		}

		void DisplayString(StringBuilder builder, int indent, FilterOr or)
		{
			builder.Append(' ', indent);
			builder.Append("or(\n");
			DisplayString(builder, indent + 1, or.Left);
			builder.Append(",\n");
			DisplayString(builder, indent + 1, or.Right);
			builder.Append('\n');
			builder.Append(' ', indent);
			builder.Append(")");
		}

		void DisplayString(StringBuilder builder, int indent, FilterCheckGreaterThan check)
		{
			builder.Append(' ', indent);
			builder.AppendFormat("check-greater(");
			DisplayString(builder, check.Left);
			builder.Append(", ");
			DisplayString(builder, check.Right);
			builder.Append(")");
		}

		void DisplayString(StringBuilder builder, int indent, FilterCheckGreaterThanOrEqual check)
		{
			builder.Append(' ', indent);
			builder.AppendFormat("check-greater-equal(");
			DisplayString(builder, check.Left);
			builder.Append(", ");
			DisplayString(builder, check.Right);
			builder.Append(")");
		}

		void DisplayString(StringBuilder builder, int indent, FilterCheckLessThan check)
		{
			builder.Append(' ', indent);
			builder.AppendFormat("check-less(");
			DisplayString(builder, check.Left);
			builder.Append(", ");
			DisplayString(builder, check.Right);
			builder.Append(")");
		}

		void DisplayString(StringBuilder builder, int indent, FilterCheckLessThanOrEqual check)
		{
			builder.Append(' ', indent);
			builder.AppendFormat("check-less-equal(");
			DisplayString(builder, check.Left);
			builder.Append(", ");
			DisplayString(builder, check.Right);
			builder.Append(")");
		}

		void DisplayString(StringBuilder builder, int indent, FilterEmpty empty)
		{
			builder.Append(' ', indent);
			builder.Append("empty");
		}

		void DisplayString(StringBuilder builder, IFilterOperand operand)
		{
			if (operand is FilterConstraintRef)
			{
				DisplayString(builder, (FilterConstraintRef)operand);
			}
			else if (operand is FilterLiteral)
			{
				DisplayString(builder, (FilterLiteral)operand);
			}
			else
			{
				throw new ArgumentException("Invalid filter operand type", nameof(operand));
			}
		}

		void DisplayString(StringBuilder builder, FilterConstraintRef constraint)
		{
			builder.Append(constraint.ConstraintName);
		}

		void DisplayString(StringBuilder builder, FilterLiteral literal)
		{
			builder.Append("'" + CargoWise.Data.DataUtils.EscapeSingleQuotes(literal.Value) + "'");
		}
		#endregion
	}
}
