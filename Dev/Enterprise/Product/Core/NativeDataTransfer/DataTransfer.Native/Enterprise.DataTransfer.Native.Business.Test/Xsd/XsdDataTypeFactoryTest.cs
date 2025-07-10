using Enterprise.DataTransfer.Native.Business.Xsd.Type;
using Enterprise.DataTransfer.Native.DB;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Xsd
{
	public class XsdDataTypeFactoryTest : TestCase
	{
		public void TestMap_String_Char_DefaultValueIsNull()
		{
			column.Setup(m => m.DataType).Returns("char");
			column.Setup(m => m.Length).Returns(0);
			column.Setup(m => m.DefaultValue).Returns(null);

			var type = XsdDataTypeFactory.Map(column.Object);
			Assert(type is XsString);
		}

		public void TestMap_String_Char_DefaultValueIsEmpty()
		{
			column.Setup(m => m.DataType).Returns("char");
			column.Setup(m => m.Length).Returns(0);
			column.Setup(m => m.DefaultValue).Returns(string.Empty);

			var type = XsdDataTypeFactory.Map(column.Object);
			Assert(type is XsString);
		}

		public void TestMap_String_Long()
		{
			column.Setup(m => m.DataType).Returns("char");
			column.Setup(m => m.Length).Returns(9);
			column.Setup(m => m.DefaultValue).Returns(string.Empty);

			var type = XsdDataTypeFactory.Map(column.Object);
			Assert(type is XsString);
		}

		public void TestMap_String_Short()
		{
			column.Setup(m => m.DataType).Returns("char");
			column.Setup(m => m.Length).Returns(3);
			column.Setup(m => m.DefaultValue).Returns(string.Empty);

			var type = XsdDataTypeFactory.Map(column.Object);
			Assert(type is XsString);
		}

		public void TestMap_Boolean_False()
		{
			column.Setup(m => m.DataType).Returns("char");
			column.Setup(m => m.Length).Returns(1);
			column.Setup(m => m.DefaultValue).Returns("('N')");

			var type = XsdDataTypeFactory.Map(column.Object);
			Assert(type is XsBoolean);
		}

		public void TestMap_Boolean_True()
		{
			column.Setup(m => m.DataType).Returns("char");
			column.Setup(m => m.Length).Returns(1);
			column.Setup(m => m.DefaultValue).Returns("('Y')");

			var type = XsdDataTypeFactory.Map(column.Object);
			Assert(type is XsBoolean);
		}

		public void TestMap_Boolean_Invalid()
		{
			column.Setup(m => m.DataType).Returns("char");
			column.Setup(m => m.Length).Returns(1);
			column.Setup(m => m.DefaultValue).Returns("('X')");

			var type = XsdDataTypeFactory.Map(column.Object);
			Assert(type is XsString);
		}

		public void TestMap_String_VarChar()
		{
			column.Setup(m => m.DataType).Returns("varchar");
			column.Setup(m => m.Length).Returns(29);
			column.Setup(m => m.DefaultValue).Returns(string.Empty);

			var type = XsdDataTypeFactory.Map(column.Object);
			Assert(type is XsString);
		}

		public void TestMap_Decimal()
		{
			column.Setup(m => m.DataType).Returns("decimal");
			column.Setup(m => m.Length).Returns(0);
			column.Setup(m => m.DefaultValue).Returns(null);

			var type = XsdDataTypeFactory.Map(column.Object);
			Assert(type is XsDecimal);
		}

		public void TestMap_Decimal_Money()
		{
			column.Setup(m => m.DataType).Returns("money");
			column.Setup(m => m.Length).Returns(0);
			column.Setup(m => m.DefaultValue).Returns(null);

			var type = XsdDataTypeFactory.Map(column.Object);
			Assert(type is XsDecimal);
		}

		public void TestMap_Short_TinyInt()
		{
			column.Setup(m => m.DataType).Returns("tinyint");
			column.Setup(m => m.Length).Returns(0);
			column.Setup(m => m.DefaultValue).Returns(null);

			var type = XsdDataTypeFactory.Map(column.Object);
			Assert(type is XsShort);
		}

		public void TestMap_Int()
		{
			column.Setup(m => m.DataType).Returns("int");
			column.Setup(m => m.Length).Returns(0);
			column.Setup(m => m.DefaultValue).Returns(null);

			var type = XsdDataTypeFactory.Map(column.Object);
			Assert(type is XsInteger);
		}

		public void TestMap_DateTime()
		{
			column.Setup(m => m.DataType).Returns("datetime");
			column.Setup(m => m.Length).Returns(0);
			column.Setup(m => m.DefaultValue).Returns(null);

			var type = XsdDataTypeFactory.Map(column.Object);
			Assert(type is XsDateTime);
		}

		public void TestMap_DateTimeOffset()
		{
			column.Setup(m => m.DataType).Returns("datetimeoffset");
			column.Setup(m => m.Length).Returns(0);
			column.Setup(m => m.DefaultValue).Returns(null);

			var type = XsdDataTypeFactory.Map(column.Object);
			Assert(type is XsDateTime);
		}

		public void TestMap_Time()
		{
			column.Setup(m => m.DataType).Returns("smalldatetime");
			column.Setup(m => m.Length).Returns(0);
			column.Setup(m => m.DefaultValue).Returns(null);

			var type = XsdDataTypeFactory.Map(column.Object);
			Assert(type is XsDateTime);
		}

		public void TestMap_Geography()
		{
			column.Setup(m => m.DataType).Returns("geography");
			column.Setup(m => m.Length).Returns(0);
			column.Setup(m => m.DefaultValue).Returns(null);

			var type = XsdDataTypeFactory.Map(column.Object);
			Assert(type is XsString);
		}

		public void TestMap_String_Text()
		{
			column.Setup(m => m.DataType).Returns("text");
			column.Setup(m => m.Length).Returns(0);
			column.Setup(m => m.DefaultValue).Returns(null);

			var type = XsdDataTypeFactory.Map(column.Object);
			Assert(type is XsString);
		}

		public void TestMap_String_Uniqueidentifier()
		{
			column.Setup(m => m.DataType).Returns("uniqueidentifier");
			column.Setup(m => m.Length).Returns(0);
			column.Setup(m => m.DefaultValue).Returns(null);

			var type = XsdDataTypeFactory.Map(column.Object);
			Assert(type is XsGuid);
		}

		public void TestMap_Base64Binary_Image()
		{
			column.Setup(m => m.DataType).Returns("image");
			column.Setup(m => m.Length).Returns(0);
			column.Setup(m => m.DefaultValue).Returns(null);

			var type = XsdDataTypeFactory.Map(column.Object);
			Assert(type is XsBase64Binary);
		}

		protected override void SetUp()
		{
			base.SetUp();
			column = new Mock<IColumnDef>();
		}
		Mock<IColumnDef> column;
	}
}
