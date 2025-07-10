using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Warehouse.Environment.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(ProductStyleClassificationWrapper))]
	sealed class ProductStyleClassificationWrapperTest : GenericWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = new ProductStyleClassificationWrapper(null, Factory);
			AssertEquals("wrapperEmpty.ToString()", "(No Default Field Value Available on ProductStyleClassification)", wrapperEmpty.ToString());
			AssertEquals("wrapperEmpty.Code", "", wrapperEmpty.Code);
			AssertEquals("wrapperEmpty.Description", "", wrapperEmpty.Description);
			AssertNull("wrapperEmpty.Style", wrapperEmpty.Style.WrappedObject);
		}

		public void TestWrapperMappingFull()
		{
			var style = Factory.New<WhsProductStyle>();
			var styleClassification = style.Classifications.AddNew();

			styleClassification.WSS_Code = "M";
			styleClassification.WSS_Description = "Male";

			var wrapper = new ProductStyleClassificationWrapper(styleClassification, Factory);
			AssertEquals("wrapper.ToString()", "(No Default Field Value Available on ProductStyleClassification)", wrapper.ToString());
			AssertEquals("wrapper.Code", "M", wrapper.Code);
			AssertEquals("wrapper.Description", "Male", wrapper.Description);
			AssertEquals("wrapper.Style", style, wrapper.Style.WrappedObject);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"ProductStyleClassification
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
			var style = Factory.New<WhsProductStyleClassification>();
			return new ProductStyleClassificationWrapper(style, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var style = Factory.New<WhsProductStyleClassification>();
			return new ProductStyleClassificationWrapper(style, Factory);
		}
	}
}
