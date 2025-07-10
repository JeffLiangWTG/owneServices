using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(PackingLine))]
	class PackingLineTest : DataObjectTestCase<PackingLine>
	{
		public void TestReferenceNumberMaxLength()
		{
			var refNum = typeof(PackingLine).GetProperties().First(x => x.Name == "ReferenceNumber");
			var methodAttribute = refNum.GetCustomAttributes(true).OfType<MaxLengthAttribute>().First();
			AssertEquals(46, methodAttribute.MaxLength);
		}

		public void TestExportReferenceNumberMaxLength()
		{
			var exportRefNum = typeof(PackingLine).GetProperties().First(x => x.Name == "ExportReferenceNumber");
			var methodAttribute = exportRefNum.GetCustomAttributes(true).OfType<MaxLengthAttribute>().First();
			AssertEquals(35, methodAttribute.MaxLength);
		}

		public void TestImportReferenceNumberMaxLength()
		{
			var importRefNum = typeof(PackingLine).GetProperties().First(x => x.Name == "ImportReferenceNumber");
			var methodAttribute = importRefNum.GetCustomAttributes(true).OfType<MaxLengthAttribute>().First();
			AssertEquals(35, methodAttribute.MaxLength);
		}

		public void TestGetCleanSingleLineGoodsDescription()
		{
			var packLine = new PackingLine() { GoodsDescription = "This is  a \r\nmultiline test full of\r\n white \tspaces." };
			AssertEquals("White spaces should be removed", "This is  a multiline test full of white spaces.", packLine.GetCleanSingleLineGoodsDescription());
			packLine = new PackingLine() { GoodsDescription = "This is a string with trailing space. " };
			AssertEquals("Trailing space should be removed", "This is a string with trailing space.", packLine.GetCleanSingleLineGoodsDescription());
			packLine = new PackingLine() { GoodsDescription = "  " };
			AssertEquals("Spaces should be trimmed", "", packLine.GetCleanSingleLineGoodsDescription());
			packLine = new PackingLine() { GoodsDescription = "" };
			AssertEquals("Should be empty", "", packLine.GetCleanSingleLineGoodsDescription());
		}

		protected override List<string> ExpectedAllowLineControlWhiteSpaceAttributePropertiesCore() => new List<string>()
		{
			nameof(PackingLine.DetailedDescription),
			nameof(PackingLine.GoodsDescription),
			nameof(PackingLine.MarksAndNos),
			nameof(PackingLine.ShippingSymbol),
			nameof(PackingLine.OutturnComment)
		};

		public void TestIsHighRiskExistsInPackingLine()
		{
			var packLine = new PackingLine() { GoodsDescription = "This is  a \r\nmultiline test full of\r\n white \tspaces." };
			packLine.IsHighRisk = true;
			AssertEquals(true, packLine.IsHighRisk);
		}

		public void TestAviationSecurityInspectionTypeExistsInPackingLine()
		{
			var packLine = new PackingLine() { GoodsDescription = "This is  a \r\nmultiline test full of\r\n white \tspaces." };
			packLine.AviationSecurityInspectionType = new CodeDescriptionPair();
			packLine.AviationSecurityAdditionalInspectionType = new CodeDescriptionPair();
			AssertNotNull(packLine.AviationSecurityInspectionType);
			AssertNotNull(packLine.AviationSecurityAdditionalInspectionType);
		}
	}
}

