using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Warehouse.Environment.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ProductStyleColourWrapper))]
	sealed class ProductStyleColourWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = new ProductStyleColourWrapper(null, Factory);
			AssertEquals("wrapperEmpty.ToString()", "(No Default Field Value Available on ProductStyleColour)", wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.Code", "", wrapperEmpty.Code);
			AssertEquals("wrapperEmpty.Description", "", wrapperEmpty.Description);
			AssertNull("wrapperEmpty.Style", wrapperEmpty.Style.WrappedObject);
		}

		public void TestWrapperMappingFull()
		{
			var style = Factory.New<WhsProductStyle>();
			var styleColour = style.Colours.AddNew();

			styleColour.WSC_Code = "COD";
			styleColour.WSC_Description = "Desc";

			var wrapper = new ProductStyleColourWrapper(styleColour, Factory);
			AssertEquals("wrapper.ToString()", "(No Default Field Value Available on ProductStyleColour)", wrapper.ToString());
			AssertEquals("wrapper.Code", "COD", wrapper.Code);
			AssertEquals("wrapper.Description", "Desc", wrapper.Description);
			AssertEquals("wrapper.Style", style, wrapper.Style.WrappedObject);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"ProductStyleColour
======================================================================
Name                                    Type
----------------------------------------------------------------------
Style                                   ProductStyle
Code                                    String
Description                             String";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"Registry : (No Default Field Value Available on Registry)
Style : (No Default Field Value Available on ProductStyle)";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var style = Factory.New<WhsProductStyleColour>();
			return new ProductStyleColourWrapper(style, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var style = Factory.New<WhsProductStyleColour>();
			return new ProductStyleColourWrapper(style, Factory);
		}
	}
}
