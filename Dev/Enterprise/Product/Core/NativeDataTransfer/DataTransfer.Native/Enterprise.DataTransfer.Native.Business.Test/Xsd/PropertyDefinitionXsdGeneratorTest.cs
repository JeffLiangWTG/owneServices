using System.Linq;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.CodeMappings;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions;
using Enterprise.DataTransfer.Native.DB;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Xsd
{
	public class PropertyDefinitionXsdGeneratorTest : TestCase
	{
		public void TestRequiredAttributeIsNotPresentWhenDoesNotRequireAValueIsFalse()
		{
			propertyDef.Setup(m => m.PropertyName).Returns(string.Empty);
			propertyDef.Setup(m => m.ColumnDef).Returns(columnDef.Object);
			columnDef.Setup(m => m.DataType).Returns(DbDataType.Text);
			columnDef.Setup(m => m.Length).Returns(2);
			columnDef.Setup(m => m.DoesNotRequireAValue).Returns(false);

			var result = generator.Generate(propertyDef.Object, null);
			var attribute = result.Attributes().FirstOrDefault(a => a.Name.LocalName == "Required");
			AssertNull(attribute);
		}

		public void TestRequiredAttributeIsNotPresentWhenDoesNotRequireAValueIsTrue()
		{
			propertyDef.Setup(m => m.PropertyName).Returns(string.Empty);
			propertyDef.Setup(m => m.ColumnDef).Returns(columnDef.Object);
			columnDef.Setup(m => m.DataType).Returns(DbDataType.Text);
			columnDef.Setup(m => m.Length).Returns(2);
			columnDef.Setup(m => m.DoesNotRequireAValue).Returns(true);

			var result = generator.Generate(propertyDef.Object, null);
			var attribute = result.Attributes().FirstOrDefault(a => a.Name.LocalName == "Required");
			AssertNull(attribute);
		}

		public void TestRelationshipAttribute_WhenAvailableFromEntitySetDefinition_IsSet()
		{
			columnDef.Setup(m => m.DataType).Returns(DbDataType.Text);
			columnDef.Setup(m => m.Length).Returns(2);
			columnDef.Setup(m => m.DoesNotRequireAValue).Returns(true);
			columnDef.Setup(m => m.Table).Returns(Table.Get("RateEntry"));

			propertyDef.Setup(m => m.PropertyName).Returns("OriginLRC");
			propertyDef.Setup(m => m.ColumnDef).Returns(columnDef.Object);

			var codeMapping = new CodeMapping()
			{
				PropertyName = "OriginLRC",
				TableName = "RateEntry",
				RelationshipResolver = "XYZ"
			};
			var codeMapping2 = new CodeMapping()
			{
				PropertyName = "OtherProperty",
				TableName = "RateEntry",
				RelationshipResolver = "ABC"
			};

			var codeMappings = new CodeMappingCollection(new[] { codeMapping, codeMapping2 });
			entitySetDefinition.Setup(m => m.CodeMappings).Returns(codeMappings);

			var result1 = generator.Generate(propertyDef.Object, entitySetDefinition.Object).ToString();
			var result2 = generator.GetNewTopLevelTypes.Single().ToString();

			AssertEquals(@"<element name=""OriginLRC"" minOccurs=""0"" xmlns=""http://www.w3.org/2001/XMLSchema"">
  <complexType>
    <simpleContent>
      <extension base=""namedType_OriginLRC"">
        <attribute name=""Relationship"" type=""xs:string"" />
      </extension>
    </simpleContent>
  </complexType>
</element>", result1);

			AssertEquals(@"<simpleType name=""namedType_OriginLRC"" xmlns=""http://www.w3.org/2001/XMLSchema"">
  <restriction base=""xs:string"">
    <maxLength value=""2"" />
  </restriction>
</simpleType>", result2);
		}

		public void TestRelationshipAttribute_WhenNotAvailableFromEntitySetDefinition_IsNotSet()
		{
			columnDef.Setup(m => m.DataType).Returns(DbDataType.Text);
			columnDef.Setup(m => m.Length).Returns(2);
			columnDef.Setup(m => m.DoesNotRequireAValue).Returns(true);
			columnDef.Setup(m => m.Table).Returns(Table.Get("RateEntry"));

			propertyDef.Setup(m => m.PropertyName).Returns("OriginLRC");
			propertyDef.Setup(m => m.ColumnDef).Returns(columnDef.Object);

			var codeMapping = new CodeMapping()
			{
				PropertyName = "OtherProperty",
				TableName = "RateEntry",
				RelationshipResolver = "XYZ"
			};
			var codeMappings = new CodeMappingCollection(new CodeMapping[] { codeMapping });
			entitySetDefinition.Setup(m => m.CodeMappings).Returns(codeMappings);

			var result1 = generator.Generate(propertyDef.Object, entitySetDefinition.Object).ToString();
			var result2 = generator.GetNewTopLevelTypes.SingleOrDefault()?.ToString();

			AssertEquals(@"<element name=""OriginLRC"" minOccurs=""0"" xmlns=""http://www.w3.org/2001/XMLSchema"">
  <simpleType>
    <restriction base=""xs:string"">
      <maxLength value=""2"" />
    </restriction>
  </simpleType>
</element>", result1);

			AssertNull(result2);
		}

		protected override void SetUp()
		{
			base.SetUp();
			entitySetDefinition = new Mock<IEntitySetDefinition>();
			propertyDef = new Mock<IPropertyDef>();
			columnDef = new Mock<IColumnDef>();
			generator = new PropertyDefinitionXsdGenerator();
		}
		Mock<IEntitySetDefinition> entitySetDefinition;
		Mock<IPropertyDef> propertyDef;
		Mock<IColumnDef> columnDef;
		PropertyDefinitionXsdGenerator generator;
	}
}
