using System.CodeDom;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Design.Testing
{
	sealed class DpiScalingSerializerHelperTest : TestCase
	{
		public void TestScaleCodeStatements()
		{
			AssertScaleCodeStatements(TableLayoutStyle.RowStyle, "ScaleToCurrentDpiY");
			AssertScaleCodeStatements(TableLayoutStyle.ColumnStyle, "ScaleToCurrentDpiX");
		}

		void AssertScaleCodeStatements(TableLayoutStyle style, string methodName)
		{
			var statements = new CodeStatementCollection();

			var styleConstructor = new CodeObjectCreateExpression($"System.Windows.Forms.{style}");
			styleConstructor.Parameters.Add(new CodeFieldReferenceExpression(null, "Absolute"));
			styleConstructor.Parameters.Add(new CodePrimitiveExpression(100f));
			var addMethodInvoke = new CodeMethodInvokeExpression(new CodePropertyReferenceExpression(null, $"{style}s"), "Add", styleConstructor);
			var styleStatement = new CodeExpressionStatement(addMethodInvoke);

			statements.Add(styleStatement);

			DpiScalingSerializerHelper.ScaleCodeStatements(null, null, statements);

			var lenght = (CodeMethodInvokeExpression)styleConstructor.Parameters[1];

			AssertEquals(methodName, lenght.Method.MethodName);
			AssertEquals(100, ((CodePrimitiveExpression)lenght.Parameters[0]).Value);
		}
	}
}
