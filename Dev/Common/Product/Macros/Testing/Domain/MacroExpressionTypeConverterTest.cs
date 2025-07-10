using System;
using System.Linq.Expressions;
using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.Macros.Testing
{
	class MacroExpressionTypeConverterTest : TestCase
	{
		public void TestFindConversion_ZBool_To_Bool()
		{
			var valueToConvert1 = (ZBool)true;

			AssertNotNull("expression created for ZBool -> bool conversion",
				MacroExpressionTypeConverter.Instance.GetConversionExpression(Expression.Constant(valueToConvert1), valueToConvert1, typeof(bool)));

			const bool valueToConvert2 = true;

			AssertNotNull("expression created for bool -> ZBool conversion",
				MacroExpressionTypeConverter.Instance.GetConversionExpression(Expression.Constant(valueToConvert2), valueToConvert2, typeof(ZBool)));
		}

		public void TestFindConversion_ZInt_To_Int()
		{
			var valueToConvert1 = (ZInt)44;

			AssertNotNull("expression created for ZInt -> int conversion",
				MacroExpressionTypeConverter.Instance.GetConversionExpression(Expression.Constant(valueToConvert1), valueToConvert1, typeof(int)));

			const int valueToConvert2 = 44;

			AssertNotNull("expression created for int -> ZInt conversion",
				MacroExpressionTypeConverter.Instance.GetConversionExpression(Expression.Constant(valueToConvert2), valueToConvert2, typeof(ZInt)));
		}

		public void TestFindConversion_ZDecimal_To_Decimal()
		{
			var valueToConvert1 = (ZDecimal)1.1d;

			AssertNotNull("expression created for ZDecimal -> decimal conversion",
				MacroExpressionTypeConverter.Instance.GetConversionExpression(Expression.Constant(valueToConvert1), valueToConvert1, typeof(decimal)));

			const double valueToConvert2 = 1.1d;

			AssertNotNull("expression created for decimal -> ZDecimal conversion",
				MacroExpressionTypeConverter.Instance.GetConversionExpression(Expression.Constant(valueToConvert2), valueToConvert2, typeof(ZDecimal)));
		}

		public void TestFindConversion_ZString_To_String()
		{
			var valueToConvert1 = (ZString)"aaa";

			AssertNotNull("expression created for ZString -> string conversion",
				MacroExpressionTypeConverter.Instance.GetConversionExpression(Expression.Constant(valueToConvert1), valueToConvert1, typeof(string)));

			const string valueToConvert2 = "aaa";

			AssertNotNull("expression created for string -> ZString conversion",
				MacroExpressionTypeConverter.Instance.GetConversionExpression(Expression.Constant(valueToConvert2), valueToConvert2, typeof(ZString)));
		}

		public void TestFindConversion_MacroObject_ZTypeToDotNetType()
		{
			var obj = new MacroObject(new ZInt(3));

			var expr = Expression.Constant(obj);

			var conversionExpr = MacroExpressionTypeConverter.Instance.GetConversionExpression(expr, obj, typeof(int));

			AssertNotNull("no need to create conversion expression", expr);

			var lambdaExpr = Expression.Lambda<Func<int>>(conversionExpr);

			var lambda = lambdaExpr.Compile();

			AssertEquals("conversion result", 3, lambda());
		}

		public void TestFindConversion_MacroObject_DotNetTypeToZType()
		{
			var obj = new MacroObject(3);

			var expr = Expression.Constant(obj);

			var conversionExpr = MacroExpressionTypeConverter.Instance.GetConversionExpression(expr, obj, typeof(ZInt));

			AssertNotNull("no need to create conversion expression", expr);

			var lambdaExpr = Expression.Lambda<Func<ZInt>>(conversionExpr);

			var lambda = lambdaExpr.Compile();

			AssertEquals("conversion result", new ZInt(3), lambda());
		}

		public void TestIsConvertible_ConvertibleMacroObject_ZTypeToDotNetType()
		{
			var obj = new MacroObject(new ZInt(2));

			var isConvertible = MacroExpressionTypeConverter.Instance.IsConvertible(obj, typeof(int));

			AssertEquals("MacroObject(ZInt) is convertible to int", true, isConvertible);
		}

		public void TestIsConvertible_ConvertibleMacroObject_DotNetTypeToZType()
		{
			var obj = new MacroObject(2);

			var isConvertible = MacroExpressionTypeConverter.Instance.IsConvertible(obj, typeof(ZInt));

			AssertEquals("MacroObject(int) is convertible to ZInt", true, isConvertible);
		}

		public void TestIsConvertible_MacroObject_Inception()
		{
			var source = new MacroObject(3);
			var obj = new MacroObject(source);

			var isConvertible = MacroExpressionTypeConverter.Instance.IsConvertible(obj, typeof(ZInt));

			AssertEquals("MacroObject(MacroObject(int)) is convertible to ZInt", true, isConvertible);
		}

		#region Impelemntation

		protected override void TearDown()
		{
			base.TearDown();
			MacroExpressionTypeConverter.Instance.Clear();
		}

		#endregion
	}
}