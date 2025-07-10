using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ProductWrapper))]
	sealed class ProductWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = new ProductWrapper(null, Factory);
			AssertEquals("wrapperEmpty.ToString()", "(No Default Field Value Available on Product)", wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.StockUnit", "", wrapperEmpty.StockUnit.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.Volume", "", wrapperEmpty.Volume.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.Weight", "", wrapperEmpty.Weight.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.NetWeight", "", wrapperEmpty.NetWeight.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.Dimensions", "", wrapperEmpty.Dimensions.ValueAndUnitCodeBlankIfZero);
			AssertEquals("wrapperEmpty.Notes", 0, wrapperEmpty.Notes.Count);
			AssertNull("wrapperEmpty.Style", wrapperEmpty.Style.WrappedObject);
			AssertNull("wrapperEmpty.StyleSize", wrapperEmpty.StyleSize.WrappedObject);
			AssertNull("wrapperEmpty.StyleColour", wrapperEmpty.StyleColour.WrappedObject);
			AssertNull("wrapperEmpty.StyleClassification", wrapperEmpty.StyleClassification.WrappedObject);
		}

		public void TestWrapperMappingFull()
		{
			var part = Factory.New<OrgSupplierPart>();
			var style = Factory.New<WhsProductStyle>();
			var styleSize = style.Sizes.AddNew();
			var styleColour = style.Colours.AddNew();
			var styleClassification = style.Classifications.AddNew();

			using (part.GetValidationSuspender())
			{
				part.OP_StockKeepingUnit = "";
				part.OP_Cubic = 123.1m;
				part.OP_CubicUQ = "M3";
				part.OP_Weight = 456.4m;
				part.OP_WeightUQ = "KG";
				part.OP_Height = 1.2m;
				part.OP_Width = 3.4m;
				part.OP_Depth = 5.6;
				part.OP_MeasureUQ = "IN";
				part.OP_NetWeight = 450.0m;
				part.OP_WSC_WhsProductStyleColour = styleColour.PK;
				part.OP_WSS_WhsProductStyleClassification = styleClassification.PK;
				part.OP_WSZ_WhsProductStyleSize = styleSize.PK;

				var note = part.Notes.AddNew();

				var wrapper = new ProductWrapper(part, Factory);
				AssertEquals("wrapper.ToString()", "(No Default Field Value Available on Product)", wrapper.ToString());
				AssertEquals("wrapper.StockUnit", "", wrapper.StockUnit.ValueAndUnitCodeBlankIfZero);
				AssertEquals("wrapper.Volume", "123.100 M3", wrapper.Volume.ValueAndUnitCodeBlankIfZero);
				AssertEquals("wrapper.Weight", "456.400 KG", wrapper.Weight.ValueAndUnitCodeBlankIfZero);
				AssertEquals("wrapper.NetWeight", "450.000 KG", wrapper.NetWeight.ValueAndUnitCodeBlankIfZero);
				AssertEquals("wrapper.Dimensions", "5.6 x 3.4 x 1.2 IN", wrapper.Dimensions.ValueAndUnitCodeBlankIfZero);
				AssertEquals("wrapper.Notes", 1, wrapper.Notes.Count);
				AssertEquals(note, wrapper.Notes[0].WrappedObject);
				AssertEquals("wrapper.Style", style, wrapper.Style.WrappedObject);
				AssertEquals("wrapper.StyleSize", styleSize, wrapper.StyleSize.WrappedObject);
				AssertEquals("wrapper.StyleColour", styleColour, wrapper.StyleColour.WrappedObject);
				AssertEquals("wrapper.StyleClassification", styleClassification, wrapper.StyleClassification.WrappedObject);
			}
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"Product
======================================================================
Name                                    Type
----------------------------------------------------------------------
Commodity                               CodeAndDescription
Dimensions                              Dimensions
Style                                   ProductStyle
StyleClassification                     ProductStyleClassification
StyleColour                             ProductStyleColour
StyleSize                               ProductStyleSize
StockUnit                               ValueAndUnit
Volume                                  Volume
NetWeight                               Weight
Weight                                  Weight

Notes                                   Note Collection
UNDGSubstances                          UNDGSubstance Collection";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"Commodity : 
Dimensions : 
NetWeight : 
Registry : (No Default Field Value Available on Registry)
StockUnit : 
Style : (No Default Field Value Available on ProductStyle)
StyleClassification : (No Default Field Value Available on ProductStyleClassification)
StyleColour : (No Default Field Value Available on ProductStyleColour)
StyleSize : (No Default Field Value Available on ProductStyleSize)
Volume : 
Weight :";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var part = Factory.New<OrgSupplierPart>();
			return new ProductWrapper(part, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var part = Factory.New<OrgSupplierPart>();
			return new ProductWrapper(part, Factory);
		}
	}
}
