using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Warehouse.Environment.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ProductStyleWrapper))]
	sealed class ProductStyleWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = new ProductStyleWrapper(null, Factory);
			AssertEquals("wrapperEmpty.ToString()", "(No Default Field Value Available on ProductStyle)", wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.Code", "", wrapperEmpty.Code);
			AssertEquals("wrapperEmpty.Description", "", wrapperEmpty.Description);
			AssertEquals("wrapperEmpty.Sizes", 0, wrapperEmpty.Sizes.Count);
		}

		public void TestWrapperMappingFull()
		{
			var style = Factory.New<WhsProductStyle>();
			style.Sizes.AddNew();
			style.Sizes.AddNew();
			style.WST_Code = "COD";
			style.WST_Description = "Desc";

			var wrapper = new ProductStyleWrapper(style, Factory);
			AssertEquals("wrapper.ToString()", "(No Default Field Value Available on ProductStyle)", wrapper.ToString());
			AssertEquals("wrapper.Code", "COD", wrapper.Code);
			AssertEquals("wrapper.Description", "Desc", wrapper.Description);
			AssertEquals("wrapperEmpty.Sizes", 2, wrapper.Sizes.Count);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"ProductStyle
======================================================================
Name                                    Type
----------------------------------------------------------------------
Code                                    String
Description                             String

Sizes                                   ProductStyleSizeCollection Collection
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"Registry : (No Default Field Value Available on Registry)";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var style = Factory.New<WhsProductStyle>();
			return new ProductStyleWrapper(style, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var style = Factory.New<WhsProductStyle>();
			return new ProductStyleWrapper(style, Factory);
		}
	}
}
