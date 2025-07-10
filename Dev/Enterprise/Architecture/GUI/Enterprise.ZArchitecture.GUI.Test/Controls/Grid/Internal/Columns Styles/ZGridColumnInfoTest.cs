using System.Reflection;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Core.Forms.Testing
{
	sealed class ZGridColumnInfoTest : TestCase
	{
		public void TestIsSortableDefaultToTrue()
		{
			var styleInfo = new ZTextBoxColumnStyleInfo();
			AssertEquals(true, styleInfo.IsSortable);

			styleInfo = new ZTextBoxColumnStyleInfo("Column1", 150);
			AssertEquals(true, styleInfo.IsSortable);
		}

		public void TestSensitiveColumnCannotSortable()
		{
			var styleInfo = new ZTextBoxColumnStyleInfo() { PasswordChar = '*' };
			AssertEquals(true, styleInfo.IsSensitiveValue);
			AssertEquals(false, styleInfo.IsSortable);
		}

		public void TestIsCustomColumnDefaultTofalse()
		{
			var styleInfo = new ZTextBoxColumnStyleInfo();
			AssertEquals(false, styleInfo.IsCustomColumn);

			styleInfo = new ZTextBoxColumnStyleInfo("Column1", 150);
			AssertEquals(false, styleInfo.IsCustomColumn);
		}

		public void TestShouldSerializeCaptionResourceStringForDesigner()
		{
			var methodName = $"ShouldSerialize{nameof(ZGridColumnInfo.CaptionResourceString)}";
			AssertNotNull($"{nameof(ZGridColumnInfo)} should have the method '{methodName}' defined for visual studio designer.", typeof(ZGridColumnInfo).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic));
		}

		public void TestIsSensitiveValueDefaultTofalse()
		{
			var styleInfo = new ZTextBoxColumnStyleInfo();
			AssertEquals(false, styleInfo.IsSensitiveValue);

			styleInfo = new ZTextBoxColumnStyleInfo("Column1", 150);
			AssertEquals(false, styleInfo.IsSensitiveValue);
		}
	}
}
